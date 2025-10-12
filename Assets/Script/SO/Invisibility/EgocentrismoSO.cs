using UnityEngine;

[CreateAssetMenu(fileName = "Egocentrismo", menuName = "Habilidades/Invisibilidad/Egocentrismo")]
public class EgocentrismoSO : AbilitySO
{
    [Header("Configuración de Nova Aturdidora")]
    public float radius = 8f;
    public float stunDuration = 2.5f;
    public LayerMask enemyLayers;

    public override void Execute(GameObject user)
    {
        Collider[] hits = Physics.OverlapSphere(user.transform.position, radius, enemyLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<StatusEffectManager>(out var effectManager))
            {
                effectManager.ApplyEffect(new Stun(hit.gameObject, stunDuration));
            }
        }
    }
}