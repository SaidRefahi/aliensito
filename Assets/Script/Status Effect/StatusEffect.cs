using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public abstract class StatusEffect
    {
        public float Duration { get; protected set; }
        protected float timer;

        protected readonly Health targetHealth;
        protected readonly GameObject targetObject;

        protected StatusEffect(GameObject target, float duration)
        {
            targetObject = target;
            targetHealth = target != null ? target.GetComponent<Health>() : null;
            Duration = duration;
            timer = duration;
        }

        public virtual void Tick(float deltaTime)
        {
            timer -= deltaTime;
        }

        public abstract void OnApply();
        public abstract void OnEnd();

        public bool IsFinished() => timer <= 0f;
    }
}
