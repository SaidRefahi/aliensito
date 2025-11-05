using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance { get; private set; }
    
    [Header("Referencias")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EvolutionUI evolutionUI;

    private PlayerAbilityHandler playerAbilityHandler;

    [Header("Configuración de Evolución")]
    [SerializeField] private int geneticMaterialNeeded = 100;
    
    [Tooltip("¡IMPORTANTE! Arrastra aquí SÓLO las habilidades de Nivel 1 (Base). Las mejoras se añadirán solas.")]
    [SerializeField] private List<AbilityGroup> baseAbilityGroups; 

    private int currentGeneticMaterial = 0;

    // --- ¡NUEVO POOL DINÁMICO! ---
    /// <summary>
    /// Esta es la lista "viva" de habilidades que se pueden ofrecer.
    /// Se modifica en tiempo real a medida que el jugador evoluciona.
    /// </summary>
    private List<AbilitySO> availableAbilityPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        
        playerAbilityHandler = playerController.GetComponent<PlayerAbilityHandler>();
        if (playerAbilityHandler == null) Debug.LogError("¡EvolutionManager no encontró PlayerAbilityHandler!");
        if (evolutionUI == null) Debug.LogError("¡EvolutionUI no asignada en el EvolutionManager!");

        // --- INICIALIZAR EL POOL DINÁMICO ---
        availableAbilityPool = new List<AbilitySO>();
        foreach (var group in baseAbilityGroups)
        {
            availableAbilityPool.AddRange(group.abilities);
        }
        
        // --- ¡SUSCRIBIRSE AL NUEVO EVENTO! ---
        playerAbilityHandler.OnAbilityDepleted += HandleAbilityDepleted;
    }

    private void OnDestroy()
    {
        // Limpia la suscripción
        if (playerAbilityHandler != null)
        {
            playerAbilityHandler.OnAbilityDepleted -= HandleAbilityDepleted;
        }
    }

    /// <summary>
    /// Se activa cuando el PlayerAbilityHandler gasta una habilidad.
    /// </summary>
    private void HandleAbilityDepleted(AbilitySO depletedAbility)
    {
        if (depletedAbility.nextLevelAbility != null)
        {
            // 1. Quita la habilidad agotada (ej. "Toque Nocivo") del pool
            availableAbilityPool.Remove(depletedAbility);
            
            // 2. Añade la nueva habilidad (ej. "Toque Nocivo +1") al pool
            availableAbilityPool.Add(depletedAbility.nextLevelAbility);
            
            Debug.Log($"¡Pool de evolución actualizado! Se quitó {depletedAbility.name}, se añadió {depletedAbility.nextLevelAbility.name}");
        }
    }

    public void AddGeneticMaterial(int amount)
    {
        currentGeneticMaterial += amount;
        Debug.Log($"Material genético: {currentGeneticMaterial}/{geneticMaterialNeeded}");
        
        if (currentGeneticMaterial >= geneticMaterialNeeded)
        {
            currentGeneticMaterial -= geneticMaterialNeeded;
            ShowEvolutionUI();
        }
    }

    public void SelectEvolution(AbilitySO selectedAbility)
    {
        if (selectedAbility == null) return;
        Debug.Log($"Evolución seleccionada: {selectedAbility.abilityName}");
        
        playerAbilityHandler.EvolveAbility(selectedAbility.slot, selectedAbility); 
        Time.timeScale = 1f;
        evolutionUI.Hide(); 
    }

    private void ShowEvolutionUI()
    {
        Time.timeScale = 0f; 
        List<AbilitySO> options = GetRandomAbilities(3);
        
        if (options.Count == 0)
        {
            Debug.LogWarning("No hay más habilidades para ofrecer. Reanudando.");
            Time.timeScale = 1f;
            return;
        }
        
        evolutionUI.Show(options, this);
    }

    // --- MÉTODO DE SORTEO (MODIFICADO) ---
    private List<AbilitySO> GetRandomAbilities(int count)
    {
        // 1. Obtener las habilidades que el jugador YA tiene equipadas
        List<AbilitySO> equippedAbilities = playerAbilityHandler.GetEquippedAbilities();
        
        // (Opcional) Obtener las *bases* de las habilidades equipadas
        List<AbilitySO> equippedBaseAbilities = new List<AbilitySO>();
        foreach (var ability in equippedAbilities)
        {
            equippedBaseAbilities.Add(ability.baseAbility ?? ability); // Añade la base, o ella misma si es la base
        }

        // 2. Filtrar el pool:
        //    - Quita las habilidades que ya están equipadas
        //    - (Opcional) Quita las habilidades *base* de las que están equipadas
        List<AbilitySO> filteredPool = availableAbilityPool.Except(equippedBaseAbilities).ToList();
        
        // 3. Barajar y tomar 'count'
        return filteredPool.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
    }
}

// --- CLASE AUXILIAR (se mantiene) ---
[System.Serializable]
public class AbilityGroup
{
    public string groupName;
    public List<AbilitySO> abilities;
}