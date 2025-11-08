// Ruta: Assets/Scripts/PlayerController/ResetComboBehaviour.cs
// ACCIÓN: ¡Script 100% NUEVO!

using UnityEngine;

/// <summary>
/// Un script de StateMachineBehaviour (KISS) que resetea un parámetro
/// Integer del Animator a 0 CADA VEZ que se ENTRA en este estado.
/// Lo pondremos en nuestro estado "Locomotion" (Idle/Walk/Run).
/// </summary>
public class ResetComboBehaviour : StateMachineBehaviour
{
    // Hacemos el nombre del parámetro público para que sea reutilizable
    [SerializeField] private string parameterName = "meleeComboStep";
    
    // Hasheamos el nombre para eficiencia (buena práctica)
    private int parameterHash;
    private bool isHashInitialized = false;

    // OnStateEnter se llama cuando una transición COMIENZA y
    // la máquina de estados empieza a evaluar este estado
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Inicializa el hash solo una vez
        if (!isHashInitialized)
        {
            parameterHash = Animator.StringToHash(parameterName);
            isHashInitialized = true;
        }

        // ¡LA MAGIA!
        // Resetea el contador del combo a 0.
        animator.SetInteger(parameterHash, 0);
    }
}