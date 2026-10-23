using System;
using Unity.Properties;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Flee From Threat",
        description: "Directs NavMeshAgent to flee away from a threat.",
        story: "[Agent] flees away from [Threat] by [FleeDistance] units",
        category: "Genetical Drift/Actions")]
    public partial class FleeAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Agent;
        [SerializeReference] public BlackboardVariable<Transform> Threat;
        [SerializeReference] public BlackboardVariable<NavMeshAgent> NavAgent;
        [SerializeReference] public BlackboardVariable<float> FleeDistance;

        private float elapsed;
        private const float MAX_FLEE_TIME = 4f;

        protected override Status OnStart()
        {
            if (Agent?.Value == null || Threat?.Value == null || NavAgent?.Value == null)
            {
                return Status.Failure;
            }

            elapsed = 0f;
            float dist = FleeDistance != null ? FleeDistance.Value : 8f;
            Vector3 fleeDirection = (Agent.Value.position - Threat.Value.position).normalized;
            Vector3 fleeTarget = Agent.Value.position + fleeDirection * dist;

            if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, dist, NavMesh.AllAreas))
            {
                NavAgent.Value.SetDestination(hit.position);
                return Status.Running;
            }

            return Status.Failure;
        }

        protected override Status OnUpdate()
        {
            if (NavAgent?.Value == null) return Status.Failure;

            elapsed += Time.deltaTime;
            if (elapsed >= MAX_FLEE_TIME)
            {
                return Status.Success;
            }

            if (!NavAgent.Value.pathPending && NavAgent.Value.remainingDistance <= NavAgent.Value.stoppingDistance + 0.3f)
            {
                return Status.Success;
            }

            return Status.Running;
        }
    }
}
