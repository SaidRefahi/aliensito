using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody rb;
    private Vector2 moveInput;

    // Propiedad pública para que el Animator pueda leerla
    public float CurrentSpeedNormalized => moveInput.magnitude;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    /// <summary>
    /// Este método público es llamado por el PlayerController (Input)
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        
        if (move.sqrMagnitude > 0.0001f) // Más eficiente que .magnitude
        {
            // Movimiento
            rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
            
            // Rotación
            Quaternion targetRot = Quaternion.LookRotation(move.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
    }
}