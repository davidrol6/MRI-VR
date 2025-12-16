using UnityEngine;
using System.Collections.Generic;

public class ScanResultDisplay : MonoBehaviour
{
    // Esta estructura nos permite hacer parejitas en el Inspector
    [System.Serializable]
    public struct RegionImages
    {
        public string name; // Solo para que te organices (ej: "Torax")
        public ClinicalCaseGenerator.BodyPart bodyPart; // El enum del caso
        public GameObject sharpImage; // Arrastra aquí 'chest_good'
        public GameObject blurredImage;  // Arrastra aquí 'chest_bad'
    }

    [Header("Configuración de Imágenes")]
    // Lista donde arrastrarás todas tus referencias
    public List<RegionImages> allImages = new List<RegionImages>();

    // Llama a esta función desde el botón "Stop Scanner"
    private void Start()
    {
        HideAllImages();
    }
    public void ShowScanResult()
    {
        var generator = ClinicalCaseGenerator.Instance;
        
        if (generator == null)
        {
            Debug.LogError("No se encuentra el ClinicalCaseGenerator");
            return;
        }

        // Recorremos todas las configuraciones posibles
        foreach (var item in allImages)
        {
            // 1. Apagamos TODO primero para limpiar la pantalla
            if (item.sharpImage != null) item.sharpImage.SetActive(false);
            if (item.blurredImage != null) item.blurredImage.SetActive(false);

            // 2. Si esta configuración coincide con la parte del cuerpo del caso actual...
            if (item.bodyPart == generator.currentBodyPart)
            {
                // 3. Decidimos si mostramos la borrosa o la nítida
                if (generator.isImageBlurred)
                {
                    if (item.blurredImage != null) item.blurredImage.SetActive(true);
                    Debug.Log($"Mostrando imagen BORROSA de {item.name}");
                }
                else
                {
                    if (item.sharpImage != null) item.sharpImage.SetActive(true);
                    Debug.Log($"Mostrando imagen NÍTIDA de {item.name}");
                }
            }
        }
    }

    

    public void HideAllImages()
    {
        foreach (var item in allImages)
        {
            if (item.sharpImage != null) item.sharpImage.SetActive(false);
            if (item.blurredImage != null) item.blurredImage.SetActive(false);
        }
    }
}