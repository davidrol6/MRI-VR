/* * SCRIPT SUMMARY:
 * This script controls the Patient Information display (e.g., on a monitor or clipboard).
 * * * How it works:
 * - It displays static patient demographics (Name, Age, DOB, Weight).
 * - It subscribes to the 'ClinicalCaseGenerator' to listen for new case data.
 * - When a case is generated, it dynamically updates the "Requested Scan" text field to reflect the specific body part and contrast requirements.
 * - It uses color coding (e.g., turning text orange) to visually highlight if contrast is required.
 */

using UnityEngine;
using TMPro; 

public class PatientInfoUI : MonoBehaviour
{
    [Header("References of UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dobText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI scanRequestedText; // The dynamic field for the specific procedure

    [Header("Fixed info (optional)")]
    // In a full simulation, these might come from a database. Here they are static placeholders.
    public string fixedName = "John Doe";
    public string fixedDOB = "12/05/1980";
    public string fixedAge = "45";
    public string fixedWeight = "78 kg";

    private void Start()
    {
        // 1. Initialize static data immediately
        SetStaticInfo();

        // 2. Subscribe to the event to know when the clinical case is ready
        if (ClinicalCaseGenerator.Instance != null)
        {
            ClinicalCaseGenerator.Instance.OnCaseGenerated += UpdateScanInfo;
        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events when the object is destroyed to prevent memory leaks
        if (ClinicalCaseGenerator.Instance != null)
        {
            ClinicalCaseGenerator.Instance.OnCaseGenerated -= UpdateScanInfo;
        }
    }

    // Callback function: Triggered when the ClinicalCaseGenerator creates a new scenario
    private void UpdateScanInfo(ClinicalCaseGenerator.BodyPart bodyPart, bool hasContrast)
    {
        string zonaEsp = "";
        
        // Convert Enum to readable string
        switch (bodyPart)
        {
            case ClinicalCaseGenerator.BodyPart.HeadNeck:
                zonaEsp = "Head and neck";
                break;
            case ClinicalCaseGenerator.BodyPart.Chest:
                zonaEsp = "Chest";
                break;
            case ClinicalCaseGenerator.BodyPart.Abdomen:
                zonaEsp = "Abdomen";
                break;
            case ClinicalCaseGenerator.BodyPart.Legs:
                zonaEsp = "Lower limbs";
                break;
        }

        string contrasteEsp = hasContrast ? "With contrast" : "Without contrast";

        // Construct the full medical order string
        string finalDescription = $"Requested {zonaEsp} MRI {contrasteEsp}";

        // Update the UI
        if (scanRequestedText != null)
        {
            scanRequestedText.text = finalDescription;
            
            // Visual feedback: Highlight in orange if contrast is required (Alert)
            if(hasContrast) 
                scanRequestedText.color = new Color(1f, 0.6f, 0f); 
            else 
                scanRequestedText.color = Color.white;
        }
    }

    // Helper method to fill in the demographic fields
    private void SetStaticInfo()
    {
        if(nameText) nameText.text = fixedName;
        if(dobText) dobText.text = fixedDOB;
        if(ageText) ageText.text = fixedAge;
        if(weightText) weightText.text = fixedWeight;
        
        if(scanRequestedText) scanRequestedText.text = "Waiting for the order...";
    }
}