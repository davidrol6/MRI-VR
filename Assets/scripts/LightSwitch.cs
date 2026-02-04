/* * SCRIPT SUMMARY:
 * This script controls a physical light switch mechanism, including the visual rotation of the switch handle and the actual lighting state.
 * * * How it works:
 * - It manages a list of Light GameObjects and toggles their 'enabled' state.
 * - It animates the switch handle (smooth rotation) between an 'On' and 'Off' position.
 * - It updates the material emission of the light fixtures to make them appear glowing (On) or dark (Off).
 * - It plays a sound effect whenever the switch is toggled.
 */

using UnityEngine;
using System.Collections;

public class LightSwitch : MonoBehaviour
{
    [Header("Components")]
    public GameObject[] lights;            // Array of Light GameObjects to control
    public GameObject switchHandle;        // The physical switch mesh that rotates
    public AudioSource audioSource;

    [Header("State")]
    private bool lightsOn = true;          // Tracks whether the lights are currently on
    private bool isRotating;               // Flag to prevent overlapping animations

    // Internal rotation targets
    private Quaternion offRotation;        // Local rotation when the switch is OFF
    private Quaternion onRotation;         // Local rotation when the switch is ON

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 100f; // Speed of the switch animation
    [SerializeField] private float rotationAngle = 14f;  // How many degrees the switch flips on Z axis

    public Color emissionColor = Color.white; // Color of the light bulb glow

    void Start()
    { 
        isRotating = false;

        // Save default rotation as the "off" position (assuming scene starts with switch in neutral/off orientation)
        // Adjust logic if scene starts ON but switch is visually OFF
        offRotation = switchHandle.transform.localRotation;

        // Calculate "On" rotation: rotate current "Off" orientation around Z axis by 'rotationAngle'
        onRotation = offRotation * Quaternion.Euler(0f, 0f, rotationAngle);

        // Ensure the switch visual matches the initial boolean state immediately
        StartCoroutine(RotateSwitch(lightsOn ? onRotation : offRotation));
    }

    // Called to toggle lights and animate the switch (e.g., via button click or VR interaction)
    public void ToggleLights()
    {
        if (audioSource != null) audioSource.Play();
        
        lightsOn = !lightsOn;
        Debug.Log("Lights toggled: " + lightsOn);

        // Animate switch handle position smoothly
        StartCoroutine(RotateSwitch(lightsOn ? onRotation : offRotation));

        // Update actual light sources
        foreach (GameObject lightObj in lights)
        {
            if (lightObj == null) continue;

            // 1. Toggle the actual Unity Light component (the source of illumination)
            Light lightComponent = lightObj.GetComponent<Light>();
            if (lightComponent != null)
                lightComponent.enabled = lightsOn;

            // 2. Toggle emission on the light housing (usually the parent mesh of the light source)
            // This makes the bulb mesh look like it's glowing.
            Transform parent = lightObj.transform.parent;
            if (parent != null)
            {
                Renderer renderer = parent.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = renderer.material;

                    if (lightsOn)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", emissionColor);
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }

    // Coroutine to animate the rotation of the switch handle over time
    private IEnumerator RotateSwitch(Quaternion target)
    {
        isRotating = true;

        // Smoothly rotate handle towards the target rotation
        while (Quaternion.Angle(switchHandle.transform.localRotation, target) > 0.5f)
        {
            switchHandle.transform.localRotation = Quaternion.RotateTowards(
                switchHandle.transform.localRotation,
                target,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Snap to exact target rotation at the end
        switchHandle.transform.localRotation = target;
        isRotating = false;
    }
}