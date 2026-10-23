using System.Collections.Generic;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Egocentrismo", menuName = "Habilidades/Invisibilidad/Egocentrismo")]
    public class EgocentrismoSO : AbilitySO
    {
        [Title("Stun Nova Settings")]
        [SerializeField] private float radius = 8f;
        [SerializeField] private float stunDuration = 2.5f;

        [Title("VFX Pool Tag")]
        [SerializeField] private string vfxPoolTag = "EgocentrismoVFX";

        public float Radius => radius;
        public float StunDuration => stunDuration;
        public string VfxPoolTag => vfxPoolTag;

        private readonly Dictionary<GameObject, float> lastUseTime = new Dictionary<GameObject, float>();
        private readonly Collider[] hitColliders = new Collider[16];

        public override bool Execute(GameObject user)
        {
            if (!CanUse(user)) return false;

            TargetingProfile targeting = user.GetComponent<TargetingProfile>();
            if (targeting == null)
            {
                Debug.LogError($"[EgocentrismoSO] {user.name} does not have a TargetingProfile!", user);
                return false;
            }

            int hitCount = Physics.OverlapSphereNonAlloc(user.transform.position, radius, hitColliders, targeting.DamageableLayers);
            for (int i = 0; i < hitCount; i++)
            {
                var hit = hitColliders[i];
                if (hit == null || hit.gameObject == user) continue;

                if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
                {
                    effectManager.ApplyEffect(new Stun(hit.gameObject, stunDuration));
                }
            }

            lastUseTime[user] = Time.time;

            if (PoolManager.Instance != null && !string.IsNullOrEmpty(vfxPoolTag))
            {
                PoolManager.Instance.SpawnFromPool(vfxPoolTag, user.transform.position, Quaternion.identity);
            }

            return true;
        }

        private bool CanUse(GameObject user)
        {
            if (!lastUseTime.TryGetValue(user, out float lastTime))
            {
                return true;
            }
            return Time.time >= lastTime + Cooldown;
        }
    }
}
