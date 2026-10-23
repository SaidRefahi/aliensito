using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Dominio", menuName = "Habilidades/Invisibilidad/Dominio")]
    public class DominioSO : InvisibilitySO
    {
        [Title("Dominio Settings")]
        [SerializeField] private float auraRadius = 6f;
        [SerializeField] private float poisonDamagePerSecond = 1f;
        [SerializeField] private float poisonDuration = 3f;

        [Title("VFX Pool")]
        [SerializeField] private string vfxPoolTag = "DominioVFX";

        public float AuraRadius => auraRadius;
        public float PoisonDamagePerSecond => poisonDamagePerSecond;
        public float PoisonDuration => poisonDuration;
        public string VfxPoolTag => vfxPoolTag;

        public override bool Execute(GameObject user)
        {
            bool executed = base.Execute(user);
            if (executed)
            {
                GameObject vfxInstance = null;
                if (PoolManager.Instance != null && !string.IsNullOrEmpty(vfxPoolTag))
                {
                    vfxInstance = PoolManager.Instance.SpawnFromPool(vfxPoolTag, user.transform.position, Quaternion.identity);
                    if (vfxInstance != null)
                    {
                        vfxInstance.transform.SetParent(user.transform);
                    }
                }

                TargetingProfile targeting = user.GetComponent<TargetingProfile>();
                if (targeting == null)
                {
                    Debug.LogError($"[DominioSO] {user.name} does not have a TargetingProfile!", user);
                    if (vfxInstance != null && PoolManager.Instance != null)
                    {
                        PoolManager.Instance.ReturnToPool(vfxPoolTag, vfxInstance);
                    }
                    return false;
                }

                var aura = user.AddComponent<PoisonAura>();
                aura.Initialize(auraRadius, poisonDamagePerSecond, poisonDuration, targeting.DamageableLayers);

                InvisibilityRunner runner = user.GetComponent<InvisibilityRunner>();
                if (runner != null)
                {
                    void CleanupAura()
                    {
                        runner.OnInvisibilityEnd -= CleanupAura;
                        if (vfxInstance != null && PoolManager.Instance != null)
                        {
                            PoolManager.Instance.ReturnToPool(vfxPoolTag, vfxInstance);
                        }
                        if (aura != null)
                        {
                            Destroy(aura);
                        }
                    }
                    runner.OnInvisibilityEnd += CleanupAura;
                }
            }
            return executed;
        }
    }
}
