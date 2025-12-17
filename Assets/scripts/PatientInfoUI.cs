using UnityEngine;
using TMPro; 

public class PatientInfoUI : MonoBehaviour
{
    [Header("References of UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dobText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI scanRequestedText; 

    [Header("Fixed info (optional)")]
    public string fixedName = "John Doe";
    public string fixedDOB = "12/05/1980";
    public string fixedAge = "45";
    public string fixedWeight = "78 kg";

    private void Start()
    {
        SetStaticInfo();

        if (ClinicalCaseGenerator.Instance != null)
        {
            ClinicalCaseGenerator.Instance.OnCaseGenerated += UpdateScanInfo;
        }
    }

    private void OnDestroy()
    {
        if (ClinicalCaseGenerator.Instance != null)
        {
            ClinicalCaseGenerator.Instance.OnCaseGenerated -= UpdateScanInfo;
        }
    }

    private void UpdateScanInfo(ClinicalCaseGenerator.BodyPart bodyPart, bool hasContrast)
    {
        string zonaEsp = "";
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

        string finalDescription = $"Requested {zonaEsp} MRI {contrasteEsp}";

        if (scanRequestedText != null)
        {
            scanRequestedText.text = finalDescription;
            
            if(hasContrast) 
                scanRequestedText.color = new Color(1f, 0.6f, 0f); 
            else 
                scanRequestedText.color = Color.white;
        }
    }

    private void SetStaticInfo()
    {
        if(nameText) nameText.text = fixedName;
        if(dobText) dobText.text = fixedDOB;
        if(ageText) ageText.text = fixedAge;
        if(weightText) weightText.text = fixedWeight;
        
        if(scanRequestedText) scanRequestedText.text = "Waiting for the order...";
    }
}