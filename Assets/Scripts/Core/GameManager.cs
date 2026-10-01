using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el flujo de la partida: cuenta regresiva, juego y fin.
/// Escucha los eventos de la serpiente para sumar puntos y detectar el final.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState { Countdown, Playing, GameOver }

    [Header("Referencias")]
    [SerializeField] private SnakeController snake;
    [SerializeField] private GameHUD hud;

    [Header("Reglas")]
    [SerializeField, Min(1)] private int pointsPerFood = 10;
    [SerializeField, Min(0)] private int countdownFrom = 3;

    public GameState State { get; private set; }
    public int Score { get; private set; }

    private int bestAtStart;

    private void Awake()
    {
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
        AudioManager.Instance?.SetMusicDucked(false);
        StartCoroutine(CountdownRoutine());
    }

    private void Update()
    {
        if (State != GameState.GameOver) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            RestartGame();
        else if (keyboard.escapeKey.wasPressedThisFrame)
            GoToMenu();
    }

    public void RestartGame()
    {
        SceneLoader.Load(SceneLoader.GameScene);
    }

    public void GoToMenu()
    {
        SceneLoader.Load(SceneLoader.MenuScene);
    }

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
    }

    private void HandleDied()
    {
        State = GameState.GameOver;
        HighScore.TrySave(Score);
        StartCoroutine(GameOverSoundRoutine());
    }

    private IEnumerator GameOverSoundRoutine()
    {
        AudioManager.Instance?.PlayCrash();
        AudioManager.Instance?.SetMusicDucked(true);
        yield return new WaitForSeconds(0.5f);
        AudioManager.Instance?.PlayGameOver();
    }
}
