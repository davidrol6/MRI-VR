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

    // Event fired when an objective is completed
    public event Action<Objective> OnObjectiveCompleted;

    private void Awake()
    {
        Instance = this;
    }

    public void CompleteObjective(string description)
    {
        Role currentRole = roleProvider.role;
        Debug.Log($"[RoleProvider] Current role: {roleProvider.role}");
        Debug.Log($"Trying to complete objective '{description}' for role {currentRole}");

        for (int i = 0; i < objectives.Count; i++)
        {
            Objective obj = objectives[i];
            if (obj.description == description && obj.role == currentRole)
            {
                if (!obj.isCompleted)
                {
                    obj.isCompleted = true;
                    Debug.Log(
                        $"Objective completed: '{obj.description}' " +
                        $"(role: {obj.role}, points: {obj.points})"
                    );

                    // Fire event
                    OnObjectiveCompleted?.Invoke(obj);
                }
                else
                {
                    Debug.Log($"Objective '{obj.description}' was already completed.");
                }
                return;
            }
        }

        Debug.LogWarning($"Objective '{description}' was not found for role {currentRole}.");
    }

    public void PrintObjectivesStatus()
    {
        Debug.Log("---- OBJECTIVES STATUS ----");
        foreach (var obj in objectives)
        {
            Debug.Log(
                $"{obj.description} ({obj.role}) -> " +
                $"{(obj.isCompleted ? "COMPLETED" : "PENDING")} | " +
                $"Points: {obj.points}"
            );
        }
    }

    // Score obtained (only completed objectives)
    public int GetFinalScore()
    {
        int score = 0;
        foreach (var obj in objectives)
        {
            if (obj.isCompleted)
                score += obj.points;
        }
        return score;
    }

    // Maximum possible score (all objectives)
    public int GetMaxScore()
    {
        int total = 0;
        foreach (var obj in objectives)
        {
            total += obj.points;
        }
        return total;
    }
}
