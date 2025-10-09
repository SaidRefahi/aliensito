using UnityEngine;
using System;

[System.Serializable]
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool destroyOnDeath = true;
    
    public static event Action<GameObject> OnActorDeath;
    
    // Nuevos eventos para el sistema de UI
    public event Action<float, float> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action OnDeath;
    
    private bool hasDied = false;

    private void Start()
    {
        currentHealth = maxHealth;
        // Notificar el valor inicial
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    //

    public void TakeDamage(float damage)
    {
        Debug.Log($"[v0] {gameObject.name} took {damage} damage. Health: {currentHealth} -> {currentHealth - damage}");
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        // Notificar cambio de salud
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        CheckDeath();
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        // Notificar cambio de salud
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    private void CheckDeath()
    {
        if (currentHealth <= 0f && !hasDied)
        {
            Debug.Log($"[v0] {gameObject.name} has died!");
            
            hasDied = true;
            OnDeath?.Invoke();
            OnActorDeath?.Invoke(gameObject);
            
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}