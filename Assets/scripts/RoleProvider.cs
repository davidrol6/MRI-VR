using UnityEngine;
using UnityEngine.Events;

public enum Role { Patient, Technician, Observer }

public class RoleProvider : MonoBehaviour
{
    public static RoleProvider Instance { get; private set; }

    [Header("Current Role")]
    // Valor por defecto en el Inspector, pero se ignora hasta que hasRole sea true.
    public Role role = Role.Patient;
    private bool hasRole = false;

    [System.Serializable] 
    public class RoleChangedEvent : UnityEvent<Role> {}
    public RoleChangedEvent OnRoleChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre escenas

        // Al iniciar, marcamos que todavía NO hay rol seleccionado por el usuario.
        hasRole = false;
        Debug.Log("[RoleProvider] Init with NO role selected yet.");
    }

    // Estos métodos los usas desde los toggles (OnValueChanged)
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
        // Si ya había rol Y el nuevo rol es el mismo, no hacemos nada.
        if (hasRole && role == newRole) return;

        // Asignamos rol y marcamos que ya hay uno válido.
        role = newRole;
        hasRole = true;

        Debug.Log($"[RoleProvider] Role change -> {role} (source: {source})\n{System.Environment.StackTrace}", this);
        OnRoleChanged?.Invoke(role);
    }
}
