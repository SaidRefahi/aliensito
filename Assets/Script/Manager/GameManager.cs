using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class GameManager : MonoBehaviour, IGameStateService
    {
        [Title("Entity Health References")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private Health bossHealth;

        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action OnVictory;
        public event Action OnDefeat;
        public event Action OnBossFightStarted;

        public static GameManager Instance { get; private set; }

        private bool gameEnded = false;
        private bool bossFightHasStarted = false;
        private bool isPaused = false;

        public bool IsPaused => isPaused;
        public bool IsGameOver => gameEnded;
        public bool BossFightHasStarted => bossFightHasStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath += HandlePlayerDeath;
            }
            if (bossHealth != null)
            {
                bossHealth.OnDeath += HandleBossDeath;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandlePlayerDeath;
            }
            if (bossHealth != null)
            {
                bossHealth.OnDeath -= HandleBossDeath;
            }
        }

        public void StartBossFight()
        {
            if (bossFightHasStarted) return;
            bossFightHasStarted = true;
            OnBossFightStarted?.Invoke();
        }

        public void SetPause(bool pause)
        {
            if (gameEnded) return;
            isPaused = pause;
            Time.timeScale = isPaused ? 0f : 1f;

            if (isPaused)
            {
                OnGamePaused?.Invoke();
            }
            else
            {
                OnGameResumed?.Invoke();
            }
        }

        public void TriggerVictory()
        {
            if (gameEnded) return;
            gameEnded = true;
            OnVictory?.Invoke();
            SceneManager.LoadScene(2);
        }

        public void TriggerDefeat()
        {
            if (gameEnded) return;
            gameEnded = true;
            OnDefeat?.Invoke();
            SceneManager.LoadScene(1);
        }

        private void HandlePlayerDeath()
        {
            TriggerDefeat();
        }

        private void HandleBossDeath()
        {
            if (bossFightHasStarted)
            {
                TriggerVictory();
            }
        }
    }
}
