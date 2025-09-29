using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIObjectiveItem : MonoBehaviour
{
    public TMP_Text descriptionText;
    public Image checkIcon;

    public void Setup(Objective obj)
    {
        descriptionText.text = obj.description;
        checkIcon.enabled = obj.isCompleted; // activa tick solo si está completado
    }

    public void Refresh(Objective obj)
    {
        checkIcon.enabled = obj.isCompleted;
    }
}
