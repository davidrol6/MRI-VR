using UnityEngine;
using System; 

public class ClinicalCaseGenerator : MonoBehaviour
{
    public static ClinicalCaseGenerator Instance;

    public enum BodyPart { HeadNeck, Chest, Abdomen, Legs }
    
    [Header("State of the case (lecture only)")]
    public BodyPart currentBodyPart;
    public bool requiresContrast;
    public bool isImageBlurred;

    public event Action<BodyPart, bool> OnCaseGenerated;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GenerateRandomCase()
    {
        currentBodyPart = (BodyPart)UnityEngine.Random.Range(0, 4);
        requiresContrast = (UnityEngine.Random.value > 0.5f); 
        isImageBlurred = (UnityEngine.Random.value > 0.5f); 

        Debug.Log($"CASO GENERADO: {currentBodyPart} | Contraste: {requiresContrast} | Borrosa: {isImageBlurred}");

        ConfigureObjectives();

        OnCaseGenerated?.Invoke(currentBodyPart, requiresContrast);
    }

    private void ConfigureObjectives()
    {
        ObjectiveManager om = ObjectiveManager.Instance;

        if (om == null) return;

        om.SetObjectiveStatus("Choose Patient Position in Software [H]", false);
        om.SetObjectiveStatus("Choose Patient Position in Software [F]", false);

        if (currentBodyPart == BodyPart.HeadNeck || currentBodyPart == BodyPart.Chest)
        {
            om.SetObjectiveStatus("Choose Patient Position in Software [H]", true);
        }
        else
        {
            om.SetObjectiveStatus("Choose Patient Position in Software [F]", true);
        }

        om.SetObjectiveStatus("Check for the use of contrast [Y]", false);
        om.SetObjectiveStatus("Check for the use of contrast [N]", false);

        if (requiresContrast)
        {
            om.SetObjectiveStatus("Check for the use of contrast [Y]", true);
        }
        else
        {
            om.SetObjectiveStatus("Check for the use of contrast [N]", true);
        }

        om.SetObjectiveStatus("Prepare Contrast Syringe", requiresContrast);
        om.SetObjectiveStatus("Prepare Contrast", requiresContrast);
        om.SetObjectiveStatus("Insert Contrast", requiresContrast);

        if (isImageBlurred)
        {
            om.SetObjectiveStatus("Repeat sequence", true);
            om.SetObjectiveStatus("Finish Exam", false);
        }
        else
        {
            om.SetObjectiveStatus("Repeat sequence", false);
            om.SetObjectiveStatus("Finish Exam", true);
        }
        
        om.RefreshObjectives();
    }
}