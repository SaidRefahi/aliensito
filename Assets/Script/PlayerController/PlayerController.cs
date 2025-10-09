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
    [Tooltip("Punto de mira compartido para melee y ranged. Crear child 'AimPoint' y arrastrarlo aquí.")]
    [SerializeField] private Transform aimPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // --- INPUT CALLBACKS (Input System) ---
    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnMelee(InputValue value) { if (value.isPressed) MeleeAttack(); }
    public void OnRanged(InputValue value) { if (value.isPressed) RangedAttack(); }
    public void OnInvisibility(InputValue value) { if (value.isPressed) ToggleInvisibility(); }

    // --- PHYSICS LOOP ---
    private void FixedUpdate() => HandleMovement();

    // --- MOVEMENT & ROTATION ---
    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        if (move.sqrMagnitude > 0.0001f)
        {
            Vector3 targetPos = rb.position + move * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);

            Quaternion targetRot = Quaternion.LookRotation(move.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    // --- ABILITY TRIGGERS ---
    private void MeleeAttack()
    {
        if (meleeAbility == null) return;

        if (aimPoint != null && meleeAbility is MeleeAbilitySO meleeSO)
            meleeSO.aimSource = aimPoint;

        meleeAbility.Execute(gameObject);
    }

    private void RangedAttack()
    {
        if (rangedAbility == null) return;

        if (aimPoint != null)
        {
            // Si la habilidad tiene un campo público aimSource lo ajustamos (segura verificando el tipo)
            if (rangedAbility is MeleeAbilitySO meleeAsRanged) // covers shared interface if ranged reuses MeleeSO
                meleeAsRanged.aimSource = aimPoint;

            // Adaptar a otra implementación de ranged: buscar propiedad 'aimSource' por reflexión opcional
            var rangedType = rangedAbility.GetType();
            var prop = rangedType.GetField("aimSource");
            if (prop != null && prop.FieldType == typeof(Transform))
                prop.SetValue(rangedAbility, aimPoint);
        }

        rangedAbility.Execute(gameObject);
    }

    private void ToggleInvisibility()
    {
        invisibilityAbility?.Execute(gameObject);
    }

    // --- API ---
    public Transform AimPoint => aimPoint;
    public void SetAimPoint(Transform t) => aimPoint = t;

    // --- EVOLUTION API ---
    public void EvolveAbility(string abilityName, AbilitySO newAbility)
    {
        switch (abilityName)
        {
            case "Melee":
                meleeAbility = newAbility;
                break;
            case "Ranged":
                rangedAbility = newAbility;
                break;
            case "Invisibility":
                invisibilityAbility = newAbility;
                break;
            default:
                Debug.LogWarning($"EvolveAbility: unknown ability name '{abilityName}'");
                return;
        }

        Debug.Log($"Evolucionada habilidad: {abilityName} → {newAbility?.abilityName ?? "null"}");
    }
}