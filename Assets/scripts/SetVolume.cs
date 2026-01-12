/* * SCRIPT SUMMARY:
 * This script provides a simple public method to adjust the global audio volume of the application.
 * * * How it works:
 * - It exposes 'SetVolumeLevel', which is designed to be linked directly to a Unity UI Slider event (OnValueChanged).
 * - It modifies 'AudioListener.volume', which affects all sounds in the scene globally.
 */

using UnityEngine;

public class SetVolume : MonoBehaviour
{
    // This will be called by UI Slider's "On Value Changed" event
    public void SetVolumeLevel(float value)
    {
        // Global volume control: 0.0 is mute, 1.0 is full volume
        AudioListener.volume = value; 
    }
}