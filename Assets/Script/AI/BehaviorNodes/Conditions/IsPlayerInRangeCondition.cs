using System;
using Unity.Properties;
using Unity.Behavior;
using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Is Player In Range",
        category: "Genetical Drift/Conditions",
        story: "[Agent] is within [Range] distance of [Target]")]
    public partial class IsPlayerInRangeCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<Transform> Agent;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<float> Range;

        public override bool IsTrue()
        {
            if (Agent?.Value == null || Target?.Value == null)
            {
                return false;
            }

            float sqrDistance = (Agent.Value.position - Target.Value.position).sqrMagnitude;
            float maxRange = Range != null ? Range.Value : 10f;
            return sqrDistance <= (maxRange * maxRange);
        }
    }
}
