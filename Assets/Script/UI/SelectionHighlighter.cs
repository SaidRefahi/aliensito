using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.UI
{
    public class SelectionHighlighter : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [Title("Highlight Target")]
        [SerializeField] private GameObject highlightTarget;

        [Title("Feedback Animation")]
        [SerializeField] private bool punchOnSelect = true;
        [SerializeField] private float punchScaleAmount = 0.08f;
        [SerializeField] private float punchDuration = 0.18f;

        private void OnEnable()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != gameObject)
            {
                if (highlightTarget != null)
                {
                    highlightTarget.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            if (highlightTarget != null)
            {
                highlightTarget.transform.DOKill();
                highlightTarget.SetActive(false);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (highlightTarget == null) return;

            highlightTarget.SetActive(true);

            if (punchOnSelect)
            {
                highlightTarget.transform.DOKill();
                highlightTarget.transform.localScale = Vector3.one;
                highlightTarget.transform.DOPunchScale(new Vector3(punchScaleAmount, punchScaleAmount, 0f), punchDuration, 1, 0.5f)
                    .SetUpdate(true);
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (highlightTarget != null)
            {
                highlightTarget.transform.DOKill();
                highlightTarget.SetActive(false);
            }
        }
    }
}
