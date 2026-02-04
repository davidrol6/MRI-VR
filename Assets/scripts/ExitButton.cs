/* * SCRIPT SUMMARY:
 * This script provides a mechanism to exit the application using a UI Toggle component.
 * * * How it works:
 * - It is designed to be linked to the 'OnValueChanged' event of a Unity UI Toggle.
 * - When the toggle is switched to the "On" state, it triggers the application shutdown.
 * - It handles platform-specific logic: stopping Play Mode in the Unity Editor or closing the executable in a built version.
 */

using UnityEngine;

public class ExitToggle : MonoBehaviour
{
    // Called by the UI Toggle component. 'isOn' is passed automatically by the UI event.
    public void OnExitToggleChanged(bool isOn)
    {
        // Only trigger the exit if the toggle was turned ON (ignoring the OFF event)
        if (isOn)
        {
            // Standard method to close a standalone built application (e.g., .exe or .apk)
            Application.Quit();

#if UNITY_EDITOR
            // Preprocessor directive: This code only compiles and runs inside the Unity Editor.
            // It allows developers to test the "Quit" functionality without closing the entire Unity Editor.
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}