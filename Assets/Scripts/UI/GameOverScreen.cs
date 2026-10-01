using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pantalla de fin de partida: muestra el puntaje, el récord
/// y los botones para reintentar o volver al menú.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestText;
    [SerializeField] private GameObject newRecordBadge;
    [SerializeField] private Button firstButton;

    [Header("Colores del título")]
    [SerializeField] private Color loseColor = new Color32(0xE5, 0x48, 0x4D, 0xFF);
    [SerializeField] private Color winColor = new Color32(0x5B, 0xE3, 0x7D, 0xFF);

    public bool IsVisible => gameObject.activeSelf;

    public void Show(int score, int best, bool isNewRecord, bool isVictory)
    {
        titleText.text = isVictory ? "¡GANASTE!" : "¡FIN DEL JUEGO!";
        titleText.color = isVictory ? winColor : loseColor;
        scoreText.text = "PUNTOS: " + score;
        bestText.text = "RÉCORD: " + best;
        newRecordBadge.SetActive(isNewRecord);

        gameObject.SetActive(true);
        StartCoroutine(FadeIn());

        // Deja seleccionado "Reintentar" para poder usar el teclado (Enter).
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }

    private IEnumerator FadeIn()
    {
        const float duration = 0.3f;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = t / duration;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
}
