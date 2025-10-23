using UnityEngine;

[CreateAssetMenu(fileName = "Orgullo Racial", menuName = "Habilidades/Melee/Orgullo Racial")]
public class OrgulloRacialSO : MeleeAbilitySO
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