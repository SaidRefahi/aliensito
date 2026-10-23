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
        name: "Teleport Agent",
        description: "Teleports agent to a safe point with VFX feedback.",
        story: "[Agent] teleports [TeleportDistance] units away within arena",
        category: "Genetical Drift/Actions")]
    public partial class TeleportAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Agent;
        [SerializeReference] public BlackboardVariable<float> TeleportDistance;
        [SerializeReference] public BlackboardVariable<Vector3> ArenaCenter;
        [SerializeReference] public BlackboardVariable<float> ArenaRadius;
        [SerializeReference] public BlackboardVariable<string> TeleportVfxTag;

        protected override Status OnStart()
        {
            if (Agent?.Value == null) return Status.Failure;

            Vector3 startPos = Agent.Value.position;
            string vfxTag = TeleportVfxTag != null ? TeleportVfxTag.Value : "TeleportVFX";

            if (PoolManager.Instance != null && !string.IsNullOrEmpty(vfxTag))
            {
                PoolManager.Instance.SpawnFromPool(vfxTag, startPos, Agent.Value.rotation);
            }

            Vector3 center = ArenaCenter != null ? ArenaCenter.Value : Vector3.zero;
            float maxRadius = ArenaRadius != null ? ArenaRadius.Value : 25f;
            float dist = TeleportDistance != null ? TeleportDistance.Value : 10f;

            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized * dist;
            Vector3 targetPos = startPos + new Vector3(randomDir.x, 0, randomDir.y);

            Vector3 fromCenter = targetPos - center;
            if (fromCenter.sqrMagnitude > maxRadius * maxRadius)
            {
                targetPos = center + fromCenter.normalized * (maxRadius * 0.8f);
            }

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }

            if (Agent.Value.TryGetComponent<NavMeshAgent>(out var navAgent))
            {
                navAgent.Warp(targetPos);
            }
            else
            {
                Agent.Value.position = targetPos;
            }

            if (PoolManager.Instance != null && !string.IsNullOrEmpty(vfxTag))
            {
                PoolManager.Instance.SpawnFromPool(vfxTag, targetPos, Agent.Value.rotation);
            }

            return Status.Success;
        }
    }
}
