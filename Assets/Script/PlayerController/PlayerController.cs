using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody rb;
    private Vector2 moveInput;

    [Header("Abilities")]
    [SerializeField] private AbilitySO meleeAbility;
    [SerializeField] private AbilitySO rangedAbility;
    [SerializeField] private AbilitySO invisibilityAbility;

    [Header("Aim")]
    [SerializeField] private Transform aimPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate() => HandleMovement();

    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        if (move.sqrMagnitude > 0.0001f)
        {
            rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
            Quaternion targetRot = Quaternion.LookRotation(move.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    // --- MÉTODOS DE INPUT LLAMADOS POR "SEND MESSAGES" ---

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnMeleeAttack(InputValue value)
    {
        if (value.isPressed)
        {
            UseAbility(meleeAbility);
        }
    }

    // Dejamos este por si quieres volver a usarlo en el futuro
    public void OnRangedAttack(InputValue value)
    {
        if (value.isPressed)
        {
            UseAbility(rangedAbility);
        }
    }

    public void OnInvisibility(InputValue value)
    {
        if (value.isPressed)
        {
            UseAbility(invisibilityAbility);
        }
    }

    // --- LÓGICA CENTRAL DE HABILIDADES ---
    private void UseAbility(AbilitySO ability)
    {
        if (ability == null) return;

        Debug.Log($"<color=lime>PlayerController: Activando habilidad '{ability.abilityName}'.</color>");

        if (ability is IAimable aimableAbility)
        {
            aimableAbility.aimSource = aimPoint;
        }

        ability.Execute(gameObject);
    }

    // El método EvolveAbility no necesita cambios
    public void EvolveAbility(string abilityName, AbilitySO newAbility) { /* ... */ }
}