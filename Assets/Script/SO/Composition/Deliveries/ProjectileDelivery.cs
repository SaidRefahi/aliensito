using System;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable]
    public class ProjectileDelivery : IDeliveryStrategy
    {
        [SerializeField] private string poolTag = "PlayerBullet";
        [SerializeField] private float speed = 25f;

        public string PoolTag => poolTag;
        public float Speed => speed;

        public bool Deliver(GameObject user, AbilitySO parentAbility, Transform aimSource, TargetingProfile targeting, IPayloadEffect[] payloads)
        {
            if (user == null || targeting == null || string.IsNullOrEmpty(poolTag)) return false;

            Transform spawnPoint = (aimSource != null) ? aimSource : user.transform;
            GameObject projGO = PoolManager.Instance != null 
                ? PoolManager.Instance.SpawnFromPool(poolTag, spawnPoint.position, spawnPoint.rotation)
                : null;

            if (projGO == null) return false;

            if (projGO.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.SetDamageableLayers(targeting.DamageableLayers);
            }

            if (projGO.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = spawnPoint.forward * speed;
            }

            return true;
        }
    }
}
