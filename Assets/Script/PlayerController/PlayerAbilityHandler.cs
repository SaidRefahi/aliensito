using UnityEngine;
using System;
using System.Collections.Generic;

// --- NUEVO ---
// Creamos una 'struct' para enviar los datos del evento de forma ordenada.
// Esto le dice a la UI qué slot se actualizó y cuántos usos tiene.
// (Esta struct SÍ se queda aquí, no está duplicada)
public struct AbilityUseData
{
    public AbilitySlot slot;
    public int usesLeft;
    public int maxUses; // Usaremos esto para saber si es infinita (maxUses == 0)

    public AbilityUseData(AbilitySlot slot, int usesLeft, int maxUses)
    {
        this.slot = slot;
        this.usesLeft = usesLeft;
        this.maxUses = maxUses;
    }
}


// --- SCRIPT COMPLETO Y MODIFICADO ---
// (Ya no contiene la definición duplicada de 'AbilitySlot')

public class PlayerAbilityHandler : MonoBehaviour
{
    // Clase interna para gestionar cada slot de habilidad
    private class EquippedAbilitySlot
    {
        public AbilitySO Ability { get; private set; }
        public int UsesLeft { get; private set; }
        
        // --- MODIFICADO ---
        // Hacemos público el 'MaxUses' para que la UI pueda leerlo
        public int MaxUses { get; private set; } 

        public EquippedAbilitySlot(AbilitySO initialAbility)
        {
            Equip(initialAbility);
        }

        public void Equip(AbilitySO newAbility)
        {
            Ability = newAbility;
            // --- MODIFICADO ---
            // Guardamos el máximo de usos
            MaxUses = newAbility.maxUses; 
            UsesLeft = (MaxUses == 0) ? int.MaxValue : MaxUses;
        }
        
        /// <summary>
        /// Consume un uso. Devuelve true si la habilidad se agotó.
        /// </summary>
        public bool ConsumeUse()
        {
            // --- MODIFICADO ---
            // Comprobamos contra 'MaxUses' en lugar de 'Ability.maxUses'
            if (MaxUses == 0) return false; // Habilidad infinita
            UsesLeft--;
            return UsesLeft <= 0;
        }
    }
    
    [Header("Dependencias")]
    [SerializeField] private Transform aimPoint;

    [Header("Habilidades Básicas (Default)")]
    [SerializeField] private AbilitySO defaultMeleeAbility;
    [SerializeField] private AbilitySO defaultRangedAbility;
    [SerializeField] private AbilitySO defaultInvisibilityAbility;
    
    // Slots de habilidades
    private EquippedAbilitySlot meleeSlot;
    private EquippedAbilitySlot rangedSlot;
    private EquippedAbilitySlot invisibilitySlot;

    // Eventos públicos
    public event Action<AbilitySO> OnAbilityDepleted;
    public event Action OnMeleeAttackTrigger;
    public event Action OnRangedAttackTrigger;
    public event Action OnAbilityTrigger;

    // --- NUEVO EVENTO PARA LA UI ---
    /// <summary>
    /// Se dispara cuando los usos de una habilidad cambian (al equipar o usar).
    /// </summary>
    public event Action<AbilityUseData> OnAbilityUsesChanged;
    // --- FIN DE LÍNEA NUEVA ---


    private void Awake()
    {
        // Inicializamos los slots con las habilidades por defecto
        meleeSlot = new EquippedAbilitySlot(defaultMeleeAbility);
        rangedSlot = new EquippedAbilitySlot(defaultRangedAbility);
        invisibilitySlot = new EquippedAbilitySlot(defaultInvisibilityAbility);
    }

    // --- NUEVO ---
    /// <summary>
    /// En Start, disparamos los eventos iniciales
    /// para que la UI se configure al empezar la escena.
    /// </summary>
    private void Start()
    {
        NotifyUsesChanged(AbilitySlot.Melee, meleeSlot);
        NotifyUsesChanged(AbilitySlot.Ranged, rangedSlot);
        NotifyUsesChanged(AbilitySlot.Invisibility, invisibilitySlot);
    }
    
    /// <summary>
    /// Llamado por el Input. Solo dispara el trigger de animación.
    /// </summary>
    public void UseMeleeAbility()
    {
        // La lógica de daño/cooldown se mueve a ApplyMeleeHit()
        OnMeleeAttackTrigger?.Invoke();
    }

    /// <summary>
    /// Llamado por el Input. Ejecuta la habilidad de rango.
    /// </summary>
    public void UseRangedAbility()
    {
        // --- MODIFICADO ---
        // Le pasamos el tipo de slot para notificar a la UI
        UseAbility(rangedSlot, defaultRangedAbility, OnRangedAttackTrigger, AbilitySlot.Ranged);
    }

    /// <summary>
    /// Llamado por el Input. Ejecuta la habilidad de invisibilidad.
    /// </summary>
    public void UseInvisibilityAbility()
    {
        // --- MODIFICADO ---
        // Le pasamos el tipo de slot para notificar a la UI
        UseAbility(invisibilitySlot, defaultInvisibilityAbility, OnAbilityTrigger, AbilitySlot.Invisibility);
    }
    
