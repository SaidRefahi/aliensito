using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;

        private Rigidbody rb;
        private Vector2 moveInput;

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

        public void SetMoveInput(Vector2 input)
        {
            moveInput = input;
        }

        private void HandleMovement()
        {
            Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);

            if (move.sqrMagnitude > 0.0001f)
            {
                rb.MovePosition(rb.position + move * (moveSpeed * Time.fixedDeltaTime));

                Quaternion targetRot = Quaternion.LookRotation(move.normalized, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
            }
        }
    }
}
