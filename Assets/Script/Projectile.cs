using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El daño que hace este proyectil.")]
    public float damage = 10f;
    [Tooltip("Capas de los objetos que pueden ser dañados por este proyectil.")]
    public LayerMask damageableLayers;
    [Tooltip("Tiempo en segundos antes de que el proyectil se autodestruya si no choca con nada.")]
    public float lifetime = 5f;

    private void Start()
    {
        // Programamos la autodestrucción del proyectil para que no se quede flotando
        // en la escena para siempre si no golpea nada.
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si el objeto con el que chocamos está en una de las capas dañables.
        if ((damageableLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            // Intentamos obtener el componente Health del objeto golpeado.
            if (other.TryGetComponent<Health>(out Health health))
            {
                // Si lo encontramos, le aplicamos el daño.
                health.TakeDamage(damage);
                Debug.Log($"Proyectil golpeó a {other.name} por {damage} de daño.");
            }
            
            // Destruimos el proyectil después de impactar.
            Destroy(gameObject);
        }
    }
}