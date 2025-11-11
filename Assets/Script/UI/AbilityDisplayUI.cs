using UnityEngine;
using TMPro; // ¡Importante! Necesitamos TextMeshPro

/// <summary>
/// Este script escucha al PlayerAbilityHandler y actualiza
/// los textos de la UI con los usos restantes.
/// </summary>
public class AbilityDisplayUI : MonoBehaviour
{
    [Header("Dependencias")]
    [Tooltip("Arrastra aquí el GameObject del Jugador (el que tiene PlayerAbilityHandler)")]
    [SerializeField] private PlayerAbilityHandler playerAbilityHandler;

    [Header("Contadores de Texto (UI)")]
    [Tooltip("El componente de texto para los usos de Melee")]
    [SerializeField] private TextMeshProUGUI meleeUsesText;
    
    [Tooltip("El componente de texto para los usos de Rango")]
    [SerializeField] private TextMeshProUGUI rangedUsesText;
    
    [Tooltip("El componente de texto para los usos de Invisibilidad")]
    [SerializeField] private TextMeshProUGUI invisibilityUsesText;

    // Símbolo para usos infinitos (opcional)
    // private const string INFINITY_SYMBOL = "∞";

    private void OnEnable()
    {
        // Verificamos que el handler esté asignado
        if (playerAbilityHandler == null)
        {
            Debug.LogError("¡AbilityDisplayUI no tiene un PlayerAbilityHandler asignado!");
            return;
        }

        // Nos suscribimos al evento.
        // Ahora, cuando 'OnAbilityUsesChanged' se dispare, se llamará a 'UpdateAbilityText'.
        playerAbilityHandler.OnAbilityUsesChanged += UpdateAbilityText;
    }

    private void OnDisable()
    {
        // Es MUY importante desuscribirse al desactivar o cambiar de escena
        // para evitar errores (memory leaks).
        if (playerAbilityHandler != null)
        {
            playerAbilityHandler.OnAbilityUsesChanged -= UpdateAbilityText;
        }
    }

    /// <summary>
    /// Este método es llamado por el evento del PlayerAbilityHandler.
    /// Recibe los datos y actualiza el texto de la UI correspondiente.
    /// </summary>
    private void UpdateAbilityText(AbilityUseData data)
    {
        TextMeshProUGUI targetText = null;

        // 1. Identificamos qué componente de texto debemos actualizar
        switch (data.slot)
        {
            case AbilitySlot.Melee:
                targetText = meleeUsesText;
                break;
            case AbilitySlot.Ranged:
                targetText = rangedUsesText;
                break;
            case AbilitySlot.Invisibility:
                targetText = invisibilityUsesText;
                break;
        }

        // Si no tenemos un texto asignado para ese slot, no hacemos nada
        if (targetText == null) return;

        // 2. Decidimos qué mostrar en el texto
        
        // Si maxUses es 0, significa que es una habilidad base (infinita)
        if (data.maxUses == 0)
        {
            // Ocultamos el contador (es más limpio que mostrar "infinito")
            targetText.gameObject.SetActive(false);
        }
        else // Si la habilidad tiene usos limitados
        {
            // Mostramos el contador y escribimos los usos restantes
            targetText.gameObject.SetActive(true);
            targetText.text = data.usesLeft.ToString();
        }
    }
}