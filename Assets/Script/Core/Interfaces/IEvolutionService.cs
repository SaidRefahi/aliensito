using System;
using System.Collections.Generic;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public interface IEvolutionService
    {
        event Action<IReadOnlyList<AbilitySO>> OnEvolutionChoicesAvailable;
        event Action OnEvolutionCompleted;
        event Action<int, int> OnGeneticMaterialChanged;

        int CurrentGeneticMaterial { get; }
        int TargetGeneticMaterial { get; }

        void AddGeneticMaterial(int amount);
        void SelectEvolution(AbilitySO chosenAbility);
    }
}
