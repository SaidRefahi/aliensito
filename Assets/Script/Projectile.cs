using UnityEngine;

public class Projectile : MonoBehaviour
{
    // --- LÓGICA DE CRÍTICO ---
    [Tooltip("El RangedAttackSO activará esto si el disparo es crítico.")]
    public bool isCritical = false;
    [Tooltip("Multiplicador de daño para los golpes críticos (2 = 200% de daño).")]
    public float criticalDamageMultiplier = 2f;
    // -------------------------

    [Header("Configuración de Impacto")]
    [Tooltip("Daño directo que inflige el proyectil al chocar.")]
    public float damage = 10f;
    [Tooltip("Capas de objetos que pueden recibir daño de este proyectil.")]
    public LayerMask damageableLayers;
    [Tooltip("Tiempo en segundos antes de que el proyectil se autodestruya.")]
    public float lifetime = 5f;

    [Header("Evolución: Saliva Ácida")]
    [Tooltip("Marcar si este proyectil debe aplicar veneno al impactar.")]
    public bool applyPoison = false;
    [Tooltip("Daño por segundo que inflige el veneno.")]
    public float poisonDamagePerSecond = 2f;
    [Tooltip("Duración total del veneno en segundos.")]
    public float poisonDuration = 5f;

    [Header("Evolución: Todos Caen")]
    [Tooltip("Marcar si este proyectil debe aplicar vulnerabilidad al impactar.")]
    public bool applyVulnerability = false;
    [Tooltip("Multiplicador de daño que el enemigo recibirá mientras sea vulnerable (1.5 = 150% de daño).")]
    public float damageMultiplier = 1.5f;
    [Tooltip("Duración de la vulnerabilidad en segundos.")]
    public float vulnerabilityDuration = 5f;

    private void Start()
    {
        // El proyectil se autodestruye después de 'lifetime' segundos si no choca con nada.
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el objeto con el que chocó está en una capa dañable.
        if ((damageableLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            StatusEffectManager effectManager = other.GetComponent<StatusEffectManager>();
            if (other.TryGetComponent<Health>(out Health health))
            {
                // --- LÓGICA DE DAÑO FINAL (INCLUYE CRÍTICO) ---
                float finalDamage = isCritical ? damage * criticalDamageMultiplier : damage;
                if (isCritical)
                {
                    Debug.Log("<color=orange>CRITICAL SHOT!</color>");
                }
                // ------------------------------------------------

                // Aplica el daño final al enemigo.
                health.TakeDamage(finalDamage);

                // Aplica otros efectos si el enemigo tiene un gestor de efectos.
                if (effectManager != null)
                {
                    if (applyPoison)
                    {
                        effectManager.ApplyEffect(new Poison(other.gameObject, poisonDuration, poisonDamagePerSecond));
                    }
                    if (applyVulnerability)
                    {
                        effectManager.ApplyEffect(new Vulnerability(other.gameObject, vulnerabilityDuration, damageMultiplier));
                    }
                }
            }
            // El proyectil se destruye al impactar.
            Destroy(gameObject);
        }
    }
}