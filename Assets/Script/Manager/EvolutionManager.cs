using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable]
    public class AbilityGroup
    {
        public string groupName;
        public List<AbilitySO> abilities;
    }

    public class EvolutionManager : MonoBehaviour, IEvolutionService
    {
        public static EvolutionManager Instance { get; private set; }

        public event Action<IReadOnlyList<AbilitySO>> OnEvolutionChoicesAvailable;
        public event Action OnEvolutionCompleted;
        public event Action<int, int> OnGeneticMaterialChanged;

        [Title("Dependencies")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerInput playerInput;

        [Title("Evolution Configuration")]
        [Min(1)]
        [SerializeField] private int geneticMaterialNeeded = 100;
        [SerializeField] private List<AbilityGroup> baseAbilityGroups = new List<AbilityGroup>();

        private PlayerAbilityHandler playerAbilityHandler;
        private int currentGeneticMaterial = 0;
        private readonly List<AbilitySO> availableAbilityPool = new List<AbilitySO>();
        private readonly List<AbilitySO> candidateBuffer = new List<AbilitySO>();
        private readonly List<AbilitySO> resultBuffer = new List<AbilitySO>();
        private readonly HashSet<AbilitySO> equippedBaseSet = new HashSet<AbilitySO>();

        public int CurrentGeneticMaterial => currentGeneticMaterial;
        public int TargetGeneticMaterial => geneticMaterialNeeded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ResolveDependencies();
            InitializeAbilityPool();
        }

        private void ResolveDependencies()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }

            if (playerController != null)
            {
                if (playerInput == null)
                {
                    playerInput = playerController.GetComponent<PlayerInput>();
                }
                playerAbilityHandler = playerController.GetComponent<PlayerAbilityHandler>();
            }
        }

        private void InitializeAbilityPool()
        {
            availableAbilityPool.Clear();
            for (int i = 0; i < baseAbilityGroups.Count; i++)
            {
                var group = baseAbilityGroups[i];
                if (group?.abilities == null) continue;

                for (int j = 0; j < group.abilities.Count; j++)
                {
                    if (group.abilities[j] != null)
                    {
                        availableAbilityPool.Add(group.abilities[j]);
                    }
                }
            }
        }

        public void AddGeneticMaterial(int amount)
        {
            if (amount <= 0) return;

            currentGeneticMaterial += amount;
            OnGeneticMaterialChanged?.Invoke(currentGeneticMaterial, geneticMaterialNeeded);

            if (currentGeneticMaterial >= geneticMaterialNeeded)
            {
                currentGeneticMaterial -= geneticMaterialNeeded;
                TriggerEvolutionChoice();
            }
        }

        public void SelectEvolution(AbilitySO selectedAbility)
        {
            if (selectedAbility == null) return;

            if (playerAbilityHandler != null)
            {
                playerAbilityHandler.EvolveAbility(selectedAbility.Slot, selectedAbility);
            }

            availableAbilityPool.Remove(selectedAbility);

            if (selectedAbility.NextLevelAbility != null)
            {
                availableAbilityPool.Add(selectedAbility.NextLevelAbility);
            }

            Time.timeScale = 1f;

            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("Player");
            }

            OnEvolutionCompleted?.Invoke();
        }

        private void TriggerEvolutionChoice()
        {
            Time.timeScale = 0f;

            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("UI");
            }

            List<AbilitySO> options = GetRandomAbilitiesNonAlloc(3);

            if (options.Count == 0)
            {
                Debug.LogWarning("[EvolutionManager] No more abilities available. Resuming.");
                Time.timeScale = 1f;
                if (playerInput != null)
                {
                    playerInput.SwitchCurrentActionMap("Player");
                }
                return;
            }

            OnEvolutionChoicesAvailable?.Invoke(options);
        }

        private List<AbilitySO> GetRandomAbilitiesNonAlloc(int count)
        {
            resultBuffer.Clear();
            candidateBuffer.Clear();
            equippedBaseSet.Clear();

            if (playerAbilityHandler != null)
            {
                var equipped = playerAbilityHandler.GetEquippedAbilities();
                for (int i = 0; i < equipped.Count; i++)
                {
                    var ab = equipped[i];
                    if (ab != null)
                    {
                        equippedBaseSet.Add(ab.BaseAbility != null ? ab.BaseAbility : ab);
                    }
                }
            }

            for (int i = 0; i < availableAbilityPool.Count; i++)
            {
                var candidate = availableAbilityPool[i];
                if (!equippedBaseSet.Contains(candidate))
                {
                    candidateBuffer.Add(candidate);
                }
            }

            // Non-allocating Fisher-Yates partial shuffle
            int pickCount = Mathf.Min(count, candidateBuffer.Count);
            for (int i = 0; i < pickCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, candidateBuffer.Count);
                var temp = candidateBuffer[i];
                candidateBuffer[i] = candidateBuffer[randomIndex];
                candidateBuffer[randomIndex] = temp;

                resultBuffer.Add(candidateBuffer[i]);
            }

            return resultBuffer;
        }
    }
}
