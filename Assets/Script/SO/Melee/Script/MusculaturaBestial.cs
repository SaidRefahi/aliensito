using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Musculatura Bestial", menuName = "Habilidades/Melee/Musculatura Bestial")]
    public class MusculaturaBestial : MeleeAbilitySO
    {
        [Title("Musculatura Bestial Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float stunChance = 0.25f;
        [SerializeField] private float stunDuration = 1.5f;

        public float StunChance => stunChance;
        public float StunDuration => stunDuration;

        public override void PerformMelee(GameObject user, float finalDamage, LayerMask damageLayers)
        {
            Transform source = (AimSource != null) ? AimSource : user.transform;
            int hits = Physics.OverlapSphereNonAlloc(source.position, Range, MeleeHitBuffer, damageLayers);

            for (int i = 0; i < hits; i++)
            {
                var hit = MeleeHitBuffer[i];
                if (hit == null || hit.gameObject == user) continue;

                if (hit.TryGetComponent<Health>(out Health health))
                {
                    health.TakeDamage(finalDamage);
                }

                if (Random.value <= stunChance)
                {
                    if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
                    {
                        effectManager.ApplyEffect(new Stun(hit.gameObject, stunDuration));
                    }
                }
            }
        }
    }
}
