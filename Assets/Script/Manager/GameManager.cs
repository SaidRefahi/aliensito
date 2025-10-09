using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Health bossHealth;

    private bool gameEnded = false;

    private void Start()
    {
        // Suscribirse a eventos si los usás, o revisar en Update
    }

    private void Update()
    {
        if (gameEnded) return;

        if (playerHealth.GetCurrentHealth() <= 0f)
        {
            gameEnded = true;
            HandleDefeat();
        }
        else if (bossHealth.GetCurrentHealth() <= 0f)
        {
            gameEnded = true;
            HandleVictory();
        }
    }

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