using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class EvolutionManager : MonoBehaviour
{
    [Header("Configuración de Evolución")]
    [SerializeField] private int materialNeeded = 100;
    private int currentMaterial = 0;
    private bool isEvolving = false;

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
        if (isEvolving) return;
        currentMaterial += amount;
        if (currentMaterial >= materialNeeded)
        {
            isEvolving = true;
            PresentEvolutionOptions();
        }
    }

    private void PresentEvolutionOptions()
    {
        List<AbilitySO> offeredEvolutions = evolutionPool.OrderBy(x => Guid.NewGuid()).Take(3).ToList();
        OnEvolutionOptionsReady?.Invoke(offeredEvolutions);
    }

    // --- MÉTODO SELECTEVOLUTION CORREGIDO Y SIMPLIFICADO ---
    public void SelectEvolution(AbilitySO chosenAbility)
    {
        currentMaterial -= materialNeeded;

        if (chosenAbility != null && playerController != null)
        {
            // Ya no hay 'if' ni 'is'. Simplemente leemos el slot de la propia habilidad.
            // Esto funcionará para 'Egocentrismo' y cualquier otra habilidad futura.
            playerController.EvolveAbility(chosenAbility.slot, chosenAbility);
        }
        
        isEvolving = false;
    }
}