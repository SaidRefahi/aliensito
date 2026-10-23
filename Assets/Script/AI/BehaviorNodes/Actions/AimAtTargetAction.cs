using System;
using Unity.Properties;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Aim At Target",
        description: "Smoothly rotates agent toward target and renders telegraph line.",
        story: "[Agent] aims at [Target] for [AimDuration] seconds",
        category: "Genetical Drift/Actions")]
    public partial class AimAtTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Agent;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<float> AimDuration;
        [SerializeReference] public BlackboardVariable<LineRenderer> AimLine;

        private float elapsed;

        protected override Status OnStart()
        {
            if (Agent?.Value == null || Target?.Value == null)
            {
                return Status.Failure;
            }

            elapsed = 0f;
            if (AimLine?.Value != null)
            {
                AimLine.Value.enabled = true;
            }

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Agent?.Value == null || Target?.Value == null)
            {
                return Status.Failure;
            }

            elapsed += Time.deltaTime;

            Vector3 direction = (Target.Value.position - Agent.Value.position).normalized;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                Agent.Value.rotation = Quaternion.Slerp(Agent.Value.rotation, targetRot, Time.deltaTime * 10f);
            }

            if (AimLine?.Value != null)
            {
                AimLine.Value.SetPosition(0, Agent.Value.position);
                AimLine.Value.SetPosition(1, Target.Value.position);
            }

            float duration = AimDuration != null ? AimDuration.Value : 1f;
            if (elapsed >= duration)
            {
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (AimLine?.Value != null)
            {
                AimLine.Value.enabled = false;
            }
        }
    }
}
