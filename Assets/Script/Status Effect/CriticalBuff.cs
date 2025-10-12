using UnityEngine;

public class CriticalBuff : StatusEffect
{
    public CriticalBuff(GameObject target, float duration) : base(target, duration) { }
    public override void OnApply() { Debug.Log($"<color=orange>{targetObject.name} tiene GOLPE CRÍTICO!</color>"); }
    public override void Tick(float deltaTime) => base.Tick(deltaTime);
    public override void OnEnd() { Debug.Log($"<color=grey>El buff de crítico en {targetObject.name} ha terminado.</color>"); }
}