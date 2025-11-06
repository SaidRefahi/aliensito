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

    [Header("Configuración de Usos y Evolución")]
    [Tooltip("Cuántas veces se puede usar. 0 = Infinito (ej. habilidad básica)")]
    public int maxUses = 0;
    
    [Tooltip("Arrastra aquí el AbilitySO del siguiente nivel (ej. 'Toque Nocivo +1')")]
    public AbilitySO nextLevelAbility;
    
    [Tooltip("Si este SO es una versión +1 o +2, arrastra su habilidad BASE aquí.")]
    public AbilitySO baseAbility;
    
    // --- ¡NUEVA LÍNEA AÑADIDA! ---
    [Header("Configuración de Cooldown")]
    [Tooltip("Tiempo en segundos que tarda en recargarse. 0 = sin cooldown.")]
    public float cooldown = 0.5f; 
    // --- FIN DE LÍNEA NUEVA ---

    public abstract bool Execute(GameObject user);
}

public enum AbilitySlot
{
    Melee,
    Ranged,
    Invisibility
}