using UnityEngine;
using UnityEngine.InputSystem;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerAbilityHandler))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerMovement playerMovement;
        private PlayerAbilityHandler playerAbilityHandler;
        private InputSystem_Actions inputActions;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerAbilityHandler = GetComponent<PlayerAbilityHandler>();
            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            inputActions.Player.Move.performed += HandleMovePerformed;
            inputActions.Player.Move.canceled += HandleMoveCanceled;
            inputActions.Player.MeleeAttack.performed += HandleMeleePerformed;
            inputActions.Player.RangedAttack.performed += HandleRangedPerformed;
            inputActions.Player.Invisibility.performed += HandleInvisibilityPerformed;
            inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= HandleMovePerformed;
            inputActions.Player.Move.canceled -= HandleMoveCanceled;
            inputActions.Player.MeleeAttack.performed -= HandleMeleePerformed;
            inputActions.Player.RangedAttack.performed -= HandleRangedPerformed;
            inputActions.Player.Invisibility.performed -= HandleInvisibilityPerformed;
            inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
        }

        private void HandleMovePerformed(InputAction.CallbackContext context)
        {
            playerMovement.SetMoveInput(context.ReadValue<Vector2>());
        }

        private void HandleMoveCanceled(InputAction.CallbackContext context)
        {
            playerMovement.SetMoveInput(Vector2.zero);
        }

        private void HandleMeleePerformed(InputAction.CallbackContext context)
        {
            playerAbilityHandler.UseMeleeAbility();
        }

        private void HandleRangedPerformed(InputAction.CallbackContext context)
        {
            playerAbilityHandler.UseRangedAbility();
        }

        private void HandleInvisibilityPerformed(InputAction.CallbackContext context)
        {
            playerAbilityHandler.UseInvisibilityAbility();
        }

        // Backward compatibility for legacy PlayerInput SendMessage invocations
        public void OnMove(InputValue value) => playerMovement.SetMoveInput(value.Get<Vector2>());
        public void OnMeleeAttack(InputValue value) { if (value.isPressed) playerAbilityHandler.UseMeleeAbility(); }
        public void OnRangedAttack(InputValue value) { if (value.isPressed) playerAbilityHandler.UseRangedAbility(); }
        public void OnInvisibility(InputValue value) { if (value.isPressed) playerAbilityHandler.UseInvisibilityAbility(); }
    }
}
