using UnityEngine;

public class GeneticMaterial : MonoBehaviour
{
    [Tooltip("Cantidad de material genético que proporciona este recurso.")]
    public int amount = 10;
    [Tooltip("Especifica qué layer pertenece al jugador.")]
    public LayerMask playerLayer;

    private void OnTriggerEnter(Collider other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            EvolutionManager evolutionManager = other.GetComponent<EvolutionManager>();
            if (evolutionManager != null)
            {
                evolutionManager.AddGeneticMaterial(amount);
                Destroy(gameObject);
            }
        }
    }
}