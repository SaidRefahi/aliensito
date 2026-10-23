using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public interface IPayloadEffect
    {
        void Apply(GameObject user, GameObject target, float multiplier = 1f);
    }
}
