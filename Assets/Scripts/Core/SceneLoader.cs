using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Cambia de escena con un fundido a negro.
/// Se crea solo la primera vez que se usa y sobrevive entre escenas.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public const string MenuScene = "MainMenu";
    public const string GameScene = "Game";

    private const float FadeDuration = 0.25f;
    private static readonly Color FadeColor = new Color32(0x14, 0x1A, 0x28, 0xFF);

    private static SceneLoader instance;

    private CanvasGroup fadeGroup;
    private bool isLoading;

    public static void Load(string sceneName)
    {
        if (instance == null) CreateInstance();
        if (instance.isLoading) return;
        instance.StartCoroutine(instance.FadeAndLoad(sceneName));
    }

    private static void CreateInstance()
    {
        var go = new GameObject("SceneLoader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneLoader>();

        // Lienzo que cubre toda la pantalla, por encima de cualquier otra UI.
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        instance.fadeGroup = go.AddComponent<CanvasGroup>();
        instance.fadeGroup.alpha = 0f;
        instance.fadeGroup.blocksRaycasts = false;

        var image = new GameObject("Fade", typeof(RectTransform), typeof(Image));
        image.transform.SetParent(go.transform, false);
        var rect = (RectTransform)image.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        image.GetComponent<Image>().color = FadeColor;
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        isLoading = true;
        fadeGroup.blocksRaycasts = true;

        yield return Fade(0f, 1f);
        Time.timeScale = 1f; // por si venimos de la pausa
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return Fade(1f, 0f);

        fadeGroup.blocksRaycasts = false;
        isLoading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        for (float t = 0f; t < FadeDuration; t += Time.unscaledDeltaTime)
        {
            fadeGroup.alpha = Mathf.Lerp(from, to, t / FadeDuration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }
}
