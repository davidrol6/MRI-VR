/* * SCRIPT SUMMARY:
 * This script bridges the gap between VR interaction events (Grab/Release) and the logical "Slot" system.
 * * * How it works:
 * - It exposes public methods (OnGrabBegin, OnGrabEnd) intended to be called by Unity Events from the VR SDK (e.g., OVRGrabbable or XR Interaction Toolkit).
 * - When an object is grabbed, it automatically detects if it was "snapped" into a Slot.
 * - If it was in a slot, it forces the object to detach (un-parent) and resets the slot's state so it becomes free again.
 * - It maintains a public "IsGrabbed" property used by other scripts (like the GrabContainerCarrier) to know if the player is controlling the object.
 */

using UnityEngine;

public class ItemGrabEvents : MonoBehaviour
{
    [Tooltip("Optional: reference to the Item component (auto-detected if left empty).")]
    public Item item;

    [Tooltip("True while the item is currently grabbed.")]
    public bool IsGrabbed { get; private set; }

    private void Awake()
    {
        // Auto-assign Item component if not set manually in the Inspector
        if (item == null) 
            item = GetComponent<Item>();
    }

    // --- Interaction Callbacks ---
    // Connect this to the SDK's "On Select/Grab Begin" event
    public void OnGrabBegin()
    {
        IsGrabbed = true;

        // Logic: If the player picks up an item that is currently snapped into a slot, we must "undock" it immediately.
        if (item.inSlot)
        {
            var parentSlot = item.currentSlot;
            if (parentSlot != null)
            {
                // 1. Tell the slot it is now empty
                parentSlot.ItemInSlot = null;
                item.inSlot = false;

                // 2. Detach the item physically (remove it from the slot's hierarchy)
                transform.parent = null;

                // 3. Restore the slot's visual state (e.g., remove highlight)
                parentSlot.ResetColor();

                // 4. Clear the item's reference to the slot
                item.currentSlot = null;
            }
        }
    }

    // Connect this to the SDK's "On Unselect/Grab End" event
    public void OnGrabEnd()
    {
        IsGrabbed = false;
        // Note: We don't need to handle snapping here. 
        // The Slot scripts themselves detect collisions and check IsGrabbed == false to snap items in automatically.
    }
}