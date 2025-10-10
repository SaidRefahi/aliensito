using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Invisibility Ability", menuName = "Abilities/Invisibility")]
public class InvisibilitySO : AbilitySO
{
    [Header("Invisibility Settings")]
    public float duration = 3f;

    [Header("Visual Settings")]
    [Range(0f, 1f)]
    public float invisibilityAlpha = 0.3f;
    public Color invisibilityColor = Color.cyan;

    [Header("Layer Settings")]
    [Tooltip("Activa esto si quieres que el jugador cambie a la capa 'Invisibility'")]
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

    // --- Clase Ayudante Interna y Optimizada ---
    public class InvisibilityRunner : MonoBehaviour
    {
        private InvisibilitySO abilityData;
        
        // Listas para guardar los componentes y sus estados originales
        private List<Renderer> renderers = new List<Renderer>();
        private List<Color> originalColors = new List<Color>();
        private int originalLayer;
        private int invisibilityLayer;

        // El Coroutine para el temporizador
        private Coroutine invisibilityCoroutine;

        public void StartInvisibility(InvisibilitySO data)
        {
            this.abilityData = data;
            invisibilityLayer = LayerMask.NameToLayer("Invisibility");

            // --- OPTIMIZACIÓN ---
            // Guardamos solo los Renderers, que son los que cambian de color.
            GetComponentsInChildren<Renderer>(renderers);
            originalColors.Clear();
            foreach (var r in renderers)
            {
                // Solo guardamos el color del material principal
                if(r.material.HasProperty("_Color"))
                {
                    originalColors.Add(r.material.color);
                }
            }

            // Aplicamos el efecto visual
            SetVisuals(true);

            // Cambiamos la capa si está activado
            if (abilityData.changeLayer)
            {
                originalLayer = gameObject.layer;
                gameObject.layer = invisibilityLayer;
            }
            
            invisibilityCoroutine = StartCoroutine(InvisibilityTimer());
        }

        public void Stop()
        {
            if (invisibilityCoroutine != null)
            {
                StopCoroutine(invisibilityCoroutine);
            }
            
            // Restauramos todo al estado original
            SetVisuals(false);
            if (abilityData.changeLayer)
            {
                gameObject.layer = originalLayer;
            }
            
            Destroy(this); // El ayudante se autodestruye
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