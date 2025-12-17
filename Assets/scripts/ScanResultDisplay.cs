using UnityEngine;
using System.Collections.Generic;

public class ScanResultDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct RegionImages
    {
        public string name;
        public ClinicalCaseGenerator.BodyPart bodyPart; 
        public GameObject sharpImage; 
        public GameObject blurredImage;  
    }

    [Header("Image config")]
    public List<RegionImages> allImages = new List<RegionImages>();

    private void Start()
    {
        HideAllImages();
    }
    public void ShowScanResult()
    {
        var generator = ClinicalCaseGenerator.Instance;
        
        if (generator == null)
        {
            Debug.LogError("Cant find ClinicalCaseGenerator");
            return;
        }

        foreach (var item in allImages)
        {
            if (item.sharpImage != null) item.sharpImage.SetActive(false);
            if (item.blurredImage != null) item.blurredImage.SetActive(false);

            if (item.bodyPart == generator.currentBodyPart)
            {
                if (generator.isImageBlurred)
                {
                    if (item.blurredImage != null) item.blurredImage.SetActive(true);
                    Debug.Log($"Showing BLURRY img of {item.name}");
                }
                else
                {
                    if (item.sharpImage != null) item.sharpImage.SetActive(true);
                    Debug.Log($"Showing NITID img of {item.name}");
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