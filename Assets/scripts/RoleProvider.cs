using UnityEngine;
using UnityEngine.Events;

public enum Role { Patient, Technician }

public class RoleProvider : MonoBehaviour
{
    public static RoleProvider Instance { get; private set; }

    [Header("Current Role")]
    public Role role = Role.Patient;

    [System.Serializable] public class RoleChangedEvent : UnityEvent<Role> {}
    public RoleChangedEvent OnRoleChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);                   // persiste entre escenas
        Debug.Log($"[RoleProvider] Init with role = {role}");
    }

    public void SetRolePatient(bool isOn)
    {
        if (isOn) SetRole(Role.Patient, "UI Toggle Patient");
    }

    public void SetRoleTechnician(bool isOn)
    {
        if (isOn) SetRole(Role.Technician, "UI Toggle Technician");
    }

    public void SetRole(Role newRole, string source = "Code")
    {
        if (role == newRole) return;
        role = newRole;
        Debug.Log($"[RoleProvider] Role change -> {role} (source: {source})\n{System.Environment.StackTrace}", this);
        OnRoleChanged?.Invoke(role);
    }
}
