using System;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public enum EffectPayloadType
    {
        Poison,
        Stun,
        Vulnerability
    }

    [Serializable]
    public class StatusEffectPayload : IPayloadEffect
    {
        [SerializeField] private EffectPayloadType effectType = EffectPayloadType.Poison;
        [SerializeField] private float duration = 4f;
        [SerializeField] private float valuePerSecond = 2f;
        [SerializeField] private float damageMultiplier = 1.5f;

        public EffectPayloadType EffectType => effectType;
        public float Duration => duration;
        public float ValuePerSecond => valuePerSecond;
        public float DamageMultiplier => damageMultiplier;

        public void Apply(GameObject user, GameObject target, float multiplier = 1f)
        {
            if (target == null) return;
            if (!target.TryGetComponent<StatusEffectManager>(out var effectManager)) return;

            switch (effectType)
            {
                case EffectPayloadType.Poison:
                    effectManager.ApplyEffect(new Poison(target, duration, valuePerSecond * multiplier));
                    break;
                case EffectPayloadType.Stun:
                    effectManager.ApplyEffect(new Stun(target, duration));
                    break;
                case EffectPayloadType.Vulnerability:
                    effectManager.ApplyEffect(new Vulnerability(target, duration, damageMultiplier));
                    break;
            }
        }
    }
}
