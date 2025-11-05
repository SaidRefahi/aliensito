using UnityEngine;

[CreateAssetMenu(fileName = "Ataque Melee Básico", menuName = "Habilidades/Ataque Melee/Básico")]
public class MeleeBase : MeleeAbilitySO
{

    public override void PerformMelee(GameObject user, float finalDamage, LayerMask damageLayers)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        

        Collider[] hits = Physics.OverlapSphere(source.position, range, damageLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue; // No te pegues a ti mismo

            if (hit.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(finalDamage);
            }
        }
    }
}