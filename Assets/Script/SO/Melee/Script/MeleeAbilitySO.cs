using System.Collections.Generic;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public abstract class MeleeAbilitySO : AbilitySO, IAimable
    {
        [Title("Melee Configuration")]
        [SerializeField] private float damage = 15f;
        [SerializeField] private float range = 2f;
        [SerializeField] private float criticalDamageMultiplier = 2f;

        [Title("Cooldown & Combo Configuration")]
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private bool enableCombos = false;
        [SerializeField] private int maxComboHits = 3;
        [SerializeField] private float comboResetTime = 1.5f;
        [SerializeField] private float[] comboDamageMultipliers = { 1f, 1.2f, 1.5f };

        public float Damage => damage;
        public float Range => range;
        public float CriticalDamageMultiplier => criticalDamageMultiplier;
        public float AttackCooldown => attackCooldown;
        public bool EnableCombos => enableCombos;
        public int MaxComboHits => maxComboHits;
        public float ComboResetTime => comboResetTime;
        public IReadOnlyList<float> ComboDamageMultipliers => comboDamageMultipliers;

        private Transform aimSource;
        public Transform AimSource => aimSource;

        public void SetAimSource(Transform source)
        {
            aimSource = source;
        }

        private readonly Dictionary<GameObject, float> lastAttackTime = new Dictionary<GameObject, float>();
        private readonly Dictionary<GameObject, int> comboIndexByUser = new Dictionary<GameObject, int>();
        private readonly Dictionary<GameObject, float> lastComboTime = new Dictionary<GameObject, float>();

        protected static readonly Collider[] MeleeHitBuffer = new Collider[16];

        public override bool Execute(GameObject user)
        {
            if (!CanAttack(user)) return false;

            TargetingProfile targeting = user.GetComponent<TargetingProfile>();
            if (targeting == null)
            {
                Debug.LogError($"[MeleeAbilitySO] {user.name} does not have a TargetingProfile!", user);
                return false;
            }

            int comboIndex = 0;
            if (enableCombos)
            {
                if (!lastComboTime.TryGetValue(user, out float last) || Time.time > last + comboResetTime)
                {
                    comboIndexByUser[user] = 0;
                }
                comboIndex = comboIndexByUser.GetValueOrDefault(user, 0);
            }

            var effectManager = user.GetComponent<StatusEffectManager>();
            var criticalBuff = effectManager != null ? effectManager.FindEffect(typeof(CriticalBuff)) : null;
            bool isCritical = criticalBuff != null;

            float finalDamage = damage;
            if (enableCombos && comboIndex < comboDamageMultipliers.Length)
            {
                finalDamage *= comboDamageMultipliers[comboIndex];
            }
            if (isCritical)
            {
                finalDamage *= criticalDamageMultiplier;
                effectManager.RemoveEffect(criticalBuff);
            }

            PerformMelee(user, finalDamage, targeting.DamageableLayers);

            lastAttackTime[user] = Time.time;
            if (enableCombos)
            {
                lastComboTime[user] = Time.time;
                comboIndexByUser[user] = (comboIndex + 1) % maxComboHits;
            }
            return true;
        }

        public abstract void PerformMelee(GameObject user, float finalDamage, LayerMask damageLayers);

        private bool CanAttack(GameObject user)
        {
            if (!lastAttackTime.TryGetValue(user, out float last)) return true;
            return Time.time >= last + attackCooldown;
        }

#if UNITY_EDITOR
        public void DrawGizmos(Transform userTransform)
        {
            if (userTransform == null) return;
            Transform source = (aimSource != null) ? aimSource : userTransform;

            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.2f);
            UnityEditor.Handles.DrawSolidArc(source.position, Vector3.up, source.forward, 360, range);

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireArc(source.position, Vector3.up, source.forward, 360, range);
        }
#endif
    }
}
