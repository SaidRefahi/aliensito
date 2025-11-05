using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerAbilityHandler : MonoBehaviour
{
    // --- CLASE INTERNA PARA GESTIONAR ESTADO ---
    /// <summary>
    /// Contenedor que almacena la habilidad equipada y sus usos restantes.
    /// </summary>
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
            // 0 = infinito, de lo contrario usa los usos de la habilidad
            UsesLeft = (newAbility.maxUses == 0) ? int.MaxValue : newAbility.maxUses;
        }

        /// <summary>
        /// Gasta un uso. Devuelve 'true' si se quedó sin usos.
        /// </summary>
        public bool ConsumeUse()
        {
            if (Ability.maxUses == 0) return false; // Es infinito

            UsesLeft--;
            return UsesLeft <= 0;
        }
    }
    // ---------------------------------------------
    
    [Header("Dependencias")]
    [SerializeField] private Transform aimPoint;

    [Header("Habilidades Básicas (Default)")]
    [Tooltip("Habilidad a la que se revierte cuando se gastan los usos.")]
    [SerializeField] private AbilitySO defaultMeleeAbility;
    [SerializeField] private AbilitySO defaultRangedAbility;
    [SerializeField] private AbilitySO defaultInvisibilityAbility;

    // Slots de habilidad que contienen el estado
    private EquippedAbilitySlot meleeSlot;
    private EquippedAbilitySlot rangedSlot;
    private EquippedAbilitySlot invisibilitySlot;

    // --- ¡NUEVO EVENTO! ---
    /// <summary>
    /// Se dispara cuando una habilidad se queda sin usos.
    /// Pasa la habilidad que se acaba de agotar (ej. "Toque Nocivo").
    /// </summary>
    public event Action<AbilitySO> OnAbilityDepleted;

    // --- Eventos para el Animator (se mantienen) ---
    public event Action OnMeleeAttackTrigger;
    public event Action OnRangedAttackTrigger;
    public event Action OnAbilityTrigger;

    private void Awake()
    {
        // Inicializamos los slots con las habilidades básicas (usos infinitos)
        meleeSlot = new EquippedAbilitySlot(defaultMeleeAbility);
        rangedSlot = new EquippedAbilitySlot(defaultRangedAbility);
        invisibilitySlot = new EquippedAbilitySlot(defaultInvisibilityAbility);
    }
    
    // --- MÉTODOS DE USO (MODIFICADOS) ---
    public void UseMeleeAbility()
    {
        UseAbility(meleeSlot, defaultMeleeAbility, OnMeleeAttackTrigger);
    }

    public void UseRangedAbility()
    {
        UseAbility(rangedSlot, defaultRangedAbility, OnRangedAttackTrigger);
    }

    public void UseInvisibilityAbility()
    {
        UseAbility(invisibilitySlot, defaultInvisibilityAbility, OnAbilityTrigger);
    }

    /// <summary>
    /// Lógica central refactorizada para usar, consumir y revertir habilidades.
    /// </summary>
    private void UseAbility(EquippedAbilitySlot slot, AbilitySO defaultAbility, Action animatorTrigger)
    {
        if (slot.Ability == null) return;
        
        // 1. Asigna el AimSource si es necesario
        if (slot.Ability is IAimable aimableAbility)
        {
            aimableAbility.aimSource = aimPoint;
        }

        // 2. Intenta ejecutar la habilidad. Si falla (cooldown), no hagas nada.
        if (!slot.Ability.Execute(gameObject))
        {
            return;
        }

        // 3. ¡Éxito! Dispara la animación
        animatorTrigger?.Invoke();

        // 4. Consume un uso y comprueba si se agotó
        if (slot.ConsumeUse())
        {
            AbilitySO depletedAbility = slot.Ability;
            Debug.Log($"¡Se agotaron los usos de {depletedAbility.abilityName}!");
            
            // 5. Revierte a la habilidad básica
            slot.Equip(defaultAbility);
            
            // 6. ¡Dispara el evento para que el EvolutionManager lo sepa!
            OnAbilityDepleted?.Invoke(depletedAbility);
        }
    }
    
    // --- MÉTODO DE EVOLUCIÓN (MODIFICADO) ---
    /// <summary>
    /// API para que el EvolutionManager pueda equipar la nueva habilidad.
    /// </summary>
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

    // --- MÉTODO DE CONSULTA (MODIFICADO) ---
    /// <summary>
    /// Devuelve las habilidades (no los slots) que el jugador tiene equipadas.
    /// </summary>
    public List<AbilitySO> GetEquippedAbilities()
    {
        List<AbilitySO> equipped = new List<AbilitySO>();
        
        if (meleeSlot.Ability != null) equipped.Add(meleeSlot.Ability);
        if (rangedSlot.Ability != null) equipped.Add(rangedSlot.Ability);
        if (invisibilitySlot.Ability != null) equipped.Add(invisibilitySlot.Ability);
        
        return equipped;
    }
}