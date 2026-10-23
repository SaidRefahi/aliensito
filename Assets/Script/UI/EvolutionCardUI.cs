using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using SaludosCamaradaHumano.GeneticalDrift.Gameplay;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.UI
{
    public class EvolutionCardUI : MonoBehaviour
    {
        [Title("Card References")]
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityDescriptionText;
        [SerializeField] private Image abilityIcon;
        [SerializeField] private Button selectButton;

        [Title("Card Type Colors")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Color meleeColor = new Color(1f, 0.8f, 0.8f);
        [SerializeField] private Color rangedColor = new Color(0.8f, 0.9f, 1f);
        [SerializeField] private Color invisibilityColor = new Color(0.9f, 0.8f, 1f);
        [SerializeField] private Color defaultColor = Color.white;

        [Title("Card Juice")]
        [SerializeField] private float punchScale = 0.08f;
        [SerializeField] private float punchDuration = 0.2f;

        private AbilitySO currentAbility;
        private IEvolutionService evolutionService;

        public Button SelectButton => selectButton;

        private void OnEnable()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(HandleCardSelected);
                selectButton.onClick.AddListener(HandleCardSelected);
            }
        }

        private void OnDisable()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(HandleCardSelected);
            }
        }

        public void Setup(AbilitySO ability, IEvolutionService service)
        {
            currentAbility = ability;
            evolutionService = service;

            if (ability == null) return;

            if (abilityNameText != null)
            {
                abilityNameText.text = ability.AbilityName;
            }

            if (abilityDescriptionText != null)
            {
                abilityDescriptionText.text = ability.Description;
            }

            if (abilityIcon != null)
            {
                abilityIcon.sprite = ability.Icon;
            }

            SetCardColor(ability);

            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 1, 0.5f)
                .SetUpdate(true);
        }

        public void Setup(AbilitySO ability, EvolutionManager manager)
        {
            Setup(ability, (IEvolutionService)manager);
        }

        private void SetCardColor(AbilitySO ability)
        {
            if (cardBackground == null) return;

            if (ability is MeleeAbilitySO)
            {
                cardBackground.color = meleeColor;
            }
            else if (ability is RangedAttackSO)
            {
                cardBackground.color = rangedColor;
            }
            else if (ability is InvisibilitySO)
            {
                cardBackground.color = invisibilityColor;
            }
            else
            {
                cardBackground.color = defaultColor;
            }
        }

        private void HandleCardSelected()
        {
            if (evolutionService != null && currentAbility != null)
            {
                evolutionService.SelectEvolution(currentAbility);
            }
        }
    }
}
