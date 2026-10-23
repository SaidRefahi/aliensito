using System;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;
using VContainer;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(StatusEffectManager))]
    public class Health : MonoBehaviour
    {
        [Title("Health Settings")]
        [Min(1f)]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath = true;

        [Title("Loot Settings")]
        [SerializeField] private GameObject geneticMaterialPrefab;

        [Title("Sound Events (Sound Designer)")]
        [SerializeField] private AudioCueSO hitAudioCue;
        [SerializeField] private AudioCueSO deathAudioCue;

        [Title("Legacy Sound Compatibility")]
        [SerializeField] private string hitSoundName;
        [SerializeField] private string deathSoundName;

        private float currentHealth;
        private bool hasDied = false;
        private float damageMultiplier = 1f;
        private HitFeedback hitFeedback;
        private IAudioService audioService;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;
        public event Action OnHit;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        private void Awake()
        {
            currentHealth = maxHealth;
            hitFeedback = GetComponent<HitFeedback>();
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(float baseDamage)
        {
            if (hasDied) return;

            float finalDamage = baseDamage * damageMultiplier;
            currentHealth = Mathf.Clamp(currentHealth - finalDamage, 0f, maxHealth);

            if (finalDamage > 0f)
            {
                OnHit?.Invoke();
                PlayHitSound();
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (hitFeedback != null)
            {
                hitFeedback.PlayEffect();
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void PlayHitSound()
        {
            if (hitAudioCue != null)
            {
                if (audioService != null)
                {
                    audioService.Play(hitAudioCue, transform.position);
                }
                else if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play(hitAudioCue, transform.position);
                }
            }
            else if (!string.IsNullOrEmpty(hitSoundName))
            {
                if (audioService != null)
                {
                    audioService.PlaySFX(hitSoundName, transform.position);
                }
                else if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(hitSoundName);
                }
            }
        }

        private void PlayDeathSound()
        {
            if (deathAudioCue != null)
            {
                if (audioService != null)
                {
                    audioService.Play(deathAudioCue, transform.position);
                }
                else if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play(deathAudioCue, transform.position);
                }
            }
            else if (!string.IsNullOrEmpty(deathSoundName))
            {
                if (audioService != null)
                {
                    audioService.PlaySFX(deathSoundName, transform.position);
                }
                else if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(deathSoundName);
                }
            }
        }

        public void SetDamageMultiplier(float multiplier)
        {
            damageMultiplier = Mathf.Max(0f, multiplier);
        }

        public void Heal(float amount)
        {
            if (hasDied || amount <= 0f) return;
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            if (hasDied) return;
            hasDied = true;

            PlayDeathSound();
            OnDeath?.Invoke();

            if (geneticMaterialPrefab != null)
            {
                Instantiate(geneticMaterialPrefab, transform.position, Quaternion.identity);
            }

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }

        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public bool IsDead() => hasDied;
    }
}
