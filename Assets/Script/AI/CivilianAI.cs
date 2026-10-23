using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class CivilianAI : MonoBehaviour
    {
        private enum AIState
        {
            Wandering,
            Fleeing
        }
        private AIState currentState;

        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Flee Settings")]
        [SerializeField] private float fleeDistance = 20f;
        [SerializeField] private float fleeSpeed = 5f;

        [Header("Wander Settings")]
        [SerializeField] private float wanderRadius = 15f;
        [SerializeField] private float wanderTimerMax = 5f;

        [Header("Unity Behavior Integration")]
        [SerializeField] private BehaviorGraphAgent behaviorAgent;

        private NavMeshAgent agent;
        private Transform playerTransform;
        private float wanderTimer;
        private float originalSpeed;

        private Animator animator;
        private readonly int moveSpeedHash = Animator.StringToHash("moveSpeed");
        private readonly Collider[] detectionColliders = new Collider[2];

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            originalSpeed = agent.speed;

            if (behaviorAgent == null)
            {
                behaviorAgent = GetComponent<BehaviorGraphAgent>();
            }

            TransitionToWandering();
        }

        private void Update()
        {
            if (agent.speed > 0.001f)
            {
                float currentSpeed = agent.velocity.magnitude / agent.speed;
                animator.SetFloat(moveSpeedHash, currentSpeed);
            }

            if (behaviorAgent != null && behaviorAgent.Graph != null)
            {
                SyncBlackboardVariables();
                return;
            }

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

        private void SyncBlackboardVariables()
        {
            if (behaviorAgent == null) return;

            behaviorAgent.SetVariableValue("Agent", transform);
            if (playerTransform != null)
            {
                behaviorAgent.SetVariableValue("Threat", playerTransform);
            }
            if (agent != null)
            {
                behaviorAgent.SetVariableValue("NavAgent", agent);
            }
            behaviorAgent.SetVariableValue("WanderRadius", wanderRadius);
            behaviorAgent.SetVariableValue("FleeDistance", fleeDistance);
        }

        private void CheckForPlayer()
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, detectionColliders, playerLayer);
            if (hits > 0 && detectionColliders[0] != null)
            {
                playerTransform = detectionColliders[0].transform;
                TransitionToFleeing();
            }
        }

        private void CheckIfPlayerLost()
        {
            if (playerTransform == null)
            {
                TransitionToWandering();
                return;
            }

            float sqrDistance = (transform.position - playerTransform.position).sqrMagnitude;
            if (sqrDistance > (fleeDistance * fleeDistance))
            {
                playerTransform = null;
                TransitionToWandering();
            }
        }

        private void TransitionToWandering()
        {
            currentState = AIState.Wandering;
            agent.speed = originalSpeed;
            wanderTimer = wanderTimerMax;
        }

        private void TransitionToFleeing()
        {
            currentState = AIState.Fleeing;
            agent.speed = fleeSpeed;
        }

        private void Wander()
        {
            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderTimerMax)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
                wanderTimer = 0f;
            }
        }

        private void Flee()
        {
            if (playerTransform == null) return;

            Vector3 directionAwayFromPlayer = (transform.position - playerTransform.position).normalized;
            Vector3 fleePosition = transform.position + directionAwayFromPlayer * fleeDistance;

            if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
        {
            Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;
            randDirection += origin;

            if (NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layermask))
            {
                return navHit.position;
            }

            return origin;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, fleeDistance);
        }
    }
}
