// Ruta: Assets/Script/SO/Ranged/RangedAttackSO.cs
// ACCIÓN: Reemplaza tu script existente con esta versión.
using UnityEngine;

[CreateAssetMenu(fileName = "Disparo Básico", menuName = "Habilidades/Ataque a Distancia/Disparo Básico")]
public class RangedAttackSO : AbilitySO, IAimable
{
    [Header("Configuración de Ataque a Distancia")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 25f;
    [Tooltip("Debe coincidir con el tag en el PoolManager")]
    public string projectilePoolTag; // Asegúrate de rellenar esto en el Inspector

    public Transform aimSource { get; set; }

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
        
        // Pide un proyectil al PoolManager.
        GameObject projGO = PoolManager.Instance.SpawnFromPool(projectilePoolTag, spawnPoint.position, spawnPoint.rotation);
        if(projGO == null) return;
        
        if (projGO.TryGetComponent<Projectile>(out Projectile projectile))
        {
            projectile.isCritical = isCritical;
        }

        if (projGO.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = spawnPoint.forward * projectileSpeed;
        }
    }
}