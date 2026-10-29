using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas

public class GameManager : MonoBehaviour
{
    [Tooltip("Tecla para reiniciar el nivel actual.")]
    public KeyCode ResetKey = KeyCode.R;

    void Update()
    {
        // Si presionas la tecla asignada (R por defecto)
        if (Input.GetKeyDown(ResetKey))
        {
            // Vuelve a cargar la escena en la que estamos actualmente
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}