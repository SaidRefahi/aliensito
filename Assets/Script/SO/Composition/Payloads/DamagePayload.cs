using System;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable]
    public class DamagePayload : IPayloadEffect
    {
        [SerializeField] private float baseDamage = 15f;
        [SerializeField] private float criticalMultiplier = 2f;

        public float BaseDamage => baseDamage;
        public float CriticalMultiplier => criticalMultiplier;

        public void Apply(GameObject user, GameObject target, float multiplier = 1f)
        {
            if (target == null) return;

            bool isCritical = false;
            if (user != null && user.TryGetComponent<StatusEffectManager>(out var userEffects))
            {
                var critBuff = userEffects.FindEffect(typeof(CriticalBuff));
                if (critBuff != null)
                {
                    isCritical = true;
                    userEffects.RemoveEffect(critBuff);
                }
            }

            float finalDamage = baseDamage * multiplier;
            if (isCritical)
            {
                finalDamage *= criticalMultiplier;
            }

            if (target.TryGetComponent<Health>(out var health) ||
                (target.transform.root != null && target.transform.root.TryGetComponent(out health)))
            {
                health.TakeDamage(finalDamage);
            }
        }
    }
}
