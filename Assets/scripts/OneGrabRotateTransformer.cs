/* * SCRIPT SUMMARY:
 * This script is a component of the Meta Interaction SDK that defines how an object rotates when grabbed with one hand.
 * * * How it works:
 * - It implements the ITransformer interface to handle the math of moving/rotating an object during a grab.
 * - It defines a Pivot Point and a Rotation Axis (e.g., rotating a door around its hinge).
 * - On 'BeginTransform', it calculates the initial angle of the hand relative to the pivot.
 * - On 'UpdateTransform', it tracks the hand's movement around the axis and applies the corresponding rotation to the object.
 * - It supports angular constraints (Min/Max limits) to prevent objects from rotating too far (e.g., a lever that stops at 90 degrees).
 */

using System;
using UnityEngine;

namespace Oculus.Interaction
{
    /// <summary>
    /// An <see cref="ITransformer"/> that rotates the target about an axis.
    /// Updates apply relative rotational changes of a GrabPoint about an axis.
    /// The axis is defined by a pivot transform: a world position and up vector.
    /// </summary>
    public class OneGrabRotateTransformer : MonoBehaviour, ITransformer
    {
        public enum Axis
        {
            Right = 0,
            Up = 1,
            Forward = 2
        }

        [SerializeField, Optional]
        private Transform _pivotTransform = null; // The center point of rotation (e.g., the hinge)

        public Transform Pivot => _pivotTransform != null ? _pivotTransform : transform;

        [SerializeField]
        private Axis _rotationAxis = Axis.Up; // Which local axis to rotate around

        public Axis RotationAxis => _rotationAxis;

        [Serializable]
        public class OneGrabRotateConstraints
        {
            public FloatConstraint MinAngle;
            public FloatConstraint MaxAngle;
        }

        [SerializeField]
        private OneGrabRotateConstraints _constraints =
            new OneGrabRotateConstraints()
            {
                MinAngle = new FloatConstraint(),
                MaxAngle = new FloatConstraint()
            };

        public OneGrabRotateConstraints Constraints
        {
            get
            {
                return _constraints;
            }

            set
            {
                _constraints = value;
            }
        }

        // Internal state tracking for rotation logic
        private float _relativeAngle = 0.0f;
        private float _constrainedRelativeAngle = 0.0f;

        private IGrabbable _grabbable;
        private Vector3 _grabPositionInPivotSpace;
        private Pose _transformPoseInPivotSpace;

        private Pose _worldPivotPose;
        private Vector3 _previousVectorInPivotSpace;

        private Quaternion _localRotation;
        private float _startAngle = 0;


        /// <summary>
        /// Implementation of <see cref="ITransformer.Initialize"/>
        /// Called by the Grabbable component when the script starts.
        /// </summary>
        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
        }

        /// <summary>
        /// Computes the pose of the rotational pivot, in world space.
        /// </summary>
        public Pose ComputeWorldPivotPose()
        {
            if (_pivotTransform != null)
            {
                return _pivotTransform.GetPose();
            }

            // If no specific pivot is assigned, calculate one based on the object's current transform
            var targetTransform = _grabbable.Transform;

            Vector3 worldPosition = targetTransform.position;
            Quaternion worldRotation = targetTransform.parent != null
                ? targetTransform.parent.rotation * _localRotation
                : _localRotation;

            return new Pose(worldPosition, worldRotation);
        }

        /// <summary>
        /// Implementation of <see cref="ITransformer.BeginTransform"/>
        /// Called once when the user first grabs the object.
        /// </summary>
        public void BeginTransform()
        {
            var grabPoint = _grabbable.GrabPoints[0]; // The position of the hand/controller
            var targetTransform = _grabbable.Transform;

            if (_pivotTransform == null)
            {
                _localRotation = targetTransform.localRotation;
            }

            // Determine the rotation axis vector in local space
            Vector3 localAxis = Vector3.zero;
            localAxis[(int)_rotationAxis] = 1f;

            _worldPivotPose = ComputeWorldPivotPose();
            Vector3 rotationAxis = _worldPivotPose.rotation * localAxis;

            Quaternion inverseRotation = Quaternion.Inverse(_worldPivotPose.rotation);

            Vector3 grabDelta = grabPoint.position - _worldPivotPose.position;
            
            // Handle edge case where grab point is exactly at the pivot (avoid divide by zero)
            if (Mathf.Abs(grabDelta.magnitude) < 0.001f)
            {
                Vector3 localAxisNext = Vector3.zero;
                localAxisNext[((int)_rotationAxis + 1) % 3] = 0.001f;
                grabDelta = _worldPivotPose.rotation * localAxisNext;
            }

            _grabPositionInPivotSpace = inverseRotation * grabDelta;

            Vector3 worldPositionDelta =
                inverseRotation * (targetTransform.position - _worldPivotPose.position);

            Quaternion worldRotationDelta = inverseRotation * targetTransform.rotation;
            _transformPoseInPivotSpace = new Pose(worldPositionDelta, worldRotationDelta);

            // Project the grab vector onto the rotation plane to measure angles
            Vector3 initialOffset = _worldPivotPose.rotation * _grabPositionInPivotSpace;
            Vector3 initialVector = Vector3.ProjectOnPlane(initialOffset, rotationAxis);
            _previousVectorInPivotSpace = Quaternion.Inverse(_worldPivotPose.rotation) * initialVector;

            _startAngle = _constrainedRelativeAngle;
            _relativeAngle = _startAngle;

            float parentScale = targetTransform.parent != null ? targetTransform.parent.lossyScale.x : 1f;
            _transformPoseInPivotSpace.position /= parentScale;
        }

