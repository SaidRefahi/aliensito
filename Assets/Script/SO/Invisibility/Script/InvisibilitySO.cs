using UnityEngine;
using System.Collections;
using System.Collections.Generic; // ¡Necesario para el Diccionario!
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

    // --- ¡NUEVAS LÍNEAS! ---
    // (Añadimos el tracker de cooldown)
    private readonly Dictionary<GameObject, float> lastUseTime = new Dictionary<GameObject, float>();
    // --- FIN DE LÍNEAS NUEVAS ---

    public override bool Execute(GameObject user)
    {
        // --- ¡¡LÓGICA COMPLETAMENTE MODIFICADA!! ---

        // 1. Comprobar si está en cooldown
        if (!CanUse(user))
        {
            return false; // Fallar silenciosamente
        }
        
        // 2. Comprobar si ya está invisible
        InvisibilityRunner activeRunner = user.GetComponent<InvisibilityRunner>();
        if (activeRunner != null)
        {
            // ¡Ya está activo! No hacer nada. Fallar.
            return false;
        }

        // 3. ¡Ok, no está en CD y no está activo! ¡ACTIVAR!
        InvisibilityRunner runner = user.AddComponent<InvisibilityRunner>();
        runner.StartInvisibility(this);

        // 4. Registrar el uso para el cooldown
        lastUseTime[user] = Time.time;
        return true;
        // --- FIN DE LÓGICA MODIFICADA ---
    }
    
    // --- ¡NUEVO MÉTODO! ---
    private bool CanUse(GameObject user)
    {
        if (!lastUseTime.TryGetValue(user, out float last))
        {
            return true; // Nunca se ha usado
        }
        
        // Comprueba si el tiempo actual ha superado el último uso + cooldown
        // (El 'cooldown' es la variable de la clase base 'AbilitySO')
        return Time.time >= last + cooldown;
    }
    // --- FIN DE MÉTODO NUEVO ---

    // --- Clase Ayudante Interna (Runner) ---
    // (Esta clase interna no necesita cambios)
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