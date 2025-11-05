using System;
using Unity.Behavior; // Necesario para los tipos de datos
using UnityEngine;
using UnityEngine.AI; // Necesario para el NavMeshAgent
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Flee", story: "[Agent] flees from [Target]", category: "Action", id: "d1ecd7e75d7f43d6f88ae86613d5fd0d")]
public partial class FleeAction : Action
{
    // --- PUERTOS DE ENTRADA (Tu plantilla original) ---
    
    [Tooltip("El agente que ejecuta esta acción (Normalmente 'Self')")]
    [SerializeReference] 
    public BlackboardVariable<GameObject> Agent;

    [Tooltip("El objetivo del que huir (leído del Blackboard)")]
    [SerializeReference] 
    public BlackboardVariable<GameObject> Target;

    // --- PARÁMETROS ---
    
    [Tooltip("A qué distancia intentará huir el agente")]
    [SerializeField] 
    public float FleeDistance = 15.0f;
    
    [Tooltip("La velocidad del agente al huir (0 usa la velocidad por defecto)")]
    [SerializeField]
    public float FleeSpeed = 0f;

    // --- VARIABLES INTERNAS (Cacheadas para eficiencia - SRP) ---
    private NavMeshAgent navMeshAgent;
    private GameObject targetObject;
    private float originalSpeed;

    protected override Status OnStart()
    {
        // 1. Obtener el 'Agent' (el enemigo) desde la variable.
        GameObject agentObject = Agent.Value; 
        if (agentObject == null)
        {
            Debug.LogError("FleeAction: La variable 'Agent' del Blackboard es nula.", agentObject);
            return Status.Failure;
        }

        // 2. Obtener el NavMeshAgent del Agente.
        if (!agentObject.TryGetComponent<NavMeshAgent>(out navMeshAgent))
        {
            Debug.LogError("FleeAction: El agente no tiene un componente NavMeshAgent.", agentObject);
            return Status.Failure;
        }

        // 3. Obtener el 'Target' (el jugador) desde la variable.
        targetObject = Target.Value;
        if (targetObject == null)
        {
            return Status.Failure;
        }

        // 4. (Opcional) Guardar y aplicar la velocidad de huida
        if (FleeSpeed > 0)
        {
            originalSpeed = navMeshAgent.speed;
            navMeshAgent.speed = FleeSpeed;
        }
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (targetObject == null || navMeshAgent == null)
        {
            return Status.Failure;
        }

        Vector3 agentPos = navMeshAgent.transform.position;
        Vector3 targetPos = targetObject.transform.position;
        Vector3 directionAwayFromTarget = (agentPos - targetPos).normalized;
        Vector3 fleeDestination = agentPos + directionAwayFromTarget * FleeDistance;

        if (NavMesh.SamplePosition(fleeDestination, out NavMeshHit hit, FleeDistance, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }
        
        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (navMeshAgent != null)
        {
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
            }
            if (FleeSpeed > 0)
            {
                navMeshAgent.speed = originalSpeed;
            }
        }
        navMeshAgent = null;
        targetObject = null;
    }
}