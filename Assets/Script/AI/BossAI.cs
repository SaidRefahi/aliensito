// Ruta: Assets/Scripts/AI/BossAI.cs
// ACCIÓN: ¡Reemplaza tu script anterior con esta versión MÁS TÁCTICA!

using UnityEngine;
using UnityEngine.AI;
using System.Collections; 

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(StatusEffectManager))]
[RequireComponent(typeof(TargetingProfile))] 
public class BossAI : MonoBehaviour
{
    // --- FSM DE JEFE ---
    private enum BossState
    {
        Waiting,
        Chasing,
        Aiming,
        Attacking,
        Cooldown,
        Deciding,
        Repositioning,
        Invisibility // ¡Ahora es "Reposicionamiento Invisible"!
    }
    private BossState currentState;
    // ---------------------------------

    [Header("1. Configuración de Detección (La 'Arena')")]
    [Tooltip("¡El Jefe 'siempre te ve' y te atacará si estás dentro de este rango!")]
    [SerializeField] private float maxEngagementRange = 50f;
    [SerializeField] private LayerMask playerLayer;

    [Header("2. Habilidades de Combate (¡KISS!)")]
    [SerializeField] private AbilitySO primaryAttackSO;
    [SerializeField] private AbilitySO multiAttackSO;
    [SerializeField] private AbilitySO invisibilityAbilitySO;
    [SerializeField] private Transform aimPoint;

    [Header("3. Tiempos y Tácticas")]
    [SerializeField] private float idealFiringRange = 20f;
    [SerializeField] private float minFiringRange = 8f;
    [SerializeField] private float aimTime = 1.0f;
    [SerializeField] private float burstFireDelay = 0.3f;
    [SerializeField] private float cooldownTime = 1.5f;
    [SerializeField] private float repositionDistance = 12f;
    [SerializeField] private int invisibilityCharges = 2;
    [SerializeField] private float invisibilityDuration = 3f;
    [Range(0f, 1f)]
    [SerializeField] private float invisibilityChance = 0.4f;

    // --- Componentes y Estado Interno ---
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Health health; 
    
