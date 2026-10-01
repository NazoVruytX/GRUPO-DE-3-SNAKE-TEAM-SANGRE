using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Comida de la serpiente: aparece en una celda libre al azar
/// y cuando la comen suelta partículas y reaparece en otro lugar.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Food : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameGrid grid;
    [SerializeField] private SnakeController snake;
    [SerializeField] private ParticleSystem eatEffect;

    [Header("Animación")]
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private float pulseAmount = 0.08f;

    public Vector2Int Cell { get; private set; }

    private void Start()
    {
        Respawn();
    }

    private void Update()
    {
        // Pequeño "latido" para que la comida llame la atención.
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    /// <summary>La serpiente llama a este método cuando choca con la comida.</summary>
    public void Eat()
    {
        if (eatEffect != null)
        {
            eatEffect.transform.position = transform.position;
            eatEffect.Play();
        }
        Respawn();
    }

    /// <summary>
    /// Mueve la comida a una celda que no esté ocupada por la serpiente.
    /// Devuelve false si ya no quedan celdas libres (tablero lleno).
    /// </summary>
    public bool Respawn()
    {
        var occupied = new HashSet<Vector2Int>(snake.Cells);
        var freeCells = new List<Vector2Int>();
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var cell = new Vector2Int(x, y);
                if (!occupied.Contains(cell))
                    freeCells.Add(cell);
            }
        }

        if (freeCells.Count == 0)
        {
            gameObject.SetActive(false);
            return false;
        }

        Cell = freeCells[Random.Range(0, freeCells.Count)];
        transform.position = grid.CellToWorld(Cell);
        return true;
    }
}
