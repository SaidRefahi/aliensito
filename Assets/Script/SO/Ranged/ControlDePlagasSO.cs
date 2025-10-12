using UnityEngine;

// Le ponemos el nombre exacto de tu GDD en el menú.
[CreateAssetMenu(fileName = "Control de Plagas", menuName = "Habilidades/Ataque a Distancia/Control de plagas")]
public class ControlDePlagasSO : RangedAttackSO // Hereda de la habilidad base.
{
    [Header("Configuración de Control de Plagas")]
    [Range(2, 10)]
    public int projectileCount = 3;
    public float spreadAngle = 30f;

    public override void Execute(GameObject user)
    {
        if (projectilePrefab == null) return;
        Transform spawnPoint = (aimSource != null) ? aimSource : user.transform;
        
        float angleStep = spreadAngle / (projectileCount - 1);
        float startingAngle = -spreadAngle / 2;

        for (int i = 0; i < projectileCount; i++)
        {
            Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(0, startingAngle + i * angleStep, 0);
            GameObject proj = Instantiate(projectilePrefab, spawnPoint.position, rotation);
            if (proj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = proj.transform.forward * projectileSpeed;
            }
        }
    }
}