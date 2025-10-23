using UnityEngine;

[CreateAssetMenu(fileName = "Musculatura Bestial", menuName = "Habilidades/Melee/Musculatura Bestial")]
public class MusculaturaBestial : MeleeAbilitySO
{
    [Header("Musculatura Bestial Settings")]
    [Range(0f, 1f)] public float stunChance = 0.25f;
    public float stunDuration = 1.5f;

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
            if (Random.value <= stunChance)
            {
                if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
                {
                    effectManager.ApplyEffect(new Stun(hit.gameObject, stunDuration));
                }
            }
        }
    }
}