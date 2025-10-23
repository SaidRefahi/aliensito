using UnityEngine;
using System.Collections.Generic;

public class EvolutionUI : MonoBehaviour
{
    public GameObject evolutionPanel;
    public EvolutionCardUI[] optionCards;
    private EvolutionManager evolutionManager;

    private void Start()
    {
        evolutionManager = FindObjectOfType<EvolutionManager>();
        if (evolutionManager == null)
        {
            Debug.LogError("No se encontró un EvolutionManager en la escena.");
            this.enabled = false;
            return;
        }

        evolutionPanel.SetActive(false);
        EvolutionManager.OnEvolutionOptionsReady += ShowOptions;
    }

    private void OnDestroy()
    {
        EvolutionManager.OnEvolutionOptionsReady -= ShowOptions;
    }

    private void ShowOptions(List<AbilitySO> options)
    {
        // La UI ahora es responsable de pausar el juego al mostrarse.
        Time.timeScale = 0f;
        
        evolutionPanel.SetActive(true);
        for (int i = 0; i < optionCards.Length; i++)
        {
            if (i < options.Count)
            {
                optionCards[i].gameObject.SetActive(true);
                optionCards[i].Setup(options[i], evolutionManager);
            }
            else
            {
                optionCards[i].gameObject.SetActive(false);
            }
        }
    }
}