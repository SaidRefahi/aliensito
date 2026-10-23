using UnityEngine;
using TMPro;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using SaludosCamaradaHumano.GeneticalDrift.Gameplay;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.UI
{
    public class AbilityDisplayUI : MonoBehaviour
    {
        [Title("Dependencies")]
        [SerializeField] private PlayerAbilityHandler playerAbilityHandler;

        [Title("UI Text Counters")]
        [SerializeField] private TextMeshProUGUI meleeUsesText;
        [SerializeField] private TextMeshProUGUI rangedUsesText;
        [SerializeField] private TextMeshProUGUI invisibilityUsesText;

        private void Awake()
        {
            if (playerAbilityHandler == null)
            {
                playerAbilityHandler = FindFirstObjectByType<PlayerAbilityHandler>();
            }
        }

        private void OnEnable()
        {
            if (playerAbilityHandler != null)
            {
                playerAbilityHandler.OnAbilityUsesChanged -= HandleAbilityUsesChanged;
                playerAbilityHandler.OnAbilityUsesChanged += HandleAbilityUsesChanged;
            }
        }

        private void OnDisable()
        {
            if (playerAbilityHandler != null)
            {
                playerAbilityHandler.OnAbilityUsesChanged -= HandleAbilityUsesChanged;
            }
        }

        private void HandleAbilityUsesChanged(AbilityUseData data)
        {
            TextMeshProUGUI targetText = null;

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

            if (targetText == null) return;

            if (data.maxUses == 0)
            {
                targetText.gameObject.SetActive(false);
            }
            else
            {
                targetText.gameObject.SetActive(true);
                targetText.SetText("{0}", data.usesLeft);
            }
        }
    }
}
