using UnityEngine;
using UnityEngine.AI;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(BossAI))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    public class BossAnimator : MonoBehaviour
    {
        private Animator animator;
        private BossAI bossAI;
        private NavMeshAgent agent;
        private Health health;

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
            if (bossAI != null)
            {
                bossAI.OnStateChanged += HandleStateChanged;
                bossAI.OnAttackTrigger += HandleAttack;
                bossAI.OnTeleportTrigger += HandleTeleport;
                bossAI.OnInvisibilityTrigger += HandleInvisibility;
            }

            if (health != null)
            {
                health.OnHit += HandleHit;
                health.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (bossAI != null)
            {
                bossAI.OnStateChanged -= HandleStateChanged;
                bossAI.OnAttackTrigger -= HandleAttack;
                bossAI.OnTeleportTrigger -= HandleTeleport;
                bossAI.OnInvisibilityTrigger -= HandleInvisibility;
            }

            if (health != null)
            {
                health.OnHit -= HandleHit;
                health.OnDeath -= HandleDeath;
            }
        }

        private void Update()
        {
            if (agent != null && agent.speed > 0.001f)
            {
                float normalizedSpeed = agent.velocity.magnitude / agent.speed;
                animator.SetFloat(moveSpeedHash, normalizedSpeed);
            }
        }

        private void HandleStateChanged(BossAI.BossState newState)
        {
            animator.SetBool(isAimingHash, newState == BossAI.BossState.Aiming);

            bool isTacticalMovement = newState == BossAI.BossState.Hunting ||
                                      newState == BossAI.BossState.Repositioning ||
                                      newState == BossAI.BossState.Invisibility;
            animator.SetBool(isHuntingHash, isTacticalMovement);
        }

        private void HandleAttack() => animator.SetTrigger(attackTriggerHash);
        private void HandleTeleport() => animator.SetTrigger(teleportTriggerHash);
        private void HandleInvisibility() => animator.SetTrigger(invisibilityTriggerHash);

        private void HandleHit()
        {
            if (health != null && health.GetCurrentHealth() > 0)
            {
                animator.SetTrigger(hitTriggerHash);
            }
        }

        private void HandleDeath()
        {
            animator.SetTrigger(deathTriggerHash);
        }
    }
}
