using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using VContainer;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class GeneticMaterial : MonoBehaviour
    {
        [Tooltip("Amount of genetic material provided.")]
        [SerializeField] private int amount = 10;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private AudioCueSO pickupSound;

        private IEvolutionService evolutionService;
        private IAudioService audioService;

        [Inject]
        public void Construct(IEvolutionService evolutionService, IAudioService audioService)
        {
            this.evolutionService = evolutionService;
            this.audioService = audioService;
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((playerLayer.value & (1 << other.gameObject.layer)) > 0)
            {
                if (evolutionService != null)
                {
                    evolutionService.AddGeneticMaterial(amount);
                }
                else if (EvolutionManager.Instance != null)
                {
                    EvolutionManager.Instance.AddGeneticMaterial(amount);
                }

                if (pickupSound != null)
                {
                    if (audioService != null) audioService.Play(pickupSound, transform.position);
                    else if (AudioManager.Instance != null) AudioManager.Instance.Play(pickupSound, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}
