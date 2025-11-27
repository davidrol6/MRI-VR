using UnityEngine;

public class ItemGrabEvents : MonoBehaviour
{
    [Tooltip("Optional: reference to the Item component (auto-detected if left empty).")]
    public Item item;

    [Tooltip("True while the item is currently grabbed.")]
    public bool IsGrabbed { get; private set; }

    private void Awake()
    {
        // Auto-assign Item component if not set manually
        if (item == null) 
            item = GetComponent<Item>();
    }

    // Connect this to the SDK's "On Select/Grab Begin" event
    public void OnGrabBegin()
    {
        IsGrabbed = true;

        // If the item was sitting in a slot, detach it
        if (item.inSlot)
        {
            var parentSlot = item.currentSlot;
            if (parentSlot != null)
            {
                parentSlot.ItemInSlot = null;
                item.inSlot = false;

                // Detach from slot's transform
                transform.parent = null;

                // Restore slot UI visuals
                parentSlot.ResetColor();

                item.currentSlot = null;
            }
        }
    }

    // Connect this to the SDK's "On Unselect/Grab End" event
    public void OnGrabEnd()
    {
        IsGrabbed = false;
        // Slots check for this state inside their trigger and will snap the item back in automatically.
    }
}
