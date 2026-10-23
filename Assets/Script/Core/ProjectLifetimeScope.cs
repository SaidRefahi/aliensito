using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<IAudioService>();
            builder.Register<JsonFileSaveService>(Lifetime.Singleton).As<ISaveService>();
        }
    }
}
