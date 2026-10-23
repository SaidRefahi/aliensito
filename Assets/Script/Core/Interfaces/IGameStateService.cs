using System;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public interface IGameStateService
    {
        event Action OnGamePaused;
        event Action OnGameResumed;
        event Action OnVictory;
        event Action OnDefeat;
        event Action OnBossFightStarted;

        bool IsPaused { get; }
        bool IsGameOver { get; }
        bool BossFightHasStarted { get; }

        void SetPause(bool pause);
        void TriggerVictory();
        void TriggerDefeat();
        void StartBossFight();
    }
}
