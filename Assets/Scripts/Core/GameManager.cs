using System.Collections;
using UnityEngine;

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
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        State = GameState.Countdown;
        for (int i = countdownFrom; i > 0; i--)
        {
            hud.ShowCountdown(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        hud.ShowCountdown("¡YA!");
        State = GameState.Playing;
        snake.enabled = true;

        yield return new WaitForSeconds(0.6f);
        hud.HideCountdown();
    }

    private void HandleFoodEaten()
    {
        Score += pointsPerFood;
        hud.SetScore(Score);

        // Si superamos el récord, el contador de récord sube en vivo.
        if (Score > bestAtStart)
            hud.SetHighScore(Score);
    }

    private void HandleDied()
    {
        State = GameState.GameOver;
        HighScore.TrySave(Score);
    }
}
