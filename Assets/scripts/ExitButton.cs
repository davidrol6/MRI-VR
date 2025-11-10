using UnityEngine;

public class ExitToggle : MonoBehaviour
{
    // Este método recibe el estado del Toggle (true = activado, false = desactivado)
    public void OnExitToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // Cierra el juego si está compilado
            Application.Quit();

            // Si estás en el Editor, detiene la simulación
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
