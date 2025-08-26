using UnityEngine;
using System.Collections;

public class LightSwitch : MonoBehaviour
{
    public GameObject[] lights; // Array of light GameObjects to control
    public GameObject switchHandle; // The physical switch handle that rotates
    public AudioSource audioSource;
    private bool lightsOn = true; // State flag to track whether the lights are on
    private bool isRotating; // Flag to avoid triggering multiple simultaneous rotations

    private Quaternion offRotation; // Default (off) rotation of the switch
    private Quaternion onRotation; // Target rotation when switch is "on"

    [SerializeField] private float rotationSpeed = 100f; // Speed of the switch handle animation
    [SerializeField] private float rotationAngle = 14f; // Angle in degrees to rotate the switch

    public Color emissionColor = Color.white; // Emission color when lights are on

    void Start()
    { 
        isRotating = false;

        // Store initial rotation of the switch handle as "off" position
        offRotation = switchHandle.transform.localRotation;

        // Define the "on" rotation by rotating around X-axis by `rotationAngle`
        onRotation = offRotation * Quaternion.Euler(0f, 0f, rotationAngle);

        // Set initial switch handle position based on light state
        StartCoroutine(RotateSwitch(lightsOn ? onRotation : offRotation));
    }

    // This function toggles the light state and animates the switch
    public void ToggleLights()
    {
        audioSource.Play();
        lightsOn = !lightsOn;
        Debug.Log("----------LUCES------------" + lightsOn);

        // Animate the switch handle rotation
        StartCoroutine(RotateSwitch(lightsOn ? onRotation : offRotation));

        foreach (GameObject light in lights)
        {
            // 1. Enable or disable the light component
            Light lightComponent = light.GetComponent<Light>();
            if (lightComponent != null)
            {
                lightComponent.enabled = lightsOn;
            }

            // 2. Enable or disable the emission on the parent object (the light housing)
            Transform parent = light.transform.parent;
            if (parent != null)
            {
                Renderer renderer = parent.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = renderer.material;

                    if (lightsOn)
                    {
                        // Turn on emission and set emission color
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", emissionColor);
                    }
                    else
                    {
                        // Turn off emission
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }


    private IEnumerator RotateSwitch(Quaternion target)
    {
        isRotating = true;

        while (Quaternion.Angle(switchHandle.transform.localRotation, target) > 0.5f)
        {
            switchHandle.transform.localRotation = Quaternion.RotateTowards(
                switchHandle.transform.localRotation,
                target,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        switchHandle.transform.localRotation = target;
        isRotating = false;
    }
}
