using System;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable]
    public class LifestealPayload : IPayloadEffect
    {
        [SerializeField] private float healPercent = 0.2f;

        public float HealPercent => healPercent;

        public void Apply(GameObject user, GameObject target, float multiplier = 1f)
        {
            if (user == null || target == null) return;

            if (user.TryGetComponent<Health>(out var userHealth))
            {
                float healAmount = 10f * healPercent * multiplier;
                userHealth.Heal(healAmount);
            }
        }
    }
}
