using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using DG.Tweening;
using VContainer;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class PauseMenu : MonoBehaviour
    {
        [Title("Menu Configuration")]
        [SerializeField] private GameObject menuPause;
        [SerializeField] private GameObject firstSelectedButton;
        [SerializeField] private int mainMenuSceneIndex = 0;

        [Title("Input Bindings")]
        [SerializeField] private InputActionReference pauseAction;
        [SerializeField] private PlayerInput playerInput;

        [Inject] private IGameStateService gameStateService;

        private bool isPaused = false;
        private CanvasGroup menuCanvasGroup;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            isPaused = false;

            if (menuPause != null)
            {
                menuCanvasGroup = menuPause.GetComponent<CanvasGroup>();
                menuPause.SetActive(false);
            }

            if (playerInput == null)
            {
                playerInput = FindFirstObjectByType<PlayerInput>();
            }

            if (gameStateService == null)
            {
                gameStateService = GameManager.Instance;
            }
        }

        private void OnEnable()
        {
            if (pauseAction != null && pauseAction.action != null)
            {
                pauseAction.action.performed -= HandlePausePerformed;
                pauseAction.action.performed += HandlePausePerformed;
                pauseAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (pauseAction != null && pauseAction.action != null)
            {
                pauseAction.action.performed -= HandlePausePerformed;
            }
        }

        private void HandlePausePerformed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            TogglePause();
        }

        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Resume()
        {
            isPaused = false;

            if (gameStateService != null)
            {
                gameStateService.SetPause(false);
            }
            else
            {
                Time.timeScale = 1f;
            }

            if (menuPause != null)
            {
                if (menuCanvasGroup != null)
                {
                    menuCanvasGroup.DOKill();
                    menuCanvasGroup.DOFade(0f, 0.2f)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            if (menuPause != null) menuPause.SetActive(false);
                        });
                }
                else
                {
                    menuPause.SetActive(false);
                }
            }

            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("Player");
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void Pause()
        {
            isPaused = true;

            if (gameStateService != null)
            {
                gameStateService.SetPause(true);
            }
            else
            {
                Time.timeScale = 0f;
            }

            if (menuPause != null)
            {
                menuPause.SetActive(true);
                menuPause.transform.DOKill();
                menuPause.transform.localScale = Vector3.one * 0.95f;
                menuPause.transform.DOScale(Vector3.one, 0.2f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);

                if (menuCanvasGroup != null)
                {
                    menuCanvasGroup.DOKill();
                    menuCanvasGroup.alpha = 0f;
                    menuCanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
                }
            }

            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("UI");
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                if (firstSelectedButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstSelectedButton);
                }
            }
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;

            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("Player");
            }

            SceneManager.LoadScene(mainMenuSceneIndex);
        }

        // Backward compatibility aliases for existing scene Inspector bindings
        public void reanudar() => Resume();
        public void pausar() => Pause();
        public void irAlMenu() => GoToMainMenu();
    }
}
