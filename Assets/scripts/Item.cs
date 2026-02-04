/* * SCRIPT SUMMARY:
 * This script represents an interactable object that can be placed into specific "Slots" or holders in the simulation.
 * * * How it works:
 * - It acts as a state container for physical objects (like tools, syringes, or coils).
 * - It tracks whether the item is currently snapped into a slot to prevent double-stacking or physics issues.
 * - It defines the specific rotation the object should adopt when placed, ensuring it fits visually (e.g., a syringe laying flat).
 * - It maintains a reference to the specific Slot instance it currently occupies.
 */

using UnityEngine;

public class Item : MonoBehaviour
{
    // State flag: True when the item is currently anchored/snapped into a slot
    public bool inSlot;

    // Configuration: The local rotation (Euler angles) this item should force when snapped into a slot
    public Vector3 slotRotation = Vector3.zero;

    // State reference: The specific Slot component this item is currently occupying (null if the item is free)
    public Slot currentSlot;
}