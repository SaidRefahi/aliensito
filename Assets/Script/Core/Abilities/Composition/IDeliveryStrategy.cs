using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public interface IDeliveryStrategy
    {
        bool Deliver(GameObject user, AbilitySO parentAbility, Transform aimSource, TargetingProfile targeting, IPayloadEffect[] payloads);
    }
}
