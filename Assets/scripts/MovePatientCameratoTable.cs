using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Puente UI → ButtonMovesObject (sin herencia)
/// - simpleClickMode = true: un click ejecuta OnPress()+OnRelease() (paso discreto)
/// - simpleClickMode = false: usa PointerDown/PointerUp para Hold
/// Opcional: overrides por botón de dirección y distancia.
public class ButtonMovesObject_UIBridge : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Destino (tu script existente)")]
    public ButtonMovesObject targetMover;

    [Header("Modo de activación")]
    public bool simpleClickMode = true;      // Click único (Toggle)
    public Button uiButton;                  // Asigna el Button del Canvas si usas simpleClickMode

    [Header("Overrides por botón (opcionales)")]
    public bool useOverrides = false;
    public Vector3 overrideDirection = Vector3.right; // se usará para el signo/axis canónico
    public float overrideDistance = 0.25f;            // step usado en Toggle
    public bool restoreAfterAction = true;            // volver a valores originales tras la acción

    // Copias para restaurar
    private Vector3 _origDirection;
    private float _origDistance;

    void Awake()
    {
        if (targetMover == null)
            Debug.LogError("[UIBridge] Falta 'targetMover' (ButtonMovesObject).");

        if (simpleClickMode && uiButton == null)
            Debug.LogWarning("[UIBridge] simpleClickMode está activo pero no has asignado 'uiButton'. Puedes llamar ClickFromUI() desde OnClick manualmente.");
    }

    void Start()
    {
        if (simpleClickMode && uiButton != null)
            uiButton.onClick.AddListener(ClickFromUI);
    }

    void OnDestroy()
    {
        if (simpleClickMode && uiButton != null)
            uiButton.onClick.RemoveListener(ClickFromUI);
    }

    // === CLICK ÚNICO (Toggle) ===
    public void ClickFromUI()
    {
        if (targetMover == null) return;

        // Guardar y aplicar overrides si procede
        if (useOverrides)
        {
            _origDirection = targetMover.direction;
            _origDistance  = targetMover.distance;
            targetMover.direction = overrideDirection;
            targetMover.distance  = overrideDistance;
        }

        // Simular un “tap” completo
        targetMover.OnPress();
        targetMover.OnRelease();

        // Restaurar
        if (useOverrides && restoreAfterAction)
        {
            targetMover.direction = _origDirection;
            targetMover.distance = _origDistance;
        }
        Debug.Log("[UIBridge] ClickFromUI");
    }

    // === HOLD (mantener pulsado) ===
    // Estos se disparan si el objeto tiene Graphic + Raycast Target y hay EventSystem en la escena.
    public void OnPointerDown(PointerEventData eventData)
    {
        if (simpleClickMode || targetMover == null) return;

        if (useOverrides)
        {
            _origDirection = targetMover.direction;
            _origDistance  = targetMover.distance;
            targetMover.direction = overrideDirection;
            targetMover.distance  = overrideDistance;
        }

        targetMover.OnPress(); // empieza a mover (PressMode.Hold debe estar en el targetMover)
        Debug.Log("[Mover] OnPress (Toggle)");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (simpleClickMode || targetMover == null) return;

        targetMover.OnRelease(); // detiene el Hold

        if (useOverrides && restoreAfterAction)
        {
            targetMover.direction = _origDirection;
            targetMover.distance  = _origDistance;
        }
    }
}
