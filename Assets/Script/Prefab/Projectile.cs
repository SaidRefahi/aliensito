using UnityEngine;

public class Projectile : MonoBehaviour
{
    public string poolTag; 
    
    public bool isCritical = false;
    public float criticalDamageMultiplier = 2f;
    
    [Header("Configuración de Impacto")]
    public float damage = 10f;
    public LayerMask damageableLayers; 
    
    [Tooltip("Capas que destruyen el proyectil SIN hacerles daño (ej. Paredes)")] // <--- ¡NUEVO!
    public LayerMask obstacleLayers; // <--- ¡NUEVO!
    
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
        Invoke(nameof(ReturnToPool), lifetime);
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeSelf) return;

        if (string.IsNullOrEmpty(poolTag))
        {
            Debug.LogWarning("¡Projectile no tiene poolTag! Usando SetActive(false).");
            gameObject.SetActive(false);
        }
        else
        {
            PoolManager.Instance.ReturnToPool(poolTag, gameObject);
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
        isCritical = false; 
    }

    // --- ¡MÉTODO MODIFICADO! ---
    private void OnTriggerEnter(Collider other)
    {
        // Guardamos la capa del objeto para una comprobación más limpia
        int otherLayer = 1 << other.gameObject.layer; // <--- NUEVO

        // --- Comprobación 1: ¿Es una capa DAÑABLE? ---
        if ((damageableLayers.value & otherLayer) != 0) // <--- MODIFICADO
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
            return; // ¡Importante! Salimos del método
        }
        
        // --- Comprobación 2: ¿Es una capa de OBSTÁCULO? ---
        // (Solo se ejecuta si la Comprobación 1 falló)
        if ((obstacleLayers.value & otherLayer) != 0) // <--- ¡NUEVO!
        {
            // No hacemos daño, no aplicamos efectos.
            // Simplemente volvemos al pool.
            ReturnToPool();
        }
    }
}