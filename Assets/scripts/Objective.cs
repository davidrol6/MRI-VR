using UnityEngine;

[System.Serializable]
public class Objective
{
    [Header("Datos del objetivo")]
    public string description;
    public Role role;
    public bool isCompleted;

    [Header("Puntos del objetivo")]
    public int points = 10;   // valor por defecto
}
