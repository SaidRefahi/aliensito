using UnityEngine;

public class PoisonAura : MonoBehaviour
{
    public float radius;
    public float poisonDamage;
    public float poisonDuration;
    public LayerMask enemyLayers;
    private float tickTimer;

    void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= 1f) // Aplica el veneno a los enemigos cercanos cada segundo.
        {
            tickTimer -= 1f;
            ApplyPoisonToNearby();
        }
    }

    void ApplyPoisonToNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
            {
                effectManager.ApplyEffect(new Poison(hit.gameObject, poisonDuration, poisonDamage));
            }
        }
    }
}