using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAbilityHandler))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerAbilityHandler playerAbilityHandler;

    // Hashes de parámetros
    private readonly int moveSpeedHash = Animator.StringToHash("moveSpeed");
    
    // --- MODIFICADO ---
    // Ya no usamos un Trigger para melee, usamos un Integer
    private readonly int meleeComboStepHash = Animator.StringToHash("meleeComboStep"); 
    // private readonly int attackMeleeHash = Animator.StringToHash("attackMelee"); // <-- BORRADO
    // --- FIN DE MODIFICACIÓN ---
    
    private readonly int attackRangedHash = Animator.StringToHash("attackRanged");
    private readonly int useAbilityHash = Animator.StringToHash("useAbility");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAbilityHandler = GetComponent<PlayerAbilityHandler>();

        // Nos suscribimos a los eventos
        playerAbilityHandler.OnMeleeAttackTrigger += HandleMeleeAttackTrigger; // <-- ¡Este ahora hace más!
        playerAbilityHandler.OnRangedAttackTrigger += HandleRangedAttackTrigger;
        playerAbilityHandler.OnAbilityTrigger += HandleAbilityTrigger;
    }

    private void Update()
    {
        animator.SetFloat(moveSpeedHash, playerMovement.CurrentSpeedNormalized);
    }

    // --- ¡MÉTODO MODIFICADO! ---
    // Este método ahora es el "cerebro" del combo.
    private void HandleMeleeAttackTrigger()
    {
        // 1. Preguntamos al Animator: "¿En qué paso del combo estamos?"
        int currentStep = animator.GetInteger(meleeComboStepHash);

        // 2. Incrementamos el paso
        currentStep++;
        
        // 3. Preguntamos al AbilityHandler: "¿Cuál es nuestro máximo combo?"
        // (¡Esto respeta tu requisito de que dependa de la habilidad!)
        int maxCombo = playerAbilityHandler.GetMaxMeleeCombo(); // <--- ¡NUEVA FUNCIÓN!
        
        // 4. Si pasamos el máximo, volvemos al paso 1
        if (currentStep > maxCombo)
        {
            currentStep = 1;
        }

        // 5. Le decimos al Animator cuál es el nuevo paso
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