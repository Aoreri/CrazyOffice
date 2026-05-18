using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class PathTextureSet
{
    [Header("Straight Lines")]
    public Sprite horizontal;
    public Sprite vertical;

    [Header("90-Degree Corners")]
    public Sprite cornerTopRight;    // Connects Up and Right
    public Sprite cornerTopLeft;     // Connects Up and Left
    public Sprite cornerBottomRight; // Connects Down and Right
    public Sprite cornerBottomLeft;  // Connects Down and Left

    [Header("Line Ends (Tip of the path)")]
    public Sprite endUp;             // Points Up
    public Sprite endDown;           // Points Down
    public Sprite endLeft;           // Points Left
    public Sprite endRight;          // Points Right

    public Sprite dot;               // Fallback for a single isolated tile
}

public class MazePuzzle : Puzzle
{
    [SerializeField] private GameObject puzzleObject;

    [Header("Colors")]
    [SerializeField] private Color wallColor = Color.black;
    [SerializeField] private Color pathColor = Color.white;
    [SerializeField] private Color entryColor = Color.green;
    [SerializeField] private Color exitColor = Color.red;
    [SerializeField] private Color visitedColor = Color.white;

    [Header("Sprites")]
    [SerializeField] private Sprite entrySprite;
    [SerializeField] private Sprite exitSprite;

    [Space(10)]
    [SerializeField] private PathTextureSet pathSprites;

    private int centerX;
    private int centerY;
    private float tileWidth;
    private float tileHeight;

    private GameObject[,] GridObjects;
    private Image[,] TileImages; // Changed from RawImage to Image
    private MazeGenerator mazeGenerator;

    private List<Vector2Int> visitedPath = new List<Vector2Int>();
    private Camera canvasCamera;

    private Vector2Int? lastDetectedGrid = null;

    public int autoCompleteCount = 3;

    void Start()
    {
        mazeGenerator = new MazeGenerator();
        RectTransform rt = puzzleObject.GetComponent<RectTransform>();
        tileWidth = rt.rect.width;
        tileHeight = rt.rect.height;

        Canvas canvas = GetComponentInParent<Canvas>();
        canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
    }

    public void GenerateMaze()
    {
        if (GridObjects != null)
        {
            for (int y = 0; y < GridObjects.GetLength(0); y++)
                for (int x = 0; x < GridObjects.GetLength(1); x++)
                    if (GridObjects[y, x] != null)
                        Destroy(GridObjects[y, x]);
        }

        mazeGenerator.Generate(21, 21);

        centerX = mazeGenerator.Cols / 2;
        centerY = mazeGenerator.Rows / 2;

        GridObjects = new GameObject[mazeGenerator.Rows, mazeGenerator.Cols];
        TileImages = new Image[mazeGenerator.Rows, mazeGenerator.Cols]; // Initializing Image array

        for (int y = 0; y < mazeGenerator.Rows; y++)
        {
            for (int x = 0; x < mazeGenerator.Cols; x++)
            {
                GameObject tile = Instantiate(puzzleObject);
                tile.transform.SetParent(transform, false);
                tile.transform.name = "[" + y + ", " + x + "]";
                float posX = (x - centerX) * tileWidth;
                float posY = (y - centerY) * tileHeight;
                tile.transform.localPosition = new Vector3(posX, posY, 0f);
                GridObjects[y, x] = tile;

                TileImages[y, x] = tile.GetComponent<Image>(); // Changed to Image
                TileImages[y, x].color = mazeGenerator.Grid[y, x] == 0 ? pathColor : wallColor;
                TileImages[y, x].sprite = null; // Using sprite instead of texture
            }
        }

        SetTileVisual(mazeGenerator.Entry, entryColor, entrySprite);
        SetTileVisual(mazeGenerator.Exit, exitColor, exitSprite);

        ResetPath();
    }

