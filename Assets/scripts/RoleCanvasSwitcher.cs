/* * SCRIPT SUMMARY:
 * This script manages the visibility of the UI Canvases in the scene based on the selected role.
 * * * How it works:
 * It handles the transition logic after the "Choose Role" decision point.
 * - If the user is a Patient, it activates the "PatientCanvas".
 * - If the user is a Technician, it activates the "TechnicianCanvas".
 * It communicates with the RoleProvider to determine which role is currently active.
 */

using UnityEngine;

public class RoleCanvasSwitcher : MonoBehaviour
{
    [Header("Role Source")]
    [SerializeField] private RoleProvider roleProvider; // Reference to the global state manager

    [Header("Scene Canvases")]
    [SerializeField] private GameObject patientCanvas;    // UI for Patient Simulation
    [SerializeField] private GameObject technicianCanvas; // UI for Tech Simulation

    private void Awake()
    {
        // Specific logic to find the RoleProvider if it wasn't assigned in the Inspector
        if (roleProvider == null)
        {
            // Checks Unity version to use the most efficient method to find the object
#if UNITY_2023_1_OR_NEWER
            roleProvider = FindFirstObjectByType<RoleProvider>(FindObjectsInactive.Include);
#else
            roleProvider = FindObjectOfType<RoleProvider>(true);
#endif
        }
    }

    // Callback method often hooked up to a UI Toggle or Checkbox
    public void OnToggleChanged(bool on)
    {
        // Safety check: ensure the provider exists before trying to access it
        if (roleProvider == null)
        {
            Debug.LogError("[RoleCanvasSwitcher] No RoleProvider assigned or found.");
            return;
        }

        // If the toggle is turned off, we hide everything to avoid UI clutter
        if (!on)
        {
            HideAll();
            return;
        }

        // If the toggle is on, check the role and show the correct UI
        ShowForCurrentRole();
    }

    // Logic to determine which UI to display based on the stored Role
    public void ShowForCurrentRole()
    {
        Debug.Log("[RoleCanvasSwitcher] Current role: " + roleProvider.role);

        // Check if the current user is a Patient
        bool isPatient = roleProvider.role == Role.Patient;

        // Activate/Deactivate canvases based on the boolean result
        if (patientCanvas)     patientCanvas.SetActive(isPatient);
        if (technicianCanvas)  technicianCanvas.SetActive(!isPatient); // Assuming !isPatient implies Technician logic here
    }

    // Helper method to disable all canvases (e.g., when returning to the main menu)
    public void HideAll()
    {
        if (patientCanvas)     patientCanvas.SetActive(false);
        if (technicianCanvas)  technicianCanvas.SetActive(false);
    }
}