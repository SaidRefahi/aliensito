using UnityEngine;
public enum AbilitySlot
{
    Melee,
    Ranged,
    Invisibility
}

public abstract class AbilitySO : ScriptableObject
{
    [Header("Información Básica")]
    public string abilityName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;

    [Header("Configuración de Slot")]
    [Tooltip("Define a qué slot pertenece esta habilidad (Melee, Ranged o Invisibility).")]
    public AbilitySlot slot;

    public abstract void Execute(GameObject user);
}