// Ruta: Assets/Scripts/AI/CivilianAI.cs
// ACCIÓN: Reemplaza tu script existente con esta versión.

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CivilianAI : MonoBehaviour
{
    // --- NUEVO: La Máquina de Estados (FSM) ---
    private enum AIState
    {
        Wandering,
        Fleeing
    }
    private AIState currentState;
    // ------------------------------------------

    [Header("Configuración de Detección")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Configuración de Huida (Flee)")]
    [SerializeField] private float fleeDistance = 20f;
    [SerializeField] private float fleeSpeed = 5f;

    [Header("Configuración de Paseo (Wander)")]
    [SerializeField] private float wanderRadius = 15f;
    [SerializeField] private float wanderTimerMax = 5f; // Tiempo entre paseos
    
    private NavMeshAgent agent;
    private Transform playerTransform;
    private float wanderTimer;
    private float originalSpeed;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed;
        
        // Estado inicial
        TransitionToWandering();
    }

    private void Update()
    {
        // El "cerebro" de la FSM. Solo ejecuta la lógica del estado actual.
        switch (currentState)
        {
            case AIState.Wandering:
                HandleWanderingState();
                break;
            
            case AIState.Fleeing:
                HandleFleeingState();
                break;
        }
    }

    // --- LÓGICA DE ESTADOS (Principio de Responsabilidad Única) ---

    private void HandleWanderingState()
    {
        // 1. ACCIÓN: Moverse aleatoriamente
        Wander();
        
        // 2. TRANSICIÓN: Buscar al jugador
        if (IsPlayerDetectedAndInvisible())
        {
            TransitionToFleeing();
        }
    }

    private void HandleFleeingState()
    {
        // 1. TRANSICIÓN: Comprobar si el jugador se fue
        if (playerTransform == null || Vector3.Distance(transform.position, playerTransform.position) > detectionRadius)
        {
            TransitionToWandering();
            return;
        }
        
        // 2. ACCIÓN: Huir
        Flee();
    }

    // --- LÓGICA DE TRANSICIÓN (KISS) ---

    private void TransitionToWandering()
    {
        // Debug.Log(gameObject.name + " está en modo 'Wandering'.");
        currentState = AIState.Wandering;
        agent.speed = originalSpeed;
        playerTransform = null; // Perdimos el objetivo
    }

    private void TransitionToFleeing()
    {
        // Debug.Log(gameObject.name + " está en modo 'Fleeing'!");
        currentState = AIState.Fleeing;
        agent.speed = fleeSpeed; // ¡Corre!
    }

    // --- LÓGICA DE ACCIONES (Los "Músculos") ---

    /// <summary>
    /// ¡CONDICIÓN 1!
    /// Comprueba si el jugador está cerca Y si tiene el script de invisibilidad.
    /// </summary>
    private bool IsPlayerDetectedAndInvisible()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        if (hitColliders.Length > 0)
        {
            // ¡Vimos algo en la layer del jugador!
            GameObject player = hitColliders[0].gameObject;
            
            // ¡CONDICIÓN CLAVE!
            // Comprobamos si el jugador tiene el componente "runner" de la invisibilidad.
            // Esto es limpio (KISS) porque no necesitamos modificar NINGÚN otro script.
            if (player.GetComponent<InvisibilitySO.InvisibilityRunner>() != null)
            {
                // ¡DETECTADO! Es invisible.
                playerTransform = player.transform;
                return true;
            }
        }
        
        // No vimos a nadie, o el que vimos NO era invisible.
        playerTransform = null;
        return false;
    }

    /// <summary>
    /// ¡CONDICIÓN 3! Lógica de paseo aleatorio.
    /// </summary>
    private void Wander()
    {
        wanderTimer -= Time.deltaTime;
        
        // Si no estamos ocupados y se acabó el tiempo, busca un nuevo punto.
        if (wanderTimer <= 0f && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius + transform.position;
            
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            
            wanderTimer = wanderTimerMax; // Reinicia el timer
        }
    }

    /// <summary>
    /// Lógica de huida.
    /// </summary>
    private void Flee()
    {
        // Calcular una dirección opuesta al jugador.
        Vector3 fleeDirection = transform.position - playerTransform.position;
        Vector3 targetPosition = transform.position + fleeDirection.normalized * fleeDistance;

        // Moverse a esa posición.
        agent.SetDestination(targetPosition);
    }
    
    /// <summary>
    /// ¡CONDICIÓN 4! (Flocking - Moverse en grupo)
    /// Esta es una lógica AVANZADA (se llama "Boids" o "Flocking").
    /// Es demasiado complejo para un solo script y violaría el principio KISS.
    /// 
    /// MI RECOMENDACIÓN: Mantener el 'Wander' aleatorio. Si QUEREMOS
    /// implementar "flocking", debemos crear un 'FlockingManager' separado
    /// que controle a todos los civiles. ¡Podemos hacerlo después!
    /// </summary>

    // --- GIZMOS ---
    private void OnDrawGizmosSelected()
    {
        // Detección
        Gizmos.color = (currentState == AIState.Fleeing) ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Paseo
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawSphere(transform.position, wanderRadius);
    }
}