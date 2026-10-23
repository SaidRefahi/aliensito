using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [CreateAssetMenu(fileName = "Invisibilidad Basica", menuName = "Habilidades/Invisibilidad/Invisibilidad Basica")]
    public class InvisibilitySO : AbilitySO
    {
        [Title("Invisibility Settings")]
        [SerializeField] private float duration = 3f;

        [Title("Visual Settings")]
        [Tooltip("Material used for transparency during invisibility.")]
        [SerializeField] private Material transparentMaterial;

        [Title("Layer Settings")]
        [SerializeField] private bool changeLayer = true;

        public float Duration => duration;
        public Material TransparentMaterial => transparentMaterial;
        public bool ChangeLayer => changeLayer;

        public override bool Execute(GameObject user)
        {
            if (transparentMaterial == null)
            {
                Debug.LogError($"[InvisibilitySO] '{name}' has no transparentMaterial assigned!", this);
                return false;
            }

            InvisibilityRunner activeRunner = user.GetComponent<InvisibilityRunner>();
            if (activeRunner != null)
            {
                activeRunner.Stop();
            }
            else
            {
                InvisibilityRunner runner = user.AddComponent<InvisibilityRunner>();
                runner.StartInvisibility(this);
            }

            return true;
        }

        public class InvisibilityRunner : MonoBehaviour
        {
            public event Action OnInvisibilityEnd;

            private InvisibilitySO abilityData;
            private readonly List<Renderer> renderers = new List<Renderer>();
            private readonly Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

            private int originalLayer;
            private int invisibilityLayer;
            private CancellationTokenSource timerCts;

            public void StartInvisibility(InvisibilitySO data)
            {
                abilityData = data;
                invisibilityLayer = LayerMask.NameToLayer("Invisibility");

                GetComponentsInChildren<Renderer>(true, renderers);
                originalMaterials.Clear();

                for (int i = 0; i < renderers.Count; i++)
                {
                    var r = renderers[i];
                    if (r != null)
                    {
                        originalMaterials[r] = r.sharedMaterials;
                    }
                }

                SetVisuals(true);

                if (abilityData.ChangeLayer)
                {
                    originalLayer = gameObject.layer;
                    gameObject.layer = invisibilityLayer;
                }

                timerCts?.Cancel();
                timerCts?.Dispose();
                timerCts = new CancellationTokenSource();

                InvisibilityTimerAsync(timerCts.Token).Forget();
            }

            public void Stop()
            {
                if (timerCts != null)
                {
                    timerCts.Cancel();
                    timerCts.Dispose();
                    timerCts = null;
                }

                SetVisuals(false);

                if (abilityData != null && abilityData.ChangeLayer)
                {
                    gameObject.layer = originalLayer;
                }

                OnInvisibilityEnd?.Invoke();
                Destroy(this);
            }

            private void OnDestroy()
            {
                if (timerCts != null)
                {
                    timerCts.Cancel();
                    timerCts.Dispose();
                    timerCts = null;
                }
            }

            private void SetVisuals(bool isInvisible)
            {
                if (isInvisible)
                {
                    for (int i = 0; i < renderers.Count; i++)
                    {
                        var r = renderers[i];
                        if (r == null) continue;

                        Material[] newMaterials = new Material[r.sharedMaterials.Length];
                        for (int j = 0; j < newMaterials.Length; j++)
                        {
                            newMaterials[j] = abilityData.TransparentMaterial;
                        }
                        r.materials = newMaterials;
                    }
                }
                else
                {
                    for (int i = 0; i < renderers.Count; i++)
                    {
                        var r = renderers[i];
                        if (r != null && originalMaterials.TryGetValue(r, out Material[] mats))
                        {
                            r.materials = mats;
                        }
                    }
                }
            }

            private async UniTaskVoid InvisibilityTimerAsync(CancellationToken ct)
            {
                bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(abilityData.Duration), cancellationToken: ct).SuppressCancellationThrow();
                if (!canceled)
                {
                    Stop();
                }
            }
        }
    }
}
