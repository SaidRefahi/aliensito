using UnityEngine;

[CreateAssetMenu(fileName = "Parpadeo", menuName = "Habilidades/Invisibilidad/Parpadeo")]
public class ParpadeoSO : InvisibilitySO
{
    [Header("Configuración de Parpadeo")]
    public float blinkDistance = 10f;

    [Tooltip("Define qué capas bloquearán el parpadeo (ej. 'wallfall')")]
    public LayerMask wallLayer; 

    // --- ¡NUEVAS LÍNEAS! ---
    [Header("Configuración de VFX")]
    [Tooltip("Tag del PoolManager para el VFX al *desaparecer* (posición inicial)")]
    public string startVfxPoolTag = "BlinkStartVFX"; // <-- Escribe el Tag de tu Pool
    
    [Tooltip("Tag del PoolManager para el VFX al *reaparecer* (posición final)")]
    public string endVfxPoolTag = "BlinkEndVFX"; // <-- Escribe el Tag de tu Pool
    // --- FIN DE LÍNEAS NUEVAS ---


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

        // 4. Calculamos la posición de inicio
        Vector3 startPosition = rb.position; // Posición actual

        // --- ¡NUEVA LÍNEA! ---
        // 5. Disparamos el VFX de *inicio* en la posición actual
        SpawnVFX(startVfxPoolTag, startPosition, user.transform.rotation);
        // --- FIN DE LÍNEA NUEVA ---

        // 6. Calculamos la dirección y la posición de destino
        Vector3 forwardDirection = user.transform.forward;
        forwardDirection.y = 0; // Previene parpadeo vertical
        Vector3 blinkVector = forwardDirection.normalized * blinkDistance;
        Vector3 endPosition = startPosition + blinkVector; // Posición de destino

        // 7. Comprobamos si hay una pared en el camino
        if (Physics.Linecast(startPosition, endPosition, wallLayer))
        {
            // ¡Hay una pared! El parpadeo se bloquea.
            Debug.Log("¡Parpadeo bloqueado por una pared!");
            // (Devolvemos true porque la invisibilidad SÍ se activó)
            return true;
        }

        // 8. ¡Camino libre! Usamos MovePosition para teletransportar el Rigidbody.
        rb.MovePosition(endPosition);

        // --- ¡NUEVA LÍNEA! ---
        // 9. Disparamos el VFX de *llegada* en la nueva posición
        SpawnVFX(endVfxPoolTag, endPosition, user.transform.rotation);
        // --- FIN DE LÍNEA NUEVA ---
        
        // 10. Devolvemos 'true'
        return true;
    }

    // Método de fallback si no se encuentra un Rigidbody
    private void PerformLegacyBlink(Transform userTransform)
    {
        // Calculamos posiciones
        Vector3 startPosition = userTransform.position;
        Vector3 forwardDirection = userTransform.forward;
        forwardDirection.y = 0;
        Vector3 blinkVector = forwardDirection.normalized * blinkDistance;
        Vector3 endPosition = startPosition + blinkVector;

        // --- ¡NUEVA LÍNEA! ---
        // Disparamos VFX de inicio
        SpawnVFX(startVfxPoolTag, startPosition, userTransform.rotation);
        // --- FIN DE LÍNEA NUEVA ---

        // Comprobamos pared
        if (Physics.Linecast(startPosition, endPosition, wallLayer))
        {
            return; // Bloqueado
        }
        
        // Teletransporte
        userTransform.position = endPosition;
        
        // --- ¡NUEVA LÍNEA! ---
        // Disparamos VFX de llegada
        SpawnVFX(endVfxPoolTag, endPosition, userTransform.rotation);
        // --- FIN DE LÍNEA NUEVA ---
    }

    // --- ¡NUEVO MÉTODO AYUDANTE! ---
    /// <summary>
    /// Método limpio (principio KISS) para spawnear VFX desde el PoolManager.
    /// </summary>
    private void SpawnVFX(string poolTag, Vector3 position, Quaternion rotation)
    {
        // Comprobamos si el PoolManager existe y el tag no está vacío
        if (PoolManager.Instance != null && !string.IsNullOrEmpty(poolTag))
        {
            // Usamos la rotación del jugador para alinear el VFX si es necesario
            PoolManager.Instance.SpawnFromPool(poolTag, position, rotation);
        }
    }
    // --- FIN DE MÉTODO NUEVO ---
}