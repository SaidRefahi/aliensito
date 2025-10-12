using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee/MusculaturaBestial")]
public class MeleeMusculaturaBestialSO : MeleeAbilitySO
{
    [Header("Musculatura Bestial Settings")]
    [Tooltip("Probabilidad de aturdir con cada golpe (0.25 = 25%)")]
    [Range(0f, 1f)]
    public float stunChance = 0.25f;

    [Tooltip("Duración del aturdimiento en segundos.")]
    public float stunDuration = 1.5f;

    public override void PerformMelee(GameObject user)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        Collider[] hits = Physics.OverlapSphere(source.position, range, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;

            // Buscamos el gestor de efectos en el objetivo.
            StatusEffectManager effectManager = hit.GetComponent<StatusEffectManager>();
            
            // Aplicamos el daño de impacto siempre.
            if (hit.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(damage);
            }

            // Si el enemigo puede ser afectado y tenemos suerte...
            if (effectManager != null && Random.value <= stunChance)
            {
                // Creamos el objeto de efecto de aturdimiento.
                Stun stunEffect = new Stun(hit.gameObject, stunDuration);
                // Le pedimos al gestor del enemigo que lo aplique.
                effectManager.ApplyEffect(stunEffect);
            }
        }
    }
}