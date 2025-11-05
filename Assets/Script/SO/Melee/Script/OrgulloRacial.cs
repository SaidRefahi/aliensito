using UnityEngine;

[CreateAssetMenu(fileName = "Orgullo Racial", menuName = "Habilidades/Melee/Orgullo Racial")]
public class OrgulloRacialSO : MeleeAbilitySO
{
    // --- ¡FIRMA MODIFICADA! ---
    public override void PerformMelee(GameObject user, float finalDamage, LayerMask damageLayers)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        
        // --- ¡LÍNEA MODIFICADA! ---
        Collider[] hits = Physics.OverlapSphere(source.position, range, damageLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;
            if (hit.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(finalDamage);
            }
        }
    }
}