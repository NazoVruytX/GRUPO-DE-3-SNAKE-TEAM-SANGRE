using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el flujo de la partida: cuenta regresiva, juego, pausa y fin.
/// Escucha los eventos de la serpiente para sumar puntos y detectar el final.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState { Countdown, Playing, Paused, GameOver }

    [Header("Referencias")]
    [SerializeField] private SnakeController snake;
    [SerializeField] private Food food;
    [SerializeField] private GameHUD hud;
    [SerializeField] private PauseScreen pauseScreen;
    [SerializeField] private GameOverScreen gameOverScreen;

    [Header("Reglas")]
    [SerializeField, Min(1)] private int pointsPerFood = 10;
    [SerializeField, Min(0)] private int countdownFrom = 3;
    [Tooltip("Segundos entre el choque y la pantalla de fin de juego.")]
    [SerializeField, Min(0f)] private float gameOverDelay = 0.9f;

    public GameState State { get; private set; }
    public int Score { get; private set; }

    private int bestAtStart;

    private void Awake()
    {
        Time.timeScale = 1f;
        // La serpiente espera quieta hasta que termine la cuenta regresiva.
        snake.enabled = false;
    }

    private void OnEnable()
    {
        snake.FoodEaten += HandleFoodEaten;
        snake.Died += HandleDied;
    }

    private void OnDisable()
    {
        snake.FoodEaten -= HandleFoodEaten;
        snake.Died -= HandleDied;
    }

    private void Start()
    {
        bestAtStart = HighScore.Get();
        hud.SetScore(0);
        hud.SetHighScore(bestAtStart);
        pauseScreen.Hide();
        AudioManager.Instance?.SetMusicDucked(false);
        StartCoroutine(CountdownRoutine());
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool pausePressed = keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame;

        switch (State)
        {
            case GameState.Playing:
                if (pausePressed) Pause();
                break;

            case GameState.Paused:
                if (pausePressed) Resume();
                break;

            case GameState.GameOver:
                if (!gameOverScreen.IsVisible) break;
                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                    RestartGame();
                else if (keyboard.escapeKey.wasPressedThisFrame)
                    GoToMenu();
                break;
        }
    }

    // ---------- Acciones (también las usan los botones de la UI) ----------

    public void Pause()
    {
        if (State != GameState.Playing) return;

        State = GameState.Paused;
        Time.timeScale = 0f;
        snake.enabled = false; // así las flechas no giran la serpiente mientras navegas el menú
        pauseScreen.Show();
        AudioManager.Instance?.SetMusicDucked(true);
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;

        State = GameState.Playing;
        Time.timeScale = 1f;
        snake.enabled = true;
        pauseScreen.Hide();
        AudioManager.Instance?.SetMusicDucked(false);
    }

    public void RestartGame()
    {
        SceneLoader.Load(SceneLoader.GameScene);
    }

    public void GoToMenu()
    {
        SceneLoader.Load(SceneLoader.MenuScene);
    }

    // ---------- Flujo de la partida ----------

    private IEnumerator CountdownRoutine()
    {
        State = GameState.Countdown;
        for (int i = countdownFrom; i > 0; i--)
        {
            hud.ShowCountdown(i.ToString());
            AudioManager.Instance?.PlayCountdownBeep();
            yield return new WaitForSeconds(1f);
        }

        hud.ShowCountdown("¡YA!");
        AudioManager.Instance?.PlayCountdownGo();
        State = GameState.Playing;
        snake.enabled = true;

        yield return new WaitForSeconds(0.6f);
        hud.HideCountdown();
    }

    private void HandleFoodEaten()
    {
        Score += pointsPerFood;
        hud.SetScore(Score);
        AudioManager.Instance?.PlayEat();

        // Si superamos el récord, el contador de récord sube en vivo.
        if (Score > bestAtStart)
            hud.SetHighScore(Score);

        // Si la comida ya no encuentra lugar libre, la serpiente llenó el tablero: ¡victoria!
        if (!food.gameObject.activeSelf)
            EndGame(true);
    }

    private void HandleDied()
    {
        EndGame(false);
    }

    private void EndGame(bool isVictory)
    {
        if (State == GameState.GameOver) return;

        State = GameState.GameOver;
        snake.enabled = false;
        bool isNewRecord = HighScore.TrySave(Score);
        StartCoroutine(GameOverRoutine(isVictory, isNewRecord));
    }

    private IEnumerator GameOverRoutine(bool isVictory, bool isNewRecord)
    {
        AudioManager.Instance?.SetMusicDucked(true);
        if (!isVictory) AudioManager.Instance?.PlayCrash();

        yield return new WaitForSeconds(gameOverDelay);

        if (isVictory) AudioManager.Instance?.PlayCountdownGo();
        else AudioManager.Instance?.PlayGameOver();
        gameOverScreen.Show(Score, HighScore.Get(), isNewRecord, isVictory);
    }
}
