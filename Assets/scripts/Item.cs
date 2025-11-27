using UnityEngine;

public class Item : MonoBehaviour
{
    // True when the item is currently placed in a slot
    public bool inSlot;

    // Rotation the item should take when snapped into a slot
    public Vector3 slotRotation = Vector3.zero;

    // Reference to the slot the item is currently in (if any)
    public Slot currentSlot;
}
