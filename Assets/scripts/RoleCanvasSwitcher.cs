using UnityEngine;

public class RoleCanvasSwitcher : MonoBehaviour
{
    [Header("Role Source")]
    [SerializeField] private RoleProvider roleProvider; // assign in Inspector if possible

    [Header("Scene Canvases")]
    [SerializeField] private GameObject patientCanvas;
    [SerializeField] private GameObject technicianCanvas;

    private void Awake()
    {
        // Resolve once to avoid doing it every toggle press.
        if (roleProvider == null)
        {
            // Unity 2023+ API (preferred)
#if UNITY_2023_1_OR_NEWER
            roleProvider = FindFirstObjectByType<RoleProvider>(FindObjectsInactive.Include);
#else
            // Fallback for older Unity
            roleProvider = FindObjectOfType<RoleProvider>(true);
#endif
        }
    }

    // Toggle hook: true = show, false = hide
    public void OnToggleChanged(bool on)
    {
        if (roleProvider == null)
        {
            Debug.LogError("[RoleCanvasSwitcher] No RoleProvider found/assigned.");
            return;
        }

        if (!on)
        {
            HideAll();
            return;
        }

        ShowForCurrentRole();
    }

    // Button-friendly helpers (optional)
    public void ShowForCurrentRole()
    {
        bool isPatient = roleProvider.role == Role.Patient;

        if (patientCanvas)   patientCanvas.SetActive(isPatient);
        if (technicianCanvas) technicianCanvas.SetActive(!isPatient);
    }

    public void HideAll()
    {
        if (patientCanvas)   patientCanvas.SetActive(false);
        if (technicianCanvas) technicianCanvas.SetActive(false);
    }
}
