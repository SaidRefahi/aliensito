using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using VContainer;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ReturnToPool : MonoBehaviour
    {
        [Tooltip("Tag matching the pool in PoolManager.")]
        [SerializeField] private string poolTag;

        private ParticleSystem ps;
        private IPoolService poolService;

        [Inject]
        public void Construct(IPoolService poolService)
        {
            this.poolService = poolService;
        }

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        private void OnEnable()
        {
            if (ps != null)
            {
                ps.Play();
            }
        }

        private void OnParticleSystemStopped()
        {
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
    }
}
