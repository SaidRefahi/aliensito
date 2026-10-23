using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class StatusEffectManager : MonoBehaviour
    {
        private readonly List<StatusEffect> activeEffects = new List<StatusEffect>();

        public IReadOnlyList<StatusEffect> ActiveEffects => activeEffects;

        public void ApplyEffect(StatusEffect effect)
        {
            if (effect == null) return;

            Type newType = effect.GetType();
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].GetType() == newType)
                {
                    activeEffects[i].OnEnd();
                    activeEffects.RemoveAt(i);
                    break;
                }
            }

            activeEffects.Add(effect);
            effect.OnApply();
        }

        public void RemoveEffect(StatusEffect effect)
        {
            if (effect != null && activeEffects.Remove(effect))
            {
                effect.OnEnd();
            }
        }

        public StatusEffect FindEffect(Type effectType)
        {
            if (effectType == null) return null;

            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].GetType() == effectType)
                {
                    return activeEffects[i];
                }
            }
            return null;
        }

        public T FindEffect<T>() where T : StatusEffect
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i] is T match)
                {
                    return match;
                }
            }
            return null;
        }

        private void Update()
        {
            if (activeEffects.Count == 0) return;

            float dt = Time.deltaTime;
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeEffects[i];
                effect.Tick(dt);

                if (effect.IsFinished())
                {
                    effect.OnEnd();
                    activeEffects.RemoveAt(i);
                }
            }
        }
    }
}
