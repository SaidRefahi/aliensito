using UnityEngine;

public class Projectile : MonoBehaviour
{
    public string poolTag; 
    
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
        // Programa la devolución automática si el proyectil no choca.
        // Usamos el nombre del nuevo método.
        Invoke(nameof(ReturnToPool), lifetime);
    }

    // Este método ahora se encarga de llamar al PoolManager
    private void ReturnToPool()
    {
        // Previene llamadas dobles (ej. si choca y el lifetime se cumple a la vez)
        if (!gameObject.activeSelf) return;

        if (string.IsNullOrEmpty(poolTag))
        {
            // Fallback: si no tiene tag, solo se desactiva (evita errores)
            Debug.LogWarning("¡Projectile no tiene poolTag! Usando SetActive(false).");
            gameObject.SetActive(false);
        }
        else
        {
            // Llamamos al método correcto del PoolManager
            PoolManager.Instance.ReturnToPool(poolTag, gameObject);
        }
    }

    private void OnDisable()
    {
        // Cancela la invocación si se desactiva por un choque.
        CancelInvoke();
        
        // Resetea el estado (¡esto ya lo hacías y es perfecto!)
        isCritical = false; 
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
            ReturnToPool();
        }
    }
}