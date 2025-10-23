using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    [Header("Información Básica")]
    public string abilityName;
    public Sprite icon;

    // --- LÍNEA AÑADIDA ---
    [Tooltip("La descripción de la habilidad que se mostrará en la UI.")]
    [TextArea(3, 5)] // Esto hace que el campo de texto sea más grande en el Inspector.
    public string description;
    // ---------------------

    public abstract void Execute(GameObject user);
}