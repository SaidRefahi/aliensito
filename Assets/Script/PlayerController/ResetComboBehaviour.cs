using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class ResetComboBehaviour : StateMachineBehaviour
    {
        [SerializeField] private string parameterName = "meleeComboStep";

        private int parameterHash;
        private bool isHashInitialized = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!isHashInitialized)
            {
                parameterHash = Animator.StringToHash(parameterName);
                isHashInitialized = true;
            }

            animator.SetInteger(parameterHash, 0);
        }
    }
}
