using UnityEngine;

public class GrabObjectiveTrigger : MonoBehaviour
{
    [Header("Referencia al ObjectiveManager")]
    public ObjectiveManager objectiveManager;

    [Header("Descripción exacta del objetivo")]
    public string objectiveDescription = "Leer consentimiento";

    [Header("Tiempo necesario para cumplir el objetivo (segundos)")]
    public float requiredHoldTime = 5f;

    private float holdTimer = 0f;
    private bool isHeld = false;

    void Update()
    {
        // Si el objeto está siendo sujetado, cuenta el tiempo
        if (isHeld)
        {
            holdTimer += Time.deltaTime;

            // Una vez superado el tiempo necesario, completa el objetivo
            if (holdTimer >= requiredHoldTime)
            {
                Debug.Log($"🟢 Objetivo cumplido tras {requiredHoldTime} segundos: {objectiveDescription}");
                objectiveManager.CompleteObjective(objectiveDescription);
                objectiveManager.PrintObjectivesStatus();

                // Evita que vuelva a cumplirse
                enabled = false;
            }
        }
        else
        {
            // Si suelta antes del tiempo, resetea el contador
            holdTimer = 0f;
        }
    }

    // Detecta cuando la mano entra en contacto
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            Debug.Log($"✋ Mano detectada, comenzando a contar para: {objectiveDescription}");
            isHeld = true;
        }
    }

    // Detecta cuando la mano suelta o sale del área
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            Debug.Log($"❌ Mano soltó el objeto: {objectiveDescription}");
            isHeld = false;
        }
    }
}
