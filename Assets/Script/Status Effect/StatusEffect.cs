// Assets/Script/StatusEffects/StatusEffect.cs
using UnityEngine;

public abstract class StatusEffect
{
    public float Duration { get; protected set; }
    protected float timer;

    protected readonly Health targetHealth;
    protected readonly GameObject targetObject;

    public StatusEffect(GameObject target, float duration)
    {
        this.targetObject = target;
        this.targetHealth = target.GetComponent<Health>();
        this.Duration = duration;
        this.timer = duration;
    }

    // Se llama cada frame por el StatusEffectManager
    public virtual void Tick(float deltaTime)
    {
        timer -= deltaTime;
    }

    // Se llama cuando el efecto se aplica por primera vez
    public abstract void OnApply();

    // Se llama cuando el efecto termina
    public abstract void OnEnd();

    public bool IsFinished() => timer <= 0;
}