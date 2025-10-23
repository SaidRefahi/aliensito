using UnityEngine;

[CreateAssetMenu(fileName = "Toque Nocivo", menuName = "Habilidades/Melee/Toque Nocivo")]
public class ToqueNocivo : MeleeAbilitySO
{
    [Header("Toque Nocivo Settings")]
    public float poisonDamagePerSecond = 2f;
    public float poisonDuration = 5f;

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
                
                if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
                {
                    effectManager.ApplyEffect(new Poison(hit.gameObject, poisonDuration, poisonDamagePerSecond));
                }
            }
        }
    }
}