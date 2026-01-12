/* * SCRIPT SUMMARY:
 * This script manages the visualization of MRI scan results on the technician's monitor.
 * * * How it works:
 * - It maintains a list of "RegionImages" that maps each body part (Head, Chest, etc.) to two specific image GameObjects: one sharp (success) and one blurred (failure).
 * - When `ShowScanResult()` is called, it checks the global ClinicalCaseGenerator to see which body part was scanned and if the result was successful.
 * - It activates the correct image object and hides all others.
 */

using UnityEngine;
using System.Collections.Generic;

public class ScanResultDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct RegionImages
    {
        public string name;                           // Descriptive name (e.g., "Brain Scan")
        public ClinicalCaseGenerator.BodyPart bodyPart; // The logic tag (Enum) matching the case generator
        public GameObject sharpImage;                 // The "Good" result image
        public GameObject blurredImage;               // The "Bad" result image (requires repeat)
    }

    [Header("Image config")]
    public List<RegionImages> allImages = new List<RegionImages>(); // Database of all possible scan results

    private void Start()
    {
        // Ensure the screen is blank when the simulation starts
        HideAllImages();
    }

    // Called typically by the "Start Scan" button after the progress bar finishes
    public void ShowScanResult()
    {
        var generator = ClinicalCaseGenerator.Instance;
        
        if (generator == null)
        {
            Debug.LogError("Cant find ClinicalCaseGenerator");
            return;
        }

        // Iterate through all configured images to find the one matching the current clinical case
        foreach (var item in allImages)
        {
            // Reset state: ensure everything is hidden first to prevent overlaps
            if (item.sharpImage != null) item.sharpImage.SetActive(false);
            if (item.blurredImage != null) item.blurredImage.SetActive(false);

            // Match found: This is the body part currently being simulated
            if (item.bodyPart == generator.currentBodyPart)
            {
                // Logic: Check if the simulation determined a failure (blurred image)
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

    // Helper to clear the monitor
    public void HideAllImages()
    {
        foreach (var item in allImages)
        {
            if (item.sharpImage != null) item.sharpImage.SetActive(false);
            if (item.blurredImage != null) item.blurredImage.SetActive(false);
        }
    }
}