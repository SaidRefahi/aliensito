using UnityEngine;
using System.Collections.Generic; // ¡Necesario para el Cooldown!

[CreateAssetMenu(fileName = "Egocentrismo", menuName = "Habilidades/Invisibilidad/Egocentrismo")]
public class EgocentrismoSO : AbilitySO
{
    [Header("Configuración de Nova Aturdidora")]
    public float radius = 8f;
    public float stunDuration = 2.5f;

    // --- ¡NUEVA LÍNEA! ---
    [Header("Configuración de VFX")]
    public string vfxPoolTag = "EgocentrismoVFX"; // <-- Escribe el Tag de tu PoolManager aquí
    // --- FIN DE LÍNEA NUEVA ---

    // (Añadimos el tracker de cooldown)
    private readonly Dictionary<GameObject, float> lastUseTime = new Dictionary<GameObject, float>();

    public override bool Execute(GameObject user)
    {
        if (!CanUse(user)) return false; // Comprobar si está en cooldown

        TargetingProfile targeting = user.GetComponent<TargetingProfile>();
        if (targeting == null)
        {
            Debug.LogError($"¡EgocentrismoSO: {user.name} no tiene un componente TargetingProfile!");
            return false;
        }
        
        // ¡Usamos el LayerMask del TargetingProfile!
        Collider[] hits = Physics.OverlapSphere(user.transform.position, radius, targeting.DamageableLayers);
        
        foreach (var hit in hits)
        {
            if (hit.gameObject == user) continue; // No aturdirse a sí mismo
            
            if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
            {
                effectManager.ApplyEffect(new Stun(hit.gameObject, stunDuration));
            }
        }
        
        lastUseTime[user] = Time.time; // Registrar el uso para el cooldown

        // --- ¡NUEVAS LÍNEAS! ---
        // Disparar el VFX usando el PoolManager
        if (PoolManager.Instance != null && !string.IsNullOrEmpty(vfxPoolTag))
        {
            // Spawnear en la posición del usuario, sin rotación específica
            PoolManager.Instance.SpawnFromPool(vfxPoolTag, user.transform.position, Quaternion.identity);
        }
        // --- FIN DE LÍNEAS NUEVAS ---

        return true;
    }
    
    private bool CanUse(GameObject user)
    {
        if (!lastUseTime.TryGetValue(user, out float last))
        {
            return true; // Nunca se ha usado
        }
        
        // Comprueba si el tiempo actual ha superado el último uso + cooldown
        return Time.time >= last + cooldown;
    }
}