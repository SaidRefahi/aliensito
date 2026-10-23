using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<IPoolService>();
            builder.RegisterComponentInHierarchy<IEvolutionService>();
            builder.RegisterComponentInHierarchy<IGameStateService>();

            // Standalone fallback: if running scene directly without ProjectLifetimeScope
            if (!builder.Exists(typeof(IAudioService)))
            {
                builder.RegisterComponentInHierarchy<IAudioService>();
            }
        }
    }
}
