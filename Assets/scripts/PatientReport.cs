using UnityEngine;
using System.Collections.Generic; // <--- ¡ESTA ES LA LÍNEA QUE TE FALTA!

[System.Serializable]
public class PatientReport
{
    public string patientName;
    public int scoreObtained;
    public int maxScorePossible;
    public List<string> missedObjectives; // Ahora List sí será reconocido

    public float GetPercentage()
    {
        if (maxScorePossible == 0) return 0f;
        return (float)scoreObtained / maxScorePossible * 100f;
    }
}