    public void ResetPath()
    {
        if (visitedPath != null)
        {
            foreach (var cell in visitedPath)
            {
                if (cell == mazeGenerator.Entry) { SetTileVisual(cell, entryColor, entrySprite); continue; }
                if (cell == mazeGenerator.Exit) { SetTileVisual(cell, exitColor, exitSprite); continue; }
                SetTileVisual(cell, pathColor, null);
            }
        }

        visitedPath = new List<Vector2Int>();
    }

    void Update()
    {
        if (GridObjects == null)
        {
            GenerateMaze();
        }

        Vector2 localPoint;
        RectTransform parentRect = transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            Input.mousePosition,
            canvasCamera,
            out localPoint
        );

        int gridX = Mathf.FloorToInt((localPoint.x + tileWidth / 2f) / tileWidth + centerX);
        int gridY = Mathf.FloorToInt((localPoint.y + tileHeight / 2f) / tileHeight + centerY);

        if (gridX < 0 || gridX >= mazeGenerator.Cols || gridY < 0 || gridY >= mazeGenerator.Rows)
        {
            lastDetectedGrid = null;
            return;
        }

        Vector2Int currentGrid = new Vector2Int(gridX, gridY);

        if (lastDetectedGrid.HasValue && lastDetectedGrid.Value != currentGrid)
        {
            foreach (Vector2Int cell in GetCellsBetween(lastDetectedGrid.Value, currentGrid))
                ProcessCell(cell);
        }
        else
        {
            ProcessCell(currentGrid);
        }

