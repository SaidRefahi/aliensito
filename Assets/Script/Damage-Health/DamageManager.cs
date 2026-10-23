using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class DamageManager : MonoBehaviour
    {
        public void ApplyDamage(DamageData data)
        {
            if (data.target == null) return;

            Health targetHealth = data.target.GetComponent<Health>();
            if (targetHealth == null) return;

            if (data.type == DamageType.Heal)
            {
                targetHealth.Heal(data.amount);
            }
            else
            {
                targetHealth.TakeDamage(data.amount);
            }
        }
    }
}
