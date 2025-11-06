using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems; 

public class MenuSystem : MonoBehaviour
{
    [Header("Paneles Principales")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Input y Navegación")]
    // [SerializeField] private PlayerInput playerInput; // <--- ¡BORRADO!
    [SerializeField] private GameObject firstSelectedButtonMainMenu; // (Ej. Botón "Jugar")
    [SerializeField] private GameObject firstSelectedButtonOptions; // (Ej. Slider de Volumen)

    [Header("Controles de Opciones")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    
    private const string PREFS_MASTER_VOL = "MasterVolume";
    private const string PREFS_FULLSCREEN = "IsFullscreen";


    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
        LoadSettings();
        AudioManager.Instance.PlayMusic("Musica_Menu");
        
        // --- ¡NUEVA LÓGICA DE INPUT (SIMPLIFICADA)! ---
        
        // ¡El EventSystem (con Input System UI Input Module) ya
        // está escuchando el mapa "UI" por defecto!
        // No necesitamos cambiar de mapa.
        
        // Solo necesitamos seleccionar el primer botón.
        EventSystem.current.SetSelectedGameObject(null); // Limpia selección
        EventSystem.current.SetSelectedGameObject(firstSelectedButtonMainMenu);
        
        // --- FIN DEL BLOQUE NUEVO ---
    }

    // --- FUNCIONES DE NAVEGACIÓN ---
    public void Play()
    {
        // Ya NO cambiamos el mapa aquí.
        // La escena del juego se encargará de activar el mapa "Player".
        SceneManager.LoadScene(3);
    }

    public void Quit()
    {
        Application.Quit();
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(0); 
    }

    // --- FUNCIONES DE OPCIONES ---
    public void ShowOptionsPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        
        // Selecciona el primer botón/slider del panel de opciones
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButtonOptions);
    }

    public void SaveAndCloseOptions()
    {
        PlayerPrefs.Save(); 
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
        // Vuelve a seleccionar el primer botón del menú principal
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButtonMainMenu);
    }

    // ... (El resto de funciones LoadSettings, SetMasterVolume, etc. no cambian) ...
    
    private void LoadSettings()
    {
        float masterVol = PlayerPrefs.GetFloat(PREFS_MASTER_VOL, 0.75f);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVol;
        }
        SetMasterVolume(masterVol); 

        bool isFullscreen = PlayerPrefs.GetInt(PREFS_FULLSCREEN, 1) == 1;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
        }
        SetFullscreen(isFullscreen);
    }
    
    public void SetMasterVolume(float volume)
    {
        if (masterMixer == null) return;
        float dbVolume = Mathf.Log10(volume) * 20;
        if (volume == 0) dbVolume = -80f;
        masterMixer.SetFloat("MasterVolumeParam", dbVolume);
        PlayerPrefs.SetFloat(PREFS_MASTER_VOL, volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(PREFS_FULLSCREEN, isFullscreen ? 1 : 0);
    }
}