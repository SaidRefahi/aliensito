using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Sed de Sangre", menuName = "Habilidades/Melee/Sed de Sangre")]
    public class SedDeSangre : MeleeAbilitySO
    {
        [Title("Sed de Sangre Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float healPercent = 0.1f;

        public float HealPercent => healPercent;

        public override void PerformMelee(GameObject user, float finalDamage, LayerMask damageLayers)
        {
            Transform source = (AimSource != null) ? AimSource : user.transform;
            int hits = Physics.OverlapSphereNonAlloc(source.position, Range, MeleeHitBuffer, damageLayers);

            if (!user.TryGetComponent<Health>(out var userHealth)) return;

            for (int i = 0; i < hits; i++)
            {
                var hit = MeleeHitBuffer[i];
                if (hit == null || hit.gameObject == user) continue;

                if (hit.TryGetComponent<Health>(out Health targetHealth))
                {
                    targetHealth.TakeDamage(finalDamage);
                    userHealth.Heal(finalDamage * healPercent);
                }
            }
        }
    }
}