    private float stateTimer; 
    private Coroutine attackCoroutine; 

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        TransitionTo(BossState.Waiting);
    }

    private void Update()
    {
        // El "cerebro" de la FSM. ¡Puro SRP!
        switch (currentState)
        {
            case BossState.Waiting:
                HandleWaitingState();
                break;
            case BossState.Chasing:
                HandleChasingState();
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
        }
    }

    // --- MANEJADORES DE ESTADO ---

    private void HandleWaitingState()
    {
        if (playerTransform == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, maxEngagementRange, playerLayer);
            if (hits.Length > 0)
            {
                playerTransform = hits[0].transform;
                Debug.LogWarning("¡¡JEFE HA DETECTADO AL JUGADOR!! ¡¡QUE EMPIECE LA FIESTA!!");
                TransitionTo(BossState.Chasing);
            }
            else
            {
                return; 
            }
        }

        if (IsPlayerInEngagementRange())
        {
            TransitionTo(BossState.Chasing);
        }
    }

    private void HandleChasingState()
    {
        if (!IsPlayerInEngagementRange())
        {
            TransitionTo(BossState.Waiting);
            return;
        }

        LookAtPlayer();
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < minFiringRange)
        {
            TransitionTo(BossState.Repositioning);
        }
        else if (distance > idealFiringRange)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);
        }
        else
        {
            TransitionTo(BossState.Aiming);
        }
    }

    private void HandleAimingState()
    {
        if (!IsPlayerInEngagementRange())
        {
            TransitionTo(BossState.Waiting);
            return;
        }
        
        agent.isStopped = true;
        LookAtPlayer();

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
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
        LookAtPlayer();
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
        if (stateTimer <= 0f)
        {
            TransitionTo(BossState.Deciding);
        }
    }

    private void HandleDecidingState()
    {
        if (invisibilityCharges > 0 && Random.value < invisibilityChance)
        {
            TransitionTo(BossState.Invisibility);
        }
        else
        {
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
        
        if (!agent.pathPending && agent.remainingDistance < 1.0f)
        {
            TransitionTo(BossState.Chasing); 
        }
    }

    // --- ¡¡MÉTODO MODIFICADO (EL ARREGLO)!! ---
    private void HandleInvisibilityState()
    {
        if (!IsPlayerInEngagementRange())
        {
            TransitionTo(BossState.Waiting);
            return;
        }
        
        // 1. ACCIÓN: ¡Sigue mirando al jugador mientras te mueves!
        LookAtPlayer();

        // 2. TRANSICIÓN: ¿Se acabó el tiempo de invisibilidad?
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            // ¡Tiempo fuera! Vuelve a cazar.
            TransitionTo(BossState.Chasing);
        }
        
        // 3. TRANSICIÓN: ¿Llegaste al destino ANTES de que se acabe el tiempo?
        if (!agent.pathPending && agent.remainingDistance < 1.0f)
        {
            // ¡Llegaste! Quédate quieto y "acecha"
            // hasta que se acabe el tiempo.
            agent.isStopped = true;
        }
    }

    // --- LÓGICA DE TRANSICIÓN (El "OnEnter" de cada estado) ---

    private void TransitionTo(BossState newState)
    {
        if (currentState == newState) return; 
        
        if (currentState == BossState.Attacking && attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        currentState = newState;
        
        switch (currentState)
        {
            case BossState.Waiting:
                agent.isStopped = true;
                break;
                
            case BossState.Chasing:
                agent.isStopped = false;
                agent.speed = 3.5f; 
                break;
                
            case BossState.Aiming:
                agent.isStopped = true;
                stateTimer = aimTime;
                break;
                
            case BossState.Attacking:
                agent.isStopped = true;
                attackCoroutine = StartCoroutine(AttackBurstCoroutine());
                break;
                
            case BossState.Cooldown:
                agent.isStopped = true;
                stateTimer = cooldownTime;
                break;
                
            case BossState.Deciding:
                HandleDecidingState(); 
                break;
                
            case BossState.Repositioning:
                agent.isStopped = false;
                agent.speed = 5f; 
                SetNewRepositionDestination();
                break;
                
            // --- ¡¡LÓGICA 'OnEnter' MODIFICADA!! ---
            case BossState.Invisibility:
                // ¡¡AQUÍ ESTÁ LA MAGIA!!
                agent.isStopped = false; // ¡NO TE QUEDES TIESO!
                agent.speed = 6f; // ¡Velocidad de reposicionamiento sigiloso!
                SetNewRepositionDestination(); // ¡BUSCA UN NUEVO PUNTO!
                
                invisibilityCharges--;
                FireAbility(invisibilityAbilitySO); // ¡ACTIVA EL PODER!
                stateTimer = invisibilityDuration; // ¡INICIA EL TIMER!
                break;
        }
    }

    // --- ¡¡LA RÁFAGA DE 3 DISPAROS!! ---

    private IEnumerator AttackBurstCoroutine()
    {
        LookAtPlayer();
        FireAbility(primaryAttackSO);
        yield return new WaitForSeconds(burstFireDelay);

        LookAtPlayer();
        FireAbility(multiAttackSO);
        yield return new WaitForSeconds(burstFireDelay);
        
        LookAtPlayer();
        AbilitySO finalShot = (Random.value > 0.5f) ? primaryAttackSO : multiAttackSO;
        FireAbility(finalShot);
        yield return new WaitForSeconds(burstFireDelay / 2); 

        TransitionTo(BossState.Cooldown);
    }

    // --- LÓGICA DE ACCIONES (Los "Músculos") ---

    private bool IsPlayerInEngagementRange()
    {
        if (playerTransform == null) return false;
        return Vector3.Distance(transform.position, playerTransform.position) <= maxEngagementRange;
    }

    private void FireAbility(AbilitySO ability)
    {
        if (ability == null)
        {
            Debug.LogError(gameObject.name + ": ¡No tiene una habilidad asignada para esta acción!", this);
            return;
        }

        if (ability is IAimable aimableAbility)
        {
            aimableAbility.aimSource = (aimPoint != null) ? aimPoint : transform;
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

    private void SetNewRepositionDestination()
    {
        if (playerTransform == null)
        {
            TransitionTo(BossState.Waiting);
            return;
        }

        Vector3 dirAwayFromPlayer = (transform.position - playerTransform.position).normalized;
        Vector3 finalDestination = transform.position + dirAwayFromPlayer * repositionDistance;
        
        if (NavMesh.SamplePosition(finalDestination, out NavMeshHit hit, repositionDistance * 1.5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Vector3 strafeDir = transform.right * (Random.value > 0.5f ? 1f : -1f);
            finalDestination = transform.position + strafeDir * repositionDistance;
            
            if(NavMesh.SamplePosition(finalDestination, out NavMeshHit strafeHit, repositionDistance * 1.5f, NavMesh.AllAreas))
            {
                agent.SetDestination(strafeHit.position);
            }
        }
    }
    
    // --- GIZMOS ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxEngagementRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, idealFiringRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minFiringRange);
    }
}