using UnityEngine;
using UnityEngine.AI;
using System.Collections; 
using System; 
using System.Collections.Generic; // Para Listas
using System.Linq; // Para Sum()

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(StatusEffectManager))]
[RequireComponent(typeof(TargetingProfile))] 
public class BossAI : MonoBehaviour
{
    // --- FSM DE JEFE ---
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
    
    // 'public' para arreglar el error CS0052
    public enum AttackType { None, Single, Burst }

    // --- CLASE HELPER PARA EL "PLAYBOOK" ---
    [System.Serializable]
    public class WeightedAttackChoice
    {
        [Tooltip("El tipo de ataque (Single o Burst) que se decidió.")]
        public AttackType attackType;
        [Tooltip("La probabilidad de esta 'jugada'. Un valor de 2 es el doble de probable que uno de 1.")]
        public float probabilityWeight = 1f;
    }

    // --- EVENTOS ---
    public event Action<BossState> OnStateChanged;
    public event Action OnAttackTrigger;
    public event Action OnTeleportTrigger;
    public event Action OnInvisibilityTrigger;
    
    [Header("1. Configuración de Detección (La 'Arena')")]
    [SerializeField] private float maxEngagementRange = 50f;
    [SerializeField] private LayerMask playerLayer;

    [Header("2. Habilidades de Combate (Asignar SOs)")]
    [SerializeField] private AbilitySO singleShotSO; 
    [SerializeField] private AbilitySO burstShotSO;
    [SerializeField] private AbilitySO invisibilitySO;
    [SerializeField] private AbilitySO teleportSO; 
    [Tooltip("¡IMPORTANTE! El punto (Transform) desde donde se originan los disparos y las líneas de mira.")]
    [SerializeField] private Transform aimPoint;

    [Header("3. Tiempos y Tácticas")]
    [Tooltip("El 'Libro de Jugadas' del Jefe. Define las 'jugadas' de ataque y sus probabilidades.")]
    [SerializeField] private List<WeightedAttackChoice> attackChoices;
    
    [SerializeField] private float idealFiringRange = 20f;
    [SerializeField] private float minFiringRange = 8f;
    [SerializeField] private float aimTime = 1.0f;
    [SerializeField] private float burstFireDelay = 0.3f;
    [SerializeField] private float cooldownTime = 1.5f;
    [SerializeField] private float repositionDistance = 12f;

    [Header("4. Tácticas de Movimiento")]
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

    [Header("5. Telegrafía (Aiming)")]
    [SerializeField] private LineRenderer centerLine;
    [SerializeField] private LineRenderer leftLine;
    [SerializeField] private LineRenderer rightLine;
    [SerializeField] private float burstSpreadAngle = 15f;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color firingColor = Color.red;
    [Tooltip("Segundos ANTES de disparar que la línea se pone roja y se fija la mira.")]
    [SerializeField] private float firingWarningTime = 0.5f;
    [SerializeField] private LayerMask lineCastLayerMask;
    [SerializeField] private float lineMaxDistance = 100f;

    // --- Componentes y Estado Interno ---
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Health health; 
    private float stateTimer; 
    private Coroutine attackCoroutine; 
    
    public BossState CurrentState { get; private set; }
    
    private Vector3 lockedPlayerPosition;
    
    // --- ¡¡LÍNEA CORREGIDA!! ---
    // (Añadimos la variable que faltaba)
    private bool isAimLockSet;
    // -------------------------

    private AttackType nextAttackType = AttackType.None;

    private Vector3 lockedAimDirection; 
    private Vector3 lockedAimStartPos;
    
    private float totalAttackWeight;
    private Transform aimPointOriginalParent;

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
        else
        {
            Debug.LogWarning("¡BossAI no tiene un 'aimPoint' asignado! Las líneas y proyectiles saldrán desde el 'transform'.", this);
        }
        
