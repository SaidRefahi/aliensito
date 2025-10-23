using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class MeleeAbilitySO : AbilitySO, IAimable
{
    [Header("Configuración Melee")]
    public float damage = 15f;
    public float range = 2f;
    public LayerMask targetLayers;
    public float criticalDamageMultiplier = 2f;

    [Header("Configuración de Cooldown y Combo")]
    public float attackCooldown = 0.5f;
    public bool enableCombos = false;
    public int maxComboHits = 3;
    public float comboResetTime = 1.5f;
    public float[] comboDamageMultipliers = { 1f, 1.2f, 1.5f };

    public Transform aimSource { get; set; }

    private readonly Dictionary<GameObject, float> lastAttackTime = new Dictionary<GameObject, float>();
    private readonly Dictionary<GameObject, int> comboIndexByUser = new Dictionary<GameObject, int>();
    private readonly Dictionary<GameObject, float> lastComboTime = new Dictionary<GameObject, float>();

    public override void Execute(GameObject user)
    {
        if (!CanAttack(user)) return;

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
        var criticalBuff = effectManager?.FindEffect(typeof(CriticalBuff));
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
        
        PerformMelee(user, finalDamage);
        
        lastAttackTime[user] = Time.time;
        if (enableCombos)
        {
            lastComboTime[user] = Time.time;
            comboIndexByUser[user] = (comboIndex + 1) % maxComboHits;
        }
    }

    public abstract void PerformMelee(GameObject user, float finalDamage);

    private bool CanAttack(GameObject user)
    {
        if (!lastAttackTime.TryGetValue(user, out float last)) return true;
        return Time.time >= last + attackCooldown;
    }

    // --- EL MÉTODO DrawGizmos DEBE ESTAR AQUÍ DENTRO ---
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