using UnityEngine;
using System.Collections.Generic;

// Define qué líneas de mira debe mostrar la IA al telegrafiar este ataque
public enum TelegraphType
{
    None,   // No mostrar líneas
    Center, // Mostrar solo la línea central
    Sides,  // Mostrar solo las líneas laterales
    All     // Mostrar las 3 líneas (Centro, Izquierda, Derecha)
}

[CreateAssetMenu(fileName = "NewAttackSequence", menuName = "Boss/Attack Sequence")]
public class AttackSequenceSO : ScriptableObject
{
    [Header("Configuración de Ataque")]
    [Tooltip("La secuencia de habilidades SO a ejecutar, en orden.")]
    public List<AbilitySO> attacksInSequence;

    [Header("Configuración de Telegrafiado")]
    [Tooltip("Qué líneas de mira debe mostrar la IA al prepararse para este ataque.")]
    public TelegraphType telegraphType = TelegraphType.Center;
}