using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; 


public class SceneNavigationManager : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("El índice de la escena del juego (ej. 3)")]
    [SerializeField] private int gameSceneIndex = 3;
    
    [Tooltip("El índice de la escena del menú principal (ej. 0)")]
    [SerializeField] private int mainMenuSceneIndex = 0;

    [Header("Navegación de UI")]
    [Tooltip("El primer botón que debe seleccionarse (ej. 'Reiniciar' o 'Menú')")]
    [SerializeField] private GameObject firstSelectedButton;

    private void Start()
    {
   
        Time.timeScale = 1f;

     
        
        EventSystem.current.SetSelectedGameObject(null); // Limpia selección
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

   
    public void RestartGame()
    {
        
        SceneManager.LoadScene(gameSceneIndex);
    }

 
    public void GoToMainMenu()
    {
        // El MenuSystem.Start() de la escena principal se encargará
        // de seleccionar su propio botón "Jugar".
        SceneManager.LoadScene(mainMenuSceneIndex);
    }
}