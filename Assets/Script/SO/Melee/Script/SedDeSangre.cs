using UnityEngine;

[CreateAssetMenu(fileName = "Sed de Sangre", menuName = "Habilidades/Melee/Sed de Sangre")]
public class SedDeSangre : MeleeAbilitySO
{
    [Header("Sed de Sangre Settings")]
    [Range(0, 1)] public float healPercent = 0.1f;

    // --- ¡FIRMA MODIFICADA! ---
    public override void PerformMelee(GameObject user, float finalDamage, LayerMask damageLayers)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        
        // --- ¡LÍNEA MODIFICADA! ---
        Collider[] hits = Physics.OverlapSphere(source.position, range, damageLayers);
        
        if (!user.TryGetComponent<Health>(out var userHealth)) return;

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;
            if (hit.TryGetComponent<Health>(out Health targetHealth))
            {
                targetHealth.TakeDamage(finalDamage);
                userHealth.Heal(finalDamage * healPercent);
            }
        }
    }
}