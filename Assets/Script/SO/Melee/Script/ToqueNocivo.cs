using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee/ToqueNocivo")]
public class MeleeToqueNocivoSO : MeleeAbilitySO
{
    [Header("Toque Nocivo Settings")]
    public float poisonDamagePerSecond = 2f;
    public float poisonDuration = 5f;

    public override void PerformMelee(GameObject user)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        Collider[] hits = Physics.OverlapSphere(source.position, range, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;

            // Buscamos el StatusEffectManager en el enemigo
            StatusEffectManager effectManager = hit.GetComponent<StatusEffectManager>();
            if (effectManager == null) continue; // Si no puede ser afectado, lo ignoramos

            if (hit.TryGetComponent<Health>(out Health health))
            {
                // 1. Daño de impacto
                health.TakeDamage(damage);

                // 2. Creamos el objeto del efecto de veneno
                Poison poisonEffect = new Poison(hit.gameObject, poisonDuration, poisonDamagePerSecond);
                
                // 3. Le pedimos al gestor del enemigo que lo aplique
                effectManager.ApplyEffect(poisonEffect);
            }
        }
    }
}