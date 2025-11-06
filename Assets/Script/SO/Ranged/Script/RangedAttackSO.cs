using UnityEngine;
using System.Collections.Generic; // ¡Necesario para el Diccionario!

[CreateAssetMenu(fileName = "Disparo Básico", menuName = "Habilidades/Ataque a Distancia/Disparo Básico")]
public class RangedAttackSO : AbilitySO, IAimable
{
    [Header("Configuración de Ataque a Distancia")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 25f;
    [Tooltip("Debe coincidir con el tag en el PoolManager")]
    public string projectilePoolTag; 

    // --- ¡LÍNEA ELIMINADA! ---
    // [Header("Configuración de Cooldown")]
    // public float cooldown = 0.5f; <-- ¡ESTA LÍNEA SE HA BORRADO!
    // (Ahora la hereda de AbilitySO)
    // ----------------------------
    
    // (Usamos un Diccionario para gestionar el CD por cada usuario (jugador, jefe, etc.))
    private readonly Dictionary<GameObject, float> lastUseTime = new Dictionary<GameObject, float>();

    public Transform aimSource { get; set; }
    
    public override bool Execute(GameObject user)
    {
        if (!CanUse(user)) return false; // Comprueba si está en cooldown

        if (projectilePrefab == null || string.IsNullOrEmpty(projectilePoolTag))
        {
            Debug.LogError($"¡RangedAttackSO '{this.name}' no tiene Prefab o Pool Tag asignado!");
            return false;
        }
        
        TargetingProfile targeting = user.GetComponent<TargetingProfile>();
        if (targeting == null)
        {
            Debug.LogError($"¡El tirador {user.name} no tiene un componente TargetingProfile!");
            return false;
        }
        
        var effectManager = user.GetComponent<StatusEffectManager>();
        var criticalBuff = effectManager?.FindEffect(typeof(CriticalBuff));
        bool isCritical = criticalBuff != null;
        if (isCritical) effectManager.RemoveEffect(criticalBuff);

        Transform spawnPoint = (aimSource != null) ? aimSource : user.transform;
        
        GameObject projGO = PoolManager.Instance.SpawnFromPool(projectilePoolTag, spawnPoint.position, spawnPoint.rotation);
        if(projGO == null) return false;
        
        if (projGO.TryGetComponent<Projectile>(out Projectile projectile))
        {
            projectile.isCritical = isCritical;
            projectile.poolTag = projectilePoolTag;
            projectile.damageableLayers = targeting.DamageableLayers;
        }

        if (projGO.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = spawnPoint.forward * projectileSpeed;
        }

        lastUseTime[user] = Time.time; // ¡Registra el uso!
        return true; 
    }
    
    private bool CanUse(GameObject user)
    {
        if (!lastUseTime.TryGetValue(user, out float last))
        {
            return true;
        }
        
        // ¡Esta línea funciona!
        // Lee el 'cooldown' heredado de la clase base AbilitySO
        return Time.time >= last + cooldown;
    }
}