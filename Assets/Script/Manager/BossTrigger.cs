using System.Collections.Generic;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;
using VContainer;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class BossTrigger : MonoBehaviour
    {
        [Title("Trigger Configuration")]
        [SerializeField] private LayerMask activationLayer;
        [SerializeField] private List<GameObject> objectsToActivate = new List<GameObject>();

        [Title("Fallback References")]
        [SerializeField] private GameManager gameManager;

        private IGameStateService gameStateService;
        private bool hasBeenTriggered = false;
        private Collider triggerCollider;

        [Inject]
        public void Construct(IGameStateService gameStateService)
        {
            this.gameStateService = gameStateService;
        }

        private void Awake()
        {
            triggerCollider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasBeenTriggered) return;

            bool isActivationLayer = (activationLayer.value & (1 << other.gameObject.layer)) != 0;
            if (!isActivationLayer) return;

            hasBeenTriggered = true;

            for (int i = 0; i < objectsToActivate.Count; i++)
            {
                if (objectsToActivate[i] != null)
                {
                    objectsToActivate[i].SetActive(true);
                }
            }

            if (gameStateService != null)
            {
                gameStateService.StartBossFight();
            }
            else if (gameManager != null)
            {
                gameManager.StartBossFight();
            }

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
        }
    }
}
