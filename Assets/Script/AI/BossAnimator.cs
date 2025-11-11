using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BossAI))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class BossAnimator : MonoBehaviour
{
    // --- Componentes ---
    private Animator animator;
    private BossAI bossAI;
    private NavMeshAgent agent;
    private Health health;

    // --- Hashes de Animación ---
    private readonly int moveSpeedHash = Animator.StringToHash("moveSpeed");
    private readonly int isAimingHash = Animator.StringToHash("isAiming");
    private readonly int isHuntingHash = Animator.StringToHash("isHunting");
    private readonly int attackTriggerHash = Animator.StringToHash("attack");
    private readonly int teleportTriggerHash = Animator.StringToHash("teleport");
    private readonly int invisibilityTriggerHash = Animator.StringToHash("invisibility");
    private readonly int hitTriggerHash = Animator.StringToHash("hit");
    private readonly int deathTriggerHash = Animator.StringToHash("death");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        bossAI = GetComponent<BossAI>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        // Suscribirse a los eventos del "cerebro" (BossAI)
        bossAI.OnStateChanged += HandleStateChanged;
        bossAI.OnAttackTrigger += HandleAttack;
        bossAI.OnTeleportTrigger += HandleTeleport;
        bossAI.OnInvisibilityTrigger += HandleInvisibility;
        
        // --- ¡¡LÍNEAS CORREGIDAS!! ---
        // Suscribirse a los eventos de Vida (Health)
        health.OnHit += HandleHit;       // <-- Corregido (antes era OnTakeDamage)
        health.OnDeath += HandleDeath;  // <-- Corregido (antes era OnDie)
        // -----------------------------
    }

    private void OnDisable()
    {
        // Desuscribirse de los eventos del "cerebro" (BossAI)
        bossAI.OnStateChanged -= HandleStateChanged;
        bossAI.OnAttackTrigger -= HandleAttack;
        bossAI.OnTeleportTrigger -= HandleTeleport;
        bossAI.OnInvisibilityTrigger -= HandleInvisibility;
        
        // --- ¡¡LÍNEAS CORREGIDAS!! ---
        // Desuscribirse de los eventos de Vida (Health)
        health.OnHit -= HandleHit;       // <-- Corregido
        health.OnDeath -= HandleDeath;   // <-- Corregido
        // -----------------------------
    }

    private void Update()
    {
        // Actualizar el 'float' de movimiento
        float normalizedSpeed = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(moveSpeedHash, normalizedSpeed);
    }

    // --- MANEJADORES DE EVENTOS (Sin cambios en su lógica) ---

    private void HandleStateChanged(BossAI.BossState newState)
    {
        // Actualiza los bools basados en el estado
        animator.SetBool(isAimingHash, newState == BossAI.BossState.Aiming);
        
        bool isTacticalMovement = newState == BossAI.BossState.Hunting || 
                                  newState == BossAI.BossState.Repositioning || 
                                  newState == BossAI.BossState.Invisibility;
        animator.SetBool(isHuntingHash, isTacticalMovement);
    }

    private void HandleAttack()
    {
        animator.SetTrigger(attackTriggerHash);
    }

    private void HandleTeleport()
    {
        animator.SetTrigger(teleportTriggerHash);
    }

    private void HandleInvisibility()
    {
        animator.SetTrigger(invisibilityTriggerHash);
    }

    /// <summary>
    /// Se llama desde el evento OnHit del script Health.
    /// </summary>
    private void HandleHit()
    {
        // Solo reacciona al golpe si no está muerto
        if (health.GetCurrentHealth() > 0)
        {
            animator.SetTrigger(hitTriggerHash);
        }
    }

    /// <summary>
    /// Se llama desde el evento OnDeath del script Health.
    /// </summary>
    private void HandleDeath()
    {
        animator.SetTrigger(deathTriggerHash);
    }
}