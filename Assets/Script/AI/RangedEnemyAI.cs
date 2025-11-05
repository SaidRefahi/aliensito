// Ruta: Assets/Scripts/AI/RangedEnemyAI.cs
// ACCIÓN: ¡Crea este nuevo script y pégalo!

using UnityEngine;
using UnityEngine.AI;
using System; // Para el Action

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))] // Asumimos que los enemigos tienen vida
[RequireComponent(typeof(StatusEffectManager))] // Asumimos que pueden tener efectos
public class RangedEnemyAI : MonoBehaviour
{
    // --- NUEVO: La Máquina de Estados (FSM) ---
    private enum AIState
    {
        Wandering,      // 1. Paseando
        Aiming,         // 2. Apuntando
        Attacking,      // 3. Atacando
        Cooldown,       // 4. Enfriamiento post-disparo
        Repositioning   // 5. Moviéndose a una nueva posición
    }
    private AIState currentState;
    // ------------------------------------------

    [Header("1. Configuración de Detección")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float loseSightRadius = 20f; // Un poco más grande para que no "parpadee"
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float viewAngle = 90f; // Campo de visión

    [Header("2. Configuración de Combate (¡KISS!)")]
    [Tooltip("El ScriptableObject de habilidad que esta IA usará para disparar")]
    [SerializeField] private AbilitySO rangedAbility;
    [Tooltip("El 'cañón' del arma. Si es nulo, usará el 'transform' del enemigo")]
    [SerializeField] private Transform aimPoint;
    [Tooltip("La distancia IDEAL que intentará mantener del jugador")]
    [SerializeField] private float idealFiringRange = 10f;
    [Tooltip("Qué tan lejos se moverá al reposicionarse")]
    [SerializeField] private float repositionDistance = 8f;

    [Header("3. Configuración de Tiempos")]
    [Tooltip("Segundos que pasa apuntando antes de disparar")]
    [SerializeField] private float aimTime = 1.5f;
    [Tooltip("Segundos que espera quieto después de disparar")]
    [SerializeField] private float cooldownTime = 1f;

    [Header("4. Configuración de Paseo (Wander)")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimerMax = 5f;

    // --- Componentes y Estado Interno ---
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Health health;
    
    private float stateTimer; // Un temporizador multiuso para Aiming y Cooldown
    private float wanderTimer;
    private float originalSpeed;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>(); // ¡Podemos usar esto!
        originalSpeed = agent.speed;
    }

    private void Start()
    {
        // ¡Todos empiezan paseando!
        TransitionTo(AIState.Wandering);
    }

    private void Update()
    {
        // El "cerebro" de la FSM. Solo ejecuta la lógica del estado actual.
        // ¡Puro SRP (Principio de Responsabilidad Única)!
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
    }

    // --- MANEJADORES DE ESTADO ---
    // Cada método es una sola responsabilidad (SRP)

    private void HandleWanderingState()
    {
        // 1. ACCIÓN: Moverse aleatoriamente
        Wander();
        
        // 2. TRANSICIÓN: Buscar al jugador
        if (IsPlayerInSight())
        {
            TransitionTo(AIState.Aiming);
        }
    }

    private void HandleAimingState()
    {
        // 1. ACCIÓN: Detenerse y mirar al jugador
        agent.isStopped = true;
        LookAtPlayer();

        // 2. TRANSICIÓN: Contar tiempo
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            TransitionTo(AIState.Attacking);
        }
        
        // 3. TRANSICIÓN DE SALIDA: Si el jugador se va
        if (!IsPlayerInSight())
        {
            TransitionTo(AIState.Wandering);
        }
    }

    private void HandleAttackingState()
    {
        // 1. ACCIÓN: ¡Fuego!
        FireAbility();
        
        // 2. TRANSICIÓN: Inmediata
        TransitionTo(AIState.Cooldown);
    }

    private void HandleCooldownState()
    {
        // 1. ACCIÓN: Quedarse quieto y mirar
        agent.isStopped = true;
        LookAtPlayer();

        // 2. TRANSICIÓN: Contar tiempo
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            TransitionTo(AIState.Repositioning);
        }
        
