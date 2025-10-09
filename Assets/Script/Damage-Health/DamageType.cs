using UnityEngine;

public enum DamageType { Physical, Poison, Fire, Stun, Heal }

public class DamageData
{
    public float amount;
    public DamageType type;
    public GameObject source;
    public GameObject target;
}
