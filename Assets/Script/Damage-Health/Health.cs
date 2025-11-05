using UnityEngine;
using System;

[RequireComponent(typeof(StatusEffectManager))] // Requerimos el manager
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;
    [Header("Loot")]
    [SerializeField] private GameObject geneticMaterialPrefab;

    private float currentHealth;
    private bool hasDied = false;
    private float damageMultiplier = 1f;
    private HitFeedback hitFeedback; 

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
        // --- NUEVA LÍNEA ---
        // Obtenemos el componente al despertar
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
            Debug.Log($"<color=red>{gameObject.name} took {finalDamage} damage.</color> Health: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // --- LÍNEA CLAVE AÑADIDA ---
        // Si el efecto existe, lo reproducimos.
        if (hitFeedback != null)
        {
            hitFeedback.PlayEffect();
        }
        // ---------------------------

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
        OnDeath?.Invoke();
        if (geneticMaterialPrefab != null) Instantiate(geneticMaterialPrefab, transform.position, Quaternion.identity);
        if (destroyOnDeath) Destroy(gameObject);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => hasDied;
}