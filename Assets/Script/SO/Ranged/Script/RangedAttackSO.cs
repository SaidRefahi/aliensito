// Ruta: Assets/Scripts/SO/Ranged/RangedAttackSO.cs
// ACCIÓN: Reemplaza tu script existente.

using UnityEngine;

[CreateAssetMenu(fileName = "Disparo Básico", menuName = "Habilidades/Ataque a Distancia/Disparo Básico")]
public class RangedAttackSO : AbilitySO, IAimable
{
    [Header("Configuración de Ataque a Distancia")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 25f;
    [Tooltip("Debe coincidir con el tag en el PoolManager")]
    public string projectilePoolTag; 

    public Transform aimSource { get; set; }
    
    public override bool Execute(GameObject user)
    {
        if (projectilePrefab == null || string.IsNullOrEmpty(projectilePoolTag))
        {
            Debug.LogError($"¡RangedAttackSO '{this.name}' no tiene Prefab o Pool Tag asignado!");
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
        return true; 
    }
}