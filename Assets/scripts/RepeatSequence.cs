using UnityEngine;
using UnityEngine.UI;

public class RepeatSequenceToggle : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto que tiene el script ScanResultDisplay")]
    public ScanResultDisplay displayController;
    
    private Toggle myToggle;

    private void Start()
    {
        // Obtenemos el componente Toggle de este mismo objeto
        myToggle = GetComponent<Toggle>();
        
        // Nos aseguramos de que esté apagado al inicio
        if (myToggle != null)
            myToggle.SetIsOnWithoutNotify(false);
    }

    // Esta función se llama desde el evento OnValueChanged del Toggle
    // Asegúrate de elegir la opción "Dynamic bool" en el Inspector
    public void OnRepeatToggleChanged(bool isOn)
    {
        // Solo actuamos si el toggle se ENCIENDE. Si se apaga, no hacemos nada.
        if (!isOn) return;

        var generator = ClinicalCaseGenerator.Instance;

        // Comprobaciones de seguridad
        if (generator == null || displayController == null)
        {
            Debug.LogWarning("Faltan referencias en RepeatSequenceToggle");
            return;
        }

        // --- LÓGICA PRINCIPAL ---
        
        // 1. ¿Está la imagen actualmente borrosa?
        if (generator.isImageBlurred)
        {
            Debug.Log("Repitiendo secuencia para corregir imagen borrosa...");

            // 2. Buscamos la configuración de imágenes correcta en el otro script
            foreach (var config in displayController.allImages)
            {
                if (config.bodyPart == generator.currentBodyPart)
                {
                    // 3. ¡EL CAMBAZO! Apagamos la mala, encendemos la buena.
                    if (config.blurredImage != null) config.blurredImage.SetActive(false);
                    if (config.sharpImage != null) config.sharpImage.SetActive(true);

                    // 4. ACTUALIZAR EL ESTADO DEL CASO
                    // Esto es vital: marcamos que la imagen YA NO está borrosa.
                    generator.isImageBlurred = false;

                    // Opcional: Desactivar el toggle para que no se pueda volver a pulsar
                    if (myToggle != null) myToggle.interactable = false;

                    // Opcional: Si tienes un objetivo por repetir, complétalo aquí
                    // ObjectiveManager.Instance.CompleteObjective("Repetir Secuencia");

                    return; // Ya hemos terminado, salimos del bucle.
                }
            }
        }
        else
        {
            Debug.Log("El usuario intentó repetir, pero la imagen ya era nítida.");
            // Opcional: Si la imagen ya era buena, volvemos a apagar el toggle automáticamente
            if (myToggle != null)
            {
                 // Usamos SetIsOnWithoutNotify para que no se vuelva a llamar a esta función en bucle
                myToggle.SetIsOnWithoutNotify(false);
            }
        }
    }
}