        /// <summary>
        /// Implementation of <see cref="ITransformer.UpdateTransform"/>
        /// Called every frame while the object is being held.
        /// </summary>
        public void UpdateTransform()
        {
            var grabPoint = _grabbable.GrabPoints[0];
            var targetTransform = _grabbable.Transform;

            Vector3 localAxis = Vector3.zero;
            localAxis[(int)_rotationAxis] = 1f;
            _worldPivotPose = ComputeWorldPivotPose();
            Vector3 rotationAxis = _worldPivotPose.rotation * localAxis;

            // Project current hand position onto the rotation plane
            Vector3 targetOffset = grabPoint.position - _worldPivotPose.position;
            Vector3 targetVector = Vector3.ProjectOnPlane(targetOffset, rotationAxis);

            Vector3 previousVectorInWorldSpace =
                _worldPivotPose.rotation * _previousVectorInPivotSpace;

            // Update stored vector for the next frame
            _previousVectorInPivotSpace = Quaternion.Inverse(_worldPivotPose.rotation) * targetVector;

            // Calculate the angle difference between last frame and this frame
            float signedAngle =
                Vector3.SignedAngle(previousVectorInWorldSpace, targetVector, rotationAxis);

            _relativeAngle += signedAngle;

            // Apply Constraints (Min/Max Angle)
            _constrainedRelativeAngle = _relativeAngle;
            if (Constraints.MinAngle.Constrain)
            {
                _constrainedRelativeAngle = Mathf.Max(_constrainedRelativeAngle, Constraints.MinAngle.Value);
            }
            if (Constraints.MaxAngle.Constrain)
            {
                _constrainedRelativeAngle = Mathf.Min(_constrainedRelativeAngle, Constraints.MaxAngle.Value);
            }

            // Apply the final rotation to the object
            Quaternion deltaRotation = Quaternion.AngleAxis(_constrainedRelativeAngle - _startAngle, rotationAxis);

            float parentScale = targetTransform.parent != null ? targetTransform.parent.lossyScale.x : 1f;
            Pose transformDeltaInWorldSpace =
                new Pose(
                    _worldPivotPose.rotation * (parentScale * _transformPoseInPivotSpace.position),
                    _worldPivotPose.rotation * _transformPoseInPivotSpace.rotation);

            Pose transformDeltaRotated = new Pose(
                deltaRotation * transformDeltaInWorldSpace.position,
                deltaRotation * transformDeltaInWorldSpace.rotation);

            targetTransform.position = _worldPivotPose.position + transformDeltaRotated.position;
            targetTransform.rotation = transformDeltaRotated.rotation;
        }

        public void EndTransform() 
        {
            // Standard implementation typically empty unless cleanup is needed
        }

        // Custom method to forcibly reset the rotation to zero/neutral
        public void ResetTransform()
        {
            Debug.Log("ResetTransform");

            var targetTransform = _grabbable.Transform;

            Vector3 localAxis = Vector3.zero;
            localAxis[(int)_rotationAxis] = 1f;
            _worldPivotPose = ComputeWorldPivotPose();
            Vector3 rotationAxis = _worldPivotPose.rotation * localAxis;

            // Reset vectors
            _previousVectorInPivotSpace = new Vector3(0f, 0f, 1f);

            // Force rotation to zero
            Quaternion deltaRotation = Quaternion.AngleAxis(0f, rotationAxis);

            float parentScale = 1f;
            Pose transformDeltaInWorldSpace =
                new Pose(
                    _worldPivotPose.rotation * (parentScale * _transformPoseInPivotSpace.position),
                    _worldPivotPose.rotation * _transformPoseInPivotSpace.rotation);

            Pose transformDeltaRotated = new Pose(
                deltaRotation * transformDeltaInWorldSpace.position,
                deltaRotation * transformDeltaInWorldSpace.rotation);

            // Apply the reset transform
            targetTransform.position = _worldPivotPose.position + transformDeltaRotated.position;
            targetTransform.rotation = transformDeltaRotated.rotation;
            
            // Reset state variables
            _relativeAngle = 0.0f;
            _constrainedRelativeAngle = 0.0f;
        }

        #region Inject

        // Dependency injection methods used by the Oculus Interaction SDK

        public void InjectOptionalPivotTransform(Transform pivotTransform)
        {
            _pivotTransform = pivotTransform;
        }

        public void InjectOptionalRotationAxis(Axis rotationAxis)
        {
            _rotationAxis = rotationAxis;
        }

        public void InjectOptionalConstraints(OneGrabRotateConstraints constraints)
        {
            _constraints = constraints;
        }

        #endregion
    }
}