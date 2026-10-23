using System.Collections.Generic;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Control de Plagas", menuName = "Habilidades/Ataque a Distancia/Control de plagas")]
    public class ControlDePlagasSO : RangedAttackSO
    {
        [Title("Control de Plagas Settings")]
        [Range(2, 10)]
        [SerializeField] private int projectileCount = 3;
        [SerializeField] private float spreadAngle = 30f;

        public int ProjectileCount => projectileCount;
        public float SpreadAngle => spreadAngle;

        private readonly Dictionary<GameObject, float> localLastUseTime = new Dictionary<GameObject, float>();

        public override bool Execute(GameObject user)
        {
            if (!CanUseLocal(user)) return false;

            if (ProjectilePrefab == null || string.IsNullOrEmpty(ProjectilePoolTag))
            {
                Debug.LogError($"[ControlDePlagasSO] '{name}' has no Prefab or Pool Tag assigned!", this);
                return false;
            }

            TargetingProfile targeting = user.GetComponent<TargetingProfile>();
            if (targeting == null)
            {
                Debug.LogError($"[ControlDePlagasSO] Shooter {user.name} does not have a TargetingProfile!", user);
                return false;
            }

            var effectManager = user.GetComponent<StatusEffectManager>();
            var criticalBuff = effectManager != null ? effectManager.FindEffect(typeof(CriticalBuff)) : null;
            bool isCritical = criticalBuff != null;
            if (isCritical) effectManager.RemoveEffect(criticalBuff);

            Transform spawnPoint = (AimSource != null) ? AimSource : user.transform;

            float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0f;
            float startingAngle = -spreadAngle / 2f;

            for (int i = 0; i < projectileCount; i++)
            {
                Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(0, startingAngle + i * angleStep, 0);

                GameObject projGO = PoolManager.Instance.SpawnFromPool(ProjectilePoolTag, spawnPoint.position, rotation);
                if (projGO == null) continue;

                if (projGO.TryGetComponent<Projectile>(out Projectile projectile))
                {
                    projectile.SetCritical(isCritical && i == projectileCount / 2);
                    projectile.SetDamageableLayers(targeting.DamageableLayers);
                }

                if (projGO.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.linearVelocity = projGO.transform.forward * ProjectileSpeed;
                }
            }

            localLastUseTime[user] = Time.time;
            return true;
        }

        private bool CanUseLocal(GameObject user)
        {
            if (!localLastUseTime.TryGetValue(user, out float last))
            {
                return true;
            }
            return Time.time >= last + Cooldown;
        }
    }
}
