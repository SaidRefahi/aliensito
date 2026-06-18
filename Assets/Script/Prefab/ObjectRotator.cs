using UnityEngine;

/// <summary>
/// Un componente simple que hace girar este GameObject
/// a una velocidad constante definida en el Inspector.
/// </summary>
public class ObjectRotator : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad de rotación en grados por segundo para cada eje (X, Y, Z).")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 45f, 0f);

    // Update se llama en cada fotograma
    void Update()
    {
        // 1. Calculamos la rotación para este fotograma.
        // Multiplicamos la velocidad (grados/seg) por Time.deltaTime (segundos)
        // para asegurar que la rotación sea suave y no dependa de los FPS.
        Vector3 rotationThisFrame = rotationSpeed * Time.deltaTime;

        // 2. Aplicamos la rotación al transform del objeto.
        // Usamos Space.Self para girar sobre sus propios ejes locales.
        // (Usa Space.World si prefieres girar sobre los ejes del mundo).
        transform.Rotate(rotationThisFrame, Space.Self);
    }
}