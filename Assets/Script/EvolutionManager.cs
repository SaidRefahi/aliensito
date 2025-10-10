using UnityEngine;
using System;

public class EvolutionManager : MonoBehaviour
{
    [Header("Genetic Material")]
    [SerializeField] private int currentGeneticMaterial = 0;
    [SerializeField] private int materialNeededForEvolution = 100;

    public static event Action<int> OnGeneticMaterialChanged;
    public static event Action OnEvolutionReady;

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        OnGeneticMaterialChanged?.Invoke(currentGeneticMaterial);
    }

    public void AddGeneticMaterial(int amount)
    {
        currentGeneticMaterial += amount;
        Debug.Log($"Material genético recolectado: {amount}. Total: {currentGeneticMaterial}");

        OnGeneticMaterialChanged?.Invoke(currentGeneticMaterial);

        if (currentGeneticMaterial >= materialNeededForEvolution)
        {
            OnEvolutionReady?.Invoke();
            Debug.Log("¡Evolución disponible!");
        }
    }

    public void Evolve(string abilityName, AbilitySO newAbility)
    {
        if (currentGeneticMaterial < materialNeededForEvolution)
        {
            Debug.LogWarning("No hay suficiente material genético para evolucionar.");
            return;
        }

        if (playerController != null)
        {
            currentGeneticMaterial -= materialNeededForEvolution;
            OnGeneticMaterialChanged?.Invoke(currentGeneticMaterial);

            playerController.EvolveAbility(abilityName, newAbility);
        }
    }
}