        lastDetectedGrid = currentGrid;
    }

    private IEnumerable<Vector2Int> GetCellsBetween(Vector2Int from, Vector2Int to)
    {
        int x0 = from.x, y0 = from.y;
        int x1 = to.x, y1 = to.y;

        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            yield return new Vector2Int(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    private void ProcessCell(Vector2Int grid)
    {
        if (grid.x < 0 || grid.x >= mazeGenerator.Cols || grid.y < 0 || grid.y >= mazeGenerator.Rows)
            return;

        if (mazeGenerator.Grid[grid.y, grid.x] != 0) return;
        if (visitedPath.Count > 0 && visitedPath[visitedPath.Count - 1] == grid) return;

        if (visitedPath.Count == 0)
        {
            if (grid != mazeGenerator.Entry) return;
            visitedPath.Add(grid);
            SetTileVisual(grid, entryColor, entrySprite);
            return;
        }

        Vector2Int lastCell = visitedPath[visitedPath.Count - 1];
        if (!AreTouching(grid, lastCell))
        {
          
            List<Vector2Int> fillPath = FindPathBFS(lastCell, grid);
            if (fillPath != null && fillPath.Count - 1 <= autoCompleteCount)
            {
               
                for (int i = 1; i < fillPath.Count; i++)
                    visitedPath.Add(fillPath[i]);

                RefreshPathVisuals();

                if (visitedPath[visitedPath.Count - 1] == mazeGenerator.Exit)
                {
                    SetTileVisual(mazeGenerator.Exit, exitColor, exitSprite);
                    OnMazeSolved();
                }
            }
            return;
        }

        int existingIndex = visitedPath.IndexOf(grid);
        if (existingIndex >= 0)
        {
            for (int i = existingIndex + 1; i < visitedPath.Count; i++)
            {
                var cell = visitedPath[i];
                if (cell == mazeGenerator.Entry) { SetTileVisual(cell, entryColor, entrySprite); continue; }
                if (cell == mazeGenerator.Exit) { SetTileVisual(cell, exitColor, exitSprite); continue; }
                SetTileVisual(cell, pathColor, null);
            }
            visitedPath.RemoveRange(existingIndex + 1, visitedPath.Count - existingIndex - 1);
            RefreshPathVisuals();
            return;
        }

        visitedPath.Add(grid);

        if (grid == mazeGenerator.Exit)
        {
            SetTileVisual(grid, exitColor, exitSprite);
            RefreshPathVisuals();
            OnMazeSolved();
            return;
        }

        RefreshPathVisuals();
    }

    private List<Vector2Int> FindPathBFS(Vector2Int start, Vector2Int goal)
    {
        var queue = new Queue<Vector2Int>();
        var visited = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        visited[start] = start;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == goal)
            {
                var path = new List<Vector2Int>();
                while (current != start)
                {
                    path.Add(current);
                    current = visited[current];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            foreach (var dir in dirs)
            {
                Vector2Int neighbor = current + dir;
                if (neighbor.x < 0 || neighbor.x >= mazeGenerator.Cols) continue;
                if (neighbor.y < 0 || neighbor.y >= mazeGenerator.Rows) continue;
                if (mazeGenerator.Grid[neighbor.y, neighbor.x] != 0) continue;
                if (visited.ContainsKey(neighbor)) continue;

                visited[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }

        return null;
    }

    // New method that calculates the correct shape for every piece of the path
    private void RefreshPathVisuals()
    {
        for (int i = 0; i < visitedPath.Count; i++)
        {
            Vector2Int current = visitedPath[i];

            // Keep Entry and Exit distinct
            if (current == mazeGenerator.Entry || current == mazeGenerator.Exit)
                continue;

            // Figure out the surrounding connections
            Vector2Int? prev = (i > 0) ? visitedPath[i - 1] : (Vector2Int?)null;
            Vector2Int? next = (i < visitedPath.Count - 1) ? visitedPath[i + 1] : (Vector2Int?)null;

            bool up = false, down = false, left = false, right = false;

            // Local helper to map neighbor coordinates to directions
            void CheckDirection(Vector2Int neighbor)
            {
                if (neighbor.y > current.y) up = true;
                if (neighbor.y < current.y) down = true;
                if (neighbor.x > current.x) right = true;
                if (neighbor.x < current.x) left = true;
            }

            if (prev.HasValue) CheckDirection(prev.Value);
            if (next.HasValue) CheckDirection(next.Value);

            Sprite segmentSprite = pathSprites.dot; // Fallback to Sprite

            // Straights
            if (up && down) segmentSprite = pathSprites.vertical;
            else if (left && right) segmentSprite = pathSprites.horizontal;

            // 90-Degree Corners
            else if (up && right) segmentSprite = pathSprites.cornerTopRight;
            else if (up && left) segmentSprite = pathSprites.cornerTopLeft;
            else if (down && right) segmentSprite = pathSprites.cornerBottomRight;
            else if (down && left) segmentSprite = pathSprites.cornerBottomLeft;

            // Ends (Tip of the line)
            else if (up) segmentSprite = pathSprites.endUp;
            else if (down) segmentSprite = pathSprites.endDown;
            else if (left) segmentSprite = pathSprites.endLeft;
            else if (right) segmentSprite = pathSprites.endRight;

            SetTileVisual(current, visitedColor, segmentSprite);
        }
    }

    private void OnMazeSolved()
    {
        Debug.Log("Maze Solved!");
        if (GridObjects != null)
        {
            for (int y = 0; y < GridObjects.GetLength(0); y++)
                for (int x = 0; x < GridObjects.GetLength(1); x++)
                    if (GridObjects[y, x] != null)
                        Destroy(GridObjects[y, x]);
        }

        GridObjects = null;
        TileImages = null;
        visitedPath = new List<Vector2Int>();

        EndPuzzle();
    }

    private void SetTileVisual(Vector2Int cell, Color color, Sprite sprite)
    {
        if (TileImages[cell.y, cell.x] != null)
        {
            TileImages[cell.y, cell.x].color = color;
            TileImages[cell.y, cell.x].sprite = sprite; // Assigns the Sprite instead of Texture
        }
    }

    private bool AreTouching(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }

    protected override void OnEndPuzzle()
    {

    }

    protected override void OnStartPuzzle()
    {

    }
}