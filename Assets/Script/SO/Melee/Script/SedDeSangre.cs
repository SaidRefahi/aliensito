using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee/SedDeSangre")]
public class MeleeSedDeSangreSO : MeleeAbilitySO
{
    [Header("Sed de Sangre Settings")]
    [Tooltip("Porcentaje del daño infligido que te curas (0.1 = 10%)")]
    [Range(0, 1)]
    public float healPercent = 0.1f;

    // Sobrescribimos el método PerformMelee para añadir la lógica de curación.
    public override void PerformMelee(GameObject user)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        Collider[] hits = Physics.OverlapSphere(source.position, range, targetLayers);
        
        // Obtenemos la vida del jugador una sola vez para eficiencia.
        Health userHealth = user.GetComponent<Health>();
        if (userHealth == null) return;

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;

            if (hit.TryGetComponent<Health>(out Health targetHealth))
            {
                // 1. Inflige daño al enemigo.
                targetHealth.TakeDamage(damage);

                // 2. Calcula la curación basada en el daño.
                float healAmount = damage * healPercent;
                
                // 3. Cúrate a ti mismo.
                userHealth.Heal(healAmount);
            }
        }
    }
}