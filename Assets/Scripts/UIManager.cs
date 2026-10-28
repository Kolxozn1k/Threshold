using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;

    private float maxHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }
        if(playerHealth != null)
        {
            maxHealth = playerHealth.currentHealth;
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // прячет панель Game Over при старте игры
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerHealth == null) return;

        UpdateHealthBar();

        if(playerHealth.isDead && gameOverPanel != null && !gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(true); // показывает панель Game Over, если игрок мертв
        }

    }

    private void UpdateHealthBar()
    {
        if(healthBarFill != null && maxHealth >0)
        {
            float fillAmount = playerHealth.currentHealth / maxHealth;
            healthBarFill.fillAmount = Mathf.Clamp01(fillAmount); // Обновляет заполнение полосы здоровья
        }
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex); // Перезагружает текущую сцену
    }
}
