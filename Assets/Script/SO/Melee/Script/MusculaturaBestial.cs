using UnityEngine;

[CreateAssetMenu(fileName = "Musculatura Bestial", menuName = "Habilidades/Melee/Musculatura Bestial")]
public class MeleeMusculaturaBestialSO : MeleeAbilitySO
{
    [Header("Musculatura Bestial Settings")]
    [Tooltip("Probabilidad de aturdir con cada golpe (0.25 = 25%)")]
    [Range(0f, 1f)]
    public float stunChance = 0.25f;

    [Tooltip("Duración del aturdimiento en segundos.")]
    public float stunDuration = 1.5f;

    public override void PerformMelee(GameObject user, float finalDamage)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        Collider[] hits = Physics.OverlapSphere(source.position, range, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;

            // 1. Aplica el daño final que nos pasa la clase base.
            if (hit.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(finalDamage);
            }

            // 2. Comprueba la probabilidad y aplica el aturdimiento.
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