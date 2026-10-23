using UnityEngine;
using UnityEngine.AI;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class Stun : StatusEffect
    {
        private readonly NavMeshAgent agent;

        public Stun(GameObject target, float duration) : base(target, duration)
        {
            agent = target != null ? target.GetComponent<NavMeshAgent>() : null;
        }

        public override void OnApply()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
        }

        public override void OnEnd()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }
    }
}
