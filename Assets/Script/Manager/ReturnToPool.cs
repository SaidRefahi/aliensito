using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ReturnToPool : MonoBehaviour
{
    [Tooltip("¡IMPORTANTE! Este Tag DEBE COINCIDIR con el Tag en el PoolManager.")]
    [SerializeField] private string poolTag;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        // Cada vez que el PoolManager lo "despierta" (activa),
        // nos aseguramos de que se reproduzca.
        ps.Play();
    }

    private void Update()
    {
        // En cada frame, comprobamos si la partícula (y todos sus sub-sistemas)
        // ha terminado de reproducirse.
        if (ps != null && !ps.IsAlive(true))
        {
            // Si terminó, ¡de vuelta al pool!
            // Usamos el PoolManager existente
            PoolManager.Instance.ReturnToPool(poolTag, gameObject);
        }
    }
}