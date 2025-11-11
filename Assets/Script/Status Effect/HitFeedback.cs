using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class HitFeedback : MonoBehaviour
{
    [Header("Efecto Squash & Stretch")]
    [Tooltip("El color al que flashea el enemigo al ser golpeado.")]
    [SerializeField] private Color hitColor = Color.red;
    [Tooltip("Duración total del flash y el estiramiento (en segundos).")]
    [SerializeField] private float effectDuration = 0.15f;
    [Tooltip("Cuánto se estira verticalmente (ej. 1.3 = 130%).")]
    [SerializeField] private float stretchAmount = 1.3f;
    [Tooltip("Cuánto se contrae horizontalmente (ej. 0.8 = 80%).")]
    [SerializeField] private float squashAmount = 0.8f;

    [Header("Efecto de Partículas (Pooling)")]
    [Tooltip("El 'Tag' (Etiqueta) de la partícula en el PoolManager. " +
             "Debe coincidir con el PoolManager y el script ReturnToPool.")]
    [SerializeField] private string particlePoolTag = "HitEffect"; 

    private Renderer enemyRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private Coroutine activeHitRoutine;

    private void Awake()
    {
        enemyRenderer = GetComponent<Renderer>();
        originalColor = enemyRenderer.material.color;
        originalScale = transform.localScale;
    }

    public void PlayEffect()
    {
        // Detener corrutina anterior si existe
        if (activeHitRoutine != null)
        {
            StopCoroutine(activeHitRoutine);
            ResetVisuals();
        }

        // --- ¡¡LÓGICA DE POOLING CORREGIDA!! ---
        if (PoolManager.Instance != null && !string.IsNullOrEmpty(particlePoolTag))
        {
            // ¡Llamada corregida! Usamos "SpawnFromPool" 
            // como se llama en tu script
            PoolManager.Instance.SpawnFromPool(particlePoolTag, transform.position, Quaternion.identity);
        }
        else
        {
            if (PoolManager.Instance == null)
                Debug.LogWarning("PoolManager.Instance no encontrado en la escena.");
        }
        // -----------------------------------------

        // Iniciar la corrutina de squash/stretch
        activeHitRoutine = StartCoroutine(HitEffectRoutine());
    }

    private IEnumerator HitEffectRoutine()
    {
        // 1. Aplicar el efecto
        enemyRenderer.material.color = hitColor;
        transform.localScale = new Vector3(originalScale.x * squashAmount, 
                                           originalScale.y * stretchAmount, 
                                           originalScale.z * squashAmount);

        // 2. Esperar
        yield return new WaitForSecondsRealtime(effectDuration);

        // 3. Revertir
        ResetVisuals();
        activeHitRoutine = null;
    }

    private void ResetVisuals()
    {
        enemyRenderer.material.color = originalColor;
        transform.localScale = originalScale;
    }
}