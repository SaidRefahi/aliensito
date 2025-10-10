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

    private void OnEnable()
    {
        InputManager.OnMelee += MeleeAttack;
        InputManager.OnRanged += RangedAttack;
        InputManager.OnInvisibilityPressed += ToggleInvisibility; // Usando el nombre corregido del evento
    }

    private void OnDisable()
    {
        InputManager.OnMelee -= MeleeAttack;
        InputManager.OnRanged -= RangedAttack;
        InputManager.OnInvisibilityPressed -= ToggleInvisibility; // Usando el nombre corregido del evento
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();

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

    private void MeleeAttack()
    {
        if (meleeAbility == null) return;
        Debug.Log("PlayerController: Activando MeleeAttack.");
        if (meleeAbility is IAimable aimableMelee) { aimableMelee.aimSource = aimPoint; }
        meleeAbility.Execute(gameObject);
    }

    private void RangedAttack()
    {
        if (rangedAbility == null) return;
        Debug.Log("PlayerController: Activando RangedAttack.");
        if (rangedAbility is IAimable aimableRanged) { aimableRanged.aimSource = aimPoint; }
        rangedAbility.Execute(gameObject);
    }

    private void ToggleInvisibility()
    {
        if (invisibilityAbility == null) return;
        Debug.Log("PlayerController: Activando Invisibility.");
        invisibilityAbility.Execute(gameObject);
    }

    // --- MÉTODO RE-AÑADIDO ---
    /// <summary>
    /// Permite al EvolutionManager cambiar la habilidad actual del jugador.
    /// </summary>
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
        Debug.Log($"<color=cyan>Habilidad evolucionada: {abilityName} → {newAbility?.abilityName ?? "ninguna"}</color>");
    }
}