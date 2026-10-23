using System;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable]
    public class MeleeConeDelivery : IDeliveryStrategy
    {
        [SerializeField] private float range = 2.5f;
        [SerializeField] private float arcAngle = 120f;

        private static readonly Collider[] HitBuffer = new Collider[16];

        public float Range => range;
        public float ArcAngle => arcAngle;

        public bool Deliver(GameObject user, AbilitySO parentAbility, Transform aimSource, TargetingProfile targeting, IPayloadEffect[] payloads)
        {
            if (user == null || targeting == null) return false;

            Transform source = (aimSource != null) ? aimSource : user.transform;
            int hits = Physics.OverlapSphereNonAlloc(source.position, range, HitBuffer, targeting.DamageableLayers);

            Vector3 forward = source.forward;
            bool hitAny = false;

            for (int i = 0; i < hits; i++)
            {
                var hit = HitBuffer[i];
                if (hit == null || hit.gameObject == user) continue;

                Vector3 directionToTarget = (hit.transform.position - source.position).normalized;
                if (Vector3.Angle(forward, directionToTarget) <= arcAngle * 0.5f)
                {
                    hitAny = true;
                    if (payloads != null)
                    {
                        for (int p = 0; p < payloads.Length; p++)
                        {
                            payloads[p]?.Apply(user, hit.gameObject);
                        }
                    }
                }
            }

            return hitAny;
        }
    }
}
