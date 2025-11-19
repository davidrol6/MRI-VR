using UnityEngine;

public class AreaObjectiveTrigger : MonoBehaviour
{
    [Header("Zona de detección (este objeto)")]
    [Tooltip("Collider que hace de 'zona'. Debe estar marcado como IsTrigger.")]
    public Collider triggerZone;

    [Header("Filtro de objeto que activa el objetivo")]
    [Tooltip("Tag que debe tener el objeto que entra en la zona. Déjalo vacío si no quieres filtrar por tag.")]
    public string requiredTag;

    [Tooltip("Si lo rellenas, solo este objeto concreto activará el objetivo.")]
    public Transform requiredObject;

    [Header("Objetivo asociado")]
    public ObjectiveManager objectiveManager;
    public string objectiveDescription;

    [Tooltip("Si es true, solo se completará la primera vez que entre el objeto.")]
    public bool onlyOnce = true;

    [Header("Objetos cuya textura/material quiero cambiar")]
    [Tooltip("Renderers de los objetos que quieres modificar al completar el objetivo.")]
    public Renderer[] targetRenderers;

    [Header("Qué cambiar al completar")]
    [Tooltip("Si es true, se cambiará el material completo al completar.")]
    public bool changeMaterial = false;
    public Material materialOnComplete;

    [Tooltip("Si es true, se cambiará la textura principal (Main Texture / Base Map).")]
    public bool changeMainTexture = false;
    public Texture mainTextureOnComplete;

    [Tooltip("Si es true, se cambiará el Normal Map (si el shader lo soporta).")]
    public bool changeNormalMap = false;
    public Texture normalMapOnComplete;

    [Tooltip("Si es true, se activará/cambiará la emisión.")]
    public bool changeEmission = false;
    public Texture emissionMapOnComplete;
    public Color emissionColor = Color.white;

    private bool objectiveCompleted = false;

    private void Reset()
    {
        // Intentar autoasignar el collider de la zona
        triggerZone = GetComponent<Collider>();
        if (triggerZone != null)
        {
            triggerZone.isTrigger = true;
        }
    }

    private void Awake()
    {
        if (triggerZone == null)
        {
            triggerZone = GetComponent<Collider>();
        }

        if (triggerZone == null)
        {
            Debug.LogWarning($"[AreaObjectiveTrigger] No hay Collider asignado en {name}. Asigna uno y márcalo como IsTrigger.");
        }
        else if (!triggerZone.isTrigger)
        {
            Debug.LogWarning($"[AreaObjectiveTrigger] El Collider de {name} no está marcado como IsTrigger. Marcándolo por código.");
            triggerZone.isTrigger = true;
        }

        if (objectiveManager == null && ObjectiveManager.Instance != null)
        {
            objectiveManager = ObjectiveManager.Instance;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && objectiveCompleted) return;
        if (triggerZone == null) return;

        // 1) Filtro por tag (si está configurado)
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            return;
        }

        // 2) Filtro por objeto concreto (si está configurado)
        if (requiredObject != null)
        {
            // Aceptamos que sea el objeto directamente o algo de su jerarquía
            if (other.transform != requiredObject && other.transform.root != requiredObject)
            {
                return;
            }
        }

        Debug.Log($"[AreaObjectiveTrigger] {other.name} ha entrado en la zona {name}. Completando objetivo…");
        CompleteObjectiveAndVisuals();
    }

    private void CompleteObjectiveAndVisuals()
    {
        if (objectiveCompleted && onlyOnce) return;
        objectiveCompleted = true;

        // Completar objetivo en el ObjectiveManager
        if (objectiveManager != null && !string.IsNullOrEmpty(objectiveDescription))
        {
            objectiveManager.CompleteObjective(objectiveDescription);
            objectiveManager.PrintObjectivesStatus();
        }
        else
        {
            Debug.LogWarning($"[AreaObjectiveTrigger] No hay ObjectiveManager o descripción de objetivo en {name}.");
        }

        // Si no hay renderers que cambiar, salimos
        if (targetRenderers == null || targetRenderers.Length == 0) return;

        // Cambiar materiales / texturas en los objetos que tú quieras
        foreach (var rend in targetRenderers)
        {
            if (rend == null) continue;

            // OJO: rend.material crea una instancia del material para ese renderer (bien si quieres cambio por instancia)
            Material mat = rend.material;

            // Cambiar material completo
            if (changeMaterial && materialOnComplete != null)
            {
                mat = materialOnComplete;
                rend.material = mat;
            }

            // Cambiar textura principal (albedo / base map)
            if (changeMainTexture && mainTextureOnComplete != null)
            {
                if (mat != null)
                {
                    // URP y Standard suelen usar "_BaseMap" o "_MainTex"
                    if (mat.HasProperty("_BaseMap"))
                        mat.SetTexture("_BaseMap", mainTextureOnComplete);
                    else if (mat.HasProperty("_MainTex"))
                        mat.SetTexture("_MainTex", mainTextureOnComplete);
                }
            }

            // Cambiar normal map
            if (changeNormalMap && normalMapOnComplete != null && mat != null)
            {
                if (mat.HasProperty("_BumpMap"))
                {
                    mat.SetTexture("_BumpMap", normalMapOnComplete);
                    mat.EnableKeyword("_NORMALMAP");
                }
            }

            // Cambiar/activar emisión
            if (changeEmission && mat != null)
            {
                if (emissionMapOnComplete != null && mat.HasProperty("_EmissionMap"))
                {
                    mat.SetTexture("_EmissionMap", emissionMapOnComplete);
                }

                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", emissionColor);
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
    }
}
