// Ruta: Assets/Scripts/EvolutionUI.cs
// ACCIÓN: Reemplaza tu script existente con esta versión.

using UnityEngine;
using System.Collections.Generic;

public class EvolutionUI : MonoBehaviour
{
    public GameObject evolutionPanel;
    public EvolutionCardUI[] optionCards;

    // Ya no necesita una referencia al manager ni suscribirse a eventos.
    // Borramos Start(), OnDestroy() y la variable evolutionManager.

    private void Awake()
    {
        // Asegúrate de que está oculto al empezar
        evolutionPanel.SetActive(false);
    }

    /// <summary>
    /// Método público que el EvolutionManager llamará.
    /// </summary>
    public void Show(List<AbilitySO> options, EvolutionManager manager)
    {
        // El Manager ya ha pausado el juego.
        // Esta clase solo se encarga de la parte VISUAL.
        
        evolutionPanel.SetActive(true);
        for (int i = 0; i < optionCards.Length; i++)
        {
            if (i < options.Count)
            {
                optionCards[i].gameObject.SetActive(true);
                // Pasa la habilidad Y la referencia al manager
                // para que el botón sepa a quién llamar
                optionCards[i].Setup(options[i], manager); 
            }
            else
            {
                optionCards[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Método público que el EvolutionManager llamará.
    /// </summary>
    public void Hide()
    {
        // El Manager se encargará de despausar.
        evolutionPanel.SetActive(false);
    }
}