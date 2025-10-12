using UnityEngine;
using System.Linq; // Necesario para LINQ

public abstract class MeleeAbilitySO : AbilitySO, IAimable
{
    [Header("Melee Settings")]
    public float damage = 15f;
    public float range = 2f;
    public LayerMask targetLayers;
    public float criticalDamageMultiplier = 2f; // Daño x2 en un crítico

    public Transform aimSource { get; set; }

    public override void Execute(GameObject user)
    {
        PerformMelee(user);
    }

    public virtual void PerformMelee(GameObject user)
    {
        // --- LÓGICA DE CRÍTICO ---
        var effectManager = user.GetComponent<StatusEffectManager>();
        var criticalBuff = effectManager?.FindEffect(typeof(CriticalBuff));
        bool isCritical = criticalBuff != null;

        float finalDamage = isCritical ? damage * criticalDamageMultiplier : damage;

        if (isCritical)
        {
            Debug.Log("<color=orange>CRITICAL HIT!</color>");
            // Consumimos el buff para que solo se use una vez.
            effectManager.RemoveEffect(criticalBuff);
        }
        // -------------------------

        Transform source = (aimSource != null) ? aimSource : user.transform;
        Collider[] hits = Physics.OverlapSphere(source.position, range, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;
            if (hit.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(finalDamage);
            }
        }
    }

    // --- BLOQUE DE GIZMOS MOVIDO AQUÍ DENTRO ---
#if UNITY_EDITOR
    public void DrawGizmos(Transform userTransform)
    {
        if (userTransform == null) return;
        
        // Usamos aimSource si existe para que el gizmo se dibuje en el lugar correcto
        Transform source = (aimSource != null) ? aimSource : userTransform;

        UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.2f);
        UnityEditor.Handles.DrawSolidArc(source.position, Vector3.up, source.forward, 360, range);
        
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireArc(source.position, Vector3.up, source.forward, 360, range);
    }
#endif
}