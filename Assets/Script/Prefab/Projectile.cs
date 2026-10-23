using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;
using VContainer;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class Projectile : MonoBehaviour
    {
        [Title("Pool Configuration")]
        [SerializeField] private string poolTag = "Projectile";

        [Title("Damage Configuration")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float criticalDamageMultiplier = 2f;
        [SerializeField] private LayerMask damageableLayers;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private float lifetime = 5f;

        [Title("Status Effects")]
        [SerializeField] private bool applyPoison = false;
        [SerializeField] private float poisonDamagePerSecond = 2f;
        [SerializeField] private float poisonDuration = 5f;
        [SerializeField] private bool applyVulnerability = false;
        [SerializeField] private float damageMultiplier = 1.5f;
        [SerializeField] private float vulnerabilityDuration = 5f;

        [SerializeField] private bool isCritical = false;

        public bool IsCritical => isCritical;
        public LayerMask DamageableLayers => damageableLayers;

        public void SetCritical(bool value)
        {
            isCritical = value;
        }

        public void SetDamageableLayers(LayerMask layers)
        {
            damageableLayers = layers;
        }

        private IPoolService poolService;
        private CancellationTokenSource lifetimeCts;

        [Inject]
        public void Construct(IPoolService poolService)
        {
            this.poolService = poolService;
        }

        private void OnEnable()
        {
            lifetimeCts?.Cancel();
            lifetimeCts?.Dispose();
            lifetimeCts = new CancellationTokenSource();

            StartLifetimeTimer(lifetimeCts.Token).Forget();
        }

        private void OnDisable()
        {
            lifetimeCts?.Cancel();
            lifetimeCts?.Dispose();
            lifetimeCts = null;
            isCritical = false;
        }

        private async UniTaskVoid StartLifetimeTimer(CancellationToken ct)
        {
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(lifetime), cancellationToken: ct).SuppressCancellationThrow();
            if (!canceled && gameObject.activeSelf)
            {
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (!gameObject.activeSelf) return;

            if (poolService != null)
            {
                poolService.Return(gameObject);
            }
            else if (PoolManager.Instance != null && !string.IsNullOrEmpty(poolTag))
            {
                PoolManager.Instance.ReturnToPool(poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            int otherLayer = 1 << other.gameObject.layer;

            if ((damageableLayers.value & otherLayer) != 0)
            {
                if (other.TryGetComponent<Health>(out Health health))
                {
                    float finalDamage = isCritical ? damage * criticalDamageMultiplier : damage;
                    health.TakeDamage(finalDamage);

                    if (other.TryGetComponent<StatusEffectManager>(out var effectManager))
                    {
                        ApplyStatusEffects(effectManager, other.gameObject);
                    }
                }
                ReturnToPool();
                return;
            }

            if ((obstacleLayers.value & otherLayer) != 0)
            {
                ReturnToPool();
            }
        }

        private void ApplyStatusEffects(StatusEffectManager effectManager, GameObject target)
        {
            if (applyPoison)
            {
                effectManager.ApplyEffect(new Poison(target, poisonDuration, poisonDamagePerSecond));
            }
            if (applyVulnerability)
            {
                effectManager.ApplyEffect(new Vulnerability(target, vulnerabilityDuration, damageMultiplier));
            }
        }
    }
}
