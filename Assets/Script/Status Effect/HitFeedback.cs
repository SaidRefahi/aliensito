using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class HitFeedback : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    [Tooltip("El color al que flashea el enemigo al ser golpeado.")]
    [SerializeField] private Color hitColor = Color.red;
    [Tooltip("Duración total del flash y el estiramiento (en segundos).")]
    [SerializeField] private float effectDuration = 0.15f;
    [Tooltip("Cuánto se estira verticalmente (ej. 1.3 = 130%).")]
    [SerializeField] private float stretchAmount = 1.3f;
    [Tooltip("Cuánto se contrae horizontalmente (ej. 0.8 = 80%).")]
    [SerializeField] private float squashAmount = 0.8f;

    private Renderer enemyRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private Coroutine activeHitRoutine;

    private void Awake()
    {
        // Guardamos los valores originales al inicio
        enemyRenderer = GetComponent<Renderer>();
        originalColor = enemyRenderer.material.color;
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Método público que llama el script de Health para iniciar el efecto.
    /// </summary>
    public void PlayEffect()
    {
        // Si ya hay un efecto corriendo (por golpes muy rápidos),
        // lo detenemos y reseteamos antes de iniciar el nuevo.
        if (activeHitRoutine != null)
        {
            StopCoroutine(activeHitRoutine);
            ResetVisuals();
        }
        activeHitRoutine = StartCoroutine(HitEffectRoutine());
    }

    private IEnumerator HitEffectRoutine()
    {
        // 1. Aplicar el efecto (Estirar y colorear)
        enemyRenderer.material.color = hitColor;
        transform.localScale = new Vector3(originalScale.x * squashAmount, 
                                           originalScale.y * stretchAmount, 
                                           originalScale.z * squashAmount);

        // 2. Esperar una fracción de segundo
        // Usamos WaitForSecondsRealtime para que el efecto funcione
        // incluso si el juego está pausado (Time.timeScale = 0).
        yield return new WaitForSecondsRealtime(effectDuration);

        // 3. Revertir a la normalidad
        ResetVisuals();
        activeHitRoutine = null;
    }

    /// <summary>
    /// Restaura el objeto a su estado visual original.
    /// </summary>
    private void ResetVisuals()
    {
        enemyRenderer.material.color = originalColor;
        transform.localScale = originalScale;
    }
}