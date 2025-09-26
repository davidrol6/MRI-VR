using UnityEngine;

public class SetVolume : MonoBehaviour
{
    // This will be called by your UI Slider
    public void SetVolumeLevel(float value)
    {
        AudioListener.volume = value; // 0.0 = mute, 1.0 = full volume
    }
}
