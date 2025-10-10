using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;
    
    // --- LÍNEA AÑADIDA ---
    [Header("Loot")]
    [Tooltip("Arrastra aquí el Prefab del material genético que debe soltar al morir.")]
    [SerializeField] private GameObject geneticMaterialPrefab;
    // ---------------------

    private float currentHealth;
    
    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;
    
    private bool hasDied = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (hasDied) return;

        float healthBeforeDamage = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        Debug.Log($"<color=red>{gameObject.name} took {damage} damage.</color> Health: {healthBeforeDamage} -> {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        hasDied = true;
        OnDeath?.Invoke();
        Debug.Log($"<color=grey>{gameObject.name} has died.</color>");

        // --- LÓGICA AÑADIDA ---
        // Si tiene un prefab de loot asignado, lo instancia en su posición.
        if (geneticMaterialPrefab != null)
        {
            Instantiate(geneticMaterialPrefab, transform.position, Quaternion.identity);
        }
        // ---------------------
        
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}