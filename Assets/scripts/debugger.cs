using UnityEngine;
using UnityEngine.UI;

public class ToggleChangeDebugger : MonoBehaviour
{
    private Toggle toggle;
    private bool lastValue;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        lastValue = toggle.isOn;
    }

    void Update()
    {
        if (toggle.isOn != lastValue)
        {
            lastValue = toggle.isOn;
            Debug.Log($"[{name}] isOn changed to {toggle.isOn}\n{System.Environment.StackTrace}");
        }
    }
}
