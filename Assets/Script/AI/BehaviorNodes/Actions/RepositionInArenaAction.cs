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
        name: "Reposition In Arena",
        description: "Directs NavMeshAgent to a repositioning point clamped within the arena boundaries.",
        story: "[Agent] repositions in arena centered at [ArenaCenter] with radius [ArenaRadius]",
        category: "Genetical Drift/Actions")]
    public partial class RepositionInArenaAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Agent;
        [SerializeReference] public BlackboardVariable<NavMeshAgent> NavAgent;
        [SerializeReference] public BlackboardVariable<Vector3> ArenaCenter;
        [SerializeReference] public BlackboardVariable<float> ArenaRadius;
        [SerializeReference] public BlackboardVariable<float> RepositionDistance;

        private float timeoutTimer;
        private const float TIMEOUT_DURATION = 4f;

        protected override Status OnStart()
        {
            if (Agent?.Value == null || NavAgent?.Value == null)
            {
                return Status.Failure;
            }

            timeoutTimer = 0f;
            Vector3 center = ArenaCenter != null ? ArenaCenter.Value : Vector3.zero;
            float maxRadius = ArenaRadius != null ? ArenaRadius.Value : 25f;
            float dist = RepositionDistance != null ? RepositionDistance.Value : 6f;

            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * dist;
            Vector3 targetPos = Agent.Value.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            Vector3 fromCenter = targetPos - center;
            if (fromCenter.sqrMagnitude > maxRadius * maxRadius)
            {
                targetPos = center + fromCenter.normalized * (maxRadius * 0.85f);
            }

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                NavAgent.Value.SetDestination(hit.position);
                return Status.Running;
            }

            return Status.Failure;
        }

        protected override Status OnUpdate()
        {
            if (NavAgent?.Value == null) return Status.Failure;

            timeoutTimer += Time.deltaTime;
            if (timeoutTimer >= TIMEOUT_DURATION)
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
