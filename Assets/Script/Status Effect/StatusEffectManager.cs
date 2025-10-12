using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System; // Necesario para Type

public class StatusEffectManager : MonoBehaviour
{
    // Cambiamos a public para que los scripts de ataque puedan leerlo
    public readonly List<StatusEffect> activeEffects = new List<StatusEffect>();

    public void ApplyEffect(StatusEffect effect)
    {
        var existingEffect = activeEffects.FirstOrDefault(e => e.GetType() == effect.GetType());
        if (existingEffect != null)
        {
            activeEffects.Remove(existingEffect);
        }
        activeEffects.Add(effect);
        effect.OnApply();
    }

    // --- NUEVO MÉTODO PARA QUITAR UN EFECTO ---
    public void RemoveEffect(StatusEffect effect)
    {
        if (effect != null && activeEffects.Contains(effect))
        {
            effect.OnEnd();
            activeEffects.Remove(effect);
        }
    }
    
    // --- NUEVO MÉTODO PARA BUSCAR UN TIPO DE EFECTO ---
    public StatusEffect FindEffect(Type effectType)
    {
        return activeEffects.FirstOrDefault(e => e.GetType() == effectType);
    }

    private void Update()
    {
        if (activeEffects.Count == 0) return;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            effect.Tick(Time.deltaTime);
            if (effect.IsFinished())
            {
                effect.OnEnd();
                activeEffects.RemoveAt(i);
            }
        }
    }
}