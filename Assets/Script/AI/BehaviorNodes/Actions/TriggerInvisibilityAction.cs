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
        name: "Trigger Invisibility",
        description: "Applies invisibility ability to agent.",
        story: "[AgentObject] activates [InvisibilityAbility]",
        category: "Genetical Drift/Actions")]
    public partial class TriggerInvisibilityAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> AgentObject;
        [SerializeReference] public BlackboardVariable<InvisibilitySO> InvisibilityAbility;

        protected override Status OnStart()
        {
            if (AgentObject?.Value == null || InvisibilityAbility?.Value == null)
            {
                return Status.Failure;
            }

            bool executed = InvisibilityAbility.Value.Execute(AgentObject.Value);
            return executed ? Status.Success : Status.Failure;
        }
    }
}
