using UnityEngine;
using UnityEngine.AI;
using System; 

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

    [Header("1. Configuración de Detección")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float loseSightRadius = 20f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float viewAngle = 90f;

    [Header("2. Configuración de Combate")]
    [SerializeField] private AbilitySO rangedAbility;
    [SerializeField] private Transform aimPoint;
    [SerializeField] private float closeRange = 7f;
    [SerializeField] private float farRange = 18f;
    [SerializeField] private float defensiveHealthThreshold = 0.5f;
    [SerializeField] private float repositionDistance = 8f;

    [Header("3. Configuración de Tiempos")]
    [SerializeField] private float aimTime = 1.5f;
    [SerializeField] private float cooldownTime = 1f;

    [Header("4. Configuración de Paseo (Wander)")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimerMax = 5f;
    
    [Header("5. Animación")]
    [Tooltip("Arrastra aquí el Animator del modelo del enemigo.")]
    [SerializeField] private Animator animator;
    
    // Hashes de parámetros (más rápido que usar strings)
    private readonly int isMovingHash = Animator.StringToHash("isMoving");
    private readonly int isAimingHash = Animator.StringToHash("isAiming");
    private readonly int attackHash = Animator.StringToHash("attack");

    // --- Componentes y Estado Interno ---
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Health health;
    private float stateTimer;
    private float wanderTimer;
    private float originalSpeed;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        originalSpeed = agent.speed;
        
        // Si no se asigna el animator en el inspector, lo busca en los hijos
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        TransitionTo(AIState.Wandering);
    }

    private void Update()
    {
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

        // --- ACTUALIZACIÓN DE ANIMACIÓN DE MOVIMIENTO ---
        // Actualiza el parámetro de velocidad del NavMeshAgent al Animator
        // Esto es mejor que usar un bool "isMoving" porque funciona para
        // Wandering y Repositioning automáticamente.
        if (animator != null)
        {
            // agent.velocity.magnitude es la velocidad actual real del enemigo
            animator.SetFloat("moveSpeed", agent.velocity.magnitude);
        }
    }

    // --- MANEJADORES DE ESTADO ---

    private void HandleWanderingState()
    {
        Wander();
        if (IsPlayerInSight()) TransitionTo(AIState.Aiming);
    }

    private void HandleAimingState()
    {
        agent.isStopped = true;
        LookAtPlayer();
        stateTimer -= Time.deltaTime;
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

    // --- LÓGICA DE TRANSICIÓN ---

    private void TransitionTo(AIState newState)
    {
        currentState = newState;
        
        // --- LÓGICA DE ANIMACIÓN CENTRALIZADA ---
        // Al cambiar de estado, actualizamos los triggers y bools del animator.
        if (animator != null)
        {
            // Resetea los bools. El movimiento se maneja en Update con un Float.
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
        // ----------------------------------------
        
        // Lógica de "OnEnter" para cada estado
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

    // --- LÓGICA DE ACCIONES ---

    private bool IsPlayerInSight()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hitColliders.Length > 0)
        {
            Transform target = hitColliders[0].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, ~playerLayer))
                {
                    playerTransform = target;
                    return true;
                }
            }
        }
        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > loseSightRadius)
        {
            playerTransform = null;
            return false;
        }
        return playerTransform != null;
    }

    private void FireAbility()
    {
        if (rangedAbility == null) return;
        if (rangedAbility is IAimable aimableAbility)
        {
            aimableAbility.aimSource = (aimPoint != null) ? aimPoint : transform;
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
        
        float currentTargetRange;
        float currentHealthPercent = health.GetCurrentHealth() / health.GetMaxHealth();

        if (currentHealthPercent > defensiveHealthThreshold)
            currentTargetRange = closeRange;
        else
            currentTargetRange = farRange;

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
        
        Vector3 fovLine1 = Quaternion.AngleAxis(viewAngle / 2, transform.up) * transform.forward * detectionRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-viewAngle / 2, transform.up) * transform.forward * detectionRadius;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);
    }
}