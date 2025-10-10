using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/RangedAttack")]
public class RangedAttackSO : AbilitySO, IAimable // <-- 1. "Firma el contrato" IAimable
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;

    // 2. Implementa la propiedad que el contrato exige
    public Transform aimSource { get; set; }

    public override void Execute(GameObject user)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("RangedAttackSO: ¡El prefab del proyectil no está asignado!");
            return;
        }

        // 3. Usa el aimSource para disparar desde el punto de mira
        Transform spawnPoint = (aimSource != null) ? aimSource : user.transform;

        GameObject proj = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        if (proj.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = spawnPoint.forward * projectileSpeed;
        }

        Debug.Log($"<color=yellow>{abilityName} ejecutado:</color> Proyectil lanzado desde {spawnPoint.name}");
    }
} 