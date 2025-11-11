using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Health bossHealth;

    private bool gameEnded = false;
    // --- ¡NUEVA LÍNEA! ---
    // Este "interruptor" nos dirá cuándo empezar a mirar la vida del jefe.
    private bool bossFightHasStarted = false; 

    private void Update()
    {
        if (gameEnded) return;

        // Comprobamos la derrota del jugador en todo momento
        if (playerHealth.GetCurrentHealth() <= 0f)
        {
            gameEnded = true;
            HandleDefeat();
        }
        
        // --- ¡LÓGICA MODIFICADA! ---
        // ¡SOLO comprobamos la victoria si la pelea ha comenzado!
        if (bossFightHasStarted && bossHealth.GetCurrentHealth() <= 0f)
        {
            gameEnded = true;
            HandleVictory();
        }
        // --- FIN DE LÓGICA MODIFICADA ---
    }

    // --- ¡NUEVO MÉTODO! ---
    // Esta función será llamada por tu 'BossTrigger' para iniciar la pelea.
    public void StartBossFight()
    {
        Debug.Log("GameManager: ¡La pelea contra el jefe ha comenzado!");
        bossFightHasStarted = true;
    }
    // --- FIN DE MÉTODO NUEVO ---

    private void HandleDefeat()
    {
        Debug.Log("Derrota: el jugador ha muerto.");
        SceneManager.LoadScene(1);
    }

    private void HandleVictory()
    {
        Debug.Log("Victoria: el jefe ha sido derrotado.");
        SceneManager.LoadScene(2);
    }
}