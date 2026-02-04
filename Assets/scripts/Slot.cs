/* * SCRIPT SUMMARY:
 * This script represents a specific "docking station" or inventory slot where an Item can be placed.
 * * * How it works:
 * - It uses a Trigger Collider to detect when a Game Object enters its volume.
 * - It continually checks (in OnTriggerStay) if the object is an valid 'Item' and if the player has released it.
 * - Once released inside the zone, the script "snaps" the item:
 * 1. Disables physics (Kinematic).
 * 2. Parents the item to this slot.
 * 3. Resets local position and applies specific rotation.
 * - It provides visual feedback (changing color) when an item is docked.
 */

using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [Header("Runtime")]
    public GameObject ItemInSlot; // The specific object currently docked here (null if empty)

    [Header("UI")]
    public Image slotImage;       // Visual background for the slot
    private Color originalColor;

    private void Start()
    {
        // Auto-assign UI reference
        if (slotImage == null)
            slotImage = GetComponentInChildren<Image>();

        if (slotImage != null)
            originalColor = slotImage.color;
    }

    // Physics Loop: Checks every frame while a collider is inside the trigger
    private void OnTriggerStay(Collider other)
    {
        // If we are already full, ignore everything
        if (ItemInSlot != null) return;

        GameObject obj = other.gameObject;
        if (!IsItem(obj)) return;

        var grabRelay = obj.GetComponent<ItemGrabEvents>();

        // LOGIC: Only insert/snap the object if the player is NOT currently holding it.
        // We use OnTriggerStay instead of Enter because the player might enter the zone
        // holding the item, and then release it a second later.
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
        // 1. Disable Physics
        // Prevents the object from falling out or colliding physically while docked
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        // 2. Parent to Slot
        // This ensures the item moves with the inventory/UI if the slot moves
        obj.transform.SetParent(transform, true);
        
        // 3. Snap Position/Rotation
        obj.transform.localPosition = Vector3.zero;  
        obj.transform.localEulerAngles = obj.GetComponent<Item>().slotRotation; // Use item's preferred rotation (e.g. flat)

        // 4. Update State Logic
        var item = obj.GetComponent<Item>();
        item.inSlot = true;
        item.currentSlot = this;

        ItemInSlot = obj;

        // 5. Visual Feedback (Darken to show it's occupied)
        if (slotImage != null)
            slotImage.color = Color.gray;
    }

    // Called by ItemGrabEvents when the player pulls the item out of the slot
    public void ResetColor()
    {
        if (slotImage != null)
            slotImage.color = originalColor;
    }
}