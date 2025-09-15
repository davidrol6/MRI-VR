using UnityEngine;

public class InventoryVR : MonoBehaviour
{
    [Header("References")]
    public GameObject Inventory;    // Root of your inventory Canvas/Panel
    public Transform Anchor;        // Wrist/hand anchor (or any transform to follow)

    [Header("Follow")]
    public Vector3 localEulerOffset = new Vector3(15f, 0f, 0f);
    public Vector3 worldPositionOffset = Vector3.zero; // small offset forward from the wrist, if desired
    public bool billboardToCamera = false;

    [Header("Editor/Optional Input")]
    public KeyCode editorToggleKey = KeyCode.I; // for quick testing in Play Mode

    [Tooltip("Optional: still allow the old OVRInput.Button.Four for controllers. Leave false if you don't use OVRInput.")]
    public bool allowOVRInputToggle = false;

    private bool _uiActive;

    private void Start()
    {
        if (Inventory != null) Inventory.SetActive(false);
        _uiActive = false;
    }

    private void Update()
    {
        // Editor/testing shortcut
        if (Input.GetKeyDown(editorToggleKey))
        {
            Toggle();
        }

        // Optional legacy controller toggle
        #if OCULUS_SDK_PRESENT
        if (allowOVRInputToggle)
        {
            try
            {
                if (OVRInput.GetDown(OVRInput.Button.Four))
                    Toggle();
            }
            catch { /* OVRInput not present at runtime */ }
        }
        #endif

        if (_uiActive && Inventory != null && Anchor != null)
        {
            // Follow anchor with offsets
            Inventory.transform.position = Anchor.position + Anchor.TransformVector(worldPositionOffset);
            Inventory.transform.rotation = Anchor.rotation * Quaternion.Euler(localEulerOffset);

            if (billboardToCamera && Camera.main != null)
            {
                Vector3 dir = Camera.main.transform.position - Inventory.transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                    Inventory.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
        }
    }

    // Expose these for UnityEvents (e.g., hand gesture selectors)
    public void Show() { SetActive(true); }
    public void Hide() { SetActive(false); }
    public void Toggle() { SetActive(!_uiActive); }

    private void SetActive(bool active)
    {
        _uiActive = active;
        if (Inventory != null) Inventory.SetActive(active);
    }
}
