using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems; // <--- ¡NUEVO! Necesitamos esto

public class EvolutionUI : MonoBehaviour
{
    public GameObject evolutionPanel;
    public EvolutionCardUI[] optionCards;

    private void Awake()
    {
        evolutionPanel.SetActive(false);
    }

    /// <summary>
    /// Método público que el EvolutionManager llamará.
    /// </summary>
    public void Show(List<AbilitySO> options, EvolutionManager manager)
    {
        evolutionPanel.SetActive(true);
        for (int i = 0; i < optionCards.Length; i++)
        {
            if (i < options.Count)
            {
                optionCards[i].gameObject.SetActive(true);
                optionCards[i].Setup(options[i], manager); 
            }
            else
            {
                optionCards[i].gameObject.SetActive(false);
            }
        }

        // --- ¡LA MAGIA DEL JOYSTICK EMPIEZA AQUÍ! ---

        // 1. Limpiamos cualquier selección anterior (por si acaso)
        EventSystem.current.SetSelectedGameObject(null); // <--- NUEVO

        // 2. Si hay opciones, seleccionamos el botón de la PRIMERA carta
        if (options.Count > 0)
        {
            // Le decimos al EventSystem: "¡Oye! El joystick ahora
            // está controlando este botón".
            EventSystem.current.SetSelectedGameObject(optionCards[0].selectButton.gameObject); // <--- NUEVO
        }
    }

    /// <summary>
    /// Método público que el EvolutionManager llamará.
    /// </summary>
    public void Hide()
    {
        // Buena práctica: cuando se oculta el panel,
        // le quitamos la selección a cualquier botón.
        EventSystem.current.SetSelectedGameObject(null); // <--- NUEVO
        
        evolutionPanel.SetActive(false);
    }
}