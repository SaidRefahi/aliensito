using UnityEngine;

[CreateAssetMenu(fileName = "Sed de Sangre", menuName = "Habilidades/Melee/Sed de Sangre")]
public class MeleeSedDeSangreSO : MeleeAbilitySO
{
    [Header("Sed de Sangre Settings")]
    [Tooltip("Porcentaje del daño infligido que te curas (0.1 = 10%)")]
    [Range(0, 1)]
    public float healPercent = 0.1f;

    // Sobrescribimos el método que recibe el daño final de la clase base.
    public override void PerformMelee(GameObject user, float finalDamage)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        Collider[] hits = Physics.OverlapSphere(source.position, range, targetLayers);
        
        Health userHealth = user.GetComponent<Health>();
        if (userHealth == null) return;

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;

            if (hit.TryGetComponent<Health>(out Health targetHealth))
            {
                // 1. Inflige el daño final (ya calculado con combos/críticos).
                targetHealth.TakeDamage(finalDamage);

                // 2. Calcula la curación basada en ese daño final.
                float healAmount = finalDamage * healPercent;
                
                // 3. Cúrate a ti mismo.
                userHealth.Heal(healAmount);
            }
        }
    }
}