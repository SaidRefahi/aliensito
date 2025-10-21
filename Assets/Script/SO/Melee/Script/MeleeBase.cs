using UnityEngine;

[CreateAssetMenu(fileName = "Ataque Básico Melee", menuName = "Habilidades/Melee/Ataque Básico")]
public class BasicMeleeSO : MeleeAbilitySO
{
    // Como PerformMelee es 'abstract' en la clase base, estamos obligados a implementarlo.
    public override void PerformMelee(GameObject user, float finalDamage)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        Collider[] hits = Physics.OverlapSphere(source.position, range, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;

            if (hit.TryGetComponent<Health>(out Health health))
            {
                // Aplica el daño final (que ya incluye combos y críticos).
                health.TakeDamage(finalDamage);
            }
        }
    }
}