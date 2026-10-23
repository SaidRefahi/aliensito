using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class ObjectRotator : MonoBehaviour
    {
        [Tooltip("Rotation speed in degrees per second.")]
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 45f, 0f);

        private void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
