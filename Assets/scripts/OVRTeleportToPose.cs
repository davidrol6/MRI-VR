using UnityEngine;
using UnityEngine.UI;

public class OVRTeleportToPose : MonoBehaviour
{
    [Header("Rig (OVR)")]
    [Tooltip("Root del rig: el objeto '[BuildingBlock] Camera Rig'")]
    public Transform rigRoot;

    [Tooltip("El CenterEyeAnchor: TrackingSpace/CenterEyeAnchor")]
    public Transform centerEyeAnchor;

    [Header("Destino")]
    [Tooltip("Transform al que quieres llevar la cabeza (posición del ojo) y su rotación.")]
    public Transform targetPose;

    [Header("Orientación")]
    [Tooltip("Alinear solo el yaw (true) o alinear rotación completa (false se ignora si alignFullRotation = true).")]
    public bool matchTargetYaw = true;

    [Tooltip("Si está activo, alinea Yaw+Pitch+Roll de la cámara con el target.")]
    public bool alignFullRotation = true;

    [Tooltip("Offset extra sobre el yaw final (grados). Solo se usa si alignFullRotation = false.")]
    public float yawOffsetDeg = 0f;

    [Header("Opcional")]
    [Tooltip("Si tu rig lleva CharacterController, arrástralo aquí para evitar empujones.")]
    public CharacterController rigCharacterController;

    [Header("UI (opcional)")]
    [Tooltip("Si lo asignas, al activar el Toggle se teletransporta.")]
    public Toggle triggerToggle;
    public bool teleportOnToggleIsOn = true;

    void Reset()
    {
        if (!rigRoot) rigRoot = transform;
        if (!centerEyeAnchor)
        {
            var t = transform.Find("TrackingSpace/CenterEyeAnchor");
            if (t) centerEyeAnchor = t;
        }
        if (!triggerToggle) triggerToggle = GetComponent<Toggle>();
    }

    void OnEnable()
    {
        if (triggerToggle)
            triggerToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnDisable()
    {
        if (triggerToggle)
            triggerToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        if (teleportOnToggleIsOn == isOn)
            TeleportNow();
    }

    [ContextMenu("Teleport Now")]
    public void TeleportNow()
    {
        if (!rigRoot || !centerEyeAnchor || !targetPose)
        {
            Debug.LogWarning("[OVRTeleportToPose] Asigna rigRoot, centerEyeAnchor y targetPose.");
            return;
        }

        bool hadCC = false;
        if (rigCharacterController)
        {
            hadCC = rigCharacterController.enabled;
            rigCharacterController.enabled = false;
        }

        // --- 1) ORIENTACIÓN ---
        if (alignFullRotation)
        {
            // Alinear rotación mundial de la cámara (incluye yaw, pitch y roll)
            Quaternion camWorldRot = centerEyeAnchor.rotation;
            Quaternion desiredWorldRot = targetPose.rotation;
            Quaternion deltaRot = desiredWorldRot * Quaternion.Inverse(camWorldRot);

            RotateRigAroundPointQuat(rigRoot, centerEyeAnchor.position, deltaRot);
        }
        else if (matchTargetYaw)
        {
            // Solo yaw
            float currentYaw = centerEyeAnchor.eulerAngles.y;
            float desiredYaw = targetPose.eulerAngles.y + yawOffsetDeg;
            float deltaYaw = Mathf.DeltaAngle(currentYaw, desiredYaw);
            RotateRigAroundPointAxis(rigRoot, centerEyeAnchor.position, Vector3.up, deltaYaw);
        }

        // --- 2) POSICIÓN ---
        // Tras girar, reposicionamos el rig para que la cabeza caiga EXACTAMENTE en targetPose.position
        Vector3 afterCamPos = centerEyeAnchor.position;
        Vector3 deltaPos = targetPose.position - afterCamPos;
        rigRoot.position += deltaPos;

        if (rigCharacterController)
            rigCharacterController.enabled = hadCC;
    }

    // Rotar el rig alrededor de un punto aplicando un eje/ángulo
    static void RotateRigAroundPointAxis(Transform rig, Vector3 pivotWorld, Vector3 axisWorld, float angleDeg)
    {
        Vector3 toRig = rig.position - pivotWorld;
        Quaternion q = Quaternion.AngleAxis(angleDeg, axisWorld);
        rig.position = pivotWorld + q * toRig;
        rig.rotation = q * rig.rotation;
    }

    // Rotar el rig alrededor de un punto aplicando un QUATERNION completo (yaw+pitch+roll)
    static void RotateRigAroundPointQuat(Transform rig, Vector3 pivotWorld, Quaternion deltaRot)
    {
        Vector3 toRig = rig.position - pivotWorld;
        rig.position = pivotWorld + deltaRot * toRig;
        rig.rotation = deltaRot * rig.rotation;
    }
}
