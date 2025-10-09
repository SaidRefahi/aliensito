using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;
    public GameObject MenuPause;
    public bool isPaused = false;

    private void Awake()
    {
        isPaused = false;
    }

    private void OnEnable()
    {
        pauseAction.action.performed += _ => TogglePause();
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= _ => TogglePause();
    }

    public void TogglePause()
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
    }

    public void pausar()
    {
        MenuPause.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void irAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }


}