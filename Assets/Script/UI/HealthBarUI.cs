using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using SaludosCamaradaHumano.GeneticalDrift.Gameplay;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [Title("Health Bar Visuals")]
        [SerializeField] private Health healthComponent;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image ghostFill;
        [SerializeField] private bool smoothTransition = true;
        [SerializeField] private float transitionDuration = 0.25f;

        private Tween activeFillTween;
        private Tween activeGhostTween;

        public bool HasHealthComponent => healthComponent != null;

        private void Start()
        {
            if (healthComponent != null)
            {
                UpdateHealthBar(healthComponent.GetCurrentHealth(), healthComponent.GetMaxHealth(), false);
            }
        }

        private void OnEnable()
        {
            if (healthComponent != null)
            {
                healthComponent.OnHealthChanged += HandleHealthChanged;
                healthComponent.OnDeath += HandleEntityDeath;
            }
        }

        private void OnDisable()
        {
            if (healthComponent != null)
            {
                healthComponent.OnHealthChanged -= HandleHealthChanged;
                healthComponent.OnDeath -= HandleEntityDeath;
            }
            activeFillTween?.Kill();
            activeGhostTween?.Kill();
        }

        private void OnDestroy()
        {
            activeFillTween?.Kill();
            activeGhostTween?.Kill();
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            UpdateHealthBar(currentHealth, maxHealth, smoothTransition);
        }

        private void UpdateHealthBar(float currentHealth, float maxHealth, bool animate)
        {
            if (healthFill == null) return;

            float ratio = maxHealth > 0f ? currentHealth / maxHealth : 0f;

            activeFillTween?.Kill();

            if (animate)
            {
                activeFillTween = healthFill.DOFillAmount(ratio, transitionDuration)
                    .SetEase(Ease.OutQuad)
                    .SetLink(gameObject);

                if (ghostFill != null)
                {
                    activeGhostTween?.Kill();
                    activeGhostTween = ghostFill.DOFillAmount(ratio, transitionDuration * 1.5f)
                        .SetDelay(0.1f)
                        .SetEase(Ease.OutQuad)
                        .SetLink(gameObject);
                }
            }
            else
            {
                healthFill.fillAmount = ratio;
                if (ghostFill != null)
                {
                    ghostFill.fillAmount = ratio;
                }
            }
        }

        private void HandleEntityDeath() { }

        public void SetHealthComponent(Health health)
        {
            if (healthComponent != null)
            {
                healthComponent.OnHealthChanged -= HandleHealthChanged;
                healthComponent.OnDeath -= HandleEntityDeath;
            }

            healthComponent = health;

            if (healthComponent != null)
            {
                healthComponent.OnHealthChanged += HandleHealthChanged;
                healthComponent.OnDeath += HandleEntityDeath;
                UpdateHealthBar(healthComponent.GetCurrentHealth(), healthComponent.GetMaxHealth(), false);
            }
        }

        public void ForceUpdateHealthBar()
        {
            if (healthComponent != null)
            {
                UpdateHealthBar(healthComponent.GetCurrentHealth(), healthComponent.GetMaxHealth(), false);
            }
        }
    }
}
