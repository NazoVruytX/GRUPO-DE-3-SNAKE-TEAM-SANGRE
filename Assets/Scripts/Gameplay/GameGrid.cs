using UnityEngine;

/// <summary>
/// Define el tablero como una cuadrícula de celdas (1 unidad = 1 celda)
/// y acomoda el piso y las paredes según el tamaño configurado.
/// </summary>
public class GameGrid : MonoBehaviour
{
    [Header("Tamaño del tablero (en celdas, usar números pares)")]
    [SerializeField, Min(6)] private int width = 26;
    [SerializeField, Min(6)] private int height = 14;

    [Header("Visuales")]
    [SerializeField] private SpriteRenderer floor;
    [SerializeField] private SpriteRenderer wallTop;
    [SerializeField] private SpriteRenderer wallBottom;
    [SerializeField] private SpriteRenderer wallLeft;
    [SerializeField] private SpriteRenderer wallRight;

    public int Width => width;
    public int Height => height;
    public Vector2Int Center => new Vector2Int(width / 2, height / 2);

    private void Awake()
    {
        Layout();
    }

    /// <summary>Convierte una celda (x, y) del tablero a posición en el mundo.</summary>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        Vector3 offset = new Vector3(cell.x - (width - 1) * 0.5f, cell.y - (height - 1) * 0.5f, 0f);
        return transform.position + offset;
    }

    public bool IsInside(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    /// <summary>Ajusta el tamaño del piso y coloca las 4 paredes alrededor del tablero.</summary>
    public void Layout()
    {
        if (floor != null)
        {
            floor.transform.localPosition = Vector3.zero;
            floor.size = new Vector2(width, height);
        }

        float halfW = width * 0.5f + 0.5f;
        float halfH = height * 0.5f + 0.5f;
        PlaceWall(wallTop, new Vector2(0f, halfH), new Vector2(width + 2, 1));
        PlaceWall(wallBottom, new Vector2(0f, -halfH), new Vector2(width + 2, 1));
        PlaceWall(wallLeft, new Vector2(-halfW, 0f), new Vector2(1, height));
        PlaceWall(wallRight, new Vector2(halfW, 0f), new Vector2(1, height));
    }

    private static void PlaceWall(SpriteRenderer wall, Vector2 localPosition, Vector2 size)
    {
        if (wall == null) return;
        wall.transform.localPosition = localPosition;
        wall.size = size;

        // El collider de la pared cubre todo el sprite para detectar el choque.
        if (wall.TryGetComponent(out BoxCollider2D wallCollider))
            wallCollider.size = size;
    }
}
