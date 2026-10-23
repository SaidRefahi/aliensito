using System;
using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(StatusEffectManager))]
    public class RangedEnemyAI : MonoBehaviour
    {
        private enum AIState
        {
            Wandering,
            Aiming,
            Attacking,
            Cooldown,
            Repositioning
        }
        private AIState currentState;

        [Title("1. Detection Settings")]
        [SerializeField] private float detectionRadius = 15f;
        [SerializeField] private float loseSightRadius = 20f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float viewAngle = 90f;

        [Title("2. Combat Settings")]
        [SerializeField] private AbilitySO rangedAbility;
        [SerializeField] private Transform aimPoint;
        [SerializeField] private float closeRange = 7f;
        [SerializeField] private float farRange = 18f;
        [SerializeField] private float defensiveHealthThreshold = 0.5f;
        [SerializeField] private float repositionDistance = 8f;

        [Title("3. Timing Settings")]
        [SerializeField] private float aimTime = 1.5f;
        [SerializeField] private float cooldownTime = 1f;

        [Title("4. Wander Settings")]
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float wanderTimerMax = 5f;

        [Title("5. Visuals & Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private LineRenderer aimingLine;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color firingColor = Color.red;
        [SerializeField] private float colorChangeTime = 0.5f;
        [SerializeField] private LayerMask lineCastLayerMask;
        [SerializeField] private float lineMaxDistance = 100f;

        private readonly int isAimingHash = Animator.StringToHash("isAiming");
        private readonly int attackHash = Animator.StringToHash("attack");
        private readonly int moveSpeedHash = Animator.StringToHash("moveSpeed");

        private NavMeshAgent agent;
        private Transform playerTransform;
        private Health health;
        private float stateTimer;
        private float wanderTimer;
        private float originalSpeed;

        [Title("6. Unity Behavior Integration")]
        [SerializeField] private BehaviorGraphAgent behaviorAgent;

        private Vector3 lockedPlayerPosition;
        private bool isAimLockSet;
        private readonly Collider[] detectionColliders = new Collider[2];

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            originalSpeed = agent.speed;

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (aimingLine == null)
            {
                aimingLine = GetComponent<LineRenderer>();
            }

            if (behaviorAgent == null)
            {
                behaviorAgent = GetComponent<BehaviorGraphAgent>();
            }
        }

        private void Start()
        {
            TransitionTo(AIState.Wandering);

            if (aimingLine != null)
            {
                aimingLine.enabled = false;
            }
        }

        private void Update()
        {
            if (behaviorAgent != null && behaviorAgent.Graph != null)
            {
                SyncBlackboardVariables();
                return;
            }

            switch (currentState)
            {
                case AIState.Wandering:
                    HandleWanderingState();
                    break;
                case AIState.Aiming:
                    HandleAimingState();
                    break;
                case AIState.Attacking:
                    HandleAttackingState();
                    break;
                case AIState.Cooldown:
                    HandleCooldownState();
                    break;
                case AIState.Repositioning:
                    HandleRepositioningState();
                    break;
            }

            if (animator != null && agent.speed > 0.001f)
            {
                animator.SetFloat(moveSpeedHash, agent.velocity.magnitude);
            }
        }

        private void HandleWanderingState()
        {
            Wander();
            if (IsPlayerInSight()) TransitionTo(AIState.Aiming);
        }

        private void HandleAimingState()
        {
            agent.isStopped = true;
            stateTimer -= Time.deltaTime;

            if (playerTransform == null)
            {
                TransitionTo(AIState.Wandering);
                return;
            }

            Vector3 targetPosition;

            if (stateTimer > colorChangeTime)
            {
                targetPosition = playerTransform.position;
                if (aimingLine != null)
                {
                    aimingLine.startColor = warningColor;
                    aimingLine.endColor = warningColor;
                }
            }
            else
            {
                if (!isAimLockSet)
                {
                    lockedPlayerPosition = playerTransform.position;
                    isAimLockSet = true;
                }
                targetPosition = lockedPlayerPosition;

                if (aimingLine != null)
                {
                    aimingLine.startColor = firingColor;
                    aimingLine.endColor = firingColor;
                }
            }

            LookAtPosition(targetPosition);

            if (aimingLine != null)
            {
                Vector3 startPos = transform.position + (Vector3.up * 0.1f);
                Vector3 targetGroundPos = new Vector3(targetPosition.x, startPos.y, targetPosition.z);
                Vector3 direction = (targetGroundPos - startPos).normalized;

                Vector3 endPos;
                if (Physics.Raycast(startPos, direction, out RaycastHit hit, lineMaxDistance, lineCastLayerMask))
                {
                    endPos = hit.point;
                }
                else
                {
                    endPos = startPos + (direction * lineMaxDistance);
                }

                aimingLine.SetPosition(0, startPos);
                aimingLine.SetPosition(1, endPos);
            }

            if (stateTimer <= 0f) TransitionTo(AIState.Attacking);
            if (!IsPlayerInSight()) TransitionTo(AIState.Wandering);
        }

        private void HandleAttackingState()
        {
            FireAbility();
            TransitionTo(AIState.Cooldown);
        }

        private void HandleCooldownState()
        {
            agent.isStopped = true;
            LookAtPlayer();
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f) TransitionTo(AIState.Repositioning);
            if (!IsPlayerInSight()) TransitionTo(AIState.Wandering);
        }

        private void HandleRepositioningState()
        {
            LookAtPlayer();
            if (!IsPlayerInSight()) TransitionTo(AIState.Wandering);
            if (!agent.pathPending && agent.remainingDistance < 1.0f)
            {
                TransitionTo(AIState.Aiming);
            }
        }

        private void TransitionTo(AIState newState)
        {
            currentState = newState;

            if (aimingLine != null)
            {
                aimingLine.enabled = (newState == AIState.Aiming);
            }

            if (animator != null)
            {
                animator.SetBool(isAimingHash, false);

                switch (currentState)
                {
                    case AIState.Aiming:
                        animator.SetBool(isAimingHash, true);
                        break;
                    case AIState.Attacking:
                        animator.SetTrigger(attackHash);
                        break;
                }
            }

            switch (currentState)
            {
                case AIState.Wandering:
                    agent.isStopped = false;
                    agent.speed = originalSpeed;
                    playerTransform = null;
                    wanderTimer = 0f;
                    break;
                case AIState.Aiming:
                    agent.isStopped = true;
                    stateTimer = aimTime;
                    isAimLockSet = false;
                    if (aimingLine != null)
                    {
                        aimingLine.startColor = warningColor;
                        aimingLine.endColor = warningColor;
                    }
                    break;
                case AIState.Attacking:
                    break;
                case AIState.Cooldown:
                    agent.isStopped = true;
                    stateTimer = cooldownTime;
                    break;
                case AIState.Repositioning:
                    agent.isStopped = false;
                    agent.speed = originalSpeed;
                    SetNewRepositionDestination();
                    break;
            }
        }

        private bool IsPlayerInSight()
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, detectionColliders, playerLayer);
            if (hits > 0 && detectionColliders[0] != null)
            {
                Transform target = detectionColliders[0].transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f)
                {
                    float sqrDist = (target.position - transform.position).sqrMagnitude;
                    float dist = Mathf.Sqrt(sqrDist);
                    if (!Physics.Raycast(transform.position, dirToTarget, dist, ~playerLayer))
                    {
                        playerTransform = target;
                        return true;
                    }
                }
            }

            if (playerTransform != null)
            {
                float sqrDist = (transform.position - playerTransform.position).sqrMagnitude;
                if (sqrDist > (loseSightRadius * loseSightRadius))
                {
                    playerTransform = null;
                    return false;
                }
            }

            return playerTransform != null;
        }

        private void FireAbility()
        {
            if (rangedAbility == null) return;
            if (rangedAbility is IAimable aimableAbility)
            {
                aimableAbility.SetAimSource((aimPoint != null) ? aimPoint : transform);
            }
            rangedAbility.Execute(gameObject);
        }

        private void LookAtPlayer()
        {
            if (playerTransform == null) return;
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed);
        }

        private void LookAtPosition(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed * 2f);
        }

        private void Wander()
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f && agent.remainingDistance < 0.5f)
            {
                Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius + transform.position;
                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                wanderTimer = wanderTimerMax;
            }
        }

        private void SetNewRepositionDestination()
        {
            if (playerTransform == null)
            {
                TransitionTo(AIState.Wandering);
                return;
            }

            float currentTargetRange = (health != null && health.GetCurrentHealth() / health.GetMaxHealth() > defensiveHealthThreshold)
                ? closeRange
                : farRange;

            Vector3 dirAwayFromPlayer = (transform.position - playerTransform.position).normalized;
            Vector3 strafeDir = new Vector3(dirAwayFromPlayer.z, 0, -dirAwayFromPlayer.x) * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
            Vector3 targetPos = transform.position + (strafeDir * repositionDistance);

            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * currentTargetRange;
            Vector3 idealPos = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            Vector3 finalDestination = Vector3.Lerp(targetPos, idealPos, 0.5f);

            if (NavMesh.SamplePosition(finalDestination, out NavMeshHit hit, repositionDistance * 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                Vector3 fallbackPos = playerTransform.position + (dirAwayFromPlayer * currentTargetRange);
                agent.SetDestination(fallbackPos);
            }
        }

        private void SyncBlackboardVariables()
        {
            if (behaviorAgent == null) return;

            behaviorAgent.SetVariableValue("Agent", transform);
            if (playerTransform != null)
            {
                behaviorAgent.SetVariableValue("Target", playerTransform);
            }
            if (agent != null)
            {
                behaviorAgent.SetVariableValue("NavAgent", agent);
            }
            behaviorAgent.SetVariableValue("WanderRadius", wanderRadius);
            behaviorAgent.SetVariableValue("AimDuration", aimTime);
            if (aimingLine != null)
            {
                behaviorAgent.SetVariableValue("AimLine", aimingLine);
            }
            if (rangedAbility != null)
            {
                behaviorAgent.SetVariableValue("AbilityToUse", rangedAbility);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, loseSightRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, closeRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, farRange);

            Vector3 fovLine1 = Quaternion.AngleAxis(viewAngle / 2f, transform.up) * transform.forward * detectionRadius;
            Vector3 fovLine2 = Quaternion.AngleAxis(-viewAngle / 2f, transform.up) * transform.forward * detectionRadius;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, fovLine1);
            Gizmos.DrawRay(transform.position, fovLine2);
        }
    }
}
