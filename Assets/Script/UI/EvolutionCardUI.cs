using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvolutionCardUI : MonoBehaviour
{
    [Header("Referencias de la Carta")]
    public TextMeshProUGUI abilityNameText;
    public TextMeshProUGUI abilityDescriptionText;
    public Image abilityIcon;
    public Button selectButton;

    private AbilitySO currentAbility;
    private EvolutionManager evolutionManager;

    public void Setup(AbilitySO ability, EvolutionManager manager)
    {
        this.currentAbility = ability;
        this.evolutionManager = manager;

        abilityNameText.text = ability.abilityName;
        abilityDescriptionText.text = ability.description;
        abilityIcon.sprite = ability.icon;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnCardSelected);
    }

    private void OnCardSelected()
    {
        // 1. Llama al manager para que aplique la lógica de la evolución.
        if (evolutionManager != null)
        {
            evolutionManager.SelectEvolution(currentAbility);
        }
        
        // 2. La carta SIEMPRE reanuda el juego y oculta el panel, pase lo que pase.
        Time.timeScale = 1f;
        GetComponentInParent<EvolutionUI>().evolutionPanel.SetActive(false);
    }
}