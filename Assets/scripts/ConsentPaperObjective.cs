using UnityEngine;

public class ConsentPaperObjective : MonoBehaviour
{
    public string objectiveDescription = "Leer consentimiento";
    public float requiredTime = 10f; // segundos necesarios

    private float timer = 0f;
    private bool isHeld = false;

    void Update()
    {
        if (isHeld)
        {
            timer += Time.deltaTime;
            if (timer >= requiredTime)
            {
                ObjectiveManager.Instance.CompleteObjective(objectiveDescription);
                enabled = false; // para no seguir contando
            }
        }
        else
        {
            timer = 0f; // resetea si suelta antes de tiempo
        }
    }

    // Detecta si el paciente está agarrando
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand")) 
        {
            isHeld = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            isHeld = false;
        }
    }
}
