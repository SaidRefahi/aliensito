using UnityEngine;
using UnityEngine.AI;

public class Stun : StatusEffect
{
    private readonly NavMeshAgent agent;

    public Stun(GameObject target, float duration) : base(target, duration)
    {
        // Guardamos la referencia al NavMeshAgent en el constructor.
        this.agent = target.GetComponent<NavMeshAgent>();
    }

    // Se llama una vez cuando el efecto se aplica.
    public override void OnApply()
    {
        if (agent != null)
        {
            Debug.Log($"<color=yellow>{targetObject.name} is STUNNED for {Duration}s.</color>");
            agent.isStopped = true; // Detenemos al enemigo.
        }
    }

    // Se llama cada frame, pero para el stun no necesitamos hacer nada aquí.
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime); // Solo restamos el tiempo.
    }

    // Se llama una vez cuando el efecto termina.
    public override void OnEnd()
    {
        if (agent != null)
        {
            Debug.Log($"<color=grey>Stun on {targetObject.name} has worn off.</color>");
            agent.isStopped = false; // Reanudamos el movimiento del enemigo.
        }
    }
}