using UnityEngine;
using UnityEngine.EventSystems; // ¡La clave!

/// <summary>
/// Un componente genérico y reutilizable (KISS) que activa un objeto
/// cuando el EventSystem selecciona este elemento, y lo desactiva
/// cuando el EventSystem lo deselecciona.
///
/// REQUIERE: Estar en el mismo GameObject que un componente "Selectable"
/// (como un Button, Toggle, o Slider).
/// </summary>
public class SelectionHighlighter : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Tooltip("El objeto (borde, brillo, etc.) que se activará al seleccionar.")]
    [SerializeField] private GameObject highlightTarget;

    private void OnEnable()
    {
        // Asegurarnos de que esté apagado si el botón se activa
        // y no está seleccionado.
        if (EventSystem.current.currentSelectedGameObject != this.gameObject)
        {
            if (highlightTarget != null)
            {
                highlightTarget.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        // Asegurarnos de que se apague si el botón se desactiva
        if (highlightTarget != null)
        {
            highlightTarget.SetActive(false);
        }
    }

    /// <summary>
    /// ¡Se llama automáticamente por el EventSystem cuando
    /// el jugador navega (con joystick) a este elemento!
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        if (highlightTarget != null)
        {
            highlightTarget.SetActive(true);
        }
    }

    /// <summary>
    /// ¡Se llama automáticamente por el EventSystem cuando
    /// el jugador navega (con joystick) FUERA de este elemento!
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        if (highlightTarget != null)
        {
            highlightTarget.SetActive(false);
        }
    }
}