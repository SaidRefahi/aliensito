// Ruta: Assets/Scripts/PlayerController/PlayerController.cs
using UnityEngine;
using UnityEngine.InputSystem;

// Este script ahora requiere los componentes que va a controlar
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAbilityHandler))]
[RequireComponent(typeof(StatusEffectManager))] // Lo mantenemos aquí
public class PlayerController : MonoBehaviour
{
    // Referencias a los componentes que controlará
    private PlayerMovement playerMovement;
    private PlayerAbilityHandler playerAbilityHandler;

    // El PlayerInput (componente) llamará a estos métodos.
    // Ya no necesitamos guardar el Animator ni las habilidades aquí.

    private void Awake()
    {
        // Obtenemos las referencias a nuestros "trabajadores"
        playerMovement = GetComponent<PlayerMovement>();
        playerAbilityHandler = GetComponent<PlayerAbilityHandler>();
    }

    // --- MÉTODOS DE INPUT (DELEGACIÓN) ---

    public void OnMove(InputValue value)
    {
        playerMovement.SetMoveInput(value.Get<Vector2>());
    }
    
    public void OnMeleeAttack(InputValue value)
    {
        if (value.isPressed)
        {
            playerAbilityHandler.UseMeleeAbility();
        }
    }

    public void OnRangedAttack(InputValue value)
    {
        if (value.isPressed)
        {
            playerAbilityHandler.UseRangedAbility();
        }
    }

    public void OnInvisibility(InputValue value)
    {
        if (value.isPressed)
        {
            playerAbilityHandler.UseInvisibilityAbility();
        }
    }

   
}