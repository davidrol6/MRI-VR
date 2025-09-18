using UnityEngine;

public class InventoryGestureController : MonoBehaviour
{
    public enum PlacementMode { FollowAnchor, SpawnNearHMD }

    [Header("Target")]
    [Tooltip("Root of your inventory Canvas/Panel")]
    public GameObject Inventory;

    [Header("Mode")]
    public PlacementMode mode = PlacementMode.SpawnNearHMD;

    [Header("Follow Anchor (mode = FollowAnchor)")]
    [Tooltip("Wrist/hand anchor (or any transform to follow)")]
    public Transform Anchor;
    public Vector3 anchorLocalEulerOffset = new Vector3(15f, 0f, 0f);
    [Tooltip("Small world-space offset from the anchor, transformed by Anchor")]
    public Vector3 anchorWorldOffset = Vector3.zero;

    [Header("Spawn Near HMD (mode = SpawnNearHMD)")]
    [Tooltip("Empty under CenterEyeAnchor (or any transform near the HMD)")]
    public Transform spawnPoint;
    [Tooltip("Extra local offset applied to the spawnPoint")]
    public Vector3 spawnExtraLocalOffset = Vector3.zero;

    [Header("Facing / Billboard")]
    [Tooltip("Force the inventory to face the camera (yaw only)")]
    public bool faceCamera = true;

    [Header("Smoothing")]
    [Tooltip("If true, smooth-follow while visible")]
    public bool smoothFollowWhileVisible = true;
    [Range(0f, 20f)] public float followLerp = 12f;

    [Header("HMD")]
    [Tooltip("Drag CenterEyeAnchor (or any XR camera) here. Falls back to Camera.main.")]
    public Transform hmdTransform;

    [Header("Auto Pose")]
    [Tooltip("When the Inventory becomes active, immediately place it once")]
    public bool poseOnceOnShow = true;

    // internal
    private bool _wasActive;

    private void Awake()
    {
        if (hmdTransform == null && Camera.main != null)
            hmdTransform = Camera.main.transform;

        if (Inventory != null)
            _wasActive = Inventory.activeSelf;
    }

    private void Update()
    {
        if (Inventory == null) return;

        bool isActive = Inventory.activeSelf;

        // Detect rising edge of visibility to place once
        if (poseOnceOnShow && isActive && !_wasActive)
        {
            PoseInventory(smooth: false);
        }

        // While visible, optionally smooth-follow
        if (isActive && smoothFollowWhileVisible)
        {
            PoseInventory(smooth: true);
        }

        _wasActive = isActive;
    }

    private void PoseInventory(bool smooth)
    {
        if (Inventory == null) return;

        Vector3 desiredPos;
        Quaternion desiredRot;

        if (mode == PlacementMode.FollowAnchor)
        {
            if (Anchor == null) return;

            desiredPos = Anchor.position + Anchor.TransformVector(anchorWorldOffset);
            desiredRot = Anchor.rotation * Quaternion.Euler(anchorLocalEulerOffset);

            if (faceCamera && hmdTransform != null)
            {
                Vector3 toCam = hmdTransform.position - desiredPos;
                toCam.y = 0f;
                if (toCam.sqrMagnitude > 0.0001f)
                    desiredRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
            }
        }
        else // SpawnNearHMD
        {
            if (spawnPoint == null) return;

            desiredPos = spawnPoint.TransformPoint(spawnExtraLocalOffset);
            desiredRot = spawnPoint.rotation;

            if (faceCamera && hmdTransform != null)
            {
                Vector3 toCam = hmdTransform.position - desiredPos;
                toCam.y = 0f;
                if (toCam.sqrMagnitude > 0.0001f)
                    desiredRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
            }
        }

        ApplyPose(desiredPos, desiredRot, smooth);
    }

    private void ApplyPose(Vector3 pos, Quaternion rot, bool smooth)
    {
        Transform t = Inventory.transform;

        if (smooth)
        {
            t.position = Vector3.Lerp(t.position, pos, Time.deltaTime * followLerp);
            t.rotation = Quaternion.Slerp(t.rotation, rot, Time.deltaTime * followLerp);
        }
        else
        {
            t.position = pos;
            t.rotation = rot;
        }
    }

    // Optional helpers you can call from other scripts/UnityEvents.
    // These DO NOT toggle by gesture; they only place/follow.
    public void ReposeNow()
    {
        if (Inventory != null && Inventory.activeSelf)
            PoseInventory(smooth: false);
    }

    public void StartFollowing()
    {
        smoothFollowWhileVisible = true;
    }

    public void StopFollowing()
    {
        smoothFollowWhileVisible = false;
    }
}
