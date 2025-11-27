using UnityEngine;

public class ExitToggle : MonoBehaviour
{
    // Called by the Toggle. 'isOn' is true when the toggle is switched on.
    public void OnExitToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // Quit the application in a built version
            Application.Quit();

#if UNITY_EDITOR
            // In the Unity Editor, stop play mode instead of quitting
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
