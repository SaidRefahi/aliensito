using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerAbilityHandler : MonoBehaviour
{
    // ... (Tu clase interna EquippedAbilitySlot no cambia) ...
    private class EquippedAbilitySlot
    {
        public AbilitySO Ability { get; private set; }
        public int UsesLeft { get; private set; }

        public EquippedAbilitySlot(AbilitySO initialAbility)
        {
            Equip(initialAbility);
        }

        public void Equip(AbilitySO newAbility)
        {
            Ability = newAbility;
            UsesLeft = (newAbility.maxUses == 0) ? int.MaxValue : newAbility.maxUses;
        }
        
        public bool ConsumeUse()
        {
            if (Ability.maxUses == 0) return false;
            UsesLeft--;
            return UsesLeft <= 0;
        }
    }
    
    // ... (Tus Headers y variables no cambian) ...
    [Header("Dependencias")]
    [SerializeField] private Transform aimPoint;

    [Header("Habilidades Básicas (Default)")]
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

    // ... (Awake no cambia) ...
    private void Awake()
    {
        meleeSlot = new EquippedAbilitySlot(defaultMeleeAbility);
        rangedSlot = new EquippedAbilitySlot(defaultRangedAbility);
        invisibilitySlot = new EquippedAbilitySlot(defaultInvisibilityAbility);
    }
    
    // --- ¡¡MÉTODO MODIFICADO!! ---
    public void UseMeleeAbility()
    {
        // ¡LA LÓGICA CAMBIÓ!
        // Esta función, llamada por el input, ahora SOLO se
        // encarga de disparar la animación.
        // El PlayerAnimator se encargará de los combos.
        // La lógica de daño/cooldown se llamará desde la animación.
        OnMeleeAttackTrigger?.Invoke();
        
        // ¡HEMOS QUITADO TODA la lógica de 'UseAbility' de aquí!
    }

    // --- ¡ESTOS MÉTODOS NO CAMBIAN! ---
    public void UseRangedAbility()
    {
        // Los proyectiles se disparan al instante,
        // así que mantienen la lógica original.
        UseAbility(rangedSlot, defaultRangedAbility, OnRangedAttackTrigger);
    }

    public void UseInvisibilityAbility()
    {
        // La invisibilidad se activa al instante.
        UseAbility(invisibilitySlot, defaultInvisibilityAbility, OnAbilityTrigger);
    }
    
    /// <summary>
    /// ¡ESTE MÉTODO SERÁ LLAMADO POR EL EVENTO DE ANIMACIÓN!
    /// Contiene la lógica que 'UseMeleeAbility' solía tener.
    /// </summary>
    public void ApplyMeleeHit()
    {
        EquippedAbilitySlot slot = meleeSlot;
        AbilitySO defaultAbility = defaultMeleeAbility;
        
        if (slot.Ability == null) return;
        
        // 1. Asigna el AimSource (si tu melee lo necesita)
        if (slot.Ability is IAimable aimableAbility)
        {
            aimableAbility.aimSource = aimPoint;
        }

        // 2. Intenta ejecutar la habilidad.
        // ¡AQUÍ es donde se comprueba el COOLDOWN!
        // Si la anim se disparó pero el CD no estaba listo,
        // el 'Execute' devolverá false y no habrá daño.
        if (!slot.Ability.Execute(gameObject))
        {
            return;
        }
        
        // 3. ¡Éxito! El golpe conectó y el CD estaba listo.
        // Ahora consume un uso y comprueba si se agotó.
        if (slot.ConsumeUse())
        {
            AbilitySO depletedAbility = slot.Ability;
            Debug.Log($"¡Se agotaron los usos de {depletedAbility.abilityName}!");
            
            // Revierte a la habilidad básica
            slot.Equip(defaultAbility);
            
            // Dispara el evento para el EvolutionManager
            OnAbilityDepleted?.Invoke(depletedAbility);
        }
    }

    // --- ¡ESTOS MÉTODOS NO CAMBIAN! ---

    /// <summary>
    /// Lógica central refactorizada (Usada por Ranged e Invisibility)
    /// </summary>
    private void UseAbility(EquippedAbilitySlot slot, AbilitySO defaultAbility, Action animatorTrigger)
    {
        if (slot.Ability == null) return;
        
        if (slot.Ability is IAimable aimableAbility)
        {
            aimableAbility.aimSource = aimPoint;
        }

        if (!slot.Ability.Execute(gameObject))
        {
            return;
        }

        animatorTrigger?.Invoke();

        if (slot.ConsumeUse())
        {
            AbilitySO depletedAbility = slot.Ability;
            Debug.Log($"¡Se agotaron los usos de {depletedAbility.abilityName}!");
            slot.Equip(defaultAbility);
            OnAbilityDepleted?.Invoke(depletedAbility);
        }
    }
    
    // ... (EvolveAbility y GetEquippedAbilities no cambian) ...
    public void EvolveAbility(AbilitySlot slot, AbilitySO newAbility)
    {
        switch (slot)
        {
            case AbilitySlot.Melee:
                meleeSlot.Equip(newAbility);
                break;
            case AbilitySlot.Ranged:
                rangedSlot.Equip(newAbility);
                break;
            case AbilitySlot.Invisibility:
                invisibilitySlot.Equip(newAbility);
                break;
        }
    }
    
    public List<AbilitySO> GetEquippedAbilities()
    {
        List<AbilitySO> equipped = new List<AbilitySO>();
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