using UnityEngine;

using UnityEngine;

[CreateAssetMenu(fileName = "Toque Nocivo", menuName = "Habilidades/Melee/Toque Nocivo")]
public class MeleeToqueNocivoSO : MeleeAbilitySO
{
    [Header("Toque Nocivo Settings")]
    public float poisonDamagePerSecond = 2f;
    public float poisonDuration = 5f;

    // Sobrescribimos el método que recibe el daño ya calculado por la clase base.
    public override void PerformMelee(GameObject user, float finalDamage)
    {
        Transform source = (aimSource != null) ? aimSource : user.transform;
        Collider[] hits = Physics.OverlapSphere(source.position, range, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue;

            if (hit.TryGetComponent<Health>(out Health health))
            {
                // 1. Aplica el daño de impacto final (calculado en la clase base).
                health.TakeDamage(finalDamage);
                
                // 2. Aplica el efecto de veneno.
                if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
                {
                    effectManager.ApplyEffect(new Poison(hit.gameObject, poisonDuration, poisonDamagePerSecond));
                }
            }
        }
    }
}