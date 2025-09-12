using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : XRGrabInteractable
{
    private Item item;
    private Slot slot;

    protected override void Awake()
    {
        base.Awake();
        item = GetComponent<Item>();
        slot = GetComponent<Slot>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (item != null)
        {
            Debug.Log("Picked up item: " + item.name);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (item != null)
        {
            item.inSlot = false;
            item.currentSlot = null;
        }

        if (slot != null)
        {
            slot.ResetColor();
            Debug.Log("Released item from slot: " + slot.name);
        }
    }
}
