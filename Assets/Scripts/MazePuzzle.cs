using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MazePuzzle : MonoBehaviour
{
    [SerializeField] private GameObject puzzleObject;

    [Header("Colors")]
    [SerializeField] private Color wallColor = Color.black;
    [SerializeField] private Color pathColor = Color.white;
    [SerializeField] private Color entryColor = Color.green;
    [SerializeField] private Color exitColor = Color.red;
    [SerializeField] private Color visitedColor = Color.yellow;

    private int centerX;
    private int centerY;
    private float tileWidth;
    private float tileHeight;

    private GameObject[,] GridObjects;
    private RawImage[,] TileImages;
    private MazeGenerator mazeGenerator;

    private List<Vector2Int> visitedPath = new List<Vector2Int>();
    private Camera canvasCamera;

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
        TileImages = new RawImage[mazeGenerator.Rows, mazeGenerator.Cols];

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
                TileImages[y, x] = tile.GetComponent<RawImage>();
                TileImages[y, x].color = mazeGenerator.Grid[y, x] == 0 ? pathColor : wallColor;
            }
        }

        SetTileColor(mazeGenerator.Entry, entryColor);
        SetTileColor(mazeGenerator.Exit, exitColor);

        ResetPath();
    }

    public void ResetPath()
    {
        if (visitedPath != null)
        {
            foreach (var cell in visitedPath)
            {
                if (cell == mazeGenerator.Entry) { SetTileColor(cell, entryColor); continue; }
                if (cell == mazeGenerator.Exit) { SetTileColor(cell, exitColor); continue; }
                SetTileColor(cell, pathColor);
            }
        }

        visitedPath = new List<Vector2Int>();
    }

    void Update()
    {
        if(GridObjects == null)
        {
            GenerateMaze();
        }

        //if (!Input.GetMouseButton(0)) return;

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
        Vector2Int grid = new Vector2Int(gridX, gridY);

        if (gridX < 0 || gridX >= mazeGenerator.Cols || gridY < 0 || gridY >= mazeGenerator.Rows)
            return;

        if (mazeGenerator.Grid[gridY, gridX] != 0)
            return;

        if (visitedPath.Count > 0 && visitedPath[visitedPath.Count - 1] == grid)
            return;

        if (visitedPath.Count == 0)
        {
            if (grid != mazeGenerator.Entry) return;
            visitedPath.Add(grid);
            SetTileColor(grid, entryColor);
            return;
        }

        Vector2Int lastCell = visitedPath[visitedPath.Count - 1];

        if (!AreTouching(grid, lastCell)) return;

        int existingIndex = visitedPath.IndexOf(grid);
        if (existingIndex >= 0)
        {
            for (int i = existingIndex + 1; i < visitedPath.Count; i++)
            {
                var cell = visitedPath[i];
                if (cell == mazeGenerator.Entry) { SetTileColor(cell, entryColor); continue; }
                if (cell == mazeGenerator.Exit) { SetTileColor(cell, exitColor); continue; }
                SetTileColor(cell, pathColor);
            }
            visitedPath.RemoveRange(existingIndex + 1, visitedPath.Count - existingIndex - 1);
            return;
        }

        if (grid == mazeGenerator.Exit)
        {
            visitedPath.Add(grid);
            SetTileColor(grid, exitColor);
            OnMazeSolved();
            return;
        }

        visitedPath.Add(grid);
        SetTileColor(grid, visitedColor);
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

        transform.gameObject.SetActive(false);
    }

    private void SetTileColor(Vector2Int cell, Color color)
    {
        if (TileImages[cell.y, cell.x] != null)
            TileImages[cell.y, cell.x].color = color;
    }

    private bool AreTouching(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }
}