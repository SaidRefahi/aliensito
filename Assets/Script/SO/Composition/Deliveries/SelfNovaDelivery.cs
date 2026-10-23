using System;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable]
    public class SelfNovaDelivery : IDeliveryStrategy
    {
        [SerializeField] private float radius = 6f;
        [SerializeField] private string vfxPoolTag = "EgocentrismoVFX";

        private static readonly Collider[] HitBuffer = new Collider[16];

        public float Radius => radius;
        public string VfxPoolTag => vfxPoolTag;

        public bool Deliver(GameObject user, AbilitySO parentAbility, Transform aimSource, TargetingProfile targeting, IPayloadEffect[] payloads)
        {
            if (user == null || targeting == null) return false;

            Vector3 center = user.transform.position;
            int hits = Physics.OverlapSphereNonAlloc(center, radius, HitBuffer, targeting.DamageableLayers);

            if (PoolManager.Instance != null && !string.IsNullOrEmpty(vfxPoolTag))
            {
                PoolManager.Instance.SpawnFromPool(vfxPoolTag, center, Quaternion.identity);
            }

            for (int i = 0; i < hits; i++)
            {
                var hit = HitBuffer[i];
                if (hit == null || hit.gameObject == user) continue;

                if (payloads != null)
                {
                    for (int p = 0; p < payloads.Length; p++)
                    {
                        payloads[p]?.Apply(user, hit.gameObject);
                    }
                }
            }

            return hits > 0;
        }
    }
}
