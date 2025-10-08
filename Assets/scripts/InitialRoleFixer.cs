using UnityEngine;
using System.Collections;

public class InitialRoleFixer : MonoBehaviour
{
    IEnumerator Start()
    {
        // Espera un frame para que los toggles de QDSUI terminen de inicializar
        yield return null;

        // Comprueba si el rol se ha cambiado automáticamente
        if (RoleProvider.Instance.role != Role.Patient)
        {
            RoleProvider.Instance.SetRole(Role.Patient, "Force Fix");
            Debug.Log("[InitialRoleFixer] Rol forzado a Patient tras inicialización.");
        }
        else
        {
            Debug.Log("[InitialRoleFixer] Rol inicial correcto (Patient).");
        }
    }
}
