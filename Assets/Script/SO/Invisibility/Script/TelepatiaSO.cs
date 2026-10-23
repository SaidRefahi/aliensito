using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Telepatia", menuName = "Habilidades/Invisibilidad/Telepatia")]
    public class TelepatiaSO : InvisibilitySO
    {
        [Title("Telepathy Settings")]
        [SerializeField] private float criticalBuffDuration = 3f;

        public float CriticalBuffDuration => criticalBuffDuration;

        public override bool Execute(GameObject user)
        {
            bool executed = base.Execute(user);
            if (executed)
            {
                var runner = user.GetComponent<InvisibilitySO.InvisibilityRunner>();
                if (runner != null)
                {
                    var listener = user.GetComponent<TelepatiaListener>();
                    if (listener == null)
                    {
                        listener = user.AddComponent<TelepatiaListener>();
                    }
                    listener.Setup(runner, criticalBuffDuration);
                }
            }
            return executed;
        }

        private class TelepatiaListener : MonoBehaviour
        {
            private InvisibilitySO.InvisibilityRunner activeRunner;
            private float buffDuration;

            public void Setup(InvisibilitySO.InvisibilityRunner runner, float duration)
            {
                activeRunner = runner;
                buffDuration = duration;
                activeRunner.OnInvisibilityEnd += HandleInvisibilityEnd;
            }

            private void HandleInvisibilityEnd()
            {
                if (activeRunner != null)
                {
                    activeRunner.OnInvisibilityEnd -= HandleInvisibilityEnd;
                }

                if (TryGetComponent<StatusEffectManager>(out var effectManager))
                {
                    effectManager.ApplyEffect(new CriticalBuff(gameObject, buffDuration));
                }

                Destroy(this);
            }
        }
    }
}
