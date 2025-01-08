using System.Collections;
using System.Collections.Generic;
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
    public int defaultPlayerLives = 3;
    public float respawnDelay = 5f;            // Delay before respawning the player
    public float countdownTime = 3f;          // Pre-game countdown time (3 seconds)
    public float gameDuration = 60f;          // Total game duration in seconds
    public List<Transform> respawnPositions;  // Respawn positions for each player

    [Header("UI Elements")]
    public TMP_Text timerText;                 // Countdown timer display using TextMeshPro
    public List<TMP_Text> playerLivesTexts;    // Lives display for each player
    public TMP_Text countdownText;             // Countdown text before game starts
    public GameObject countdownImage;          // Image displayed during the countdown
    public GameObject winImage;                // Image displayed during win state

    private List<GameObject> players = new List<GameObject>();
    private Dictionary<GameObject, int> playerLives = new Dictionary<GameObject, int>();
    private bool gamePaused = false;
    private float remainingTime;               // Remaining time for the countdown

    public int CurrentPlayerID { get; private set; }

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
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players.AddRange(playerObjects);

        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = respawnPositions[i].position;
            playerLives[players[i]] = defaultPlayerLives;
            BombermanController controller = players[i].GetComponent<BombermanController>();
            if (controller != null)
            {
                controller.SetPlayerID(i);
            }
        }

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

        foreach (var player in players)
        {
            BombermanController controller = player.GetComponent<BombermanController>();
            if (controller != null)
            {
                controller.DisableMovement();
            }
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

        foreach (var player in players)
        {
            BombermanController controller = player.GetComponent<BombermanController>();
            if (controller != null)
            {
                controller.EnableMovement();
            }
        }

        UpdateAllLivesText();
    }

    public void RespawnPlayer(GameObject player)
    {
        if (playerLives[player] > 0)
        {
            StartCoroutine(RespawnPlayerCoroutine(player));
        }
        else
        {
            PlayerOut(player);
        }
    }

    private IEnumerator RespawnPlayerCoroutine(GameObject player)
    {
        yield return new WaitForSeconds(respawnDelay);

        int playerIndex = players.IndexOf(player);
        player.transform.position = respawnPositions[playerIndex].position;
        player.SetActive(true);

        BombermanController controller = player.GetComponent<BombermanController>();
        if (controller != null)
        {
            controller.RespawnPlayer();
        }
    }

    public void SetCurrentPlayerID(int playerID)
    {
        CurrentPlayerID = playerID;
        Debug.Log($"Current player ID set to {playerID}");
    }

    public void PlayerDied(GameObject player)
    {
        playerLives[player]--;
        UpdateLivesText(player);

        if (playerLives[player] > 0)
        {
            RespawnPlayer(player);
        }
        else
        {
            Debug.Log($"{player.name} is out of lives!");
            PlayerOut(player);
        }
    }

    private void PlayerOut(GameObject player)
    {
        players.Remove(player);
        CheckGameOver();
    }

    public void PlayerWon(GameObject player)
    {
        currentState = GameState.Win;  // Set state to Win
        winImage.SetActive(true);
        Debug.Log($"{player.name} wins!");
    }

    private void CheckGameOver()
    {
        if (players.Count == 0)
        {
            currentState = GameState.GameOver; // Set state to GameOver
            Debug.Log("Game Over! All players are out of lives.");
        }
    }

    private void EndGame()
    {
        currentState = GameState.GameOver;
        Debug.Log("Game Over! Time's up.");
    }

    public void TogglePauseGame()
    {
        gamePaused = !gamePaused;
        Time.timeScale = gamePaused ? 0 : 1;

        currentState = gamePaused ? GameState.Paused : GameState.Playing;
        Debug.Log(gamePaused ? "Game Paused" : "Game Resumed");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GainExtraLife(GameObject player, int lives)
    {
        playerLives[player] += lives;
        UpdateLivesText(player);
        Debug.Log($"{player.name} gained extra lives! Total lives: {playerLives[player]}");
    }

    private void UpdateLivesText(GameObject player)
    {
        int playerIndex = players.IndexOf(player);
        if (playerIndex >= 0 && playerIndex < playerLivesTexts.Count)
        {
            playerLivesTexts[playerIndex].text = playerLives[player].ToString();
        }
    }

    private void UpdateAllLivesText()
    {
        foreach (var player in players)
        {
            UpdateLivesText(player);
        }
    }
    #endregion

    #region Timer Logic
    private void UpdateGameTimer()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime; // Decrease remaining time by the elapsed time

            remainingTime = Mathf.Max(remainingTime, 0); // Clamp remaining time to not go below zero

            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);

            if (timerText != null)
            {
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds); // Display countdown using TMPro
            }

            if (remainingTime <= 0)
            {
                EndGame();
            }
        }
    }
    #endregion
}