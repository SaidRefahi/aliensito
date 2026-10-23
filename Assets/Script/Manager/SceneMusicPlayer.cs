using UnityEngine;
using VContainer;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class SceneMusicPlayer : MonoBehaviour
    {
        [Title("Audio Configuration")]
        [Tooltip("Direct AudioCueSO reference (recommended for sound designers)")]
        [SerializeField] private AudioCueSO musicCue;

        [Tooltip("Legacy track name fallback")]
        [SerializeField] private string musicName;

        [Inject] private IAudioService audioService;

        private void Awake()
        {
            if (audioService == null)
            {
                audioService = AudioManager.Instance;
            }
        }

        private void Start()
        {
            if (audioService != null)
            {
                if (musicCue != null)
                {
                    audioService.Play(musicCue);
                }
                else if (!string.IsNullOrEmpty(musicName))
                {
                    audioService.PlayMusic(musicName);
                }
            }
            else
            {
                Debug.LogWarning("[SceneMusicPlayer] AudioService is not available in the scene.");
            }
        }
    }
}
