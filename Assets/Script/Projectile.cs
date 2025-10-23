// Ruta: Assets/Script/Projectile.cs
// ACCIÓN: Reemplaza tu script existente con esta versión.
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool isCritical = false;
    public float criticalDamageMultiplier = 2f;
    
    [Header("Configuración de Impacto")]
    public float damage = 10f;
    public LayerMask damageableLayers;
    public float lifetime = 5f;

    [Header("Evolución: Saliva Ácida")]
    public bool applyPoison = false;
    public float poisonDamagePerSecond = 2f;
    public float poisonDuration = 5f;

    [Header("Evolución: Todos Caen")]
    public bool applyVulnerability = false;
    public float damageMultiplier = 1.5f;
    public float vulnerabilityDuration = 5f;

    private void OnEnable()
    {
        // Programa la desactivación automática si el proyectil no choca.
        Invoke(nameof(Deactivate), lifetime);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Cancela la invocación si se desactiva por un choque.
        CancelInvoke();
        isCritical = false; // Resetea el estado para la próxima vez que se use.
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((damageableLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            StatusEffectManager effectManager = other.GetComponent<StatusEffectManager>();
            if (other.TryGetComponent<Health>(out Health health))
            {
                float finalDamage = isCritical ? damage * criticalDamageMultiplier : damage;
                health.TakeDamage(finalDamage);

                if (effectManager != null)
                {
                    if (applyPoison)
                        effectManager.ApplyEffect(new Poison(other.gameObject, poisonDuration, poisonDamagePerSecond));
                    if (applyVulnerability)
                        effectManager.ApplyEffect(new Vulnerability(other.gameObject, vulnerabilityDuration, damageMultiplier));
                }
            }
            // En lugar de destruir, lo desactivamos para devolverlo al pool.
            gameObject.SetActive(false);
        }
    }
}