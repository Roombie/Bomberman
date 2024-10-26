using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState
{
    Countdown,
    Playing,
    Paused,
    GameOver,
    Win
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameState currentState;

    [Header("Game Settings")]
    public int playerLives = 3;
    public float respawnDelay = 5f;            // Delay before respawning the player
    public float countdownTime = 3f;           // Pre-game countdown time (3 seconds)
    public float gameDuration = 60f;           // Total game duration in seconds
    public Transform respawnPosition;

    [Header("UI Elements")]
    public TMP_Text timerText;                 // Countdown timer display using TextMeshPro
    public TMP_Text livesText;                 // Lives display using TextMeshPro
    public TMP_Text countdownText;             // Countdown text before game starts
    public GameObject countdownImage;          // Image displayed during the countdown
    public GameObject winImage;                // Image displayed during win state

    private GameObject currentPlayer;
    private bool gamePaused = false;
    private float remainingTime;                // Remaining time for the countdown

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep the game manager persistent
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentPlayer = GameObject.FindWithTag("Player");
        currentPlayer.transform.position = respawnPosition.position;
        StartCoroutine(StartGameWithCountdown());
    }

    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            UpdateGameTimer();
        }
    }

    #region Game Logic
    private IEnumerator StartGameWithCountdown()
    {
        currentState = GameState.Countdown;

        BombermanController bombermanController = currentPlayer.GetComponent<BombermanController>();
        if (bombermanController != null)
        {
            bombermanController.DisableMovement();
        }

        // Show countdown image and text
        countdownImage.SetActive(false);
        winImage.SetActive(false);
        float countdown = countdownTime;
        countdownText.gameObject.SetActive(true);

        while (countdown > 0)
        {
            countdownText.text = Mathf.CeilToInt(countdown).ToString(); // Show 3, 2, 1
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        countdownText.gameObject.SetActive(false); // Hide countdown text
        countdownImage.SetActive(true); // Show "Go!" image
        yield return new WaitForSeconds(1f); // Wait a moment after displaying "Go!"
        countdownImage.SetActive(false); // Hide the countdown image

        StartGame(); // Begin the game logic
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        remainingTime = gameDuration;  // Set remaining time to total game duration
        playerLives = 3;

        if (livesText != null)
            livesText.text = playerLives.ToString();  // Update the lives display

        BombermanController bombermanController = currentPlayer.GetComponent<BombermanController>();
        if (bombermanController != null)
        {
            bombermanController.EnableMovement();
        }
    }

    public void RespawnPlayer()
    {
        if (playerLives > 0)
        {
            StartCoroutine(RespawnPlayerCoroutine());
        }
        else
        {
            EndGame();
        }
    }

    private IEnumerator RespawnPlayerCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        currentPlayer.transform.position = respawnPosition.position;
        currentPlayer.SetActive(true);

        // Enable movement on the player after respawning
        BombermanController bombermanController = currentPlayer.GetComponent<BombermanController>();
        if (bombermanController != null)
        {
            bombermanController.RespawnPlayer();
        }
    }

    public void PlayerDied()
    {
        playerLives--;

        UpdateLiveText();

        if (playerLives > 0)
        {
            RespawnPlayer();
        }
        else
        {
            EndGame();
        }
    }

    public void PlayerWon()
    {
        currentState = GameState.Win;  // Set state to Win
        winImage.SetActive(true);
        Debug.Log("You Win!");
    }


    private void EndGame()
    {
        currentState = GameState.GameOver; // Set state to GameOver
        Debug.Log("Game Over! Out of lives.");
    }

    public void TogglePauseGame()
    {
        gamePaused = !gamePaused;
        Time.timeScale = gamePaused ? 0 : 1;

        // Check the current state and update accordingly
        if (gamePaused)
        {
            currentState = GameState.Paused; // Update state to Paused
            Debug.Log("Game Paused");
        }
        else
        {
            currentState = GameState.Playing; // Update state back to Playing
            Debug.Log("Game Resumed");
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GainExtraLife(int lives)
    {
        playerLives += lives;
        UpdateLiveText();
        Debug.Log("Extra life gained! Total lives: " + playerLives);
    }

    private void UpdateLiveText()
    {
        if (livesText != null)
            livesText.text = playerLives.ToString();  // Update the lives display
    }
    #endregion

    #region Timer Logic

    // Update the countdown timer
    private void UpdateGameTimer()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime; // Decrease remaining time by the elapsed time

            // Clamp remaining time to not go below zero
            remainingTime = Mathf.Max(remainingTime, 0);

            // Display the remaining time in minutes and seconds
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);

            if (timerText != null)
            {
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds); // Display countdown using TMPro
            }

            // Check for game over condition
            if (remainingTime <= 0)
            {
                EndGame(); // End game when countdown reaches zero
            }
        }
    }

    #endregion
}
