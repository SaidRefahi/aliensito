using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class CriticalBuff : StatusEffect
    {
        public CriticalBuff(GameObject target, float duration) : base(target, duration) { }
        public override void OnApply() { }
        public override void Tick(float deltaTime) => base.Tick(deltaTime);
        public override void OnEnd() { }
    }
}
