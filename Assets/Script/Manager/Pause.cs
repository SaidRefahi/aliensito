using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // <--- ¡NUEVO! Para controlar la selección

public class PauseMenu : MonoBehaviour
{
    [Header("Configuración del Menú")]
    public GameObject MenuPause;
    
    [Tooltip("Arrastra aquí el botón 'Reanudar' o el primer botón del menú")]
    [SerializeField] private GameObject firstSelectedButton; // <--- ¡NUEVO!

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private PlayerInput playerInput; // <--- ¡NUEVO!
    
    public bool isPaused = false;

    private void Awake()
    {
        isPaused = false;
        
        // <--- NUEVO: Auto-asignación del PlayerInput ---
        if (playerInput == null)
        {
            // Busca el PlayerInput en la escena (asumiendo que solo hay uno)
            playerInput = FindFirstObjectByType<PlayerInput>();
        }
        if (playerInput == null) Debug.LogError("¡PauseMenu no encontró el PlayerInput!");
        // --- Fin del bloque nuevo ---
    }

    private void OnEnable()
    {
        pauseAction.action.performed += TogglePause; // <--- MODIFICADO (forma más segura)
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= TogglePause; // <--- MODIFICADO (forma más segura)
    }

    public void TogglePause(InputAction.CallbackContext context) // <--- MODIFICADO
    {
        // Solo reacciona si la acción se realizó (botón presionado)
        if (!context.performed) return; 
        
        TogglePause(); // Llama a la lógica principal
    }

    public void TogglePause() // Lógica pública principal
    {
        if (isPaused)
        {
            reanudar();
        }
        else
        {
            pausar();
        }
    }

    public void reanudar()
    {
        MenuPause.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        
        // --- ¡NUEVO! Devolvemos el control al Action Map "Player" ---
        playerInput.SwitchCurrentActionMap("Player");
        
        // --- ¡NUEVO! Limpiamos la selección del EventSystem ---
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void pausar()
    {
        MenuPause.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        
        // --- ¡NUEVO! Cambiamos al Action Map "UI" ---
        playerInput.SwitchCurrentActionMap("UI");
        
        // --- ¡NUEVO! Seleccionamos el primer botón para el joystick ---
        EventSystem.current.SetSelectedGameObject(null); // Limpia por si acaso
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void irAlMenu()
    {
        Time.timeScale = 1f;
        
        // --- ¡NUEVO! Asegúrate de volver al mapa "Player" antes de cambiar de escena ---
        // (El menú principal probablemente volverá a cambiarlo a "UI", pero esto es seguro)
        playerInput.SwitchCurrentActionMap("Player"); 
        
        SceneManager.LoadScene(0);
    }
}