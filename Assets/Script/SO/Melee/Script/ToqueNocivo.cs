using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Toque Nocivo", menuName = "Habilidades/Melee/Toque Nocivo")]
    public class ToqueNocivo : MeleeAbilitySO
    {
        [Title("Toque Nocivo Settings")]
        [SerializeField] private float poisonDamagePerSecond = 2f;
        [SerializeField] private float poisonDuration = 5f;

        public float PoisonDamagePerSecond => poisonDamagePerSecond;
        public float PoisonDuration => poisonDuration;

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
                    if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
                    {
                        effectManager.ApplyEffect(new Poison(hit.gameObject, poisonDuration, poisonDamagePerSecond));
                    }
                }
            }
        }
    }
}
