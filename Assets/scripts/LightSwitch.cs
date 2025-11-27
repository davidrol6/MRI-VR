using UnityEngine;
using System.Collections;

public class LightSwitch : MonoBehaviour
{
    public GameObject[] lights;            // Light GameObjects to control
    public GameObject switchHandle;        // The switch handle that rotates
    public AudioSource audioSource;

    private bool lightsOn = true;          // Tracks whether the lights are currently on
    private bool isRotating;               // Prevents multiple rotate coroutines from overlapping

    private Quaternion offRotation;        // Local rotation when the switch is OFF
    private Quaternion onRotation;         // Local rotation when the switch is ON

    [SerializeField] private float rotationSpeed = 100f; 
    [SerializeField] private float rotationAngle = 14f;  

    public Color emissionColor = Color.white;

    void Start()
    { 
        isRotating = false;

        // Save default rotation as "off" position
        offRotation = switchHandle.transform.localRotation;

        // "On" rotation = rotate around Z axis by 'rotationAngle'
        onRotation = offRotation * Quaternion.Euler(0f, 0f, rotationAngle);

        // Set initial switch orientation based on state
        StartCoroutine(RotateSwitch(lightsOn ? onRotation : offRotation));
    }

    // Called to toggle lights and animate the switch
    public void ToggleLights()
    {
        audioSource.Play();
        lightsOn = !lightsOn;
        Debug.Log("Lights toggled: " + lightsOn);

        // Animate switch handle position
        StartCoroutine(RotateSwitch(lightsOn ? onRotation : offRotation));

        foreach (GameObject light in lights)
        {
            // 1. Toggle the actual Light component
            Light lightComponent = light.GetComponent<Light>();
            if (lightComponent != null)
                lightComponent.enabled = lightsOn;

            // 2. Toggle emission on the light housing (usually the parent mesh)
            Transform parent = light.transform.parent;
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

    private IEnumerator RotateSwitch(Quaternion target)
    {
        isRotating = true;

        // Smoothly rotate handle to the new target rotation
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
