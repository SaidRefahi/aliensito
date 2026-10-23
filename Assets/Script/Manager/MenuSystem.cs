using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using VContainer;
using Cysharp.Threading.Tasks;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class MenuSystem : MonoBehaviour
    {
        [Title("Main Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject optionsPanel;

        [Title("Navigation Defaults")]
        [SerializeField] private GameObject firstSelectedButtonMainMenu;
        [SerializeField] private GameObject firstSelectedButtonOptions;

        [Title("Option Controls")]
        [SerializeField] private AudioMixer masterMixer;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;

        [Title("Audio Configuration")]
        [SerializeField] private AudioCueSO menuMusicCue;
        [SerializeField] private string menuMusicName = "Musica_Menu";

        [Title("Scene Settings")]
        [SerializeField] private int gameSceneIndex = 3;
        [SerializeField] private int mainMenuSceneIndex = 0;

        [Inject] private IAudioService audioService;
        [Inject] private ISaveService saveService;

        private GameSettingsData currentSettings = new GameSettingsData();
        private const string SETTINGS_SAVE_KEY = "GameSettings";
        private const float PANEL_FADE_DURATION = 0.25f;

        private void Awake()
        {
            if (audioService == null)
            {
                audioService = AudioManager.Instance;
            }

            if (saveService == null)
            {
                saveService = new JsonFileSaveService();
            }
        }

        private async void Start()
        {
            Time.timeScale = 1f;

            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);

            await LoadSettingsAsync();
            PlayMenuMusic();

            SelectFirstButton(firstSelectedButtonMainMenu);
        }

        private void PlayMenuMusic()
        {
            if (audioService == null) return;

            if (menuMusicCue != null)
            {
                audioService.Play(menuMusicCue);
            }
            else if (!string.IsNullOrEmpty(menuMusicName))
            {
                audioService.PlayMusic(menuMusicName);
            }
        }

        public void Play()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneIndex);
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
            SceneManager.LoadScene(mainMenuSceneIndex);
        }

        public void ShowOptionsPanel()
        {
            TransitionPanels(mainMenuPanel, optionsPanel, firstSelectedButtonOptions);
        }

        public void SaveAndCloseOptions()
        {
            SaveSettingsAsync().Forget();
            TransitionPanels(optionsPanel, mainMenuPanel, firstSelectedButtonMainMenu);
        }

        private void TransitionPanels(GameObject fromPanel, GameObject toPanel, GameObject buttonToSelect)
        {
            if (fromPanel != null)
            {
                CanvasGroup fromCg = fromPanel.GetComponent<CanvasGroup>();
                if (fromCg != null)
                {
                    fromCg.DOFade(0f, PANEL_FADE_DURATION)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            fromPanel.SetActive(false);
                            ActivateTargetPanel(toPanel, buttonToSelect);
                        });
                    return;
                }
                fromPanel.SetActive(false);
            }

            ActivateTargetPanel(toPanel, buttonToSelect);
        }

        private void ActivateTargetPanel(GameObject toPanel, GameObject buttonToSelect)
        {
            if (toPanel != null)
            {
                toPanel.SetActive(true);
                CanvasGroup toCg = toPanel.GetComponent<CanvasGroup>();
                if (toCg != null)
                {
                    toCg.alpha = 0f;
                    toCg.DOFade(1f, PANEL_FADE_DURATION).SetUpdate(true);
                }
            }

            SelectFirstButton(buttonToSelect);
        }

        private void SelectFirstButton(GameObject target)
        {
            if (EventSystem.current != null && target != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(target);
            }
        }

        private async UniTask LoadSettingsAsync()
        {
            if (saveService != null)
            {
                currentSettings = await saveService.LoadAsync(SETTINGS_SAVE_KEY, new GameSettingsData());
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = currentSettings.masterVolume;
            }
            ApplyMasterVolume(currentSettings.masterVolume);

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = currentSettings.isFullscreen;
            }
            ApplyFullscreen(currentSettings.isFullscreen);
        }

        private async UniTaskVoid SaveSettingsAsync()
        {
            if (masterVolumeSlider != null)
            {
                currentSettings.masterVolume = masterVolumeSlider.value;
            }
            if (fullscreenToggle != null)
            {
                currentSettings.isFullscreen = fullscreenToggle.isOn;
            }

            if (saveService != null)
            {
                await saveService.SaveAsync(SETTINGS_SAVE_KEY, currentSettings);
            }
        }

        public void SetMasterVolume(float volume)
        {
            currentSettings.masterVolume = volume;
            ApplyMasterVolume(volume);
        }

        private void ApplyMasterVolume(float volume)
        {
            if (masterMixer != null)
            {
                float dbVolume = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
                masterMixer.SetFloat("MasterVolumeParam", dbVolume);
            }
        }

        public void SetFullscreen(bool isFullscreen)
        {
            currentSettings.isFullscreen = isFullscreen;
            ApplyFullscreen(isFullscreen);
        }

        private void ApplyFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
    }
}
