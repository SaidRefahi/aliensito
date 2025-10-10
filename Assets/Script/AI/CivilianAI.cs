using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CivilianAI : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Comportamiento")]
    [SerializeField] private float fleeDistance = 20f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private bool isFleeing = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        DetectPlayer();

        if (isFleeing && playerTransform != null)
        {
            // Calcular una dirección opuesta al jugador.
            Vector3 fleeDirection = transform.position - playerTransform.position;
            Vector3 targetPosition = transform.position + fleeDirection.normalized * fleeDistance;

            // Moverse a esa posición.
            agent.SetDestination(targetPosition);
        }
    }

    private void DetectPlayer()
    {
        // Lanzamos una esfera invisible para detectar al jugador.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        if (hitColliders.Length > 0)
        {
            // Hemos detectado al jugador.
            playerTransform = hitColliders[0].transform;
            isFleeing = true;
            agent.speed = 3f; // Aumentar la velocidad al huir.
        }
    }

    // Para visualizar el radio de detección en el editor.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}