        // 3. TRANSICIÓN DE SALIDA: Si el jugador se va
        if (!IsPlayerInSight())
        {
            TransitionTo(AIState.Wandering);
        }
    }

    private void HandleRepositioningState()
    {
        // 1. ACCIÓN: Moverse (el destino se fijó en la transición)
        LookAtPlayer(); // Mira al jugador mientras te mueves
        
        // 2. TRANSICIÓN: Si el jugador se va
        if (!IsPlayerInSight())
        {
            TransitionTo(AIState.Wandering);
        }
        
        // 3. TRANSICIÓN: Al llegar al destino
        if (!agent.pathPending && agent.remainingDistance < 1.0f)
        {
            // ¡Llegamos! Repetir secuencia.
            TransitionTo(AIState.Aiming);
        }
    }

    // --- LÓGICA DE TRANSICIÓN ---
    // Un solo método para manejar todas las transiciones (KISS)

    private void TransitionTo(AIState newState)
    {
        currentState = newState;
        
        // Lógica de "OnEnter" para cada estado
        switch (currentState)
        {
            case AIState.Wandering:
                agent.isStopped = false;
                agent.speed = originalSpeed;
                playerTransform = null;
                wanderTimer = 0f; // Buscar un punto de paseo nuevo
                break;
                
            case AIState.Aiming:
                agent.isStopped = true;
                stateTimer = aimTime; // "se tome un momento para apuntarte"
                break;
                
            case AIState.Attacking:
                // Sin setup, es instantáneo
                break;
                
            case AIState.Cooldown:
                agent.isStopped = true;
                stateTimer = cooldownTime; // "se quede quieto un segundo"
                break;
                
            case AIState.Repositioning:
                agent.isStopped = false;
                agent.speed = originalSpeed;
                SetNewRepositionDestination(); // "se reposicione lejos de ti"
                break;
        }
    }

    // --- LÓGICA DE ACCIONES (Los "Músculos") ---

    private bool IsPlayerInSight()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        if (hitColliders.Length > 0)
        {
            // 1. Vimos a alguien en la layer
            Transform target = hitColliders[0].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // 2. Comprobar si está en nuestro ángulo de visión
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                // 3. Comprobar si hay una pared en medio (Raycast)
                if (!Physics.Raycast(transform.position, dirToTarget, Vector3.Distance(transform.position, target.position), ~playerLayer))
                {
                    playerTransform = target;
                    return true;
                }
            }
        }
        
        // Si el jugador está fuera del radio "loseSightRadius", lo olvidamos
        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > loseSightRadius)
        {
            playerTransform = null;
            return false;
        }
        
        return playerTransform != null; // Mantenemos el target si aún está cerca
    }

    private void FireAbility()
    {
        if (rangedAbility == null)
        {
            Debug.LogError(gameObject.name + ": ¡No tiene 'rangedAbility' asignada!", this);
            return;
        }

        // ¡¡REUTILIZACIÓN DE CÓDIGO (KISS)!!
        // Igual que en el PlayerController, le pasamos el 'aimPoint'
        if (rangedAbility is IAimable aimableAbility)
        {
            aimableAbility.aimSource = (aimPoint != null) ? aimPoint : transform;
        }
        
        // ¡La IA ejecuta la misma habilidad que el jugador!
        rangedAbility.Execute(gameObject);
        
        // Debug.Log(gameObject.name + " ¡FUEGO!", this);
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
            // Si perdimos al jugador, no podemos reposicionarnos
            TransitionTo(AIState.Wandering);
            return;
        }

        // 1. Intenta moverse "lejos" Y "al costado" (strafe)
        Vector3 dirAwayFromPlayer = (transform.position - playerTransform.position).normalized;
        Vector3 strafeDir = new Vector3(dirAwayFromPlayer.z, 0, -dirAwayFromPlayer.x) * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
        
        Vector3 targetPos = transform.position + (strafeDir * repositionDistance);
        
        // 2. Intenta encontrar un punto en el rango ideal
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * idealFiringRange;
        Vector3 idealPos = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // 3. Mezclamos las dos ideas (50/50)
        Vector3 finalDestination = Vector3.Lerp(targetPos, idealPos, 0.5f);

        if (NavMesh.SamplePosition(finalDestination, out NavMeshHit hit, repositionDistance * 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Si falla, solo huye
            agent.SetDestination(transform.position + dirAwayFromPlayer * repositionDistance);
        }
    }
    
    // --- GIZMOS ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseSightRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, idealFiringRange);
        
        // Dibujar el ángulo de visión
        Vector3 fovLine1 = Quaternion.AngleAxis(viewAngle / 2, transform.up) * transform.forward * detectionRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-viewAngle / 2, transform.up) * transform.forward * detectionRadius;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);
        
        if(playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}