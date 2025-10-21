using UnityEngine;

[CreateAssetMenu(fileName = "Orgullo Racial", menuName = "Habilidades/Melee/Orgullo Racial")]
public class OrgulloRacialSO : MeleeAbilitySO
{
    // Como PerformMelee es 'abstract' en la clase base, estamos obligados a implementarlo.
    // La lógica es la misma que un ataque básico: aplicar el daño final a los enemigos.
    // El aumento de daño y rango se configura en el Inspector de este ScriptableObject.
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