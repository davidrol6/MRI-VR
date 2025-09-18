using UnityEngine;

public class ItemGrabEvents : MonoBehaviour
{
    [Tooltip("Optional: reference to Item (auto-fetched if null).")]
    public Item item;
    [Tooltip("True while the item is being held (grabbed).")]
    public bool IsGrabbed { get; private set; }

    private void Awake()
    {
        if (item == null) item = GetComponent<Item>();
    }

    // Hook this to the SDK's "On Select/Grab Begin" UnityEvent
    public void OnGrabBegin()
    {
        IsGrabbed = true;

        // If the item is sitting in a slot, free that slot now
        if (item.inSlot)
        {
            var parentSlot = item.currentSlot;
            if (parentSlot != null)
            {
                parentSlot.ItemInSlot = null;
                item.inSlot = false;

                // Break parent so the item is truly detached
                transform.parent = null;

                // Restore slot UI color
                parentSlot.ResetColor();

                item.currentSlot = null;
            }
        }
    }

    // Hook this to the SDK's "On Unselect/Grab End" UnityEvent
    public void OnGrabEnd()
    {
        IsGrabbed = false;
        // The Slot will detect this state (not grabbed) inside its trigger and snap the item in.
    }
}
