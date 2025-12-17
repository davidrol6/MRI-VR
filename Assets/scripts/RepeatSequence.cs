using UnityEngine;
using UnityEngine.UI;

public class RepeatSequenceToggle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drop here the object with script ScanResultDisplay")]
    public ScanResultDisplay displayController;
    
    private Toggle myToggle;

    private void Start()
    {
        myToggle = GetComponent<Toggle>();
        
        if (myToggle != null)
            myToggle.SetIsOnWithoutNotify(false);
    }

    public void OnRepeatToggleChanged(bool isOn)
    {
        if (!isOn) return;

        var generator = ClinicalCaseGenerator.Instance;

        if (generator == null || displayController == null)
        {
            Debug.LogWarning("Faltan referencias en RepeatSequenceToggle");
            return;
        }
        
        if (generator.isImageBlurred)
        {
            Debug.Log("Repeating sequence...");

            foreach (var config in displayController.allImages)
            {
                if (config.bodyPart == generator.currentBodyPart)
                {
                    if (config.blurredImage != null) config.blurredImage.SetActive(false);
                    if (config.sharpImage != null) config.sharpImage.SetActive(true);

                    generator.isImageBlurred = false;

                    if (myToggle != null) myToggle.interactable = false;

                    return; 
                }
            }
        }
        else
        {
            Debug.Log("User tried to repeat but img was nitid.");
            if (myToggle != null)
            {
                myToggle.SetIsOnWithoutNotify(false);
            }
        }
    }
}