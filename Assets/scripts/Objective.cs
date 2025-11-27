using UnityEngine;

[System.Serializable]
public class Objective
{
    [Header("Objective data")]
    public string description;
    public Role role;
    public bool isCompleted;

    [Header("Objective points")]
    public int points = 10; // Default value
}
