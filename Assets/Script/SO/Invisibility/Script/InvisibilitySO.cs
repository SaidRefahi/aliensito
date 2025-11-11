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
    // --- LÍNEAS ANTIGUAS (YA NO SE USAN, PERO SE PUEDEN DEJAR) ---
    [Range(0f, 1f)]
    public float invisibilityAlpha = 0.3f;
    public Color invisibilityColor = Color.cyan;
    // --- FIN LÍNEAS ANTIGUAS ---

    // --- ¡NUEVO CAMPO AÑADIDO! ---
    [Tooltip("Arrastra aquí el material transparente (con modo 'Fade' o 'Transparent') que se usará para la invisibilidad.")]
    public Material transparentMaterial;
    // --- FIN NUEVO CAMPO ---

    [Header("Layer Settings")]
    public bool changeLayer = true;

    // --- ¡TU LÓGICA DE EXECUTE (TOGGLE) SE QUEDA IGUAL! ---
    // (¡No he cambiado nada aquí, solo he añadido una comprobación
    // de que el 'transparentMaterial' no esté vacío!)
    public override bool Execute(GameObject user)
    {
        // Comprobación de seguridad
        if (transparentMaterial == null)
        {
            Debug.LogError($"¡InvisibilitySO '{this.name}' no tiene un 'Transparent Material' asignado!");
            return false; // No se puede ejecutar si falta el material
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

    // --- CLASE INTERNA InvisibilityRunner (¡MODIFICADA!) ---
    public class InvisibilityRunner : MonoBehaviour
    {
        public event Action OnInvisibilityEnd;

        private InvisibilitySO abilityData;
        private List<Renderer> renderers = new List<Renderer>();

        // --- ¡MODIFICADO! Almacenamos los materiales originales ---
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        
        private int originalLayer;
        private int invisibilityLayer;
        private Coroutine invisibilityCoroutine;

        public void StartInvisibility(InvisibilitySO data)
        {
            this.abilityData = data;
            invisibilityLayer = LayerMask.NameToLayer("Invisibility");

            // Obtenemos todos los renderers (incluyendo hijos)
            GetComponentsInChildren<Renderer>(true, renderers);
            originalMaterials.Clear();

            // --- ¡MODIFICADO! Guardamos los materiales originales ---
            foreach (var r in renderers)
            {
                // Guardamos una copia de los materiales actuales
                originalMaterials.Add(r, r.sharedMaterials); 
            }

            SetVisuals(true); // Aplicamos el material transparente

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
            
            SetVisuals(false); // Restauramos los materiales originales
            if (abilityData.changeLayer) gameObject.layer = originalLayer;
            
            OnInvisibilityEnd?.Invoke();

            Destroy(this);
        }

        // --- ¡MÉTODO SetVisuals COMPLETAMENTE MODIFICADO! ---
        private void SetVisuals(bool isInvisible)
        {
            if (isInvisible)
            {
                // APLICAR MATERIAL TRANSPARENTE
                foreach (var r in renderers)
                {
                    // Creamos un array de materiales transparente del tamaño
                    // correcto para este renderer (por si usa múltiples materiales)
                    Material[] newMaterials = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < newMaterials.Length; i++)
                    {
                        // Asignamos nuestro material transparente a todos los slots
                        newMaterials[i] = abilityData.transparentMaterial; 
                    }
                    r.materials = newMaterials; // Asignamos el array
                }
            }
            else
            {
                // RESTAURAR MATERIALES ORIGINALES
                foreach (var r in renderers)
                {
                    // Buscamos los materiales que guardamos para este renderer
                    if (originalMaterials.TryGetValue(r, out Material[] mats))
                    {
                        r.materials = mats; // Los restauramos
                    }
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