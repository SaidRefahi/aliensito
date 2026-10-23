using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class SceneNavigationManager : MonoBehaviour
    {
        [Title("Scene Configuration")]
        [SerializeField] private int gameSceneIndex = 3;
        [SerializeField] private int mainMenuSceneIndex = 0;

        [Title("UI Navigation")]
        [SerializeField] private GameObject firstSelectedButton;

        private void Start()
        {
            Time.timeScale = 1f;

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                if (firstSelectedButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstSelectedButton);
                }
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneIndex);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneIndex);
        }
    }
}
