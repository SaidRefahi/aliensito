// Assets/Script/StatusEffects/Poison.cs
using UnityEngine;

public class Poison : StatusEffect
{
    private readonly float damagePerTick;
    private float tickTimer;

    public Poison(GameObject target, float duration, float dps) : base(target, duration)
    {
        this.damagePerTick = dps;
    }

    public override void OnApply()
    {
        Debug.Log($"<color=#9400D3>{targetObject.name} has been POISONED.</color>");
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime); // Resta la duración total
        
        tickTimer += deltaTime;
        if (tickTimer >= 1f) // Aplica daño cada 1 segundo
        {
            tickTimer -= 1f;
            if (targetHealth != null && !targetHealth.IsDead())
            {
                targetHealth.TakeDamage(damagePerTick);
            }
        }
    }

    public override void OnEnd()
    {
        Debug.Log($"<color=grey>Poison on {targetObject.name} has worn off.</color>");
    }
}