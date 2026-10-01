using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Interfaz durante la partida: contador de puntos, récord y cuenta regresiva.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text countdownText;

    private Coroutine scorePunch;
    private Coroutine countdownPop;

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
        if (scorePunch != null) StopCoroutine(scorePunch);
        scorePunch = StartCoroutine(Punch(scoreText.transform, 0.3f));
    }

    public void SetHighScore(int highScore)
    {
        highScoreText.text = highScore.ToString();
    }

    public void ShowCountdown(string text)
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = text;
        if (countdownPop != null) StopCoroutine(countdownPop);
        countdownPop = StartCoroutine(Punch(countdownText.transform, 0.5f));
    }

    public void HideCountdown()
    {
        countdownText.gameObject.SetActive(false);
    }

    /// <summary>Agranda el texto un instante y lo devuelve a su tamaño (efecto "pop").</summary>
    private static IEnumerator Punch(Transform target, float strength)
    {
        const float duration = 0.18f;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float scale = 1f + strength * Mathf.Sin(t / duration * Mathf.PI);
            target.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        target.localScale = Vector3.one;
    }
}
