using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public class TargetingProfile : MonoBehaviour
    {
        [Tooltip("Defines the layers that this entity can damage.")]
        [SerializeField] private LayerMask damageableLayers;

        public LayerMask DamageableLayers => damageableLayers;

        public void SetDamageableLayers(LayerMask layers)
        {
            damageableLayers = layers;
        }
    }
}
