using UnityEngine;
using UnityEngine.Serialization;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public class TargetingProfile : MonoBehaviour
    {
        [Tooltip("Defines the layers that this entity can damage.")]
        [FormerlySerializedAs("DamageableLayers")]
        [SerializeField] private LayerMask damageableLayers;

        public LayerMask DamageableLayers
        {
            get
            {
                if (damageableLayers.value != 0)
                {
                    return damageableLayers;
                }

                return ResolveFallbackLayers();
            }
        }

        public void SetDamageableLayers(LayerMask layers)
        {
            damageableLayers = layers;
        }

        private void Awake()
        {
            if (damageableLayers.value == 0)
            {
                damageableLayers = ResolveFallbackLayers();
            }
        }

        private LayerMask ResolveFallbackLayers()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            if ((playerLayer != -1 && gameObject.layer == playerLayer) || gameObject.name.Contains("Player"))
            {
                return (enemyLayer != -1) ? (LayerMask)(1 << enemyLayer) : damageableLayers;
            }

            if ((enemyLayer != -1 && gameObject.layer == enemyLayer) ||
                gameObject.name.Contains("Enemy") || gameObject.name.Contains("Boss") || gameObject.name.Contains("Civil"))
            {
                return (playerLayer != -1) ? (LayerMask)(1 << playerLayer) : damageableLayers;
            }

            return damageableLayers;
        }
    }
}
