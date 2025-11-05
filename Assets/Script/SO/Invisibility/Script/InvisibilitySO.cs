using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 

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

    // --- ¡MÉTODO CORREGIDO! ---
    // 1. Cambiamos 'void' por 'bool'
    public override bool Execute(GameObject user)
    {
        // Esta lógica de "runner" es excelente para manejar
        // efectos que duran en el tiempo (Coroutines).
        InvisibilityRunner activeRunner = user.GetComponent<InvisibilityRunner>();
        
        // Si ya hay un runner (está invisible), paramos el efecto.
        if (activeRunner != null)
        {
            activeRunner.Stop();
        }
        else // Si no está invisible, iniciamos el efecto.
        {
            InvisibilityRunner runner = user.AddComponent<InvisibilityRunner>();
            runner.StartInvisibility(this);
        }

        // 2. Devolvemos 'true' porque la habilidad se ejecutó
        //    correctamente (ya sea para activar o desactivar).
        return true;
    }

    // --- Clase Ayudante Interna (Runner) ---
    // (Esta clase interna está bien y no necesita cambios)
    public class InvisibilityRunner : MonoBehaviour
    {
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