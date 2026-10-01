using TMPro;
using UnityEngine;

/// <summary>
/// Menú principal: jugar, activar/desactivar sonido y salir.
/// Los botones llaman a estos métodos desde su evento OnClick (ver Inspector).
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text soundButtonText;

    private bool? lastMuted;

    private void Start()
    {
        Time.timeScale = 1f;
        highScoreText.text = "RÉCORD: " + HighScore.Get();
        AudioManager.Instance?.SetMusicDucked(false);
    }

    private void Update()
    {
        // El sonido también se puede cambiar con la tecla M, así que se revisa siempre.
        bool muted = AudioManager.Instance != null && AudioManager.Instance.IsMuted;
        if (lastMuted != muted)
        {
            lastMuted = muted;
            soundButtonText.text = muted ? "SONIDO: NO" : "SONIDO: SÍ";
        }
    }

    public void Play()
    {
        SceneLoader.Load(SceneLoader.GameScene);
    }

    public void ToggleSound()
    {
        AudioManager.Instance?.ToggleMute();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
