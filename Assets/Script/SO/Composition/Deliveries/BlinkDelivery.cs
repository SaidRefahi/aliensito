using System;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable]
    public class BlinkDelivery : IDeliveryStrategy
    {
        [SerializeField] private float distance = 8f;
        [SerializeField] private LayerMask obstacleLayer;

        public float Distance => distance;
        public LayerMask ObstacleLayer => obstacleLayer;

        public bool Deliver(GameObject user, AbilitySO parentAbility, Transform aimSource, TargetingProfile targeting, IPayloadEffect[] payloads)
        {
            if (user == null) return false;

            Transform source = (aimSource != null) ? aimSource : user.transform;
            Vector3 forwardDirection = source.forward;
            forwardDirection.y = 0;
            Vector3 startPos = user.transform.position;
            Vector3 targetPos = startPos + forwardDirection.normalized * distance;

            if (Physics.Linecast(startPos, targetPos, obstacleLayer))
            {
                return false;
            }

            if (user.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.MovePosition(targetPos);
            }
            else
            {
                user.transform.position = targetPos;
            }

            if (payloads != null)
            {
                for (int p = 0; p < payloads.Length; p++)
                {
                    payloads[p]?.Apply(user, user);
                }
            }

            return true;
        }
    }
}
