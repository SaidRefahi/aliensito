using System.Collections.Generic;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Disparo Básico", menuName = "Habilidades/Ataque a Distancia/Disparo Básico")]
    public class RangedAttackSO : AbilitySO, IAimable
    {
        [Title("Ranged Attack Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 25f;
        [SerializeField] private string projectilePoolTag;

        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public string ProjectilePoolTag => projectilePoolTag;

        private readonly Dictionary<GameObject, float> lastUseTime = new Dictionary<GameObject, float>();
        private Transform aimSource;

        public Transform AimSource => aimSource;

        public void SetAimSource(Transform source)
        {
            aimSource = source;
        }

        public override bool Execute(GameObject user)
        {
            if (!CanUse(user)) return false;

            if (projectilePrefab == null || string.IsNullOrEmpty(projectilePoolTag))
            {
                Debug.LogError($"[RangedAttackSO] '{name}' has no Prefab or Pool Tag assigned!", this);
                return false;
            }

            TargetingProfile targeting = user.GetComponent<TargetingProfile>();
            if (targeting == null)
            {
                Debug.LogError($"[RangedAttackSO] Shooter {user.name} does not have a TargetingProfile!", user);
                return false;
            }

            var effectManager = user.GetComponent<StatusEffectManager>();
            var criticalBuff = effectManager != null ? effectManager.FindEffect(typeof(CriticalBuff)) : null;
            bool isCritical = criticalBuff != null;
            if (isCritical) effectManager.RemoveEffect(criticalBuff);

            Transform spawnPoint = (aimSource != null) ? aimSource : user.transform;

            GameObject projGO = PoolManager.Instance.SpawnFromPool(projectilePoolTag, spawnPoint.position, spawnPoint.rotation);
            if (projGO == null) return false;

            if (projGO.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.SetCritical(isCritical);
                projectile.SetDamageableLayers(targeting.DamageableLayers);
            }

            if (projGO.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = spawnPoint.forward * projectileSpeed;
            }

            lastUseTime[user] = Time.time;
            return true;
        }

        protected bool CanUse(GameObject user)
        {
            if (!lastUseTime.TryGetValue(user, out float last))
            {
                return true;
            }
            return Time.time >= last + Cooldown;
        }
    }
}
