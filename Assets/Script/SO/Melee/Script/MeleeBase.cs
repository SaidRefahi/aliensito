using UnityEngine;

[CreateAssetMenu(fileName = "Ataque Básico Melee", menuName = "Habilidades/Melee/Ataque Básico")]
public class BasicMeleeSO : MeleeAbilitySO
{
    public override void PerformMelee(GameObject user, float finalDamage)
    {
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
}