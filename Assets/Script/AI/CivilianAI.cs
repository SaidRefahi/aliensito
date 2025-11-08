using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))] // <--- AÑADIDO
public class CivilianAI : MonoBehaviour
{
    // --- MÁQUINA DE ESTADOS (FSM) ---
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
    
    // --- NUEVO: Variables de Animación ---
    private Animator animator;
    private readonly int moveSpeedHash = Animator.StringToHash("moveSpeed");
    // -------------------------------------

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // <--- NUEVO
        originalSpeed = agent.speed;
        
        TransitionToWandering();
    }

    private void Update()
    {
        // --- NUEVO: Lógica de Animación ---
        // Normaliza la velocidad actual (0 a 1) y la pasa al Animator.
        // Si está huyendo, agent.speed será 'fleeSpeed', así que
        // 'currentSpeed' seguirá siendo 1.0 (máxima velocidad).
        float currentSpeed = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(moveSpeedHash, currentSpeed);
        // ----------------------------------
        
        switch (currentState)
        {
            case AIState.Wandering:
                CheckForPlayer();
                Wander();
                break;
            case AIState.Fleeing:
                CheckIfPlayerLost();
                Flee();
                break;
        }
    }

    // --- LÓGICA DE ESTADOS (Asumo que esta lógica ya la tienes) ---

    private void CheckForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (colliders.Length > 0)
        {
            playerTransform = colliders[0].transform;
            TransitionToFleeing();
        }
    }
    
    private void CheckIfPlayerLost()
    {
        if (playerTransform == null || Vector3.Distance(transform.position, playerTransform.position) > detectionRadius * 1.5f)
        {
            playerTransform = null;
            TransitionToWandering();
        }
    }
    
    private void TransitionToWandering()
    {
        currentState = AIState.Wandering;
        agent.speed = originalSpeed; // Vuelve a la velocidad de paseo
        wanderTimer = wanderTimerMax;
    }
    
    private void TransitionToFleeing()
    {
        currentState = AIState.Fleeing;
        agent.speed = fleeSpeed; // Aumenta la velocidad
    }
    
    private void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f || !agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            wanderTimer = wanderTimerMax;
        }
    }

    private void Flee()
    {
        Vector3 fleeDirection = transform.position - playerTransform.position;
        Vector3 targetPosition = transform.position + fleeDirection.normalized * fleeDistance;

        agent.SetDestination(targetPosition);
    }
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = (currentState == AIState.Fleeing) ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}