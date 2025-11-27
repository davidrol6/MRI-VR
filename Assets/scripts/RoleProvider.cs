using UnityEngine;
using UnityEngine.Events;

public enum Role { Patient, Technician, Observer }

public class RoleProvider : MonoBehaviour
{
    public static RoleProvider Instance { get; private set; }

    [Header("Current Role")]
    // Default inspector value. Ignored until hasRole becomes true.
    public Role role = Role.Patient;
    private bool hasRole = false;

    [System.Serializable]
    public class RoleChangedEvent : UnityEvent<Role> {}
    public RoleChangedEvent OnRoleChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes

        hasRole = false; // No role selected yet
        Debug.Log("[RoleProvider] Initialized with no role selected.");
    }

    // Called from UI toggles (OnValueChanged)
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

    public void SetRole(Role newRole, string source = "Code")
    {
        // If a role was already set and the new role matches, nothing to update
        if (hasRole && role == newRole)
            return;

        role = newRole;
        hasRole = true;

        Debug.Log($"[RoleProvider] Role changed -> {role} (source: {source})\n{System.Environment.StackTrace}", this);

        OnRoleChanged?.Invoke(role);
    }
}
