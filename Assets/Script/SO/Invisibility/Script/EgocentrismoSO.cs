using UnityEngine;
using System.Collections.Generic; // ¡Necesario para el Cooldown!

[CreateAssetMenu(fileName = "Egocentrismo", menuName = "Habilidades/Invisibilidad/Egocentrismo")]
public class EgocentrismoSO : AbilitySO
{
    [Header("Configuración de Nova Aturdidora")]
    public float radius = 8f;
    public float stunDuration = 2.5f;
    // public LayerMask enemyLayers; // <-- ¡BORRADO! Usaremos el TargetingProfile (SRP)

    // --- ¡NUEVAS LÍNEAS! ---
    // (Añadimos el tracker de cooldown)
    private readonly Dictionary<GameObject, float> lastUseTime = new Dictionary<GameObject, float>();
    // --- FIN DE LÍNEAS NUEVAS ---

    public override bool Execute(GameObject user)
    {
        // --- ¡NUEVA LÍNEA! ---
        if (!CanUse(user)) return false; // Comprobar si está en cooldown
        // --- FIN DE LÍNEA NUEVA ---

        // --- ¡LÓGICA MODIFICADA! ---
        TargetingProfile targeting = user.GetComponent<TargetingProfile>();
        if (targeting == null)
        {
            Debug.LogError($"¡EgocentrismoSO: {user.name} no tiene un componente TargetingProfile!");
            return false;
        }
        
        // ¡Usamos el LayerMask del TargetingProfile!
        Collider[] hits = Physics.OverlapSphere(user.transform.position, radius, targeting.DamageableLayers);
        // --- FIN DE LÓGICA MODIFICADA ---
        
        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue; // No aturdirse a sí mismo
            
            if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
            {
                effectManager.ApplyEffect(new Stun(hit.gameObject, stunDuration));
            }
        }
        
        // --- ¡NUEVA LÍNEA! ---
        lastUseTime[user] = Time.time; // Registrar el uso para el cooldown
        // --- FIN DE LÍNEA NUEVA ---
        return true;
    }
    
    // --- ¡NUEVO MÉTODO! ---
    private bool CanUse(GameObject user)
    {
        if (!lastUseTime.TryGetValue(user, out float last))
        {
            return true; // Nunca se ha usado
        }
        
        // Comprueba si el tiempo actual ha superado el último uso + cooldown
        // (El 'cooldown' es la variable de la clase base 'AbilitySO')
        return Time.time >= last + cooldown;
    }
    // --- FIN DE MÉTODO NUEVO ---
}