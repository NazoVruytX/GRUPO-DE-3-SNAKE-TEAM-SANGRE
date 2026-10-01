using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Respuesta visual y sonora de los botones: se agrandan al estar seleccionados
/// (con mouse o teclado) y suenan al hacer clic.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonFeedback : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private float selectedScale = 1.08f;
    [SerializeField] private float speed = 14f;

    private Vector3 targetScale = Vector3.one;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => AudioManager.Instance?.PlayClick());
    }

    private void OnDisable()
    {
        targetScale = Vector3.one;
        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        // unscaledDeltaTime: funciona aunque el juego esté en pausa (timeScale = 0).
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Pasar el mouse por encima selecciona el botón, igual que con el teclado.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnSelect(BaseEventData eventData)
    {
        targetScale = Vector3.one * selectedScale;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        targetScale = Vector3.one;
    }
}
