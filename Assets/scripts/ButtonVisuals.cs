using UnityEngine;

/// Simple physical push button: depresses along a local axis and tints colors on hover/press.
/// No UnityEvents. You can (optionally) assign a ButtonMovesObject to control directly.
[RequireComponent(typeof(Collider))]
public class MRIPushButton : MonoBehaviour
{
    [Header("Cap Motion")]
    public Transform buttonCap;                       // The moving mesh (defaults to this transform)
    public Vector3 localAxis = new Vector3(0, 0, -1); // Inward axis in LOCAL space
    [Min(0.001f)] public float travelDistance = 0.02f;
    [Min(0.001f)] public float pressThreshold = 0.015f;
    [Min(0.0001f)] public float releaseThreshold = 0.008f;
    [Min(0.1f)] public float springSpeed = 16f;
    [Range(0f, 0.5f)] public float hoverIndentRatio = 0.1f; // small sink while hovered

    [Header("Visuals")]
    public Renderer[] colorTargets;
    public string colorProperty = "_BaseColor";       // URP: _BaseColor | Built-in: _Color
    public string emissionProperty = "_EmissionColor";
    public bool useEmission = true;
    [Range(0f,3f)] public float emissionIntensity = 1.2f;

    public Color baseColor = new(0.85f, 0.85f, 0.85f);
    public Color hoverColor = new(0.75f, 0.9f, 1f);
    public Color pressedColor = new(0.2f, 0.8f, 0.4f);

    // ---- read-only state that other scripts can poll ----
    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }

    public void OnPress()
    {
        // Do whatever should happen on press
        Debug.Log("Button pressed");
    }

    public void OnRelease()
    {
        // Do whatever should happen on release
        Debug.Log("Button released");
    }


    // ---- internals ----
    private Transform _cap;
    private Vector3 _startLocalPos, _axisNorm;
    private float _currentDepth, _targetDepth;
    private int _hoverCount;
    private readonly System.Collections.Generic.List<Collider> _pushers = new();

    private MaterialPropertyBlock _mpb;

    void Awake()
    {
        _cap = buttonCap != null ? buttonCap : transform;
        _startLocalPos = _cap.localPosition;
        _axisNorm = (localAxis.sqrMagnitude < 1e-6f ? Vector3.back : localAxis).normalized;
        _mpb = new MaterialPropertyBlock();

        var col = GetComponent<Collider>();
        col.isTrigger = true;

        ApplyColors(baseColor, emissionOn:false);
    }

    void Update()
    {
        // Target depth from pushers
        float depthFromPushers = 0f;
        for (int i = _pushers.Count - 1; i >= 0; i--)
        {
            var c = _pushers[i];
            if (c == null) { _pushers.RemoveAt(i); continue; }

            // Use closest point to the cap, measured in this button's local space
            Vector3 worldPoint = c.bounds.ClosestPoint(_cap.position);
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

            float d = Vector3.Dot(localPoint, _axisNorm); // positive along axis
            float inward = Mathf.Max(0f, -d);             // inward if axis points forward
            depthFromPushers = Mathf.Max(depthFromPushers, Mathf.Min(travelDistance, inward));
        }

        // Small idle indent while hovered
        if (_hoverCount > 0 && depthFromPushers < hoverIndentRatio * travelDistance)
            depthFromPushers = Mathf.Max(depthFromPushers, hoverIndentRatio * travelDistance);

        _targetDepth = depthFromPushers;

        // Spring
        _currentDepth = Mathf.MoveTowards(_currentDepth, _targetDepth, springSpeed * Time.deltaTime);
        _cap.localPosition = _startLocalPos + _axisNorm * (-_currentDepth);

        // Press / release edges (no UnityEvents)
        if (!IsPressed && _currentDepth >= pressThreshold)
        {
            IsPressed = true;
            ApplyColors(pressedColor, emissionOn:true);
        }
        else if (IsPressed && _currentDepth <= releaseThreshold)
        {
            IsPressed = false;
            ApplyColors(IsHovered ? hoverColor : baseColor, emissionOn:IsHovered);
        }
    }

    // ----- triggers drive hover + pushing -----
    private void OnTriggerEnter(Collider other)
    {
        if (!_pushers.Contains(other)) _pushers.Add(other);
        _hoverCount++;
        IsHovered = true;

        if (!IsPressed) ApplyColors(hoverColor, emissionOn:true);
    }

    private void OnTriggerExit(Collider other)
    {
        _pushers.Remove(other);
        _hoverCount = Mathf.Max(0, _hoverCount - 1);
        IsHovered = _hoverCount > 0;

        if (!IsHovered && !IsPressed)
        {
            _targetDepth = 0f; // spring back
            ApplyColors(baseColor, emissionOn:false);
        }
    }

    // ----- visuals -----
    private void ApplyColors(Color c, bool emissionOn)
    {
        if (colorTargets == null) return;
        Color em = c * Mathf.LinearToGammaSpace(emissionIntensity);

        foreach (var r in colorTargets)
        {
            if (r == null) continue;

            r.GetPropertyBlock(_mpb);
            if (!string.IsNullOrEmpty(colorProperty))
                _mpb.SetColor(colorProperty, c);

            if (useEmission && !string.IsNullOrEmpty(emissionProperty))
                _mpb.SetColor(emissionProperty, emissionOn ? em : Color.black);

            r.SetPropertyBlock(_mpb);

            // Keywords must be set on materials (Renderer.EnableKeyword is removed in newer Unity)
            if (useEmission)
            {
                foreach (var mat in r.materials)
                {
                    if (emissionOn) mat.EnableKeyword("_EMISSION");
                    else            mat.DisableKeyword("_EMISSION");
                }
            }
        }
    }
}
