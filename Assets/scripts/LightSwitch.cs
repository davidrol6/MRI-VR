using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public GameObject[] luces;
    private bool encendidas = true;

    public Color colorEncendido = Color.white;

    public void ToggleLights()
    {
        encendidas = !encendidas;

        foreach (GameObject luz in luces)
        {
            // 1. Encender o apagar la luz
            Light componente = luz.GetComponent<Light>();
            if (componente != null)
            {
                componente.enabled = encendidas;
            }

            // 2. Activar o desactivar la Emission del padre
            Transform padre = luz.transform.parent;
            if (padre != null)
            {
                Renderer renderer = padre.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = renderer.material;

                    if (encendidas)
                    {
                        // Activar la emisión y establecer el color deseado
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", colorEncendido);
                    }
                    else
                    {
                        // Desactivar completamente la emisión
                        material.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }
}
