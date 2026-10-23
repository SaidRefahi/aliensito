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
        name: "Wander On NavMesh",
        description: "Directs agent to a random wander position on NavMesh.",
        story: "[Agent] wanders within [WanderRadius] units using [NavAgent]",
        category: "Genetical Drift/Actions")]
    public partial class WanderAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Agent;
        [SerializeReference] public BlackboardVariable<NavMeshAgent> NavAgent;
        [SerializeReference] public BlackboardVariable<float> WanderRadius;

        private float elapsed;
        private const float MAX_WANDER_TIME = 5f;

        protected override Status OnStart()
        {
            if (Agent?.Value == null || NavAgent?.Value == null)
            {
                return Status.Failure;
            }

            elapsed = 0f;
            float radius = WanderRadius != null ? WanderRadius.Value : 5f;
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 randomTarget = Agent.Value.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (NavMesh.SamplePosition(randomTarget, out NavMeshHit hit, radius, NavMesh.AllAreas))
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
            if (elapsed >= MAX_WANDER_TIME)
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
