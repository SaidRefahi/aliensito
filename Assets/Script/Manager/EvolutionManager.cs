using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class EvolutionManager : MonoBehaviour
{
    [Header("Configuración de Evolución")]
    [SerializeField] private int materialNeeded = 100;
    private int currentMaterial = 0;

    [Header("Pool de Evoluciones")]
    [Tooltip("Arrastra aquí todos los ScriptableObjects de las posibles evoluciones.")]
    [SerializeField] private List<AbilitySO> evolutionPool;
    
    private PlayerController playerController;

    public static event Action<List<AbilitySO>> OnEvolutionOptionsReady;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void AddGeneticMaterial(int amount)
    {
        currentMaterial += amount;
        Debug.Log($"Material genético: {currentMaterial}/{materialNeeded}");
        if (currentMaterial >= materialNeeded)
        {
            currentMaterial -= materialNeeded;
            PresentEvolutionOptions();
        }
    }

    private void PresentEvolutionOptions()
    {
        // El manager ya no pausa el juego. Solo prepara y envía las opciones.
        List<AbilitySO> offeredEvolutions = evolutionPool.OrderBy(x => Guid.NewGuid()).Take(3).ToList();
        OnEvolutionOptionsReady?.Invoke(offeredEvolutions);
    }

    public void SelectEvolution(AbilitySO chosenAbility)
    {
        if (chosenAbility == null || playerController == null) return;

        string slot;
        if (chosenAbility is MeleeAbilitySO) slot = "Melee";
        else if (chosenAbility is RangedAttackSO) slot = "Ranged";
        else if (chosenAbility is InvisibilitySO) slot = "Invisibility";
        else 
        {
            Debug.LogWarning($"Tipo de habilidad '{chosenAbility.GetType()}' no reconocido para asignar a un slot.");
            return;
        }

        playerController.EvolveAbility(slot, chosenAbility);
    }
}