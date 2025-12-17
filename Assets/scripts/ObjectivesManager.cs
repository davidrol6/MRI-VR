using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [Header("Role provider reference")]
    public RoleProvider roleProvider;

    [Header("Objective list")]
    public List<Objective> objectives = new List<Objective>();

    public event Action<Objective> OnObjectiveCompleted;
    public event Action OnObjectivesListUpdated; 

    private void Awake()
    {
        Instance = this;
    }

    public void SetObjectiveStatus(string exactDescription, bool isActive)
    {
        Objective obj = objectives.Find(o => o.description.Equals(exactDescription, StringComparison.OrdinalIgnoreCase));
        
        if (obj != null)
        {
            obj.isActive = isActive;
            obj.isCompleted = false; 
        }
        else
        {
            Debug.LogWarning($"[ObjectiveManager] No se encontró el objetivo: {exactDescription}");
        }
    }

    public void RefreshObjectives()
    {
        OnObjectivesListUpdated?.Invoke();
    }

    public void CompleteObjective(string description)
    {
        Role currentRole = roleProvider.role;

        Objective obj = objectives.Find(o => o.description == description && o.role == currentRole);

        if (obj != null)
        {
            if (!obj.isActive) 
            {
                Debug.Log($"El objetivo '{description}' existe pero no está activo en este caso clínico.");
                return; 
            }

            if (!obj.isCompleted)
            {
                obj.isCompleted = true;
                Debug.Log($"Objective completed: '{obj.description}'");
                OnObjectiveCompleted?.Invoke(obj);
            }
        }
        else
        {
            Debug.LogWarning($"Objective '{description}' not found for role {currentRole}.");
        }
    }

    public int GetMaxScore()
    {
        int total = 0;
        foreach (var obj in objectives)
        {
            if (obj.isActive) total += obj.points;
        }
        return total;
    }

    public int GetFinalScore()
    {
        int score = 0;
        foreach (var obj in objectives)
        {
            if (obj.isCompleted && obj.isActive) 
                score += obj.points;
        }
        return score;
    }

    public void PrintObjectivesStatus()
    {
        Debug.Log("---- ESTADO DEL CASO CLÍNICO ----");
        foreach (var obj in objectives)
        {
            if (obj.isActive)
            {
                string estado = obj.isCompleted ? "<color=green>COMPLETADO</color>" : "<color=red>PENDIENTE</color>";
                Debug.Log($"[ACTIVO] {obj.description} -> {estado}");
            }
            else
            {
                Debug.Log($"<color=grey>[INACTIVO] {obj.description} (No requerido en este caso)</color>");
            }
        }
        Debug.Log("----------------------------------");
    }
}