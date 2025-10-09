using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/RangedAttack")]
public class RangedAttackSO : AbilitySO
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;

    public override void Execute(GameObject user)
    {
        if (projectilePrefab == null) return;

        // Instanciar proyectil frente al jugador
        Transform t = user.transform;
        GameObject proj = Instantiate(projectilePrefab, t.position + t.forward, t.rotation);

        // Aplicar velocidad
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = t.forward * projectileSpeed;
        }

        Debug.Log($"{abilityName} ejecutado: proyectil lanzado");
    }
}