using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using VContainer;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.UI
{
    public class EvolutionUI : MonoBehaviour
    {
        [Title("UI Elements")]
        [SerializeField] private GameObject evolutionPanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private EvolutionCardUI[] optionCards;

        [Title("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float punchScaleAmount = 0.05f;

        [Inject] private IEvolutionService evolutionService;

        private Tween activeTween;

        private void Awake()
        {
            if (panelCanvasGroup == null && evolutionPanel != null)
            {
                panelCanvasGroup = evolutionPanel.GetComponent<CanvasGroup>();
            }

            if (evolutionPanel != null)
            {
                evolutionPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (evolutionService == null)
            {
                evolutionService = FindFirstObjectByType<MonoBehaviour>() is IEvolutionService foundService 
                    ? foundService 
                    : FindFirstObjectByType<GeneticalDrift.Gameplay.EvolutionManager>();
            }

            if (evolutionService != null)
            {
                SubscribeToService();
            }
        }

        private void OnEnable()
        {
            if (evolutionService != null)
            {
                SubscribeToService();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromService();
            KillActiveTween();
        }

        private void OnDestroy()
        {
            UnsubscribeFromService();
            KillActiveTween();
        }

        private void SubscribeToService()
        {
            if (evolutionService == null) return;
            evolutionService.OnEvolutionChoicesAvailable -= HandleEvolutionChoices;
            evolutionService.OnEvolutionChoicesAvailable += HandleEvolutionChoices;
            evolutionService.OnEvolutionCompleted -= HandleEvolutionCompleted;
            evolutionService.OnEvolutionCompleted += HandleEvolutionCompleted;
        }

        private void UnsubscribeFromService()
        {
            if (evolutionService == null) return;
            evolutionService.OnEvolutionChoicesAvailable -= HandleEvolutionChoices;
            evolutionService.OnEvolutionCompleted -= HandleEvolutionCompleted;
        }

        private void HandleEvolutionChoices(IReadOnlyList<AbilitySO> options)
        {
            Show(options, evolutionService);
        }

        private void HandleEvolutionCompleted()
        {
            Hide();
        }

        public void Show(IReadOnlyList<AbilitySO> options, IEvolutionService service)
        {
            if (evolutionPanel == null || options == null) return;

            KillActiveTween();
            evolutionPanel.SetActive(true);

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                activeTween = panelCanvasGroup.DOFade(1f, fadeDuration)
                    .SetUpdate(true);
            }

            evolutionPanel.transform.localScale = Vector3.one * (1f - punchScaleAmount);
            evolutionPanel.transform.DOScale(Vector3.one, fadeDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            for (int i = 0; i < optionCards.Length; i++)
            {
                if (optionCards[i] == null) continue;

                if (i < options.Count)
                {
                    optionCards[i].gameObject.SetActive(true);
                    optionCards[i].Setup(options[i], service);
                }
                else
                {
                    optionCards[i].gameObject.SetActive(false);
                }
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                if (options.Count > 0 && optionCards.Length > 0 && optionCards[0] != null && optionCards[0].SelectButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(optionCards[0].SelectButton.gameObject);
                }
            }
        }

        public void Hide()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            if (panelCanvasGroup != null)
            {
                KillActiveTween();
                activeTween = panelCanvasGroup.DOFade(0f, fadeDuration)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        if (evolutionPanel != null)
                        {
                            evolutionPanel.SetActive(false);
                        }
                    });
            }
            else if (evolutionPanel != null)
            {
                evolutionPanel.SetActive(false);
            }
        }

        private void KillActiveTween()
        {
            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill();
                activeTween = null;
            }
        }
    }
}
