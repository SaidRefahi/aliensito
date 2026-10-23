using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class PoisonAura : MonoBehaviour
    {
        [SerializeField] private float radius;
        [SerializeField] private float poisonDamage;
        [SerializeField] private float poisonDuration;
        [SerializeField] private LayerMask enemyLayers;

        public float Radius => radius;
        public float PoisonDamage => poisonDamage;
        public float PoisonDuration => poisonDuration;
        public LayerMask EnemyLayers => enemyLayers;

        private float tickTimer;
        private readonly Collider[] hitColliders = new Collider[16];

        public void Initialize(float radius, float damage, float duration, LayerMask layers)
        {
            this.radius = radius;
            this.poisonDamage = damage;
            this.poisonDuration = duration;
            this.enemyLayers = layers;
        }

        private void Update()
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                ApplyPoisonToNearby();
            }
        }

        private void ApplyPoisonToNearby()
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, radius, hitColliders, enemyLayers);
            for (int i = 0; i < hits; i++)
            {
                var hit = hitColliders[i];
                if (hit != null && hit.TryGetComponent<StatusEffectManager>(out var effectManager))
                {
                    effectManager.ApplyEffect(new Poison(hit.gameObject, poisonDuration, poisonDamage));
                }
            }
        }
    }
}
