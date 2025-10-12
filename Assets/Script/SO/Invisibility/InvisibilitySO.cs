using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // Necesario para 'Action'

[CreateAssetMenu(fileName = "Invisibilidad Basica", menuName = "Habilidades/Invisibilidad/Invisibilidad Basica")]
public class InvisibilitySO : AbilitySO
{
    [Header("Invisibility Settings")]
    public float duration = 3f;

    [Header("Visual Settings")]
    [Range(0f, 1f)]
    public float invisibilityAlpha = 0.3f;
    public Color invisibilityColor = Color.cyan;

    [Header("Layer Settings")]
    public bool changeLayer = true;

    public override void Execute(GameObject user)
    {
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
    }

    // --- Clase Ayudante Interna (Runner) ---
    public class InvisibilityRunner : MonoBehaviour
    {
        // --- EVENTO AÑADIDO ---
        // Este evento notificará cuando la invisibilidad termine.
        public event Action OnInvisibilityEnd;

        private InvisibilitySO abilityData;
        private List<Renderer> renderers = new List<Renderer>();
        private List<Color> originalColors = new List<Color>();
        private int originalLayer;
        private int invisibilityLayer;
        private Coroutine invisibilityCoroutine;

        public void StartInvisibility(InvisibilitySO data)
        {
            this.abilityData = data;
            invisibilityLayer = LayerMask.NameToLayer("Invisibility");

            GetComponentsInChildren<Renderer>(renderers);
            originalColors.Clear();
            foreach (var r in renderers)
            {
                if(r.material.HasProperty("_Color"))
                {
                    originalColors.Add(r.material.color);
                }
            }

            SetVisuals(true);

            if (abilityData.changeLayer)
            {
                originalLayer = gameObject.layer;
                gameObject.layer = invisibilityLayer;
            }
            
            invisibilityCoroutine = StartCoroutine(InvisibilityTimer());
        }

        public void Stop()
        {
            if (invisibilityCoroutine != null) StopCoroutine(invisibilityCoroutine);
            
            SetVisuals(false);
            if (abilityData.changeLayer) gameObject.layer = originalLayer;
            
            // --- INVOCAMOS EL EVENTO ---
            // Justo antes de destruir el componente, avisamos que terminó.
            OnInvisibilityEnd?.Invoke();

            Destroy(this);
        }

        private void SetVisuals(bool isInvisible)
        {
            Color targetColor = abilityData.invisibilityColor;
            targetColor.a = abilityData.invisibilityAlpha;

            for(int i = 0; i < renderers.Count; i++)
            {
                if(renderers[i].material.HasProperty("_Color"))
                {
                    renderers[i].material.color = isInvisible ? targetColor : originalColors[i];
                }
            }
        }

        private IEnumerator InvisibilityTimer()
        {
            yield return new WaitForSeconds(abilityData.duration);
            Stop();
        }
    }
}