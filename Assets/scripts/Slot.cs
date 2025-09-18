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
        // Only allow insert if the item is *not* currently grabbed
        if (grabRelay != null && !grabRelay.IsGrabbed)
        {
            InsertItem(obj);
        }
    }

    private bool IsItem(GameObject obj)
    {
        return obj.GetComponent<Item>();
    }

    private void InsertItem(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().isKinematic = true; //stops phisics
        // Parent under the slot so it rides with the inventory
        obj.transform.SetParent(gameObject.transform, true);
        obj.transform.localPosition = Vector3.zero; //snaps slot to center
        obj.transform.localEulerAngles = obj.GetComponent<Item>().slotRotation;

        var item = obj.GetComponent<Item>();
        item.inSlot = true;
        item.currentSlot = this;

        ItemInSlot = obj;
        slotImage.color = Color.gray;
    }

    public void ResetColor()
    {
        slotImage.color = originalColor;
    }
}
