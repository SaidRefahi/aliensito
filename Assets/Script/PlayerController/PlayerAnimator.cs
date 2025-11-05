// Ruta: Assets/Scripts/PlayerController/PlayerAnimator.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAbilityHandler))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerAbilityHandler playerAbilityHandler;

    // Hashes de parámetros (¡Buena práctica que ya tenías!)
    private readonly int moveSpeedHash = Animator.StringToHash("moveSpeed");
    private readonly int attackMeleeHash = Animator.StringToHash("attackMelee");
    private readonly int attackRangedHash = Animator.StringToHash("attackRanged");
    private readonly int useAbilityHash = Animator.StringToHash("useAbility");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAbilityHandler = GetComponent<PlayerAbilityHandler>();

        // --- LA MAGIA DEL DESACOPLAMIENTO ---
        // Nos suscribimos a los eventos del AbilityHandler
        playerAbilityHandler.OnMeleeAttackTrigger += HandleMeleeAttackTrigger;
        playerAbilityHandler.OnRangedAttackTrigger += HandleRangedAttackTrigger;
        playerAbilityHandler.OnAbilityTrigger += HandleAbilityTrigger;
    }

    private void Update()
    {
        // Actualizamos la animación de movimiento leyendo la velocidad
        // actual del componente de movimiento.
        animator.SetFloat(moveSpeedHash, playerMovement.CurrentSpeedNormalized);
    }

    // --- MÉTODOS SUSCRITOS A EVENTOS ---
    
    private void HandleMeleeAttackTrigger()
    {
        animator.SetTrigger(attackMeleeHash);
    }

    private void HandleRangedAttackTrigger()
    {
        animator.SetTrigger(attackRangedHash);
    }

    private void HandleAbilityTrigger()
    {
        animator.SetTrigger(useAbilityHash);
    }

    /// <summary>
    /// Limpiamos las suscripciones cuando el objeto se destruye
    /// </summary>
    private void OnDestroy()
    {
        if (playerAbilityHandler != null)
        {
            playerAbilityHandler.OnMeleeAttackTrigger -= HandleMeleeAttackTrigger;
            playerAbilityHandler.OnRangedAttackTrigger -= HandleRangedAttackTrigger;
            playerAbilityHandler.OnAbilityTrigger -= HandleAbilityTrigger;
        }
    }
}