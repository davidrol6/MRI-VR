using UnityEngine;
using UnityEngine.UI;

public class OVRTeleportToPose : MonoBehaviour
{
    [Header("Rig (OVR)")]
    [Tooltip("Root of the rig, e.g. '[BuildingBlock] Camera Rig'.")]
    public Transform rigRoot;

    [Tooltip("The CenterEyeAnchor, e.g. TrackingSpace/CenterEyeAnchor.")]
    public Transform centerEyeAnchor;

    [Header("Destination")]
    [Tooltip("Transform that defines the desired head position (eye) and orientation.")]
    public Transform targetPose;

    [Header("Orientation")]
    [Tooltip("If true, align only the yaw. Ignored if alignFullRotation is true.")]
    public bool matchTargetYaw = true;

    [Tooltip("If true, align full rotation (yaw + pitch + roll) of the camera to the target.")]
    public bool alignFullRotation = true;

    [Tooltip("Extra yaw offset in degrees. Used only if alignFullRotation is false.")]
    public float yawOffsetDeg = 0f;

    [Header("Optional")]
    [Tooltip("If your rig uses a CharacterController, assign it here to avoid pushback when teleporting.")]
    public CharacterController rigCharacterController;

    [Header("UI (optional)")]
    [Tooltip("If assigned, teleport will be triggered when the Toggle changes state.")]
    public Toggle triggerToggle;
    public bool teleportOnToggleIsOn = true;

    private void Reset()
    {
        if (!rigRoot) 
            rigRoot = transform;

        if (!centerEyeAnchor)
        {
            var t = transform.Find("TrackingSpace/CenterEyeAnchor");
            if (t) centerEyeAnchor = t;
        }

        if (!triggerToggle) 
            triggerToggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        if (triggerToggle)
            triggerToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDisable()
    {
        if (triggerToggle)
            triggerToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (teleportOnToggleIsOn == isOn)
            TeleportNow();
    }

    [ContextMenu("Teleport Now")]
    public void TeleportNow()
    {
        if (!rigRoot || !centerEyeAnchor || !targetPose)
        {
            Debug.LogWarning("[OVRTeleportToPose] Please assign rigRoot, centerEyeAnchor and targetPose.");
            return;
        }

        bool hadCC = false;
        if (rigCharacterController)
        {
            hadCC = rigCharacterController.enabled;
            rigCharacterController.enabled = false;
        }

        // ORIENTATION
        if (alignFullRotation)
        {
            // Match full world rotation of the camera (yaw + pitch + roll)
            Quaternion camWorldRot = centerEyeAnchor.rotation;
            Quaternion desiredWorldRot = targetPose.rotation;
            Quaternion deltaRot = desiredWorldRot * Quaternion.Inverse(camWorldRot);

            RotateRigAroundPointQuat(rigRoot, centerEyeAnchor.position, deltaRot);
        }
        else if (matchTargetYaw)
        {
            // Match yaw only
            float currentYaw = centerEyeAnchor.eulerAngles.y;
            float desiredYaw = targetPose.eulerAngles.y + yawOffsetDeg;
            float deltaYaw = Mathf.DeltaAngle(currentYaw, desiredYaw);
            RotateRigAroundPointAxis(rigRoot, centerEyeAnchor.position, Vector3.up, deltaYaw);
        }

        // POSITION
        // After rotation, reposition the rig so the head lands exactly at targetPose.position
        Vector3 afterCamPos = centerEyeAnchor.position;
        Vector3 deltaPos = targetPose.position - afterCamPos;
        rigRoot.position += deltaPos;

        if (rigCharacterController)
            rigCharacterController.enabled = hadCC;
    }

    // Rotate rig around a world-space pivot using axis/angle.
    private static void RotateRigAroundPointAxis(Transform rig, Vector3 pivotWorld, Vector3 axisWorld, float angleDeg)
    {
        Vector3 toRig = rig.position - pivotWorld;
        Quaternion q = Quaternion.AngleAxis(angleDeg, axisWorld);
        rig.position = pivotWorld + q * toRig;
        rig.rotation = q * rig.rotation;
    }

    // Rotate rig around a world-space pivot using a full quaternion (yaw + pitch + roll).
    private static void RotateRigAroundPointQuat(Transform rig, Vector3 pivotWorld, Quaternion deltaRot)
    {
        Vector3 toRig = rig.position - pivotWorld;
        rig.position = pivotWorld + deltaRot * toRig;
        rig.rotation = deltaRot * rig.rotation;
    }
}
