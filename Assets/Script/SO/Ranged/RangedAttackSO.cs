using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Nuevo Ataque a Distancia", menuName = "Habilidades/Ataque a Distancia/Disparo Básico")]
public class RangedAttackSO : AbilitySO, IAimable
{
    [Header("Configuración de Ataque a Distancia")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 25f;

    public Transform aimSource { get; set; }

    public override void Execute(GameObject user)
    {
        if (projectilePrefab == null) return;
        
        // --- LÓGICA DE CRÍTICO ---
        var effectManager = user.GetComponent<StatusEffectManager>();
        var criticalBuff = effectManager?.FindEffect(typeof(CriticalBuff));
        bool isCritical = criticalBuff != null;

        if (isCritical)
        {
            // Consumimos el buff.
            effectManager.RemoveEffect(criticalBuff);
        }
        // -------------------------

        Transform spawnPoint = (aimSource != null) ? aimSource : user.transform;
        GameObject projGO = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        // Le decimos al proyectil si es un golpe crítico.
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