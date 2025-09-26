using UnityEngine;
using UnityEngine.UI;

public class BrightnessControl : MonoBehaviour
{
    public Image brightnessOverlay; // UI Image filling the screen

    public void SetBrightness(float value)
    {
        Color c = brightnessOverlay.color;
        c.a = 1f - value; // value 0 = dark, 1 = bright
        brightnessOverlay.color = c;
    }
}
