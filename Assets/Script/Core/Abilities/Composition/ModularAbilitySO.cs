using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    [CreateAssetMenu(fileName = "NewModularAbility", menuName = "Genetical Drift/Modular Ability")]
    public class ModularAbilitySO : AbilitySO, IAimable
    {
        [Title("Delivery Mechanism")]
        [SerializeReference] private IDeliveryStrategy deliveryStrategy;

        [Title("Payload Effects")]
        [SerializeReference] private IPayloadEffect[] payloadEffects;

        public IDeliveryStrategy DeliveryStrategy => deliveryStrategy;
        public IReadOnlyList<IPayloadEffect> PayloadEffects => payloadEffects;

        private Transform aimSource;
        public Transform AimSource => aimSource;

        public void SetAimSource(Transform source)
        {
            aimSource = source;
        }

        public void SetComposition(IDeliveryStrategy delivery, IPayloadEffect[] payloads)
        {
            deliveryStrategy = delivery;
            payloadEffects = payloads;
        }

        public override bool Execute(GameObject user)
        {
            if (deliveryStrategy == null) return false;
            if (!user.TryGetComponent<TargetingProfile>(out var targeting)) return false;

            return deliveryStrategy.Deliver(user, this, AimSource, targeting, payloadEffects);
        }
    }
}
