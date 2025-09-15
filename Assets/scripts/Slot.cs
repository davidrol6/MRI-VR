using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [Header("Runtime")]
    public GameObject ItemInSlot;

    [Header("UI")]
    public Image slotImage;
    private Color originalColor;

    [Header("Snapping")]
    [Tooltip("How close (in meters) the item must be to the slot's center to snap in.")]
    public float snapRadius = 0.15f;
    [Tooltip("If true, item rotation will be forced to Item.slotRotation when inserted.")]
    public bool applySlotRotation = true;

    private void Start()
    {
        if (slotImage == null)
            slotImage = GetComponentInChildren<Image>();
        if (slotImage != null)
            originalColor = slotImage.color;
    }

    private void OnTriggerStay(Collider other)
    {
        if (ItemInSlot != null) return;

        GameObject obj = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        if (!IsItem(obj)) return;

        var grabRelay = obj.GetComponent<ItemGrabEvents>();
        // Only allow insert if the item is *not* currently grabbed
        if (grabRelay != null && !grabRelay.IsGrabbed)
        {
            // Optional proximity guard so you don't snap from across the slot
            float dist = Vector3.Distance(obj.transform.position, transform.position);
            if (dist <= snapRadius)
            {
                InsertItem(obj);
            }
        }
    }

    private bool IsItem(GameObject obj)
    {
        return obj.GetComponent<Item>() != null;
    }

    private void InsertItem(GameObject obj)
    {
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Parent under the slot so it rides with the inventory
        obj.transform.SetParent(transform, true);
        obj.transform.localPosition = Vector3.zero;

        var item = obj.GetComponent<Item>();
        if (applySlotRotation && item != null)
            obj.transform.localEulerAngles = item.slotRotation;

        if (item != null)
        {
            item.inSlot = true;
            item.currentSlot = this;
        }

        ItemInSlot = obj;

        if (slotImage != null)
            slotImage.color = Color.gray;
    }

    public void ResetColor()
    {
        if (slotImage != null)
            slotImage.color = originalColor;
    }
}
