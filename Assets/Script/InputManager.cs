using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static event Action OnMelee;
    public static event Action OnRanged;
    
    // --- LÍNEA CORREGIDA ---
    public static event Action OnInvisibilityPressed; // Renombrado de OnInvisibility a OnInvisibilityPressed

    public void OnMeleeAttack(InputValue value)
    {
        if (value.isPressed) OnMelee?.Invoke();
    }

    public void OnRangedAttack(InputValue value)
    {
        if (value.isPressed) OnRanged?.Invoke();
    }

    // El nombre de este MÉTODO es correcto, porque lo busca el PlayerInput
    public void OnInvisibility(InputValue value)
    {
        // --- LÍNEA CORREGIDA ---
        if (value.isPressed) OnInvisibilityPressed?.Invoke(); // Invocamos el evento renombrado
    }
}