    /// <summary>
    /// ¡LLAMADO POR EVENTO DE ANIMACIÓN!
    /// Aplica la lógica de la habilidad Melee (daño, cooldown, usos).
    /// </summary>
    public void ApplyMeleeHit()
    {
        EquippedAbilitySlot slot = meleeSlot;
        AbilitySO defaultAbility = defaultMeleeAbility;
        
        if (slot.Ability == null) return;
        
        if (slot.Ability is IAimable aimableAbility)
        {
            aimableAbility.aimSource = aimPoint;
        }

        if (!slot.Ability.Execute(gameObject))
        {
            return; // Falló (ej. en cooldown)
        }
        
        if (slot.ConsumeUse())
        {
            AbilitySO depletedAbility = slot.Ability;
            Debug.Log($"¡Se agotaron los usos de {depletedAbility.abilityName}!");
            
            slot.Equip(defaultAbility); // Revierte a la básica
            OnAbilityDepleted?.Invoke(depletedAbility); // Notifica al EvolutionManager
        }

        // --- NUEVO ---
        // Notificamos a la UI que los usos de Melee han cambiado.
        NotifyUsesChanged(AbilitySlot.Melee, slot);
        // --- FIN DE LÍNEA NUEVA ---
    }

    /// <summary>
    /// Lógica central para habilidades de ejecución instantánea (Ranged, Invisibility)
    /// --- MODIFICADO ---
    /// Añadimos el parámetro 'slotType' para saber qué UI actualizar
    /// </summary>
    private void UseAbility(EquippedAbilitySlot slot, AbilitySO defaultAbility, Action animatorTrigger, AbilitySlot slotType)
    {
        if (slot.Ability == null) return;
        
        if (slot.Ability is IAimable aimableAbility)
        {
            aimableAbility.aimSource = aimPoint;
        }

        if (!slot.Ability.Execute(gameObject))
        {
            return; // Falló (ej. en cooldown)
        }

        animatorTrigger?.Invoke();

        if (slot.ConsumeUse())
        {
            AbilitySO depletedAbility = slot.Ability;
            Debug.Log($"¡Se agotaron los usos de {depletedAbility.abilityName}!");
            slot.Equip(defaultAbility);
            OnAbilityDepleted?.Invoke(depletedAbility);
        }

        // --- NUEVO ---
        // Notificamos a la UI que los usos de este slot han cambiado.
        NotifyUsesChanged(slotType, slot);
        // --- FIN DE LÍNEA NUEVA ---
    }
    
    /// <summary>
    /// Evoluciona un slot de habilidad con una nueva habilidad.
    /// ¡INCLUYE LA CORRECCIÓN PARA EL BLOQUEO!
    /// </summary>
    public void EvolveAbility(AbilitySlot slot, AbilitySO newAbility)
    {
        // --- (Tu lógica de limpieza de 'runner' no cambia) ---
        switch (slot)
        {
            case AbilitySlot.Melee:
                break;
            case AbilitySlot.Ranged:
                break;
            case AbilitySlot.Invisibility:
                InvisibilitySO.InvisibilityRunner activeRunner = GetComponent<InvisibilitySO.InvisibilityRunner>();
                if (activeRunner != null)
                {
                    activeRunner.Stop(); 
                }
                break;
        }

        // Lógica original: Equipar la nueva habilidad
        switch (slot)
        {
            case AbilitySlot.Melee:
                meleeSlot.Equip(newAbility);
                // --- NUEVO --- 
                // Notificamos a la UI del cambio
                NotifyUsesChanged(AbilitySlot.Melee, meleeSlot);
                break;
            case AbilitySlot.Ranged:
                rangedSlot.Equip(newAbility);
                // --- NUEVO --- 
                // Notificamos a la UI del cambio
                NotifyUsesChanged(AbilitySlot.Ranged, rangedSlot);
                break;
            case AbilitySlot.Invisibility:
                invisibilitySlot.Equip(newAbility);
                // --- NUEVO --- 
                // Notificamos a la UI del cambio
                NotifyUsesChanged(AbilitySlot.Invisibility, invisibilitySlot);
                break;
        }
    }

    // --- NUEVO MÉTODO ---
    /// <summary>
    /// Dispara el evento OnAbilityUsesChanged con los datos actuales del slot.
    /// Es un método ayudante para no repetir código.
    /// </summary>
    private void NotifyUsesChanged(AbilitySlot slotType, EquippedAbilitySlot slot)
    {
        // Creamos el paquete de datos
        AbilityUseData data = new AbilityUseData(slotType, slot.UsesLeft, slot.MaxUses);
        // Disparamos el evento (si alguien está escuchando)
        OnAbilityUsesChanged?.Invoke(data);
    }
    // --- FIN DE MÉTODO NUEVO ---
    
    /// <summary>
    /// Devuelve una lista de las habilidades equipadas actualmente.
    /// </summary>
    public List<AbilitySO> GetEquippedAbilities()
    {
        List<AbilitySO> equipped = new List<AbilitySO>();
        if (meleeSlot.Ability != null) equipped.Add(meleeSlot.Ability);
        if (rangedSlot.Ability != null) equipped.Add(rangedSlot.Ability);
        if (invisibilitySlot.Ability != null) equipped.Add(invisibilitySlot.Ability);
        return equipped;
    }
    
    /// <summary>
    /// Devuelve el número máximo de combos (actualmente fijo).
    /// </summary>
    public int GetMaxMeleeCombo()
    {
        return 3;
    }
}