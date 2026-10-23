using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public enum DamageType
    {
        Physical,
        Poison,
        Fire,
        Stun,
        Heal
    }

    public struct DamageData
    {
        public float amount;
        public DamageType type;
        public GameObject source;
        public GameObject target;

        public DamageData(float amount, DamageType type, GameObject source = null, GameObject target = null)
        {
            this.amount = amount;
            this.type = type;
            this.source = source;
            this.target = target;
        }
    }
}
