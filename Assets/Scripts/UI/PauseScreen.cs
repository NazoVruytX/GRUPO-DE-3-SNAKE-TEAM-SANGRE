using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Menú de pausa: Continuar, Reiniciar y Menú (los botones llaman al GameManager).</summary>
public class PauseScreen : MonoBehaviour
{
    [SerializeField] private Button firstButton;

    public void Show()
    {
        gameObject.SetActive(true);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
