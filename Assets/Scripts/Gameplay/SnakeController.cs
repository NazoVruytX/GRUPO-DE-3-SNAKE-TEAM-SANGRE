using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la serpiente: lee el teclado, avanza celda por celda
/// y hace que cada segmento del cuerpo siga al anterior.
/// Este componente va en la cabeza de la serpiente, que tiene un
/// Rigidbody2D y un collider "trigger" para detectar las colisiones.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class SnakeController : MonoBehaviour
{
    private const string ObstacleTag = "Obstacle";

    [Header("Referencias")]
    [SerializeField] private GameGrid grid;
    [SerializeField] private Transform bodyPrefab;
    [SerializeField] private Transform bodyContainer;

    [Header("Movimiento")]
    [Tooltip("Segundos entre cada paso (menos = más rápido).")]
    [SerializeField, Min(0.06f)] private float stepInterval = 0.14f;
    [Tooltip("Cuánto se acelera (en segundos) cada vez que come.")]
    [SerializeField, Min(0f)] private float speedUpPerFood = 0.004f;
    [Tooltip("Intervalo mínimo: la serpiente nunca será más rápida que esto.")]
    [SerializeField, Min(0.06f)] private float minStepInterval = 0.07f;
    [SerializeField, Min(2)] private int initialLength = 4;

    [Header("Apariencia")]
    [Tooltip("Color de la punta de la cola; el cuerpo se degrada desde blanco hasta este color.")]
    [SerializeField] private Color tailTint = new Color(0.7f, 0.7f, 0.7f);
    [Tooltip("Color que toma la serpiente después de chocar.")]
    [SerializeField] private Color deadTint = new Color(0.55f, 0.5f, 0.5f);

    [Header("Efectos")]
    [SerializeField] private ParticleSystem crashEffect;

    /// <summary>Se dispara cada vez que la serpiente come.</summary>
    public event Action FoodEaten;
    /// <summary>Se dispara cuando la serpiente choca con una pared o consigo misma.</summary>
    public event Action Died;

    public IReadOnlyList<Vector2Int> Cells => cells;
    public bool IsAlive { get; private set; } = true;

    private readonly List<Transform> segments = new List<Transform>();     // [0] = cabeza
    private readonly List<Vector2Int> cells = new List<Vector2Int>();       // celda de cada segmento
    private readonly List<Vector2Int> pendingTurns = new List<Vector2Int>(); // giros en espera (máx. 2)

    private Vector2Int direction = Vector2Int.right;
    private float stepTimer;
    private int pendingGrowth;

    private void Awake()
    {
        SpawnSnake();
    }

    private void Update()
    {
        if (!IsAlive) return;

        Vector2Int input = ReadInput();
        if (input != Vector2Int.zero)
            QueueTurn(input);

        stepTimer += Time.deltaTime;
        if (stepTimer >= stepInterval)
        {
            stepTimer -= stepInterval;
            Step();
        }
    }

    /// <summary>Coloca la cabeza en el centro del tablero y crea el cuerpo inicial hacia la izquierda.</summary>
    private void SpawnSnake()
    {
        direction = Vector2Int.right;
        Vector2Int start = grid.Center;

        segments.Add(transform);
        cells.Add(start);
        for (int i = 1; i < initialLength; i++)
            AddSegment(start - direction * i);

        UpdateVisuals();
    }

    private void AddSegment(Vector2Int cell)
    {
        Transform segment = Instantiate(bodyPrefab, grid.CellToWorld(cell), Quaternion.identity, bodyContainer);
        segment.name = "Segment " + segments.Count;
        segments.Add(segment);
        cells.Add(cell);
        ShadeBody();
    }

    private static Vector2Int ReadInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) return Vector2Int.up;
            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) return Vector2Int.down;
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) return Vector2Int.left;
            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) return Vector2Int.right;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            if (gamepad.dpad.up.wasPressedThisFrame) return Vector2Int.up;
            if (gamepad.dpad.down.wasPressedThisFrame) return Vector2Int.down;
            if (gamepad.dpad.left.wasPressedThisFrame) return Vector2Int.left;
            if (gamepad.dpad.right.wasPressedThisFrame) return Vector2Int.right;
        }

        return Vector2Int.zero;
    }

    /// <summary>
    /// Guarda el giro para el siguiente paso. Se ignoran los giros repetidos
    /// y los de 180° (la serpiente no puede darse la vuelta sobre sí misma).
    /// </summary>
    private void QueueTurn(Vector2Int newDirection)
    {
        Vector2Int last = pendingTurns.Count > 0 ? pendingTurns[pendingTurns.Count - 1] : direction;
        if (newDirection == last || newDirection == -last || pendingTurns.Count >= 2)
            return;

        pendingTurns.Add(newDirection);
    }

    /// <summary>
    /// Avanza la serpiente una celda: cada segmento ocupa el lugar del anterior.
    /// Si comió, aparece un segmento nuevo donde estaba la cola.
    /// </summary>
    private void Step()
    {
        if (pendingTurns.Count > 0)
        {
            direction = pendingTurns[0];
            pendingTurns.RemoveAt(0);
        }

        Vector2Int previousTail = cells[cells.Count - 1];
        for (int i = cells.Count - 1; i > 0; i--)
            cells[i] = cells[i - 1];
        cells[0] += direction;

        UpdateVisuals();

        if (pendingGrowth > 0)
        {
            pendingGrowth--;
            AddSegment(previousTail);
        }
    }

    /// <summary>
    /// Colisiones: la cabeza (trigger + Rigidbody2D) avisa con qué chocó.
    /// Comida = crecer; pared o cuerpo (tag "Obstacle") = perder.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive) return;

        if (other.TryGetComponent(out Food food))
        {
            pendingGrowth++;
            stepInterval = Mathf.Max(minStepInterval, stepInterval - speedUpPerFood);
            food.Eat();
            FoodEaten?.Invoke();
        }
        else if (other.CompareTag(ObstacleTag))
        {
            Die();
        }
    }

    private void Die()
    {
        IsAlive = false;
        pendingTurns.Clear();

        if (crashEffect != null)
        {
            crashEffect.transform.position = transform.position;
            crashEffect.Play();
        }

        StartCoroutine(DeathBlink());
        Died?.Invoke();
    }

    /// <summary>La serpiente parpadea 3 veces y queda oscurecida.</summary>
    private IEnumerator DeathBlink()
    {
        var sprites = new List<SpriteRenderer>();
        foreach (Transform segment in segments)
            sprites.Add(segment.GetComponent<SpriteRenderer>());

        for (int i = 0; i < 6; i++)
        {
            bool visible = i % 2 == 1;
            foreach (SpriteRenderer sprite in sprites)
                sprite.enabled = visible;
            yield return new WaitForSeconds(0.1f);
        }

        foreach (SpriteRenderer sprite in sprites)
            sprite.color *= deadTint;
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < segments.Count; i++)
            segments[i].position = grid.CellToWorld(cells[i]);

        // El sprite de la cabeza mira a la derecha; se rota según la dirección.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>Oscurece gradualmente el cuerpo hacia la cola.</summary>
    private void ShadeBody()
    {
        for (int i = 1; i < segments.Count; i++)
        {
            float t = (float)i / segments.Count;
            segments[i].GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, tailTint, t);
        }
    }
}
