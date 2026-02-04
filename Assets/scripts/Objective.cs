/* * SCRIPT SUMMARY:
 * This script defines the data structure for a single objective within the simulation.
 * * * How it works:
 * - It is a plain C# class (not a MonoBehaviour) marked as [Serializable], so it can be viewed and edited in the Unity Inspector lists.
 * - It holds the description, associated role, completion status, point value, and active state of a task.
 * - This data is used by the ObjectiveManager to track overall progress and calculate scores.
 */

using UnityEngine;

[System.Serializable]
public class Objective
{
    [Header("Objective data")]
    public string description;  // Text shown to the user (e.g., "Wash Hands")
    public Role role;           // Which role this objective belongs to (Patient vs Technician)
    public bool isCompleted;    // Current status
    
    [Header("Objective points")]
    public int points = 10;     // Score awarded upon completion (Weighted scoring)
    public bool isActive;       // If false, this objective is ignored (useful for dynamic scenarios)
}