using System;
using System.Collections.Generic;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    [Serializable]
    public class PlayerProgressionData
    {
        public int highestGeneticMaterialCollected = 0;
        public List<string> unlockedAbilityIDs = new List<string>();
        public int bossesDefeated = 0;
    }
}
