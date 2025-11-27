using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [Header("Runtime")]
    public GameObject ItemInSlot;

    [Header("UI")]
    public Image slotImage;
    private Color originalColor;

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

        GameObject obj = other.gameObject;
        if (!IsItem(obj)) return;

        var grabRelay = obj.GetComponent<ItemGrabEvents>();

        // Only insert if the object is not currently being held
        if (grabRelay != null && !grabRelay.IsGrabbed)
        {
            InsertItem(obj);
        }
    }

    private bool IsItem(GameObject obj)
    {
        return obj.GetComponent<Item>() != null;
    }

    private void InsertItem(GameObject obj)
    {
        // Stop physics
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        // Parent under the slot so it moves with the inventory
        obj.transform.SetParent(transform, true);
        obj.transform.localPosition = Vector3.zero;  
        obj.transform.localEulerAngles = obj.GetComponent<Item>().slotRotation;

        // Update item state
        var item = obj.GetComponent<Item>();
        item.inSlot = true;
        item.currentSlot = this;

        ItemInSlot = obj;

        // Visual feedback
        slotImage.color = Color.gray;
    }

    public void ResetColor()
    {
        slotImage.color = originalColor;
    }
}
