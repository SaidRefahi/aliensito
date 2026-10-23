using UnityEngine;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public enum AbilitySlot
    {
        Melee,
        Ranged,
        Invisibility
    }

    public abstract class AbilitySO : ScriptableObject
    {
        [Title("Basic Information")]
        [SerializeField] private string abilityName;
        [SerializeField] private Sprite icon;
        [TextArea(3, 5)]
        [SerializeField] private string description;

        [Title("Slot & Lifecycle")]
        [SerializeField] private AbilitySlot slot;
        [Min(0)]
        [SerializeField] private int maxUses = 0;
        [SerializeField] private AbilitySO nextLevelAbility;
        [SerializeField] private AbilitySO baseAbility;

        [Title("Cooldown & Execution")]
        [Min(0f)]
        [SerializeField] private float cooldown = 0.5f;

        public string AbilityName => abilityName;
        public Sprite Icon => icon;
        public string Description => description;
        public AbilitySlot Slot => slot;
        public int MaxUses => maxUses;
        public AbilitySO NextLevelAbility => nextLevelAbility;
        public AbilitySO BaseAbility => baseAbility;
        public float Cooldown => cooldown;

        public abstract bool Execute(GameObject user);
    }
}
