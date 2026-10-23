using System;
using System.Collections.Generic;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public struct AbilityUseData
    {
        public AbilitySlot slot;
        public int usesLeft;
        public int maxUses;

        public AbilityUseData(AbilitySlot slot, int usesLeft, int maxUses)
        {
            this.slot = slot;
            this.usesLeft = usesLeft;
            this.maxUses = maxUses;
        }
    }

    public class PlayerAbilityHandler : MonoBehaviour
    {
        private class EquippedAbilitySlot
        {
            public AbilitySO Ability { get; private set; }
            public int UsesLeft { get; private set; }
            public int MaxUses { get; private set; }

            public EquippedAbilitySlot(AbilitySO initialAbility)
            {
                Equip(initialAbility);
            }

            public void Equip(AbilitySO newAbility)
            {
                Ability = newAbility;
                if (newAbility != null)
                {
                    MaxUses = newAbility.MaxUses;
                    UsesLeft = (MaxUses == 0) ? int.MaxValue : MaxUses;
                }
                else
                {
                    MaxUses = 0;
                    UsesLeft = 0;
                }
            }

            public bool ConsumeUse()
            {
                if (MaxUses == 0) return false;
                UsesLeft--;
                return UsesLeft <= 0;
            }
        }

        [Title("Aim Settings")]
        [SerializeField] private Transform aimPoint;

        [Title("Default Starting Abilities")]
        [SerializeField] private AbilitySO defaultMeleeAbility;
        [SerializeField] private AbilitySO defaultRangedAbility;
        [SerializeField] private AbilitySO defaultInvisibilityAbility;

        private EquippedAbilitySlot meleeSlot;
        private EquippedAbilitySlot rangedSlot;
        private EquippedAbilitySlot invisibilitySlot;

        public event Action<AbilitySO> OnAbilityDepleted;
        public event Action OnMeleeAttackTrigger;
        public event Action OnRangedAttackTrigger;
        public event Action OnAbilityTrigger;
        public event Action<AbilityUseData> OnAbilityUsesChanged;

        private void Awake()
        {
            meleeSlot = new EquippedAbilitySlot(defaultMeleeAbility);
            rangedSlot = new EquippedAbilitySlot(defaultRangedAbility);
            invisibilitySlot = new EquippedAbilitySlot(defaultInvisibilityAbility);
        }

        private void Start()
        {
            NotifyUsesChanged(AbilitySlot.Melee, meleeSlot);
            NotifyUsesChanged(AbilitySlot.Ranged, rangedSlot);
            NotifyUsesChanged(AbilitySlot.Invisibility, invisibilitySlot);
        }

        public void UseMeleeAbility()
        {
            OnMeleeAttackTrigger?.Invoke();
        }

        public void UseRangedAbility()
        {
            UseAbility(rangedSlot, defaultRangedAbility, OnRangedAttackTrigger, AbilitySlot.Ranged);
        }

        public void UseInvisibilityAbility()
        {
            UseAbility(invisibilitySlot, defaultInvisibilityAbility, OnAbilityTrigger, AbilitySlot.Invisibility);
        }

        public void ApplyMeleeHit()
        {
            EquippedAbilitySlot slot = meleeSlot;
            AbilitySO defaultAbility = defaultMeleeAbility;

            if (slot.Ability == null) return;

            if (slot.Ability is IAimable aimableAbility)
            {
                aimableAbility.SetAimSource(aimPoint);
            }

            if (!slot.Ability.Execute(gameObject))
            {
                return;
            }

            if (slot.ConsumeUse())
            {
                AbilitySO depletedAbility = slot.Ability;
                slot.Equip(defaultAbility);
                OnAbilityDepleted?.Invoke(depletedAbility);
            }

            NotifyUsesChanged(AbilitySlot.Melee, slot);
        }

        private void UseAbility(EquippedAbilitySlot slot, AbilitySO defaultAbility, Action animatorTrigger, AbilitySlot slotType)
        {
            if (slot.Ability == null) return;

            if (slot.Ability is IAimable aimableAbility)
            {
                aimableAbility.SetAimSource(aimPoint);
            }

            if (!slot.Ability.Execute(gameObject))
            {
                return;
            }

            animatorTrigger?.Invoke();

            if (slot.ConsumeUse())
            {
                AbilitySO depletedAbility = slot.Ability;
                slot.Equip(defaultAbility);
                OnAbilityDepleted?.Invoke(depletedAbility);
            }

            NotifyUsesChanged(slotType, slot);
        }

        public void EvolveAbility(AbilitySlot slot, AbilitySO newAbility)
        {
            switch (slot)
            {
                case AbilitySlot.Melee:
                    meleeSlot.Equip(newAbility);
                    NotifyUsesChanged(AbilitySlot.Melee, meleeSlot);
                    break;
                case AbilitySlot.Ranged:
                    rangedSlot.Equip(newAbility);
                    NotifyUsesChanged(AbilitySlot.Ranged, rangedSlot);
                    break;
                case AbilitySlot.Invisibility:
                    invisibilitySlot.Equip(newAbility);
                    NotifyUsesChanged(AbilitySlot.Invisibility, invisibilitySlot);
                    break;
            }
        }

        private void NotifyUsesChanged(AbilitySlot slotType, EquippedAbilitySlot slot)
        {
            AbilityUseData data = new AbilityUseData(slotType, slot.UsesLeft, slot.MaxUses);
            OnAbilityUsesChanged?.Invoke(data);
        }

        public List<AbilitySO> GetEquippedAbilities()
        {
            var equipped = new List<AbilitySO>(3);
            if (meleeSlot.Ability != null) equipped.Add(meleeSlot.Ability);
            if (rangedSlot.Ability != null) equipped.Add(rangedSlot.Ability);
            if (invisibilitySlot.Ability != null) equipped.Add(invisibilitySlot.Ability);
            return equipped;
        }

        public int GetMaxMeleeCombo()
        {
            return 3;
        }
    }
}
