using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reproduce la música y los efectos de sonido.
/// Es un singleton que sobrevive al cambio de escena (DontDestroyOnLoad),
/// así la música no se corta al pasar del menú al juego.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private const string MuteKey = "Snake.Muted";

    public static AudioManager Instance { get; private set; }

    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Música")]
    [SerializeField] private AudioClip music;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.4f;
    [Tooltip("Volumen de la música cuando se baja (por ejemplo, al perder).")]
    [SerializeField, Range(0f, 1f)] private float duckedVolume = 0.12f;

    [Header("Efectos")]
    [SerializeField] private AudioClip eat;
    [SerializeField] private AudioClip crash;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip countdownBeep;
    [SerializeField] private AudioClip countdownGo;
    [SerializeField] private AudioClip click;

    public bool IsMuted { get; private set; }

    private float targetMusicVolume;

    private void Awake()
    {
        // Si ya existe uno (venimos de otra escena), este sobra.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsMuted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
        AudioListener.volume = IsMuted ? 0f : 1f;

        targetMusicVolume = musicVolume;
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (Instance != this) return; // duplicado que está por destruirse

        // Cambio suave del volumen de la música.
        musicSource.volume = Mathf.MoveTowards(musicSource.volume, targetMusicVolume, Time.unscaledDeltaTime * 0.5f);

        // Atajo: la tecla M silencia o activa el sonido en cualquier escena.
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            ToggleMute();
    }

    public void PlayEat() => PlaySfx(eat, Random.Range(0.95f, 1.1f));
    public void PlayCrash() => PlaySfx(crash);
    public void PlayGameOver() => PlaySfx(gameOver);
    public void PlayCountdownBeep() => PlaySfx(countdownBeep);
    public void PlayCountdownGo() => PlaySfx(countdownGo);
    public void PlayClick() => PlaySfx(click);

    /// <summary>Baja la música (true) o la devuelve a su volumen normal (false).</summary>
    public void SetMusicDucked(bool ducked)
    {
        targetMusicVolume = ducked ? duckedVolume : musicVolume;
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        AudioListener.volume = IsMuted ? 0f : 1f;
        PlayerPrefs.SetInt(MuteKey, IsMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void PlaySfx(AudioClip clip, float pitch = 1f)
    {
        if (clip == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip);
    }
}
