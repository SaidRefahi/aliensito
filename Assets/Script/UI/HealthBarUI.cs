using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health healthComponent;
    [SerializeField] private Image healthFill;
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionSpeed = 2f;
    
    private float targetFillAmount;

    private void Start()
    {
        Debug.Log("HealthBarUI Start() called");
        
        if (healthComponent != null)
        {
            UpdateHealthBar(healthComponent.GetCurrentHealth(), healthComponent.GetMaxHealth());
        }
        else
        {
            Debug.LogError("Health component is NULL in Start()!");
        }
        
        if (healthFill == null)
        {
            Debug.LogError("healthFill is NULL in Start()!");
        }
    }

    private void Update()
    {
        if (smoothTransition && healthFill != null)
        {
            healthFill.fillAmount = Mathf.MoveTowards(healthFill.fillAmount, targetFillAmount, transitionSpeed * Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged += OnHealthChanged;
            healthComponent.OnDeath += OnEntityDeath;
        }
    }

    private void OnDisable()
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= OnHealthChanged;
            healthComponent.OnDeath -= OnEntityDeath;
        }
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFill == null) return;
        
        float ratio = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        if (smoothTransition)
        {
            targetFillAmount = ratio;
        }
        else
        {
            healthFill.fillAmount = ratio;
        }
    }

    private void OnEntityDeath()
    {
        Debug.Log($"Entity {healthComponent.gameObject.name} died - Health bar updated");
    }

    public void SetHealthComponent(Health health)
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= OnHealthChanged;
            healthComponent.OnDeath -= OnEntityDeath;
        }
        
        healthComponent = health;
        
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged += OnHealthChanged;
            healthComponent.OnDeath += OnEntityDeath;
            UpdateHealthBar(healthComponent.GetCurrentHealth(), healthComponent.GetMaxHealth());
        }
    }

    public bool HasHealthComponent => healthComponent != null;
    
    public void ForceUpdateHealthBar()
    {
        if (healthComponent != null)
        {
            UpdateHealthBar(healthComponent.GetCurrentHealth(), healthComponent.GetMaxHealth());
        }
    }
}