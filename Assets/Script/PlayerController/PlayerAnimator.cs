using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerAbilityHandler))]
    public class PlayerAnimator : MonoBehaviour
    {
        private Animator animator;
        private PlayerMovement playerMovement;
        private PlayerAbilityHandler playerAbilityHandler;

        private readonly int moveSpeedHash = Animator.StringToHash("moveSpeed");
        private readonly int meleeComboStepHash = Animator.StringToHash("meleeComboStep");
        private readonly int attackRangedHash = Animator.StringToHash("attackRanged");
        private readonly int useAbilityHash = Animator.StringToHash("useAbility");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            playerMovement = GetComponent<PlayerMovement>();
            playerAbilityHandler = GetComponent<PlayerAbilityHandler>();
        }

        private void OnEnable()
        {
            if (playerAbilityHandler != null)
            {
                playerAbilityHandler.OnMeleeAttackTrigger += HandleMeleeAttackTrigger;
                playerAbilityHandler.OnRangedAttackTrigger += HandleRangedAttackTrigger;
                playerAbilityHandler.OnAbilityTrigger += HandleAbilityTrigger;
            }
        }

        private void OnDisable()
        {
            if (playerAbilityHandler != null)
            {
                playerAbilityHandler.OnMeleeAttackTrigger -= HandleMeleeAttackTrigger;
                playerAbilityHandler.OnRangedAttackTrigger -= HandleRangedAttackTrigger;
                playerAbilityHandler.OnAbilityTrigger -= HandleAbilityTrigger;
            }
        }

        private void Update()
        {
            animator.SetFloat(moveSpeedHash, playerMovement.CurrentSpeedNormalized);
        }

        private void HandleMeleeAttackTrigger()
        {
            int currentStep = animator.GetInteger(meleeComboStepHash);
            currentStep++;

            int maxCombo = playerAbilityHandler.GetMaxMeleeCombo();
            if (currentStep > maxCombo)
            {
                currentStep = 1;
            }

            animator.SetInteger(meleeComboStepHash, currentStep);
        }

        private void HandleRangedAttackTrigger()
        {
            animator.SetTrigger(attackRangedHash);
        }

        private void HandleAbilityTrigger()
        {
            animator.SetTrigger(useAbilityHash);
        }
    }
}
