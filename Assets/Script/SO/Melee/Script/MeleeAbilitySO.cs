using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeAbilitySO : AbilitySO
{
    [Header("Core")]
    public float damage = 10f;
    public float range = 1.5f;

    [Header("Targeting")]
    public LayerMask targetLayers = ~0;
    public LayerMask ignoreLayers = 0;
    public Vector3 firePointOffset = default;
    [Tooltip("Half-angle in degrees of the forward arc where hits are valid (0 = only exact forward)")]
    public float forwardArcHalfAngle = 90f; // 180° cone by default

    // Optional external aim source (assign from PlayerController)
    [Tooltip("Optional transform used as origin/direction for attacks (assign from PlayerController).")]
    public Transform aimSource;

    [Header("Cooldown & Combo")]
    public float attackCooldown = 1f;
    public bool enableCombos = false;
    public int maxComboHits = 1;
    public float comboResetTime = 2f;
    public float[] comboDamageMultipliers = { 1f };

    // Per-user state keyed by user GameObject (ScriptableObject is shared)
    private readonly Dictionary<GameObject, float> lastAttackTime = new Dictionary<GameObject, float>();
    private readonly Dictionary<GameObject, int> comboIndexByUser = new Dictionary<GameObject, int>();
    private readonly Dictionary<GameObject, float> lastComboTime = new Dictionary<GameObject, float>();

    private void OnEnable()
    {
        if (maxComboHits <= 0) maxComboHits = 1;
        if (comboDamageMultipliers == null || comboDamageMultipliers.Length != maxComboHits)
        {
            System.Array.Resize(ref comboDamageMultipliers, maxComboHits);
            for (int i = 0; i < maxComboHits; i++)
                if (comboDamageMultipliers[i] == 0f)
                    comboDamageMultipliers[i] = 1f + (i * 0.1f);
        }
    }

    private void OnDisable()
    {
        lastAttackTime.Clear();
        comboIndexByUser.Clear();
        lastComboTime.Clear();
    }

    public override void Execute(GameObject user)
    {
        if (user == null) return;
        if (!CanAttack(user)) return;

        float now = Time.time;
        lastAttackTime[user] = now;

        if (enableCombos)
        {
            if (!lastComboTime.TryGetValue(user, out float last) || now > last + comboResetTime)
                comboIndexByUser[user] = 0;
            lastComboTime[user] = now;
        }
        else
        {
            comboIndexByUser[user] = 0;
        }

        int comboIdx = comboIndexByUser.ContainsKey(user) ? comboIndexByUser[user] : 0;
        float finalDamage = damage;
        if (enableCombos && comboIdx < comboDamageMultipliers.Length)
            finalDamage *= comboDamageMultipliers[comboIdx];

        PerformMelee(user, finalDamage);

        if (enableCombos)
            comboIndexByUser[user] = (comboIdx + 1) % Mathf.Max(1, maxComboHits);
    }

    // Allow PlayerController to set aim at runtime without requiring SO edit in inspector
    public virtual void SetAimSource(Transform t) => aimSource = t;

    protected void PerformMelee(GameObject user, float finalDamage)
    {
        // Prefer external aimSource if provided, otherwise use user.transform
        Transform source = (aimSource != null) ? aimSource : user.transform;

        // Origin in source space (allows aimSource to be a child with local offset)
        Vector3 origin = source.TransformPoint(firePointOffset);

        // Use source.forward as direction
        Vector3 forward = source.forward;

        // Broad-phase: sphere overlap using targetLayers
        Collider[] hits = Physics.OverlapSphere(origin, range, targetLayers);

        float cosThreshold = Mathf.Cos(Mathf.Deg2Rad * forwardArcHalfAngle);

        foreach (var hit in hits)
        {
            if (hit == null) continue;
            var targetGO = hit.gameObject;
            if (targetGO == user) continue;
            if (((1 << targetGO.layer) & ignoreLayers) != 0) continue;

            // Angle check: ensure target is within forward arc (optional, skip if arc >= 180)
            if (forwardArcHalfAngle < 180f)
            {
                Vector3 toTarget = (hit.ClosestPoint(origin) - origin).normalized;
                float dot = Vector3.Dot(forward, toTarget);
                if (dot < cosThreshold) continue;
            }

            var health = targetGO.GetComponent<Health>() ?? targetGO.GetComponentInParent<Health>() ?? targetGO.GetComponentInChildren<Health>();
            if (health != null)
            {
                health.TakeDamage(finalDamage);
            }
        }
    }

    public bool CanAttack(GameObject user)
    {
        if (user == null) return false;
        if (!lastAttackTime.TryGetValue(user, out float last)) return true;
        return Time.time >= last + attackCooldown;
    }

    public void ResetCombo(GameObject user)
    {
        if (user == null) return;
        comboIndexByUser[user] = 0;
        lastComboTime[user] = -999f;
    }

#if UNITY_EDITOR
    public void DrawGizmosFor(GameObject user)
    {
        if (user == null) return;

        Transform source = (aimSource != null) ? aimSource : user.transform;
        Vector3 origin = source.TransformPoint(firePointOffset);
        Vector3 forward = source.forward;

        // Solid translucent disc on XZ plane and wire disc
        UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.12f);
        UnityEditor.Handles.DrawSolidDisc(origin, Vector3.up, range);

        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(origin, Vector3.up, range);

        // Forward indicator
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawLine(origin, origin + forward * Mathf.Min(range, 1f));

        // Draw forward arc lines for visualization
        if (forwardArcHalfAngle < 180f)
        {
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.8f);
            for (int i = -1; i <= 1; i += 2)
            {
                float angle = forwardArcHalfAngle * i;
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                UnityEditor.Handles.DrawLine(origin, origin + dir.normalized * range);
            }
        }
    }
#endif
}