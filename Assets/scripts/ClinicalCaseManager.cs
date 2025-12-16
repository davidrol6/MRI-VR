using UnityEngine;
using System; // Necesario para 'Action'

public class ClinicalCaseGenerator : MonoBehaviour
{
    // Singleton para poder acceder desde otros scripts (como el Canvas del paciente)
    public static ClinicalCaseGenerator Instance;

    // Variables del caso actual
    public enum BodyPart { HeadNeck, Chest, Abdomen, Legs }
    
    [Header("Estado del Caso (Solo Lectura)")]
    public BodyPart currentBodyPart;
    public bool requiresContrast;
    public bool isImageBlurred;

    // --- EVENTO ---
    // Avisa a quien esté escuchando (PatientInfoUI) que se ha creado un caso nuevo.
    // Envía dos datos: (Parte del cuerpo, Si lleva contraste)
    public event Action<BodyPart, bool> OnCaseGenerated;

    private void Awake()
    {
        // Configuración básica del Singleton
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
        // 1. Aleatorizar variables
        currentBodyPart = (BodyPart)UnityEngine.Random.Range(0, 4);
        requiresContrast = (UnityEngine.Random.value > 0.5f); // 50% probabilidad
        // 50% de probabilidad de salir borrosa
        isImageBlurred = (UnityEngine.Random.value > 0.5f); 

        Debug.Log($"CASO GENERADO: {currentBodyPart} | Contraste: {requiresContrast} | Borrosa: {isImageBlurred}");

        // 2. Configurar los objetivos (Lógica interna del juego)
        ConfigureObjectives();

        // 3. Avisar al Canvas de Información del Paciente para que actualice el texto
        OnCaseGenerated?.Invoke(currentBodyPart, requiresContrast);
    }

    private void ConfigureObjectives()
    {
        ObjectiveManager om = ObjectiveManager.Instance;

        if (om == null) return;

        // --- 1. POSICIONAMIENTO ---
        // Desactivamos ambos primero para limpiar estado anterior
        om.SetObjectiveStatus("Choose Patient Position in Software [H]", false);
        om.SetObjectiveStatus("Choose Patient Position in Software [F]", false);

        if (currentBodyPart == BodyPart.HeadNeck || currentBodyPart == BodyPart.Chest)
        {
            // Cabeza o Pecho -> Entra de cabeza (Head First)
            om.SetObjectiveStatus("Choose Patient Position in Software [H]", true);
        }
        else
        {
            // Abdomen o Piernas -> Entra de pies (Feet First)
            om.SetObjectiveStatus("Choose Patient Position in Software [F]", true);
        }

        // --- 2. CONTRASTE ---
        // Primero desactivamos las versiones [Y] y [N] para limpiar estado anterior
        om.SetObjectiveStatus("Check for the use of contrast [Y]", false);
        om.SetObjectiveStatus("Check for the use of contrast [N]", false);

        if (requiresContrast)
        {
            // Si REQUIERE contraste: Activamos la opción 'Yes'
            om.SetObjectiveStatus("Check for the use of contrast [Y]", true);
        }
        else
        {
            // Si NO requiere contraste: Activamos la opción 'No' 
            // (El usuario tendrá que marcar "No Contraste" en el software para ganar los puntos)
            om.SetObjectiveStatus("Check for the use of contrast [N]", true);
        }

        // Objetivos físicos/procedimentales de contraste (Jeringuilla, etc.)
        // Estos solo se activan si hay contraste. Si no hay, se quedan en false.
        om.SetObjectiveStatus("Prepare Contrast Syringe", requiresContrast);
        om.SetObjectiveStatus("Prepare Contrast", requiresContrast);
        om.SetObjectiveStatus("Insert Contrast", requiresContrast);

        // --- 3. CALIDAD DE IMAGEN ---
        // Si sale borrosa, obligamos a repetir. Si sale bien, permitimos finalizar.
        if (isImageBlurred)
        {
            om.SetObjectiveStatus("Repeat sequence", true);
            om.SetObjectiveStatus("Finish Exam", false); // No dejar finalizar si está mal
        }
        else
        {
            om.SetObjectiveStatus("Repeat sequence", false);
            om.SetObjectiveStatus("Finish Exam", true);
        }
        
        // 5. IMPORTANTE: Avisar al ObjectiveManager para que redibuje la lista visual
        om.RefreshObjectives();
    }
}