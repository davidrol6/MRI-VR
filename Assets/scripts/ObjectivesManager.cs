using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [Header("Referencia al proveedor de rol")]
    public RoleProvider roleProvider;

    [Header("Lista de objetivos")]
    public List<Objective> objectives = new List<Objective>();

    // EVENTO para avisar de que un objetivo se ha completado
    public event Action<Objective> OnObjectiveCompleted;

    void Awake()
    {
        Instance = this;
    }

    public void CompleteObjective(string description)
    {
        Role currentRole = roleProvider.role;
        Debug.Log($"[RoleProvider] Rol actual: {roleProvider.role}");
        Debug.Log($"Intentando completar objetivo '{description}' para el rol {currentRole}");

        for (int i = 0; i < objectives.Count; i++)
        {
            Objective obj = objectives[i];
            if (obj.description == description && obj.role == currentRole)
            {
                if (!obj.isCompleted)
                {
                    obj.isCompleted = true;
                    Debug.Log($"✅ Objetivo completado: '{obj.description}' (rol: {obj.role})");

                    // Llamamos al evento
                    OnObjectiveCompleted?.Invoke(obj);
                }
                else
                {
                    Debug.Log($"ℹ️ Objetivo '{obj.description}' ya estaba completado.");
                }
                return;
            }
        }

        Debug.LogWarning($"❌ No se encontró el objetivo '{description}' para el rol {currentRole}.");
    }

    public void PrintObjectivesStatus()
    {
        Debug.Log("---- ESTADO DE LOS OBJETIVOS ----");
        foreach (var obj in objectives)
        {
            Debug.Log($"{obj.description} ({obj.role}) -> {(obj.isCompleted ? "✔️ COMPLETADO" : "❌ PENDIENTE")}");
        }
    }
}
