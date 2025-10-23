using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(StatusEffectManager))]
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
    [SerializeField] public Transform aimPoint;

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

    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    public void OnMeleeAttack(InputValue value) { if (value.isPressed) UseAbility(meleeAbility); }
    public void OnRangedAttack(InputValue value) { if (value.isPressed) UseAbility(rangedAbility); }
    public void OnInvisibility(InputValue value) { if (value.isPressed) UseAbility(invisibilityAbility); }
    
    private void UseAbility(AbilitySO ability)
    {
        if (ability == null) return;
        if (ability is IAimable aimableAbility)
        {
            aimableAbility.aimSource = aimPoint;
        }
        ability.Execute(gameObject);
    }

    // --- MÉTODO EVOLVEABILITY CORREGIDO ---
    public void EvolveAbility(AbilitySlot slot, AbilitySO newAbility)
    {
        switch (slot)
        {
            case AbilitySlot.Melee:
                meleeAbility = newAbility;
                break;
            case AbilitySlot.Ranged:
                rangedAbility = newAbility;
                break;
            case AbilitySlot.Invisibility:
                invisibilityAbility = newAbility;
                break;
        }
        Debug.Log($"<color=cyan>Habilidad del slot {slot} evolucionada a: {newAbility?.abilityName ?? "ninguna"}</color>");
    }
}