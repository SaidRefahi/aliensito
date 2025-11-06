using UnityEngine;
using System.Collections.Generic; // ¡Necesario para el Diccionario!

[CreateAssetMenu(fileName = "Control de Plagas", menuName = "Habilidades/Ataque a Distancia/Control de plagas")]
public class ControlDePlagasSO : RangedAttackSO
{
    [Header("Configuración de Control de Plagas")]
    [Range(2, 10)]
    public int projectileCount = 3;
    public float spreadAngle = 30f;
    
    // --- ¡NUEVAS LÍNEAS! ---
    // (Añadimos su propio tracker de cooldown porque sobrescribe Execute)
    private readonly Dictionary<GameObject, float> lastUseTime = new Dictionary<GameObject, float>();
    // --- FIN DE LÍNEAS NUEVAS ---
    
    public override bool Execute(GameObject user)
    {
        // --- ¡NUEVA LÍNEA! ---
        if (!CanUse(user)) return false;
        // --- FIN DE LÍNEA NUEVA ---

        if (projectilePrefab == null || string.IsNullOrEmpty(projectilePoolTag))
        {
            Debug.LogError($"¡ControlDePlagasSO '{this.name}' no tiene Prefab o Pool Tag asignado!");
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
        
        float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0;
        float startingAngle = -spreadAngle / 2;

        for (int i = 0; i < projectileCount; i++)
        {
            Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(0, startingAngle + i * angleStep, 0);
            
            GameObject projGO = PoolManager.Instance.SpawnFromPool(projectilePoolTag, spawnPoint.position, rotation);
            if(projGO == null) continue;
            
            if (projGO.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.isCritical = (isCritical && i == projectileCount / 2);
                projectile.poolTag = projectilePoolTag;
                projectile.damageableLayers = targeting.DamageableLayers;
            }

            if (projGO.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = projGO.transform.forward * projectileSpeed;
            }
        }
        
        // --- ¡NUEVA LÍNEA! ---
        lastUseTime[user] = Time.time; // ¡Registra el uso!
        // --- FIN DE LÍNEA NUEVA ---
        return true;
    }
    
    // --- ¡NUEVO MÉTODO! ---
    private bool CanUse(GameObject user)
    {
        if (!lastUseTime.TryGetValue(user, out float last))
        {
            return true;
        }
        // ¡Importante! Lee el 'cooldown' de la clase base AbilitySO
        // (que heredó a través de RangedAttackSO)
        return Time.time >= last + cooldown;
    }
    // --- FIN DE MÉTODO NUEVO ---
}