using UnityEngine;
using DG.Tweening;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;
using VContainer;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(Renderer))]
    public class HitFeedback : MonoBehaviour
    {
        [Title("Squash & Stretch Feedback (DOTween)")]
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private float effectDuration = 0.15f;
        [SerializeField] private Vector3 punchScale = new Vector3(-0.2f, 0.3f, -0.2f);

        [Title("VFX Pooling")]
        [SerializeField] private string particlePoolTag = "HitEffect";

        private Renderer enemyRenderer;
        private Color originalColor;
        private IPoolService poolService;
        private Tween activeColorTween;
        private Tween activeScaleTween;

        [Inject]
        public void Construct(IPoolService poolService)
        {
            this.poolService = poolService;
        }

        private void Awake()
        {
            enemyRenderer = GetComponent<Renderer>();
            originalColor = enemyRenderer.material.color;
        }

        private void OnDestroy()
        {
            activeColorTween?.Kill();
            activeScaleTween?.Kill();
        }

        public void PlayEffect()
        {
            SpawnHitParticle();
            PlayJuiceTweens();
        }

        private void SpawnHitParticle()
        {
            if (string.IsNullOrEmpty(particlePoolTag)) return;

            if (poolService != null)
            {
                poolService.Spawn(particlePoolTag, transform.position, Quaternion.identity);
            }
            else if (PoolManager.Instance != null)
            {
                PoolManager.Instance.SpawnFromPool(particlePoolTag, transform.position, Quaternion.identity);
            }
        }

        private void PlayJuiceTweens()
        {
            activeScaleTween?.Kill(true);
            activeColorTween?.Kill(true);

            activeScaleTween = transform.DOPunchScale(punchScale, effectDuration, 10, 1f)
                .SetLink(gameObject);

            activeColorTween = enemyRenderer.material.DOColor(hitColor, effectDuration * 0.5f)
                .SetLoops(2, LoopType.Yoyo)
                .SetLink(gameObject)
                .OnComplete(() => enemyRenderer.material.color = originalColor);
        }
    }
}
