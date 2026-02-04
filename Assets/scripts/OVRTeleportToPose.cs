/* * SCRIPT SUMMARY:
 * This script teleports the VR Player Rig to a specific destination point (Transform).
 * * * How it works:
 * - Unlike standard teleportation which moves the Rig Root, this script aligns the *Camera* (CenterEyeAnchor) to the target.
 * - This ensures that the player's head ends up exactly where intended, regardless of where they are standing in their physical play area (Room Scale).
 * - It handles complex rotation math to rotate the Rig around the player's head.
 * - It temporarily disables the CharacterController (if present) to prevent physics conflicts during the move.
 * - It can be triggered manually via code or by a UI Toggle.
 */

using UnityEngine;
using UnityEngine.UI;

public class OVRTeleportToPose : MonoBehaviour
{
    [Header("Rig (OVR)")]
    [Tooltip("Root of the rig, e.g. '[BuildingBlock] Camera Rig'.")]
    public Transform rigRoot; // The top-most parent of the VR setup

    [Tooltip("The CenterEyeAnchor, e.g. TrackingSpace/CenterEyeAnchor.")]
    public Transform centerEyeAnchor; // The specific object tracking the headset position

    [Header("Destination")]
    [Tooltip("Transform that defines the desired head position (eye) and orientation.")]
    public Transform targetPose; // Where the player should end up

    [Header("Orientation")]
    [Tooltip("If true, align only the yaw. Ignored if alignFullRotation is true.")]
    public bool matchTargetYaw = true; // Keeps the floor level (only rotates Y axis)

    [Tooltip("If true, align full rotation (yaw + pitch + roll) of the camera to the target.")]
    public bool alignFullRotation = true; // Forces the head to look exactly at the target rotation (useful for cinematics or lying down)

    [Tooltip("Extra yaw offset in degrees. Used only if alignFullRotation is false.")]
    public float yawOffsetDeg = 0f;

    [Header("Optional")]
    [Tooltip("If your rig uses a CharacterController, assign it here to avoid pushback when teleporting.")]
    public CharacterController rigCharacterController; // Optional reference to handle physics conflicts

    [Header("UI (optional)")]
    [Tooltip("If assigned, teleport will be triggered when the Toggle changes state.")]
    public Toggle triggerToggle;
    public bool teleportOnToggleIsOn = true;

    private void Reset()
    {
        // specific logic to try and auto-find standard OVR hierarchy objects
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
        // Trigger the teleport if the toggle matches the desired state
        if (teleportOnToggleIsOn == isOn)
            TeleportNow();
    }

    // Main function to perform the move. Can be called from other scripts or buttons.
    [ContextMenu("Teleport Now")]
    public void TeleportNow()
    {
        if (!rigRoot || !centerEyeAnchor || !targetPose)
        {
            Debug.LogWarning("[OVRTeleportToPose] Please assign rigRoot, centerEyeAnchor and targetPose.");
            return;
        }

        // 1. Temporarily disable CharacterController
        // If enabled, the CC will try to override the position update or collide with geometry during the teleport.
        bool hadCC = false;
        if (rigCharacterController)
        {
            hadCC = rigCharacterController.enabled;
            rigCharacterController.enabled = false;
        }

        // 2. ORIENTATION LOGIC
        // We need to rotate the Rig so that the Camera (child) matches the Target rotation.
        if (alignFullRotation)
        {
            // Match full world rotation (Yaw, Pitch, Roll)
            Quaternion camWorldRot = centerEyeAnchor.rotation;
            Quaternion desiredWorldRot = targetPose.rotation;
            
            // Calculate the difference required
            Quaternion deltaRot = desiredWorldRot * Quaternion.Inverse(camWorldRot);

            // Apply rotation to the Rig, pivoting around the Camera's current position
            RotateRigAroundPointQuat(rigRoot, centerEyeAnchor.position, deltaRot);
        }
        else if (matchTargetYaw)
        {
            // Match vertical (Y-axis) rotation only
            float currentYaw = centerEyeAnchor.eulerAngles.y;
            float desiredYaw = targetPose.eulerAngles.y + yawOffsetDeg;
            float deltaYaw = Mathf.DeltaAngle(currentYaw, desiredYaw);
            
            RotateRigAroundPointAxis(rigRoot, centerEyeAnchor.position, Vector3.up, deltaYaw);
        }

        // 3. POSITION LOGIC
        // Rotating the rig might have shifted the camera slightly if the pivot wasn't perfect.
        // Now we simply translate the rig so the camera lands exactly on the target position.
        Vector3 afterCamPos = centerEyeAnchor.position;
        Vector3 deltaPos = targetPose.position - afterCamPos;
        rigRoot.position += deltaPos;

        // 4. Restore CharacterController
        if (rigCharacterController)
            rigCharacterController.enabled = hadCC;
    }

    // Helper: Rotates the Rig around a specific pivot point (the player's head) using an axis/angle
    private static void RotateRigAroundPointAxis(Transform rig, Vector3 pivotWorld, Vector3 axisWorld, float angleDeg)
    {
        Vector3 toRig = rig.position - pivotWorld;
        Quaternion q = Quaternion.AngleAxis(angleDeg, axisWorld);
        rig.position = pivotWorld + q * toRig;
        rig.rotation = q * rig.rotation;
    }

    // Helper: Rotates the Rig around a specific pivot point using a full quaternion
    private static void RotateRigAroundPointQuat(Transform rig, Vector3 pivotWorld, Quaternion deltaRot)
    {
        Vector3 toRig = rig.position - pivotWorld;
        rig.position = pivotWorld + deltaRot * toRig;
        rig.rotation = deltaRot * rig.rotation;
    }
}