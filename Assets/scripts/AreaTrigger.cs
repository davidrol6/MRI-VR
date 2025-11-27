using UnityEngine;

public class AreaObjectiveTrigger : MonoBehaviour
{
    [Header("Detection zone (this GameObject)")]
    [Tooltip("Collider that defines the area. Must be marked as IsTrigger.")]
    public Collider triggerZone;

    [Header("Filter for the object that triggers the objective")]
    [Tooltip("Tag required for the object entering the zone. Leave empty to ignore tags.")]
    public string requiredTag;

    [Tooltip("If set, only this specific Transform (or something in its hierarchy) will trigger the objective.")]
    public Transform requiredObject;

    [Header("Linked objective")]
    public ObjectiveManager objectiveManager;
    public string objectiveDescription;

    [Tooltip("If true, the objective is only completed the first time something enters.")]
    public bool onlyOnce = true;

    [Header("Renderers to modify when the objective completes")]
    [Tooltip("Renderers whose material/texture will be changed on completion.")]
    public Renderer[] targetRenderers;

    [Header("What to change on completion")]
    [Tooltip("If true, replace the whole material when the objective completes.")]
    public bool changeMaterial = false;
    public Material materialOnComplete;

    [Tooltip("If true, change the main texture (albedo / base map).")]
    public bool changeMainTexture = false;
    public Texture mainTextureOnComplete;

    [Tooltip("If true, change the normal map (if the shader supports it).")]
    public bool changeNormalMap = false;
    public Texture normalMapOnComplete;

    [Tooltip("If true, enable/change emission.")]
    public bool changeEmission = false;
    public Texture emissionMapOnComplete;
    public Color emissionColor = Color.white;

    // Internal flag to avoid completing multiple times when onlyOnce is enabled
    private bool objectiveCompleted = false;

    private void Reset()
    {
        // Try to automatically assign the collider on this GameObject
        triggerZone = GetComponent<Collider>();
        if (triggerZone != null)
        {
            triggerZone.isTrigger = true;
        }
    }

    private void Awake()
    {
        // Make sure we have a trigger collider
        if (triggerZone == null)
        {
            triggerZone = GetComponent<Collider>();
        }

        if (triggerZone == null)
        {
            Debug.LogWarning($"[AreaObjectiveTrigger] No Collider assigned on {name}. Assign one and mark it as IsTrigger.");
        }
        else if (!triggerZone.isTrigger)
        {
            Debug.LogWarning($"[AreaObjectiveTrigger] Collider on {name} is not marked as IsTrigger. Marking it in code.");
            triggerZone.isTrigger = true;
        }

        // Try to grab a global ObjectiveManager instance if none is set in the inspector
        if (objectiveManager == null && ObjectiveManager.Instance != null)
        {
            objectiveManager = ObjectiveManager.Instance;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && objectiveCompleted) return;
        if (triggerZone == null) return;

        // 1) tag filter
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            return;
        }

        // 2) specific object filter
        if (requiredObject != null)
        {
            // Accept either the exact object or something in its hierarchy
            if (other.transform != requiredObject && other.transform.root != requiredObject)
            {
                return;
            }
        }

        Debug.Log($"[AreaObjectiveTrigger] {other.name} entered area {name}. Completing objective.");
        CompleteObjectiveAndVisuals();
    }


    /// Marks the objective as completed and applies visual changes.

    private void CompleteObjectiveAndVisuals()
    {
        if (objectiveCompleted && onlyOnce) return;
        objectiveCompleted = true;

        // Notify ObjectiveManager
        if (objectiveManager != null && !string.IsNullOrEmpty(objectiveDescription))
        {
            objectiveManager.CompleteObjective(objectiveDescription);
            objectiveManager.PrintObjectivesStatus();
        }
        else
        {
            Debug.LogWarning($"[AreaObjectiveTrigger] Missing ObjectiveManager or objectiveDescription on {name}.");
        }

        // If there are no renderers to modify, we're done
        if (targetRenderers == null || targetRenderers.Length == 0) return;

        // Apply material/texture changes to the configured renderers
        foreach (var rend in targetRenderers)
        {
            if (rend == null) continue;

            // rend.material returns an instanced material for this renderer
            Material mat = rend.material;

            // Replace whole material
            if (changeMaterial && materialOnComplete != null)
            {
                mat = materialOnComplete;
                rend.material = mat;
            }

            // Change main texture (albedo / base map)
            if (changeMainTexture && mainTextureOnComplete != null && mat != null)
            {
                // URP and Standard commonly use "_BaseMap" or "_MainTex"
                if (mat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", mainTextureOnComplete);
                else if (mat.HasProperty("_MainTex"))
                    mat.SetTexture("_MainTex", mainTextureOnComplete);
            }

            // Change normal map
            if (changeNormalMap && normalMapOnComplete != null && mat != null)
            {
                if (mat.HasProperty("_BumpMap"))
                {
                    mat.SetTexture("_BumpMap", normalMapOnComplete);
                    mat.EnableKeyword("_NORMALMAP");
                }
            }

            // Change / enable emission
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
