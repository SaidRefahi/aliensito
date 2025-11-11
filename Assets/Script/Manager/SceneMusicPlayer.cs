using UnityEngine;

/// <summary>
/// Un script simple que le dice al AudioManager qué música
/// debe sonar al iniciar esta escena.
/// </summary>
public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Configuración de Música")]
    [Tooltip("El nombre EXACTO del track que pusiste en el AudioManager (ej. 'Level1_BGM')")]
    public string musicName;

    void Start()
    {
        // Asegurarnos de que el AudioManager existe
        if (AudioManager.Instance != null)
        {
            // ¡Simplemente le pedimos que ponga la música!
            AudioManager.Instance.PlayMusic(musicName);
        }
        else
        {
            Debug.LogWarning("¡SceneMusicPlayer no pudo encontrar el AudioManager!");
        }
    }
}