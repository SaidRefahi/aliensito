using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Parpadeo", menuName = "Habilidades/Invisibilidad/Parpadeo")]
    public class ParpadeoSO : InvisibilitySO
    {
        [Title("Blink Settings")]
        [SerializeField] private float blinkDistance = 10f;
        [SerializeField] private LayerMask wallLayer;

        [Title("VFX Pool Tags")]
        [SerializeField] private string startVfxPoolTag = "BlinkStartVFX";
        [SerializeField] private string endVfxPoolTag = "BlinkEndVFX";

        public float BlinkDistance => blinkDistance;
        public LayerMask WallLayer => wallLayer;
        public string StartVfxPoolTag => startVfxPoolTag;
        public string EndVfxPoolTag => endVfxPoolTag;

        public override bool Execute(GameObject user)
        {
            bool executed = base.Execute(user);
            if (!executed) return false;

            Rigidbody rb = user.GetComponent<Rigidbody>();
            if (rb == null)
            {
                PerformLegacyBlink(user.transform);
                return true;
            }

            Vector3 startPosition = rb.position;
            SpawnVFX(startVfxPoolTag, startPosition, user.transform.rotation);

            Vector3 forwardDirection = user.transform.forward;
            forwardDirection.y = 0;
            Vector3 blinkVector = forwardDirection.normalized * blinkDistance;
            Vector3 endPosition = startPosition + blinkVector;

            if (Physics.Linecast(startPosition, endPosition, wallLayer))
            {
                return true;
            }

            rb.MovePosition(endPosition);
            SpawnVFX(endVfxPoolTag, endPosition, user.transform.rotation);
            return true;
        }

        private void PerformLegacyBlink(Transform userTransform)
        {
            Vector3 startPosition = userTransform.position;
            Vector3 forwardDirection = userTransform.forward;
            forwardDirection.y = 0;
            Vector3 blinkVector = forwardDirection.normalized * blinkDistance;
            Vector3 endPosition = startPosition + blinkVector;

            SpawnVFX(startVfxPoolTag, startPosition, userTransform.rotation);

            if (Physics.Linecast(startPosition, endPosition, wallLayer))
            {
                return;
            }

            userTransform.position = endPosition;
            SpawnVFX(endVfxPoolTag, endPosition, userTransform.rotation);
        }

        private void SpawnVFX(string tag, Vector3 position, Quaternion rotation)
        {
            if (PoolManager.Instance != null && !string.IsNullOrEmpty(tag))
            {
                PoolManager.Instance.SpawnFromPool(tag, position, rotation);
            }
        }
    }
}
