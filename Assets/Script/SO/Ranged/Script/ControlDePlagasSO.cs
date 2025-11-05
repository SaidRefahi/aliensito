using UnityEngine;

[CreateAssetMenu(fileName = "Control de Plagas", menuName = "Habilidades/Ataque a Distancia/Control de plagas")]
public class ControlDePlagasSO : RangedAttackSO
{
    [Header("Configuración de Control de Plagas")]
    [Range(2, 10)]
    public int projectileCount = 3;
    public float spreadAngle = 30f;
    
    public override bool Execute(GameObject user)
    {
        if (projectilePrefab == null || string.IsNullOrEmpty(projectilePoolTag))
        {
            Debug.LogError($"¡ControlDePlagasSO '{this.name}' no tiene Prefab o Pool Tag asignado!");
            return false;
        }
        
        // --- ¡LÓGICA NUEVA! ---
        TargetingProfile targeting = user.GetComponent<TargetingProfile>();
        if (targeting == null)
        {
            Debug.LogError($"¡El tirador {user.name} no tiene un componente TargetingProfile!");
            return false;
        }
        // --- FIN LÓGICA NUEVA ---

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
                
                // --- ¡LA LÍNEA MÁGICA (EN BUCLE)! ---
                projectile.damageableLayers = targeting.DamageableLayers;
            }

            if (projGO.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = projGO.transform.forward * projectileSpeed;
            }
        }
        return true;
    }
}