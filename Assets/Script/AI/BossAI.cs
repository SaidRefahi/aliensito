using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;
using Cysharp.Threading.Tasks;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(StatusEffectManager))]
    [RequireComponent(typeof(TargetingProfile))]
    public class BossAI : MonoBehaviour
    {
        public enum BossState
        {
            Waiting,
            Hunting,
            Aiming,
            Attacking,
            Cooldown,
            Deciding,
            Repositioning,
            Invisibility,
            Teleporting
        }

        public enum AttackType { None, Single, Burst }

        [Serializable]
        public class WeightedAttackChoice
        {
            [Tooltip("Type of attack decided.")]
            public AttackType attackType;
            [Min(0f)]
            [Tooltip("Probability weight for this choice.")]
            public float probabilityWeight = 1f;
        }

        public event System.Action<BossState> OnStateChanged;
        public event System.Action OnAttackTrigger;
        public event System.Action OnTeleportTrigger;
        public event System.Action OnInvisibilityTrigger;

        [Title("1. Arena & Detection")]
        [SerializeField] private float maxEngagementRange = 50f;
        [SerializeField] private LayerMask playerLayer;

        [Title("2. Combat Abilities (ScriptableObjects)")]
        [SerializeField] private AbilitySO singleShotSO;
        [SerializeField] private AbilitySO burstShotSO;
        [SerializeField] private AbilitySO invisibilitySO;
        [SerializeField] private AbilitySO teleportSO;
        [SerializeField] private Transform aimPoint;

        [Title("3. Attack Tactics & Playbook")]
        [SerializeField] private List<WeightedAttackChoice> attackChoices = new List<WeightedAttackChoice>();
        [SerializeField] private float idealFiringRange = 20f;
        [SerializeField] private float minFiringRange = 8f;
        [SerializeField] private float aimTime = 1.0f;
        [SerializeField] private float burstFireDelay = 0.3f;
        [SerializeField] private float cooldownTime = 1.5f;
        [SerializeField] private float repositionDistance = 12f;

        [Title("4. Movement Dynamics")]
        [SerializeField] private Vector3 arenaCenter = Vector3.zero;
        [SerializeField] private float arenaRadius = 40f;
        [SerializeField] private float circularMoveSpeed = 4f;
        [SerializeField] private float repositionMoveSpeed = 6f;
        [SerializeField] private int invisibilityCharges = 2;
        [SerializeField] private float invisibilityDuration = 3f;
        [SerializeField] private int teleportCharges = 3;
        [Range(0f, 1f)]
        [SerializeField] private float invisibilityChance = 0.4f;
        [Range(0f, 1f)]
        [SerializeField] private float teleportChance = 0.4f;

        [Title("5. Aim Telegraphed Lines")]
        [SerializeField] private LineRenderer centerLine;
        [SerializeField] private LineRenderer leftLine;
        [SerializeField] private LineRenderer rightLine;
        [SerializeField] private float burstSpreadAngle = 15f;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color firingColor = Color.red;
        [SerializeField] private float firingWarningTime = 0.5f;
        [SerializeField] private LayerMask lineCastLayerMask;
        [SerializeField] private float lineMaxDistance = 100f;

        [Title("6. Unity Behavior Integration")]
        [SerializeField] private BehaviorGraphAgent behaviorAgent;

        private NavMeshAgent agent;
        private Transform playerTransform;
        private Health health;
        private float stateTimer;
        private CancellationTokenSource attackCts;

        public BossState CurrentState { get; private set; }

        private Vector3 lockedPlayerPosition;
        private bool isAimLockSet;
        private AttackType nextAttackType = AttackType.None;
        private float totalAttackWeight;
        private Transform aimPointOriginalParent;

        // Zero-GC detection buffer
        private readonly Collider[] detectionColliders = new Collider[2];

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();

            if (centerLine == null)
            {
                centerLine = GetComponent<LineRenderer>();
            }

            if (aimPoint != null)
            {
                aimPointOriginalParent = aimPoint.parent;
            }

            if (behaviorAgent == null)
            {
                behaviorAgent = GetComponent<BehaviorGraphAgent>();
            }

            totalAttackWeight = 0f;
            for (int i = 0; i < attackChoices.Count; i++)
            {
                totalAttackWeight += attackChoices[i].probabilityWeight;
            }
        }

        private void Start()
        {
            TransitionTo(BossState.Waiting);
            DisableAllAimLines();
        }

        private void OnDisable()
        {
            CancelAttackTask();
        }

        private void OnDestroy()
        {
            CancelAttackTask();
        }

        private void CancelAttackTask()
        {
            if (attackCts != null)
            {
                attackCts.Cancel();
                attackCts.Dispose();
                attackCts = null;
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                int hitCount = Physics.OverlapSphereNonAlloc(transform.position, maxEngagementRange, detectionColliders, playerLayer);
                if (hitCount > 0 && detectionColliders[0] != null)
                {
                    playerTransform = detectionColliders[0].transform;
                }
            }

            if (playerTransform == null) return;

            if (behaviorAgent != null && behaviorAgent.Graph != null)
            {
                SyncBlackboardVariables();
                return;
            }

            switch (CurrentState)
            {
                case BossState.Waiting:
                    HandleWaitingState();
                    break;
                case BossState.Hunting:
                    HandleHuntingState();
                    break;
                case BossState.Aiming:
                    HandleAimingState();
                    break;
                case BossState.Attacking:
                    HandleAttackingState();
                    break;
                case BossState.Cooldown:
                    HandleCooldownState();
                    break;
                case BossState.Deciding:
                    HandleDecidingState();
                    break;
                case BossState.Repositioning:
                    HandleRepositioningState();
                    break;
                case BossState.Invisibility:
                    HandleInvisibilityState();
                    break;
                case BossState.Teleporting:
                    HandleTeleportingState();
                    break;
            }
        }

        private void HandleWaitingState()
        {
            if (IsPlayerInEngagementRange()) TransitionTo(BossState.Hunting);
        }

        private void HandleHuntingState()
        {
            if (!IsPlayerInEngagementRange())
            {
                TransitionTo(BossState.Waiting);
                return;
            }

            LookAtPlayer();

            float sqrDist = (transform.position - playerTransform.position).sqrMagnitude;
            float minRangeSqr = minFiringRange * minFiringRange;
            float idealRangeWithBufferSqr = (idealFiringRange + 2f) * (idealFiringRange + 2f);

            if (sqrDist < minRangeSqr)
            {
                TransitionTo(BossState.Deciding);
            }
            else if (sqrDist > idealRangeWithBufferSqr)
            {
                agent.isStopped = false;
                agent.SetDestination(GetClampedPosition(playerTransform.position));
            }
            else
            {
                if (nextAttackType != AttackType.None)
                {
                    TransitionTo(BossState.Aiming);
                }
                else
                {
                    if (UnityEngine.Random.value < 0.1f)
                    {
                        TransitionTo(BossState.Deciding);
                    }
                    else
                    {
                        agent.isStopped = false;
                        MoveCircularly();
                    }
                }
            }
        }

        private void HandleAimingState()
        {
            agent.isStopped = true;
            stateTimer -= Time.deltaTime;

            if (playerTransform == null || !IsPlayerInEngagementRange())
            {
                TransitionTo(BossState.Waiting);
                return;
            }

            Vector3 targetPosition;
            Color currentAimColor;

            if (stateTimer > firingWarningTime)
            {
                targetPosition = playerTransform.position;
                currentAimColor = warningColor;
                isAimLockSet = false;
            }
            else
            {
                if (!isAimLockSet)
                {
                    lockedPlayerPosition = playerTransform.position;
                    isAimLockSet = true;
                }
                targetPosition = lockedPlayerPosition;
                currentAimColor = firingColor;
            }

            AimAtTarget(targetPosition);

            Transform spawnPoint = (aimPoint != null) ? aimPoint : transform;
            Vector3 startPos = spawnPoint.position;
            Vector3 forwardDirection = spawnPoint.forward;

            DisableAllAimLines();

            if (nextAttackType == AttackType.Single)
            {
                if (centerLine != null)
                {
                    centerLine.enabled = true;
                    DrawAimLine(centerLine, startPos, forwardDirection, currentAimColor);
                }
            }
            else if (nextAttackType == AttackType.Burst)
            {
                if (centerLine != null)
                {
                    centerLine.enabled = true;
                    DrawAimLine(centerLine, startPos, forwardDirection, currentAimColor);
                }
                if (leftLine != null && rightLine != null)
                {
                    leftLine.enabled = true;
                    rightLine.enabled = true;
                    Vector3 leftDirection = Quaternion.Euler(0, -burstSpreadAngle, 0) * forwardDirection;
                    Vector3 rightDirection = Quaternion.Euler(0, burstSpreadAngle, 0) * forwardDirection;
                    DrawAimLine(leftLine, startPos, leftDirection, currentAimColor);
                    DrawAimLine(rightLine, startPos, rightDirection, currentAimColor);
                }
            }

            if (stateTimer <= 0f)
            {
                if (!isAimLockSet)
                {
                    lockedPlayerPosition = targetPosition;
                    isAimLockSet = true;
                }
                TransitionTo(BossState.Attacking);
            }
        }

        private void HandleAttackingState()
        {
            if (!IsPlayerInEngagementRange())
            {
                TransitionTo(BossState.Waiting);
            }
        }

        private void HandleCooldownState()
        {
            if (!IsPlayerInEngagementRange())
            {
                TransitionTo(BossState.Waiting);
                return;
            }

            agent.isStopped = true;
            LookAtPlayer();
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f) TransitionTo(BossState.Deciding);
        }

        private void HandleDecidingState()
        {
            float choice = UnityEngine.Random.value;
            if (invisibilityCharges > 0 && choice < invisibilityChance)
            {
                TransitionTo(BossState.Invisibility);
                return;
            }

            choice = UnityEngine.Random.value;
            if (teleportCharges > 0 && choice < teleportChance)
            {
                TransitionTo(BossState.Teleporting);
                return;
            }

            if (UnityEngine.Random.value > 0.3f && attackChoices.Count > 0)
            {
                nextAttackType = GetRandomAttackType();
                TransitionTo(BossState.Hunting);
            }
            else
            {
                nextAttackType = AttackType.None;
                TransitionTo(BossState.Repositioning);
            }
        }

        private void HandleRepositioningState()
        {
            if (!IsPlayerInEngagementRange())
            {
                TransitionTo(BossState.Waiting);
                return;
            }

            LookAtPlayer();
            if (!agent.pathPending || agent.remainingDistance < 1.0f)
            {
                TransitionTo(BossState.Deciding);
            }
        }

        private void HandleInvisibilityState()
        {
            if (!IsPlayerInEngagementRange())
            {
                TransitionTo(BossState.Waiting);
                return;
            }

            if (!agent.pathPending || agent.remainingDistance < 1.0f)
            {
                SetNewRepositionDestination(false);
            }

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                TransitionTo(BossState.Deciding);
            }
        }

        private void HandleTeleportingState() { }

        private void TransitionTo(BossState newState)
        {
            if (CurrentState == newState) return;

            CancelAttackTask();

            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            if (newState != BossState.Aiming && newState != BossState.Attacking)
            {
                DisableAllAimLines();
                if (aimPoint != null && aimPoint.parent != aimPointOriginalParent)
                {
                    aimPoint.SetParent(aimPointOriginalParent, true);
                    aimPoint.localRotation = Quaternion.identity;
                }
            }

            switch (CurrentState)
            {
                case BossState.Waiting:
                    agent.isStopped = true;
                    nextAttackType = AttackType.None;
                    break;
                case BossState.Hunting:
                    agent.isStopped = false;
                    agent.speed = circularMoveSpeed;
                    break;
                case BossState.Aiming:
                    agent.isStopped = true;
                    stateTimer = aimTime;
                    isAimLockSet = false;
                    break;
                case BossState.Attacking:
                    agent.isStopped = true;
                    if (aimPoint != null)
                    {
                        aimPoint.SetParent(null, true);
                    }

                    attackCts = new CancellationTokenSource();
                    if (nextAttackType == AttackType.Burst)
                    {
                        ExecuteAttackBurstAsync(attackCts.Token).Forget();
                    }
                    else
                    {
                        ExecuteAttackSingleAsync(attackCts.Token).Forget();
                    }

                    OnAttackTrigger?.Invoke();
                    break;
                case BossState.Cooldown:
                    agent.isStopped = true;
                    stateTimer = cooldownTime;
                    nextAttackType = AttackType.None;
                    break;
                case BossState.Deciding:
                    HandleDecidingState();
                    break;
                case BossState.Repositioning:
                    agent.isStopped = false;
                    agent.speed = repositionMoveSpeed;
                    SetNewRepositionDestination(false);
                    break;
                case BossState.Invisibility:
                    agent.isStopped = false;
                    agent.speed = repositionMoveSpeed;
                    invisibilityCharges--;
                    FireAbility(invisibilitySO);
                    SetNewRepositionDestination(false);
                    stateTimer = invisibilityDuration;
                    OnInvisibilityTrigger?.Invoke();
                    break;
                case BossState.Teleporting:
                    agent.isStopped = true;
                    teleportCharges--;
                    FireAbility(teleportSO);
                    Vector3 newPos = SetNewRepositionDestination(true);
                    agent.Warp(newPos);
                    OnTeleportTrigger?.Invoke();
                    TransitionTo(BossState.Cooldown);
                    break;
            }
        }

        private async UniTaskVoid ExecuteAttackBurstAsync(CancellationToken ct)
        {
            for (int i = 0; i < 3; i++)
            {
                AbilitySO shot = GetRandomAttackSO();
                FireAbility(shot);

                bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(burstFireDelay), cancellationToken: ct).SuppressCancellationThrow();
                if (canceled) return;
            }

            TransitionTo(BossState.Cooldown);
        }

        private async UniTaskVoid ExecuteAttackSingleAsync(CancellationToken ct)
        {
            FireAbility(singleShotSO);
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(burstFireDelay), cancellationToken: ct).SuppressCancellationThrow();
            if (!canceled)
            {
                TransitionTo(BossState.Cooldown);
            }
        }

        private AbilitySO GetRandomAttackSO()
        {
            return UnityEngine.Random.value > 0.5f ? singleShotSO : burstShotSO;
        }

        private AttackType GetRandomAttackType()
        {
            float randomValue = UnityEngine.Random.Range(0f, totalAttackWeight);
            float cumulativeWeight = 0f;

            for (int i = 0; i < attackChoices.Count; i++)
            {
                cumulativeWeight += attackChoices[i].probabilityWeight;
                if (randomValue <= cumulativeWeight)
                {
                    return attackChoices[i].attackType;
                }
            }

            return AttackType.Single;
        }

        private bool IsPlayerInEngagementRange()
        {
            if (playerTransform == null) return false;
            float sqrDist = (transform.position - playerTransform.position).sqrMagnitude;
            return sqrDist <= (maxEngagementRange * maxEngagementRange);
        }

        private void FireAbility(AbilitySO ability)
        {
            if (ability == null) return;

            if (ability is IAimable aimableAbility)
            {
                aimableAbility.SetAimSource((aimPoint != null) ? aimPoint : transform);
            }

            ability.Execute(gameObject);
        }

        private void LookAtPlayer()
        {
            if (playerTransform == null) return;
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed);
        }

        private void AimAtTarget(Vector3 targetPosition)
        {
            if (playerTransform == null) return;

            Vector3 bodyDirection = (targetPosition - transform.position).normalized;
            Quaternion bodyRotation = Quaternion.LookRotation(new Vector3(bodyDirection.x, 0, bodyDirection.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, bodyRotation, Time.deltaTime * agent.angularSpeed * 2f);

            if (aimPoint != null)
            {
                Vector3 aimDirection = (targetPosition - aimPoint.position).normalized;
                Quaternion aimRotation = Quaternion.LookRotation(aimDirection);
                aimPoint.rotation = Quaternion.Slerp(aimPoint.rotation, aimRotation, Time.deltaTime * agent.angularSpeed * 4f);
            }
        }

        private void DrawAimLine(LineRenderer line, Vector3 startPos, Vector3 direction, Color color)
        {
            if (line == null) return;

            Vector3 endPos;
            if (Physics.Raycast(startPos, direction, out RaycastHit hit, lineMaxDistance, lineCastLayerMask))
            {
                endPos = hit.point;
            }
            else
            {
                endPos = startPos + (direction * lineMaxDistance);
            }

            line.SetPosition(0, startPos);
            line.SetPosition(1, endPos);
            line.startColor = color;
            line.endColor = color;
        }

        private void DisableAllAimLines()
        {
            if (centerLine != null) centerLine.enabled = false;
            if (leftLine != null) leftLine.enabled = false;
            if (rightLine != null) rightLine.enabled = false;
        }

        private void MoveCircularly()
        {
            if (playerTransform == null) return;
            Vector3 strafeDirection = transform.right * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
            Vector3 targetPos = transform.position + strafeDirection * 5f;
            agent.SetDestination(GetClampedPosition(targetPos));
        }

        private Vector3 SetNewRepositionDestination(bool teleportToFarSide)
        {
            if (playerTransform == null)
            {
                TransitionTo(BossState.Waiting);
                return transform.position;
            }

            Vector3 finalDestination;
            if (teleportToFarSide)
            {
                Vector3 dirFromCenter = (playerTransform.position - arenaCenter).normalized;
                Vector3 oppositeDir = -dirFromCenter;
                finalDestination = arenaCenter + oppositeDir * (arenaRadius * 0.8f);
            }
            else
            {
                Vector3 dirAwayFromPlayer = (transform.position - playerTransform.position).normalized;
                finalDestination = transform.position + dirAwayFromPlayer * repositionDistance;
            }

            if (NavMesh.SamplePosition(GetClampedPosition(finalDestination), out NavMeshHit hit, repositionDistance * 1.5f, NavMesh.AllAreas))
            {
                if (!teleportToFarSide) agent.SetDestination(hit.position);
                return hit.position;
            }
            else
            {
                Vector3 randomPos = arenaCenter + (Vector3)UnityEngine.Random.insideUnitCircle * arenaRadius;
                if (!teleportToFarSide) agent.SetDestination(randomPos);
                return randomPos;
            }
        }

        public void TriggerAttack() => OnAttackTrigger?.Invoke();
        public void TriggerTeleport() => OnTeleportTrigger?.Invoke();
        public void TriggerInvisibility() => OnInvisibilityTrigger?.Invoke();
        public void SetState(BossState state) => TransitionTo(state);

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
            behaviorAgent.SetVariableValue("ArenaCenter", arenaCenter);
            behaviorAgent.SetVariableValue("ArenaRadius", arenaRadius);
            behaviorAgent.SetVariableValue("RepositionDistance", repositionDistance);
            behaviorAgent.SetVariableValue("AimDuration", aimTime);
            if (centerLine != null)
            {
                behaviorAgent.SetVariableValue("AimLine", centerLine);
            }
            if (burstShotSO != null)
            {
                behaviorAgent.SetVariableValue("AbilityToUse", burstShotSO);
            }
        }

        private Vector3 GetClampedPosition(Vector3 position)
        {
            Vector3 fromCenter = position - arenaCenter;
            if (fromCenter.sqrMagnitude > arenaRadius * arenaRadius)
            {
                return arenaCenter + fromCenter.normalized * arenaRadius;
            }
            return position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(arenaCenter, arenaRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxEngagementRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, idealFiringRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minFiringRange);
        }
    }
}
