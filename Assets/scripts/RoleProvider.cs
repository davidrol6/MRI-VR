using UnityEngine;

public enum Role { Patient, Technician }

public class RoleProvider : MonoBehaviour
{
    [Header("Current Role (auto-set by UI)")]
    public Role role = Role.Patient;

    private void Awake()
    {
        if (role == Role.Patient)
            SetRolePatient(true);
        else
            SetRoleTechnician(true);
    }
    public void SetRolePatient(bool isOn)
    {
        if (isOn)
            role = Role.Patient;
    }

    public void SetRoleTechnician(bool isOn)
    {
        if (isOn)
            role = Role.Technician;
    }
}
