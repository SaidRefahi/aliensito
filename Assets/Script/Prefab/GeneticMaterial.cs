// Ruta: Assets/Scripts/GeneticMaterial.cs
// ACCIÓN: Reemplaza tu script existente con esta versión.

using UnityEngine;

public class GeneticMaterial : MonoBehaviour
{
    [Tooltip("Cantidad de material genético que proporciona este recurso.")]
    public int amount = 10;
    [Tooltip("Especifica qué layer pertenece al jugador (o lo que sea que lo recoja).")]
    public LayerMask playerLayer;

    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el objeto que entró está en la layer correcta
        if ((playerLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // --- ¡LÍNEA MODIFICADA! ---
            // Llama al Singleton. No le importa si el manager está
            // en el jugador o en cualquier otro sitio.
            EvolutionManager.Instance.AddGeneticMaterial(amount);
            
            // (Opcional: añadir un efecto de sonido o partículas aquí)
            
            Destroy(gameObject);
        }
    }
}