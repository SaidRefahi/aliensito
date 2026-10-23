using System;
using Unity.Properties;
using Unity.Behavior;
using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Is Target In Sight",
        category: "Genetical Drift/Conditions",
        story: "[Agent] has [Target] in sight angle [ViewAngle] within [ObstacleMask]")]
    public partial class IsTargetInSightCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<Transform> Agent;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<float> ViewAngle;
        [SerializeReference] public BlackboardVariable<LayerMask> ObstacleMask;

        public override bool IsTrue()
        {
            if (Agent?.Value == null || Target?.Value == null) return false;

            Vector3 toTarget = (Target.Value.position - Agent.Value.position);
            Vector3 forward = Agent.Value.forward;
            float maxAngle = ViewAngle != null ? ViewAngle.Value : 90f;

            if (Vector3.Angle(forward, toTarget) > maxAngle * 0.5f)
            {
                return false;
            }

            LayerMask mask = ObstacleMask != null ? ObstacleMask.Value : default;
            if (mask.value != 0 && Physics.Linecast(Agent.Value.position, Target.Value.position, mask))
            {
                return false;
            }

            return true;
        }
    }
}
