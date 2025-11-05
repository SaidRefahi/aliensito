// Ruta: Assets/Script/SO/Invisibility/ParpadeoSO.cs
// ACCIÓN: Reemplaza tu script existente con esta versión.
using UnityEngine;

[CreateAssetMenu(fileName = "Parpadeo", menuName = "Habilidades/Invisibilidad/Parpadeo")]
public class ParpadeoSO : InvisibilitySO
{
    [Header("Configuración de Parpadeo")]
    public float blinkDistance = 10f;

    public override bool Execute(GameObject user)
    {
        // 1. Ejecutamos la invisibilidad de la clase base
        bool executed = base.Execute(user);
        
        // 2. Si la ejecución base falló (ej. cooldown), detenemos
        if (!executed) return false;

        // --- ¡LÓGICA CORREGIDA AQUÍ! ---

        // 3. Obtenemos el Rigidbody del usuario
        Rigidbody rb = user.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"¡ParpadeoSO no pudo encontrar un Rigidbody en {user.name}!");
            // Si no hay Rigidbody, usamos el método 'transform' como fallback
            // (Aunque esto no debería pasar si tu jugador está bien configurado)
            PerformLegacyBlink(user.transform);
            return true;
        }

        // 4. Calculamos la dirección (tu lógica original estaba perfecta)
        Vector3 forwardDirection = user.transform.forward;
        forwardDirection.y = 0; // Previene parpadeo vertical
        Vector3 blinkVector = forwardDirection.normalized * blinkDistance;

        // 5. Calculamos la nueva posición basándonos en la posición DEL RIGIDBODY
        Vector3 newPosition = rb.position + blinkVector;

        // 6. Usamos MovePosition para teletransportar el Rigidbody.
        // Esto es seguro y le informa al motor de físicas.
        rb.MovePosition(newPosition);
        
        // 7. Devolvemos 'true'
        return true;
    }

    // Método de fallback si no se encuentra un Rigidbody
    private void PerformLegacyBlink(Transform userTransform)
    {
        Vector3 forwardDirection = userTransform.forward;
        forwardDirection.y = 0;
        Vector3 blinkVector = forwardDirection.normalized * blinkDistance;
        userTransform.position += blinkVector;
    }
}