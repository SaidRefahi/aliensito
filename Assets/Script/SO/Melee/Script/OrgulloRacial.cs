using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Orgullo Racial", menuName = "Habilidades/Melee/Orgullo Racial")]
    public class OrgulloRacialSO : MeleeAbilitySO
    {
        public override void PerformMelee(GameObject user, float finalDamage, LayerMask damageLayers)
        {
            Transform source = (AimSource != null) ? AimSource : user.transform;
            int hits = Physics.OverlapSphereNonAlloc(source.position, Range, MeleeHitBuffer, damageLayers);

            for (int i = 0; i < hits; i++)
            {
                var hit = MeleeHitBuffer[i];
                if (hit == null || hit.gameObject == user) continue;

                if (hit.TryGetComponent<Health>(out Health health) ||
                    (hit.transform.root != null && hit.transform.root.TryGetComponent(out health)))
                {
                    health.TakeDamage(finalDamage);
                }
            }
        }
    }
}
