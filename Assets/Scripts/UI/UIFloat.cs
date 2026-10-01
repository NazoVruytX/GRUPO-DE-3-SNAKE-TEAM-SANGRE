using UnityEngine;

/// <summary>Hace "flotar" un elemento de la UI subiendo y bajando suavemente.</summary>
[RequireComponent(typeof(RectTransform))]
public class UIFloat : MonoBehaviour
{
    [SerializeField] private float amplitude = 10f;
    [SerializeField] private float speed = 2f;

    private RectTransform rect;
    private Vector2 startPosition;

    private void Awake()
    {
        rect = (RectTransform)transform;
        startPosition = rect.anchoredPosition;
    }

    private void Update()
    {
        rect.anchoredPosition = startPosition + Vector2.up * (Mathf.Sin(Time.unscaledTime * speed) * amplitude);
    }
}
