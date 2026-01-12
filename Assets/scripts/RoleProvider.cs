/* * SCRIPT SUMMARY:
 * This script acts as a global manager that stores the user's current role 
 * (Patient, Technician, or Observer).
 * * * How it works:
 * This handles the "Choose Role" decision logic at the start of the simulation.
 * It ensures that the decision made in the Main Menu is remembered throughout the 
 * entire simulation life-cycle, even when changing scenes.
 */

using UnityEngine;
using UnityEngine.Events;

// Defines the possible roles available in the simulation
public enum Role { Patient, Technician, Observer }

public class RoleProvider : MonoBehaviour
{
    // Singleton Instance: Allows other scripts to access this easily via RoleProvider.Instance
    public static RoleProvider Instance { get; private set; }

    [Header("Current Role")]
    // The actual variable storing the selected role. Default is Patient.
    public Role role = Role.Patient;
    
    // Flag to track if a selection has actually been made
    private bool hasRole = false;

    // Custom Unity Event that other scripts can subscribe to (Observer pattern)
    // This allows the system to react immediately when the role changes.
    [System.Serializable]
    public class RoleChangedEvent : UnityEvent<Role> {}
    public RoleChangedEvent OnRoleChanged;

    private void Awake()
    {
        // Singleton pattern enforcement:
        // If an instance already exists, destroy this duplicate to prevent conflicts.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // CRITICAL: This keeps the object alive when changing scenes.

        hasRole = false; // Initialize as false so we know the user hasn't chosen yet
        Debug.Log("[RoleProvider] Initialized with no role selected.");
    }

    // --- Public methods usually connected to UI Buttons in the Main Menu ---

    public void SetRolePatient(bool isOn)
    {
        if (isOn) SetRole(Role.Patient, "UI Toggle Patient");
    }

    public void SetRoleTechnician(bool isOn)
    {
        if (isOn) SetRole(Role.Technician, "UI Toggle Technician");
    }

    public void SetRoleObserver(bool isOn)
    {
        if (isOn) SetRole(Role.Observer, "UI Toggle Observer");
    }

    // Core logic to update the role
    public void SetRole(Role newRole, string source = "Code")
    {
        // Optimization: If the role is arguably the same, do nothing to save performance.
        if (hasRole && role == newRole)
            return;

        role = newRole;
        hasRole = true;

        // Log for debugging purposes (useful to trace logic flow in the console)
        Debug.Log($"[RoleProvider] Role changed -> {role} (source: {source})\n{System.Environment.StackTrace}", this);

        // Notify any listeners (like the RoleCanvasSwitcher) that the role has changed
        OnRoleChanged?.Invoke(role);
    }
}