        totalAttackWeight = 0f;
        foreach (var choice in attackChoices)
        {
            totalAttackWeight += choice.probabilityWeight;
        }
    }

    private void Start()
    {
        TransitionTo(BossState.Waiting);
        DisableAllAimLines();
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, maxEngagementRange, playerLayer);
            if (hits.Length > 0)
            {
                playerTransform = hits[0].transform;
            }
        }
        
        if (playerTransform == null) return;
        
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

    // --- MANEJADORES DE ESTADO ---

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
        LookAtPlayer(); // Solo gira el cuerpo
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < minFiringRange)
        {
            TransitionTo(BossState.Deciding);
        }
        else if (distance > idealFiringRange + 2f)
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
                if(UnityEngine.Random.value < 0.1f)
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
            
            // Guardamos la info de disparo JUSTO AHORA
            lockedAimStartPos = spawnPoint.position;
            lockedAimDirection = spawnPoint.forward;
            
            TransitionTo(BossState.Attacking);
        }
    }

    private void HandleAttackingState()
    {
        if (!IsPlayerInEngagementRange())
        {
            TransitionTo(BossState.Waiting);
            return;
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
        LookAtPlayer(); // Gira el cuerpo
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

    private void HandleTeleportingState()
    {
        // Lógica en TransitionTo
    }


    // --- LÓGICA DE TRANSICIÓN ---
    private void TransitionTo(BossState newState)
    {
        if (CurrentState == newState) return; 
        
        if (CurrentState == BossState.Attacking && attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        CurrentState = newState;
        
        OnStateChanged?.Invoke(newState);

        if (newState != BossState.Aiming && newState != BossState.Attacking)
        {
            DisableAllAimLines();
        }
        
        // Lógica de Re-vincular (Re-attach)
        if (CurrentState != BossState.Aiming && CurrentState != BossState.Attacking)
        {
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
                
                // Desvinculamos el 'aimPoint'
                if (aimPoint != null)
                {
                    aimPoint.SetParent(null, true); 
                }
                
                if (nextAttackType == AttackType.Burst)
                {
                    attackCoroutine = StartCoroutine(AttackBurstCoroutine());
                }
                else 
                {
                    attackCoroutine = StartCoroutine(AttackSingleCoroutine());
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

    // --- CORRUTINAS DE ATAQUE ---
    
    private IEnumerator AttackBurstCoroutine()
    {
        // Las líneas ya están rojas y fijas desde 'Aiming'
        
        AbilitySO shot1 = GetRandomAttackSO();
        FireAbility(shot1);
        yield return new WaitForSeconds(burstFireDelay);

        AbilitySO shot2 = GetRandomAttackSO();
        FireAbility(shot2);
        yield return new WaitForSeconds(burstFireDelay);

        AbilitySO shot3 = GetRandomAttackSO();
        FireAbility(shot3);
        yield return new WaitForSeconds(burstFireDelay);
        
        TransitionTo(BossState.Cooldown);
    }
    
    private IEnumerator AttackSingleCoroutine()
    {
        FireAbility(singleShotSO);
        yield return new WaitForSeconds(burstFireDelay);
        
        TransitionTo(BossState.Cooldown);
    }

    private AbilitySO GetRandomAttackSO()
    {
        if (UnityEngine.Random.value > 0.5f)
        {
            return singleShotSO;
        }
        else
        {
            return burstShotSO;
        }
    }
    
    
    // --- LÓGICA DE ACCIONES Y HELPERS ---
    
    private AttackType GetRandomAttackType()
    {
        float randomValue = UnityEngine.Random.Range(0f, totalAttackWeight);
        float cumulativeWeight = 0f;

        foreach (var choice in attackChoices)
        {
            cumulativeWeight += choice.probabilityWeight;
            if (randomValue <= cumulativeWeight)
            {
                return choice.attackType;
            }
        }
        
        return AttackType.Single;
    }
    
    private bool IsPlayerInEngagementRange()
    {
        if (playerTransform == null) return false;
        return Vector3.Distance(transform.position, playerTransform.position) <= maxEngagementRange;
    }
    
    private bool CanSeePlayerClearly()
    {
        if (playerTransform == null) return false;
        return true; 
    }

    private void FireAbility(AbilitySO ability)
    {
        if (ability == null)
        {
            Debug.LogError(gameObject.name + ": ¡Falta asignar un AbilitySO!", this);
            return;
        }

        if (ability is IAimable aimableAbility)
        {
            aimableAbility.aimSource = (aimPoint != null) ? aimPoint : transform;
        }

        ability.Execute(gameObject);
    }
    
    // Rota el CUERPO del Jefe (plano)
    private void LookAtPlayer()
    {
        if (playerTransform == null) return;
        Vector3 direction = (playerTransform.position - transform.position).normalized; 
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed);
    }

    /// <summary>
    /// Rota el cuerpo (plano) Y el 'aimPoint' (cañón)
    /// </summary>
    private void AimAtTarget(Vector3 targetPosition)
    {
        if (playerTransform == null) return;

        // 1. Rotar el CUERPO
        Vector3 bodyDirection = (targetPosition - transform.position).normalized;
        Quaternion bodyRotation = Quaternion.LookRotation(new Vector3(bodyDirection.x, 0, bodyDirection.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, bodyRotation, Time.deltaTime * agent.angularSpeed * 2f); 

        // 2. Rotar el 'aimPoint'
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
            if(!teleportToFarSide) agent.SetDestination(hit.position);
            return hit.position; 
        }
        else
        {
            Vector3 randomPos = arenaCenter + (Vector3)UnityEngine.Random.insideUnitCircle * arenaRadius;
            if(!teleportToFarSide) agent.SetDestination(randomPos);
            return randomPos; 
        }
    }

    private Vector3 GetClampedPosition(Vector3 position)
    {
        Vector3 fromCenter = position - arenaCenter;
        if (fromCenter.magnitude > arenaRadius)
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