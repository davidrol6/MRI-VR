using UnityEngine;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public RoleProvider roleProvider; // Asigna tu RoleProvider en el Inspector
    public List<Objective> objectives; // Lista rellena desde el inspector

    void Awake()
    {
        Instance = this;
    }

    public void CompleteObjective(string description)
    {
        // Busca el objetivo que coincida con la descripción y el rol actual
        Objective obj = objectives.Find(o => o.description == description && o.role == roleProvider.role);
        if (obj != null && !obj.isCompleted)
        {
            obj.isCompleted = true;

            // Si tienes un UI que refrescar, lo llamas aquí:
            // uiObjectiveList.UpdateUI(); 
        }
    }
}
