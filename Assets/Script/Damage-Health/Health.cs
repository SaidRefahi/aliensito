using UnityEngine;
using System;

[RequireComponent(typeof(StatusEffectManager))]
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;
    [Header("Loot")]
    [SerializeField] private GameObject geneticMaterialPrefab;

    // --- ¡NUEVAS LÍNEAS! ---
    [Header("Audio")]
    [Tooltip("Nombre del SFX en el AudioManager al recibir daño")]
    [SerializeField] private string hitSoundName;
    [Tooltip("Nombre del SFX en el AudioManager al morir")]
    [SerializeField] private string deathSoundName;
    // --- FIN DE LÍNEAS NUEVAS ---

    private float currentHealth;
    private bool hasDied = false;
    private float damageMultiplier = 1f;
    private HitFeedback hitFeedback; 

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;
    public event Action OnHit; 

    private void Awake()
    {
        currentHealth = maxHealth;
        hitFeedback = GetComponent<HitFeedback>();
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float baseDamage)
    {
        if (hasDied) return;

        float finalDamage = baseDamage * damageMultiplier;
        currentHealth = Mathf.Clamp(currentHealth - finalDamage, 0f, maxHealth);
        
        if(finalDamage > 0)
        {
            Debug.Log($"<color=red>{gameObject.name} took {finalDamage} damage.</color> Health: {currentHealth}/{maxHealth}");
            OnHit?.Invoke(); 
            
            // --- ¡NUEVA LÍNEA! ---
            // Llama al AudioManager si el nombre del sonido no está vacío
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(hitSoundName))
            {
                AudioManager.Instance.PlaySFX(hitSoundName);
            }
            // --- FIN DE LÍNEA NUEVA ---
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (hitFeedback != null)
        {
            hitFeedback.PlayEffect();
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void SetDamageMultiplier(float multiplier)
    {
        this.damageMultiplier = Mathf.Max(0, multiplier);
    }

    public void Heal(float amount)
    {
        if (hasDied || amount <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if(hasDied) return;
        hasDied = true;
        
        // --- ¡NUEVA LÍNEA! ---
        // Llama al AudioManager para el sonido de muerte
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(deathSoundName))
        {
            AudioManager.Instance.PlaySFX(deathSoundName);
        }
        // --- FIN DE LÍNEA NUEVA ---
        
        OnDeath?.Invoke();
        if (geneticMaterialPrefab != null) Instantiate(geneticMaterialPrefab, transform.position, Quaternion.identity);
        if (destroyOnDeath) Destroy(gameObject);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => hasDied;
}