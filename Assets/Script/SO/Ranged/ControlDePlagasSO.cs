using UnityEngine;

[CreateAssetMenu(fileName = "Control de Plagas", menuName = "Habilidades/Ataque a Distancia/Control de plagas")]
public class ControlDePlagasSO : RangedAttackSO
{
    [Header("Configuración de Control de Plagas")]
    [Range(2, 10)]
    public int projectileCount = 3;
    public float spreadAngle = 30f;

    public override void Execute(GameObject user)
    {
        if (projectilePrefab == null) return;

        var effectManager = user.GetComponent<StatusEffectManager>();
        var criticalBuff = effectManager?.FindEffect(typeof(CriticalBuff));
        bool isCritical = criticalBuff != null;
        
        if (isCritical)
        {
            effectManager.RemoveEffect(criticalBuff);
        }

        Transform spawnPoint = (aimSource != null) ? aimSource : user.transform;
        
        float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0;
        float startingAngle = -spreadAngle / 2;

        for (int i = 0; i < projectileCount; i++)
        {
            Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(0, startingAngle + i * angleStep, 0);
            
            // --- LÍNEA CORREGIDA ---
            // Ahora llama al PoolManager
            GameObject projGO = PoolManager.Instance.SpawnFromPool(projectilePoolTag, spawnPoint.position, rotation);
            if(projGO == null) continue;
            
            if (projGO.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.isCritical = (isCritical && i == projectileCount / 2);
            }

            if (projGO.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = projGO.transform.forward * projectileSpeed;
            }
        }
    }
}