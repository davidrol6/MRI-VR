using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class ActiveStateRisingEdgeToggle : MonoBehaviour
{
    [Header("Source (IActiveState)")]
    [Tooltip("Arrastra aquí tu HandGestureActiveState u otro MonoBehaviour que implemente IActiveState")]
    public MonoBehaviour activeStateSource; // debe implementar IActiveState

    [Header("Target")]
    public GameObject targetToToggle; // tu panel/canvas
    [Tooltip("Si prefieres invocar un método propio en vez de SetActive")]
    public UnityEvent onToggle; // opcional

    [Header("Guardas")]
    public float cooldown = 0.4f;

    private IActiveState _state;
    private bool _prev;
    private float _lastToggleTime = -999f;

    void Awake()
    {
        _state = activeStateSource as IActiveState;
        if (_state == null)
        {
            Debug.LogError("ActiveStateRisingEdgeToggle: activeStateSource no implementa IActiveState.");
        }
    }

    void Update()
    {
        if (_state == null) return;

        bool curr = _state.Active;

        // Flanco de subida: false -> true
        if (!_prev && curr && Time.time - _lastToggleTime >= cooldown)
        {
            _lastToggleTime = Time.time;

            if (targetToToggle != null)
                targetToToggle.SetActive(!targetToToggle.activeSelf);

            if (onToggle != null)
                onToggle.Invoke();
        }

        _prev = curr;
    }
}
