using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class Poison : StatusEffect
    {
        private readonly float damagePerTick;
        private float tickTimer;

        public Poison(GameObject target, float duration, float dps) : base(target, duration)
        {
            damagePerTick = dps;
        }

        public override void OnApply() { }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            tickTimer += deltaTime;
            if (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                if (targetHealth != null && !targetHealth.IsDead())
                {
                    targetHealth.TakeDamage(damagePerTick);
                }
            }
        }

        public override void OnEnd() { }
    }
}
