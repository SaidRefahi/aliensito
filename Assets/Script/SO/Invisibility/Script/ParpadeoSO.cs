using UnityEngine;

[CreateAssetMenu(fileName = "Parpadeo", menuName = "Habilidades/Invisibilidad/Parpadeo")]
public class ParpadeoSO : InvisibilitySO
{
    [Header("Configuración de Parpadeo")]
    public float blinkDistance = 10f;

    // --- ¡NUEVA LÍNEA! ---
    [Tooltip("Define qué capas bloquearán el parpadeo (ej. 'wallfall')")]
    public LayerMask wallLayer; 
    // --- FIN DE LÍNEA NUEVA ---

    public override bool Execute(GameObject user)
    {
        // 1. Ejecutamos la invisibilidad de la clase base
        bool executed = base.Execute(user);
        
        // 2. Si la ejecución base falló (ej. cooldown), detenemos
        if (!executed) return false;

        // 3. Obtenemos el Rigidbody del usuario
        Rigidbody rb = user.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"¡ParpadeoSO no pudo encontrar un Rigidbody en {user.name}!");
            // (Tu método de fallback)
            PerformLegacyBlink(user.transform);
            return true;
        }

        // 4. Calculamos la dirección y las posiciones
        Vector3 startPosition = rb.position; // Posición actual
        Vector3 forwardDirection = user.transform.forward;
        forwardDirection.y = 0; // Previene parpadeo vertical
        Vector3 blinkVector = forwardDirection.normalized * blinkDistance;
        Vector3 endPosition = startPosition + blinkVector; // Posición de destino

        // --- ¡¡LÓGICA CORREGIDA AQUÍ!! ---
        // 5. Comprobamos si hay una pared en el camino
        // Lanzamos un rayo desde el inicio hasta el fin,
        // buscando SÓLO en la 'wallLayer'
        if (Physics.Linecast(startPosition, endPosition, wallLayer))
        {
            // ¡Hay una pared! El parpadeo se bloquea.
            // Devolvemos 'true' porque la habilidad (invisibilidad) SÍ se ejecutó,
            // pero el movimiento fue bloqueado, lo cual es la lógica correcta.
            Debug.Log("¡Parpadeo bloqueado por una pared!");
            return true;
        }
        // --- FIN DE LÓGICA CORREGIDA ---

        // 6. ¡Camino libre! Usamos MovePosition para teletransportar el Rigidbody.
        rb.MovePosition(endPosition);
        
        // 7. Devolvemos 'true'
        return true;
    }

    // Método de fallback si no se encuentra un Rigidbody
    private void PerformLegacyBlink(Transform userTransform)
    {
        // (Este método de fallback también debería tener la comprobación del Linecast)
        Vector3 startPosition = userTransform.position;
        Vector3 forwardDirection = userTransform.forward;
        forwardDirection.y = 0;
        Vector3 blinkVector = forwardDirection.normalized * blinkDistance;
        Vector3 endPosition = startPosition + blinkVector;

        if (Physics.Linecast(startPosition, endPosition, wallLayer))
        {
            return; // Bloqueado
        }
        
        userTransform.position = endPosition;
    }
}