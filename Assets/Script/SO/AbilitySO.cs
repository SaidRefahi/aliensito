using UnityEngine;
using System.Collections.Generic; 
public abstract class AbilitySO : ScriptableObject
{
    [Header("Información Básica")]
    public string abilityName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;

    [Header("Configuración de Slot")]
    [Tooltip("Define a qué slot pertenece esta habilidad.")]
    public AbilitySlot slot;

    // --- ¡NUEVAS VARIABLES AÑADIDAS! ---
    [Header("Configuración de Usos y Evolución")]
    [Tooltip("Cuántas veces se puede usar. 0 = Infinito (ej. habilidad básica)")]
    public int maxUses = 0;
    
    [Tooltip("Arrastra aquí el AbilitySO del siguiente nivel (ej. 'Toque Nocivo +1')")]
    public AbilitySO nextLevelAbility;
    
    // --- NUEVAS VARIABLES PARA UPGRADE CHAIN (Opcional pero recomendado) ---
    [Tooltip("Si este SO es una versión +1 o +2, arrastra su habilidad BASE aquí.")]
    public AbilitySO baseAbility; // Si es null, esta ES la habilidad base.
    
    // --- FIN DE NUEVAS VARIABLES ---

    /// <summary>
    /// Ejecuta la lógica de la habilidad.
    /// Devuelve 'true' si la habilidad se usó con éxito (ej. no está en cooldown).
    /// </summary>
    public abstract bool Execute(GameObject user);
}

// (El enum AbilitySlot se mantiene igual)
public enum AbilitySlot
{
    Melee,
    Ranged,
    Invisibility
}