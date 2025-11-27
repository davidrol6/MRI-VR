using UnityEngine;

public class RoleCanvasSwitcher : MonoBehaviour
{
    [Header("Role Source")]
    [SerializeField] private RoleProvider roleProvider;

    [Header("Scene Canvases")]
    [SerializeField] private GameObject patientCanvas;
    [SerializeField] private GameObject technicianCanvas;

    private void Awake()
    {
        if (roleProvider == null)
        {
#if UNITY_2023_1_OR_NEWER
            roleProvider = FindFirstObjectByType<RoleProvider>(FindObjectsInactive.Include);
#else
            roleProvider = FindObjectOfType<RoleProvider>(true);
#endif
        }
    }

    // Toggle callback
    public void OnToggleChanged(bool on)
    {
        if (roleProvider == null)
        {
            Debug.LogError("[RoleCanvasSwitcher] No RoleProvider assigned or found.");
            return;
        }

        if (!on)
        {
            HideAll();
            return;
        }

        ShowForCurrentRole();
    }

    public void ShowForCurrentRole()
    {
        Debug.Log("[RoleCanvasSwitcher] Current role: " + roleProvider.role);

        bool isPatient = roleProvider.role == Role.Patient;

        if (patientCanvas)     patientCanvas.SetActive(isPatient);
        if (technicianCanvas)  technicianCanvas.SetActive(!isPatient);
    }

    public void HideAll()
    {
        if (patientCanvas)     patientCanvas.SetActive(false);
        if (technicianCanvas)  technicianCanvas.SetActive(false);
    }
}
