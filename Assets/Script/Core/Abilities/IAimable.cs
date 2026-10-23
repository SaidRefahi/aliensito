using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public interface IAimable
    {
        Transform AimSource { get; }
        void SetAimSource(Transform source);
    }
}
