using UnityEngine;

/// <summary>
/// Guarda el récord (puntaje más alto) en PlayerPrefs,
/// así se conserva aunque se cierre el juego.
/// </summary>
public static class HighScore
{
    private const string Key = "Snake.HighScore";

    public static int Get()
    {
        return PlayerPrefs.GetInt(Key, 0);
    }

    /// <summary>Guarda el puntaje si supera al récord. Devuelve true si es un nuevo récord.</summary>
    public static bool TrySave(int score)
    {
        if (score <= Get()) return false;

        PlayerPrefs.SetInt(Key, score);
        PlayerPrefs.Save();
        return true;
    }
}
