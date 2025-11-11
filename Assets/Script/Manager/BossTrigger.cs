using UnityEngine;
using System.Collections.Generic;

public class BossTrigger : MonoBehaviour
{
    [Header("Objetos a Activar")]
    public List<GameObject> objectsToActivate;

    [Header("Configuración del Trigger")]
    public LayerMask activationLayer;

    // --- ¡NUEVA LÍNEA! ---
    [Header("Referencias del Manager")]
    [Tooltip("Arrastra aquí el objeto de la escena que tiene el script 'GameManager'.")]
    [SerializeField] private GameManager gameManager;
    // --- FIN DE LÍNEA NUEVA ---

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return;
        
        bool isActivationLayer = (activationLayer.value & (1 << other.gameObject.layer)) != 0;
        if (!isActivationLayer) return; 

        hasBeenTriggered = true;

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        // --- ¡NUEVA LÍNEA! ---
        // ¡Avisamos al GameManager que la pelea ha comenzado!
        if (gameManager != null)
        {
            gameManager.StartBossFight();
        }
        else
        {
            Debug.LogError("¡BossTrigger no tiene una referencia al GameManager!");
        }
        // --- FIN DE LÍNEA NUEVA ---

        GetComponent<Collider>().enabled = false;
    }

    // (El código de OnDrawGizmos se queda igual...)
}