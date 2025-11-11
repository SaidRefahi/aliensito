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

    // --- ¡NUEVAS LÍNEAS! ---
    [Header("Configuración de Color de Tipo")]
    [Tooltip("La imagen de fondo de la carta que cambiará de color.")]
    [SerializeField] private Image cardBackground; 
    
    [SerializeField] private Color meleeColor = new Color(1f, 0.8f, 0.8f); // Rojo claro
    [SerializeField] private Color rangedColor = new Color(0.8f, 0.9f, 1f); // Azul claro
    [SerializeField] private Color invisibilityColor = new Color(0.9f, 0.8f, 1f); // Morado claro
    [SerializeField] private Color defaultColor = Color.white; // Color por defecto
    // --- FIN DE LÍNEAS NUEVAS ---

    private AbilitySO currentAbility;
    private EvolutionManager evolutionManager;

    public void Setup(AbilitySO ability, EvolutionManager manager)
    {
        this.currentAbility = ability;
        this.evolutionManager = manager;

        // Asignación de datos (tu código original)
        abilityNameText.text = ability.abilityName;
        abilityDescriptionText.text = ability.description;
        abilityIcon.sprite = ability.icon;

        // Configuración del botón (tu código original)
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnCardSelected);
        
        // --- ¡NUEVA LÓGICA DE COLOR! ---
        SetCardColor(ability);
        // --- FIN DE LÓGICA NUEVA ---
    }

    /// <summary>
    /// Comprueba el tipo de AbilitySO y asigna el color correspondiente al fondo.
    /// </summary>
    private void SetCardColor(AbilitySO ability)
    {
        if (cardBackground == null)
        {
            Debug.LogWarning($"No hay 'cardBackground' asignado en {this.name}");
            return;
        }

        // Comprobamos el tipo de habilidad usando 'is'.
        // 'is' también funciona para clases que heredan (ej. ParpadeoSO 'is' InvisibilitySO)
        if (ability is MeleeAbilitySO)
        {
            cardBackground.color = meleeColor;
        }
        else if (ability is RangedAttackSO) // <-- ¡Asegúrate que este sea el nombre de tu clase base de Rango!
        {
            cardBackground.color = rangedColor;
        }
        else if (ability is InvisibilitySO)
        {
            cardBackground.color = invisibilityColor;
        }
        else
        {
            // Si es un tipo no reconocido, usamos el color por defecto
            cardBackground.color = defaultColor;
        }
    }

    private void OnCardSelected()
    {
        if (evolutionManager != null)
        {
            evolutionManager.SelectEvolution(currentAbility);
        }
    }
}