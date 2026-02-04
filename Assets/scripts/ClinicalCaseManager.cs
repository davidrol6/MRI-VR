/* * SCRIPT SUMMARY:
 * This script serves as the central logic controller for generating dynamic clinical scenarios.
 * * * How it works:
 * - It creates a "randomized" patient case by selecting a Body Part, a Contrast requirement, and an Image Quality state.
 * - It acts as a bridge between the random generation and the game's rules.
 * - Based on the generated data, it automatically configures the ObjectiveManager to activate only the relevant goals (e.g., if the body part is "Legs", it activates the "Feet First" positioning objective).
 */

using UnityEngine;
using System; 

public class ClinicalCaseGenerator : MonoBehaviour
{
    public static ClinicalCaseGenerator Instance;

    // Defines the possible anatomical regions for the exam
    public enum BodyPart { HeadNeck, Chest, Abdomen, Legs }
    
    [Header("State of the case (lecture only)")]
    public BodyPart currentBodyPart;
    public bool requiresContrast;
    public bool isImageBlurred; // Simulates a failure state requiring a repeat scan

    [Header("UI References")]
    public FinalEvaluationUI finalEvaluationUI;

    // Event triggered when a new case is created, allowing other scripts to react
    public event Action<BodyPart, bool> OnCaseGenerated;

    private void Awake()
    {
        // Singleton pattern to ensure only one generator exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Main entry point: Call this to start a new simulation scenario
    public void GenerateRandomCase()
    {
        // 1. Randomize parameters
        currentBodyPart = (BodyPart)UnityEngine.Random.Range(0, 4); // 0 to 3
        requiresContrast = (UnityEngine.Random.value > 0.5f);       // 50% chance
        isImageBlurred = (UnityEngine.Random.value > 0.5f);         // 50% chance

        Debug.Log($"CASO GENERADO: {currentBodyPart} | Contraste: {requiresContrast} | Borrosa: {isImageBlurred}");

        // 2. Update the game objectives based on these random values
        ConfigureObjectives();

        // 3. Notify listeners (like the UI or scoring system)
        OnCaseGenerated?.Invoke(currentBodyPart, requiresContrast);
    }

    private void ConfigureObjectives()
    {
        ObjectiveManager om = ObjectiveManager.Instance;

        if (om == null) return;

        // Reset mutually exclusive objectives to ensure a clean state
        om.SetObjectiveStatus("Choose Patient Position in Software [H]", false);
        om.SetObjectiveStatus("Choose Patient Position in Software [F]", false);

        // LOGIC: Patient Positioning
        // Head/Neck and Chest require "Head First" ([H])
        // Abdomen and Legs require "Feet First" ([F])
        if (currentBodyPart == BodyPart.HeadNeck || currentBodyPart == BodyPart.Chest)
        {
            om.SetObjectiveStatus("Choose Patient Position in Software [H]", true);
        }
        else
        {
            om.SetObjectiveStatus("Choose Patient Position in Software [F]", true);
        }

        // LOGIC: Contrast Protocol
        om.SetObjectiveStatus("Check for the use of contrast [Y]", false);
        om.SetObjectiveStatus("Check for the use of contrast [N]", false);

        if (requiresContrast)
        {
            // If contrast is needed, activate the [Y] check and the preparation steps
            om.SetObjectiveStatus("Check for the use of contrast [Y]", true);
        }
        else
        {
            // If not, activate the [N] check
            om.SetObjectiveStatus("Check for the use of contrast [N]", true);
        }

        // Set sub-objectives for the physical preparation of the contrast
        om.SetObjectiveStatus("Prepare Contrast Syringe", requiresContrast);
        om.SetObjectiveStatus("Prepare Contrast", requiresContrast);
        om.SetObjectiveStatus("Insert Contrast", requiresContrast);

        // LOGIC: Image Quality / Repeat Scan
        if (isImageBlurred)
        {
            // If the image is blurry, the exam is NOT finished; the student must repeat
            om.SetObjectiveStatus("Repeat sequence", true);
            om.SetObjectiveStatus("Finish Exam", false);
        }
        else
        {
            // If the image is clear, the student can finish
            om.SetObjectiveStatus("Repeat sequence", false);
            om.SetObjectiveStatus("Finish Exam", true);
        }
        
        // Apply changes to the UI
        om.RefreshObjectives();
    }
    public void OnToggleCaseReset(bool isOn)
{
    // Normalmente queremos que ocurra al activar el toggle (o al cambiar de estado)
    if (isOn)
    {
        ResetWholeSimulation();
    }
}

    public void ResetWholeSimulation()
    {
        Debug.Log("--- INICIANDO RESET DE SIMULACIÓN ---");

        // 1. Resetear el ObjectiveManager (Lógica de puntos y completado)
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.PrepareNextPatient();
        }

        // 2. Limpiar la interfaz de evaluación
        if (finalEvaluationUI != null)
        {
            finalEvaluationUI.ResetUI();
        }
        else
        {
            // Failsafe: Si no hay referencia, intentamos buscarlo
            UnityEngine.Object.FindFirstObjectByType<FinalEvaluationUI>()?.ResetUI();        }

        // 3. Generar un caso clínico totalmente nuevo
        GenerateRandomCase();
    }
}