using System.Collections;
using UnityEngine;

public class UIAreaFollowCanvas : MonoBehaviour
{
    [Header("References")]
    public RectTransform canvasRoot;         // RectTransform del Canvas (World Space)
    public Transform followTarget;           // La cámara; si es null, usa Camera.main
    public CanvasGroup canvasGroup;          // Para fade in/out (asignar el del Canvas)

    [Header("Placement")]
    [Tooltip("Distancia por delante de la cámara donde se colocará el Canvas (en metros).")]
    public float forwardDistance = 1.2f;
    [Tooltip("Desplazamiento vertical respecto a la cámara (en metros).")]
    public float verticalOffset = 0.0f;

    [Header("Smoothing")]
    [Tooltip("Suavizado de posición (mayor = más rápido).")]
    public float positionLerpSpeed = 10f;
    [Tooltip("Suavizado de rotación (mayor = más rápido).")]
    public float rotationLerpSpeed = 12f;

    [Header("Facing")]
    [Tooltip("Si true, el Canvas gira sólo en Y (no inclina con el pitch de la cabeza).")]
    public bool yawOnly = true;

    [Header("Fade")]
    public float fadeInTime = 0.15f;
    public float fadeOutTime = 0.12f;

    [Header("Debug")]
    public bool startHidden = true;
    private Quaternion baseRotationOffset;

    Collider _area; 
    bool _playerInside = false;
    bool _isVisible = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        _area = GetComponent<Collider>();
        if (!_area)
            Debug.LogWarning("[UIAreaFollowCanvas] Este objeto necesita un Collider marcado como Trigger.");

        if (!canvasGroup && canvasRoot)
            canvasGroup = canvasRoot.GetComponent<CanvasGroup>();

        if (startHidden) SetVisibleImmediate(false);
        baseRotationOffset = canvasRoot.rotation;
    }

    void OnEnable()
    {
        if (!followTarget)
        {
            // Intenta encontrar la cámara principal (VR o desktop)
            var cam = Camera.main;
            if (cam) followTarget = cam.transform;
        }
    }

    void LateUpdate()
    {
        if (!_isVisible || !canvasRoot || !followTarget) return;

        // Posición objetivo: delante de la cámara + offset vertical
        Vector3 desiredPos = followTarget.position + followTarget.forward * forwardDistance + Vector3.up * verticalOffset;
        canvasRoot.position = Vector3.Lerp(canvasRoot.position, desiredPos, Time.deltaTime * positionLerpSpeed);

        // Orientar mirando a la cámara (tipo billboard)
        Quaternion targetRot;
        if (yawOnly)
        {
            Vector3 toCam = canvasRoot.position - followTarget.position;
            toCam.y = 0f; // sin pitch/roll
            if (toCam.sqrMagnitude < 0.0001f) return;
            targetRot = Quaternion.LookRotation(toCam);
        }
        else
        {
            // En vez de mirar HACIA la cámara, mira en la dirección opuesta
        targetRot = Quaternion.LookRotation(canvasRoot.position - followTarget.position);
        }
        canvasRoot.rotation = Quaternion.Slerp(canvasRoot.rotation, targetRot, Time.deltaTime * rotationLerpSpeed);
    }

    // --- Detección por Trigger ---
    void OnTriggerEnter(Collider other)
    {
        if (IsPlayerRoot(other.transform))
        {
            _playerInside = true;
            Show();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayerRoot(other.transform))
        {
            _playerInside = false;
            Hide();
        }
    }

    bool IsPlayerRoot(Transform t)
    {
        // Acepta Player o MainCamera, o cualquier cosa que contenga una Camera
        if (t.CompareTag("Player") || t.CompareTag("MainCamera")) return true;
        if (t.GetComponentInChildren<Camera>() != null) return true;
        if (t.GetComponentInParent<Camera>() != null) return true;
        return false;
    }

    // --- Visibilidad y Fade ---
    public void Show()
    {
        if (_isVisible) return;
        _isVisible = true;
        StopAllCoroutines();
        StartCoroutine(FadeTo(1f, fadeInTime));
    }

    public void Hide()
    {
        if (!_isVisible) return;
        _isVisible = false;
        StopAllCoroutines();
        StartCoroutine(FadeTo(0f, fadeOutTime));
    }

    void SetVisibleImmediate(bool visible)
    {
        _isVisible = visible;
        if (canvasGroup) canvasGroup.alpha = visible ? 1f : 0f;
        if (canvasRoot) canvasRoot.gameObject.SetActive(true); // mantenlo activo para poder interpolar
    }

    IEnumerator FadeTo(float target, float time)
    {
        if (!canvasGroup)
        {
            // Si no hay CanvasGroup, alterna activo/inactivo
            canvasRoot.gameObject.SetActive(target > 0.5f);
            yield break;
        }

        float start = canvasGroup.alpha;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }
        canvasGroup.alpha = target;
    }
}
