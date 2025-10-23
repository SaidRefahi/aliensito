using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class EvolutionManager : MonoBehaviour
{
    [Header("Configuración de Evolución")]
    [SerializeField] private int materialNeeded = 100;
    private int currentMaterial = 0;
    private bool isEvolving = false; // Bandera para evitar abrir el menú múltiples veces

    [Header("Pool de Evoluciones")]
    [SerializeField] private List<AbilitySO> evolutionPool;
    
    private PlayerController playerController;
    public static event Action<List<AbilitySO>> OnEvolutionOptionsReady;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void AddGeneticMaterial(int amount)
    {
        if (isEvolving) return; // Si ya estamos en el proceso de evolucionar, no hacemos nada

        currentMaterial += amount;
        Debug.Log($"Material genético: {currentMaterial}/{materialNeeded}");

        if (currentMaterial >= materialNeeded)
        {
            isEvolving = true; // Bloqueamos la posibilidad de volver a entrar
            PresentEvolutionOptions();
            // --- YA NO RESTAMOS EL MATERIAL AQUÍ ---
        }
    }

    private void PresentEvolutionOptions()
    {
        List<AbilitySO> offeredEvolutions = evolutionPool.OrderBy(x => Guid.NewGuid()).Take(3).ToList();
        OnEvolutionOptionsReady?.Invoke(offeredEvolutions);
    }

    public void SelectEvolution(AbilitySO chosenAbility)
    {
        // --- RESTAMOS EL MATERIAL AQUÍ, DESPUÉS DE LA SELECCIÓN ---
        currentMaterial -= materialNeeded;

        if (chosenAbility == null || playerController == null)
        {
            isEvolving = false; // Desbloqueamos
            return;
        }

        string slot;
        if (chosenAbility is MeleeAbilitySO) slot = "Melee";
        else if (chosenAbility is RangedAttackSO) slot = "Ranged";
        else if (chosenAbility is InvisibilitySO) slot = "Invisibility";
        else 
        {
            Debug.LogWarning($"Tipo de habilidad '{chosenAbility.GetType()}' no reconocido.");
            isEvolving = false; // Desbloqueamos
            return;
        }

        playerController.EvolveAbility(slot, chosenAbility);
        isEvolving = false; // Desbloqueamos para la siguiente evolución
    }
}