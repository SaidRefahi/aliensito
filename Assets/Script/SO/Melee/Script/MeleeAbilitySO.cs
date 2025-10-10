using UnityEngine;

public abstract class MeleeAbilitySO : AbilitySO, IAimable
{
    [Header("Core")]
    public float damage = 10f;
    public float range = 1.5f;

    [Header("Targeting")]
    public LayerMask targetLayers = ~0;
    
    public Transform aimSource { get; set; }

    public override void Execute(GameObject user)
    {
        Debug.Log($"Executing Melee Ability: {abilityName} from {user.name}");
        PerformMelee(user, damage);
    }

    protected void PerformMelee(GameObject user, float finalDamage)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        
        Collider[] hits = Physics.OverlapSphere(source.position, range, targetLayers);

        if (hits.Length == 0)
        {
            Debug.Log("Melee Attack: Missed. No colliders found in range.");
            return;
        }

        Debug.Log($"Melee Attack: Hit {hits.Length} colliders.");

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;

            if (hit.TryGetComponent<Health>(out Health health))
            {
                Debug.Log($"<color=green>Melee Attack: Dealing {finalDamage} damage to {hit.name}.</color>");
                health.TakeDamage(finalDamage);
            }
            else
            {
                Debug.Log($"Melee Attack: Hit {hit.name}, but it has no Health component.");
            }
        }
    }

#if UNITY_EDITOR
    public void DrawGizmos(Transform userTransform)
    {
        if (userTransform == null) return;
        UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.2f);
        UnityEditor.Handles.DrawSolidArc(userTransform.position, Vector3.up, userTransform.forward, 360, range);
    }
#endif
}