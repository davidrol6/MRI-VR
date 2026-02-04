/* * SCRIPT SUMMARY:
 * This script is the central manager for the simulation's scoring and progression system.
 * * * How it works:
 * - It implements the Singleton pattern, allowing any object (Button, Trigger, UI) to access it globally.
 * - It maintains a list of all possible 'Objective' objects.
 * - It provides methods to Activate/Deactivate specific objectives based on the generated clinical case.
 * - It handles the logic for completing objectives: checking if they are active, if they match the user's role, and awarding points.
 * - It calculates the final score for the evaluation phase.
 */

using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [Header("Role provider reference")]
    public RoleProvider roleProvider; // Reference to check if the current user (Patient/Technician) is allowed to complete the task

    [Header("Objective list")]
    public List<Objective> objectives = new List<Objective>(); // The database of all tasks

    public List<PatientReport> history = new List<PatientReport>();
    private int patientCount = 1; // For many cases of patients

    // Events to notify the UI when progress changes
    public event Action<Objective> OnObjectiveCompleted;
    public event Action OnObjectivesListUpdated; 

    private void Awake()
    {
        Instance = this;
    }

    // Called by ClinicalCaseGenerator to configure the scenario (e.g., enable "Contrast" tasks only if contrast is required)
    public void SetObjectiveStatus(string exactDescription, bool isActive)
    {
        Objective obj = objectives.Find(o => o.description.Equals(exactDescription, StringComparison.OrdinalIgnoreCase));
        
        if (obj != null)
        {
            obj.isActive = isActive;
        }
        else
        {
            Debug.LogWarning($"[ObjectiveManager] Objective not found: {exactDescription}");
        }
    }

    // Forces the UI to repaint the objective list (useful after batch updating statuses)
    public void RefreshObjectives()
    {
        OnObjectivesListUpdated?.Invoke();
    }

    // The main entry point for gameplay interactions
    public void CompleteObjective(string description)
    {
        Role currentRole = roleProvider.role;

        // Find the objective that matches both the description AND the current player role
        Objective obj = objectives.Find(o => o.description == description && o.role == currentRole);

        if (obj != null)
        {
            // If the objective exists but is not part of this specific clinical case, ignore it
            if (!obj.isActive) 
            {
                Debug.Log($"The objective '{description}' exists but is not active for this clinical case.");
                return; 
            }

            // Only complete if not already done
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

    // Calculates the total possible points for the CURRENT active scenario
    public int GetMaxScore()
    {
        int total = 0;
        foreach (var obj in objectives)
        {
            if (obj.isActive) total += obj.points;
        }
        return total;
    }

    // Calculates the points actually earned by the user
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

    public void SaveCurrentPatientResults()
    {
        PatientReport report = new PatientReport();
        report.patientName = $"Paciente {patientCount}"; 
        report.scoreObtained = GetFinalScore();
        report.maxScorePossible = GetMaxScore();
        report.missedObjectives = new List<string>();

        // Identificar qué falló (Activo PERO NO Completado)
        foreach (var obj in objectives)
        {
            if (obj.isActive && !obj.isCompleted)
            {
                report.missedObjectives.Add(obj.description);
            }
        }

        history.Add(report);
        patientCount++;
        
        Debug.Log($"[ObjectiveManager] Reporte guardado para {report.patientName}. Score: {report.GetPercentage()}%");
    }

    // Debugging tool to see the state of the simulation in the Console
    public void PrintObjectivesStatus()
    {
        Debug.Log("---- CLINICAL CASE STATUS ----");
        foreach (var obj in objectives)
        {
            if (obj.isActive)
            {
                string estado = obj.isCompleted ? "<color=green>COMPLETED</color>" : "<color=red>PENDING</color>";
                Debug.Log($"[ACTIVE] {obj.description} -> {estado}");
            }
            else
            {
                Debug.Log($"<color=grey>[INACTIVE] {obj.description} (Not required)</color>");
            }
        }
        Debug.Log("----------------------------------");
    }

    public void PrepareNextPatient()
    {
        // 1. Resetear todos los objetivos a false (incluso los inactivos, por seguridad)
        foreach (var obj in objectives)
        {
            obj.isCompleted = false;
        }

        // 2. Notificar al generador de casos (Si lo llamas desde fuera) o simplemente refrescar UI
        RefreshObjectives();
    }
}