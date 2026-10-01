using UnityEngine;

/// <summary>
/// Ajusta la cámara ortográfica para que el tablero completo (con paredes)
/// se vea en cualquier resolución, dejando espacio arriba para el marcador.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [SerializeField] private GameGrid grid;
    [Tooltip("Margen alrededor del tablero (en celdas).")]
    [SerializeField] private float padding = 1f;
    [Tooltip("Espacio libre en la parte superior para el HUD (en celdas).")]
    [SerializeField] private float hudSpace = 2f;

    private Camera cam;
    private float lastAspect = -1f;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        Fit();
    }

    private void LateUpdate()
    {
        if (!Mathf.Approximately(cam.aspect, lastAspect))
            Fit();
    }

    public void Fit()
    {
        if (grid == null) return;
        if (cam == null) cam = GetComponent<Camera>();

        lastAspect = cam.aspect;
        float neededWidth = grid.Width + 2f + padding * 2f;           // +2 por las paredes
        float neededHeight = grid.Height + 2f + padding * 2f + hudSpace;
        cam.orthographicSize = Mathf.Max(neededHeight * 0.5f, neededWidth * 0.5f / cam.aspect);

        Vector3 center = grid.transform.position;
        transform.position = new Vector3(center.x, center.y + hudSpace * 0.5f, transform.position.z);
    }
}
