using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.InputSystem; // Necesario para el PlayerInput

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance { get; private set; }
    
    [Header("Referencias")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EvolutionUI evolutionUI;
    [SerializeField] private PlayerInput playerInput; // Arrastra tu componente PlayerInput aquí

    private PlayerAbilityHandler playerAbilityHandler;

    [Header("Configuración de Evolución")]
    [SerializeField] private int geneticMaterialNeeded = 100;
    
    [Tooltip("¡IMPORTANTE! Arrastra aquí SÓLO las habilidades de Nivel 1 (Base). Las mejoras se añadirán solas.")]
    [SerializeField] private List<AbilityGroup> baseAbilityGroups; 

    private int currentGeneticMaterial = 0;
    private List<AbilitySO> availableAbilityPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        
        // Asignación automática de PlayerInput si no está asignado
        if (playerInput == null)
        {
            playerInput = playerController.GetComponent<PlayerInput>();
        }
        
        playerAbilityHandler = playerController.GetComponent<PlayerAbilityHandler>();
        if (playerAbilityHandler == null) Debug.LogError("¡EvolutionManager no encontró PlayerAbilityHandler!");
        if (evolutionUI == null) Debug.LogError("¡EvolutionUI no asignada en el EvolutionManager!");
        if (playerInput == null) Debug.LogError("¡PlayerInput no asignado en el EvolutionManager! No se podrá cambiar de mapa.");
        
        availableAbilityPool = new List<AbilitySO>();
        foreach (var group in baseAbilityGroups)
        {
            availableAbilityPool.AddRange(group.abilities);
        }
    }

    // ... (AddGeneticMaterial no necesita cambios) ...
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
        
        // 1. Equipa la habilidad en el jugador
        playerAbilityHandler.EvolveAbility(selectedAbility.slot, selectedAbility); 

        // 2. Lógica de desbloqueo
        if (availableAbilityPool.Contains(selectedAbility))
        {
            availableAbilityPool.Remove(selectedAbility);
        }

        if (selectedAbility.nextLevelAbility != null)
        {
            availableAbilityPool.Add(selectedAbility.nextLevelAbility);
            Debug.Log($"¡Pool actualizado! Se desbloqueó {selectedAbility.nextLevelAbility.name}");
        }

        // 3. Reanuda el juego
        Time.timeScale = 1f;
        
        // --- ¡CORREGIDO! ---
        // Al reanudar, devolvemos el control a tu Action Map "Player"
        playerInput.SwitchCurrentActionMap("Player"); 
        
        evolutionUI.Hide(); 
    }

    private void ShowEvolutionUI()
    {
        // 1. Pausa el juego
        Time.timeScale = 0f; 
        
        // Inmediatamente cambiamos al Action Map "UI"
        playerInput.SwitchCurrentActionMap("UI"); 
        
        // 2. Prepara las opciones
        List<AbilitySO> options = GetRandomAbilities(3);
        
        if (options.Count == 0)
        {
            Debug.LogWarning("No hay más habilidades para ofrecer. Reanudando.");
            // Si no hay opciones, debemos revertir todo
            Time.timeScale = 1f;
            
            // --- ¡CORREGIDO! ---
            // También revertimos al Action Map "Player"
            playerInput.SwitchCurrentActionMap("Player"); 
            
            return;
        }
        
        // 3. Muestra la UI (esto también seleccionará el primer botón)
        evolutionUI.Show(options, this);
    }

    // ... (GetRandomAbilities no necesita cambios) ...
    private List<AbilitySO> GetRandomAbilities(int count)
    {
        List<AbilitySO> equippedAbilities = playerAbilityHandler.GetEquippedAbilities();
        List<AbilitySO> equippedBaseAbilities = new List<AbilitySO>();
        
        foreach (var ability in equippedAbilities)
        {
            equippedBaseAbilities.Add(ability.baseAbility ?? ability); 
        }

        List<AbilitySO> filteredPool = availableAbilityPool.Except(equippedBaseAbilities).ToList();
        
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