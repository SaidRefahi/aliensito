using System;
using System.Threading;
using Unity.Properties;
using Unity.Behavior;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Execute Attack Burst",
        description: "Executes an ability in burst mode asynchronously via UniTask.",
        story: "[AgentObject] fires [AbilityToUse] with burst count [BurstCount]",
        category: "Genetical Drift/Actions")]
    public partial class ExecuteAttackBurstAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> AgentObject;
        [SerializeReference] public BlackboardVariable<AbilitySO> AbilityToUse;
        [SerializeReference] public BlackboardVariable<int> BurstCount;
        [SerializeReference] public BlackboardVariable<float> DelayBetweenShots;

        private CancellationTokenSource cts;
        private bool isFinished;
        private bool hasFailed;

        protected override Status OnStart()
        {
            if (AgentObject?.Value == null || AbilityToUse?.Value == null)
            {
                return Status.Failure;
            }

            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            isFinished = false;
            hasFailed = false;

            int count = BurstCount != null ? Mathf.Max(1, BurstCount.Value) : 1;
            float delay = DelayBetweenShots != null ? Mathf.Max(0.05f, DelayBetweenShots.Value) : 0.2f;

            ExecuteBurstAsync(count, delay, cts.Token).Forget();
            return Status.Running;
        }

        private async UniTaskVoid ExecuteBurstAsync(int count, float delay, CancellationToken ct)
        {
            for (int i = 0; i < count; i++)
            {
                if (ct.IsCancellationRequested || AgentObject?.Value == null || AbilityToUse?.Value == null)
                {
                    hasFailed = true;
                    return;
                }

                AbilityToUse.Value.Execute(AgentObject.Value);

                if (i < count - 1)
                {
                    bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct).SuppressCancellationThrow();
                    if (canceled)
                    {
                        hasFailed = true;
                        return;
                    }
                }
            }

            isFinished = true;
        }

        protected override Status OnUpdate()
        {
            if (hasFailed) return Status.Failure;
            return isFinished ? Status.Success : Status.Running;
        }

        protected override void OnEnd()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }
    }
}
