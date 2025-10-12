using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;
    [Header("Loot")]
    [SerializeField] private GameObject geneticMaterialPrefab;

    private float currentHealth;
    private bool hasDied = false;
    private float damageMultiplier = 1f; // Para el efecto de vulnerabilidad

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float baseDamage)
    {
        if (hasDied) return;
        float finalDamage = baseDamage * damageMultiplier;
        currentHealth = Mathf.Clamp(currentHealth - finalDamage, 0f, maxHealth);
        Debug.Log($"<color=red>{gameObject.name} took {finalDamage} damage.</color> Health: {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0f) Die();
    }

    public void SetDamageMultiplier(float multiplier)
    {
        this.damageMultiplier = multiplier;
    }
    
    // El resto de tus métodos (Heal, Die, Getters...)
    public void Heal(float amount)
    {
        if (hasDied) return;
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