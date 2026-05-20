using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MazePuzzle : MonoBehaviour
{
    [SerializeField] private GameObject puzzleObject;

    // YENÝ: Görev bitince kapanmasý gereken arka plan panosu (backplate)
    [Header("Arka Plan Objesi")]
    [SerializeField] private GameObject backplateObject;

    [Header("Yuva ve Arka Plan Resimleri")]
    [SerializeField] private Texture entrySocketTexture;
    [SerializeField] private Texture exitSocketTexture;
    [SerializeField] private Texture emptyChannelTexture;
    [SerializeField] private Texture wallTexture;

    [Header("Kesintisiz Kablo (Kuyruk)")]
    [SerializeField] private Color cableColor = new Color(0.1f, 0.6f, 1f, 1f);
    [SerializeField] private float overlapScale = 1.25f;

    [Header("Kablo Ucu (Fiþ / Baþ Kýsým)")]
    [SerializeField] private Texture cableHeadTexture;
    public float cableHeadOffset = -270f;

    [Header("Boyut Büyütme Ayarlarý")]
    [SerializeField] private float socketScale = 1.4f;
    [SerializeField] private float headScale = 1.4f;

    private int centerX, centerY;
    private float tileWidth; // Satýr 31 hatasý düzeltildi
    private float tileHeight;
    private GameObject[,] GridObjects;
    private RawImage[,] TileImages;
    private RectTransform[,] TileRects;
    private MazeGenerator mazeGenerator;
    private List<Vector2Int> visitedPath = new List<Vector2Int>();
    private Camera canvasCamera;
    private Vector2Int? lastDetectedGrid = null;

    enum Appearance { Socket, Exit, Cable, CableHead, Empty, Wall }

    void Start()
    {
        mazeGenerator = new MazeGenerator();
        RectTransform rt = puzzleObject.GetComponent<RectTransform>();
        tileWidth = rt.rect.width; tileHeight = rt.rect.height;
        Canvas canvas = GetComponentInParent<Canvas>();
        canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
    }

    public void GenerateMaze()
    {
        if (GridObjects != null) foreach (var go in GridObjects) if (go != null) Destroy(go);

        // Eðer yeni oyun baþlayýnca backplate kapalýysa geri açalým
        if (backplateObject != null) backplateObject.SetActive(true);

        mazeGenerator.Generate(21, 21);
        centerX = mazeGenerator.Cols / 2; centerY = mazeGenerator.Rows / 2;

        GridObjects = new GameObject[mazeGenerator.Rows, mazeGenerator.Cols];
        TileImages = new RawImage[mazeGenerator.Rows, mazeGenerator.Cols];
        TileRects = new RectTransform[mazeGenerator.Rows, mazeGenerator.Cols];

        for (int y = 0; y < mazeGenerator.Rows; y++)
        {
            for (int x = 0; x < mazeGenerator.Cols; x++)
            {
                GameObject tile = Instantiate(puzzleObject);
                tile.transform.SetParent(transform, false);
                tile.transform.name = $"[{y}, {x}]";
                tile.transform.localPosition = new Vector3((x - centerX) * tileWidth, (y - centerY) * tileHeight, 0f);
                GridObjects[y, x] = tile;
                TileImages[y, x] = tile.GetComponent<RawImage>();
                TileRects[y, x] = tile.GetComponent<RectTransform>();

                UpdateTileAppearance(new Vector2Int(x, y), mazeGenerator.Grid[y, x] == 0 ? Appearance.Empty : Appearance.Wall);
            }
        }

        UpdateTileAppearance(mazeGenerator.Entry, Appearance.Socket);
        UpdateTileAppearance(mazeGenerator.Exit, Appearance.Exit);
        ResetPath();
    }

    public void ResetPath()
    {
        visitedPath = new List<Vector2Int>();
        RefreshPathVisuals();
    }

    void Update()
    {
        if (GridObjects == null) GenerateMaze();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, Input.mousePosition, canvasCamera, out localPoint);

        int gridX = Mathf.FloorToInt((localPoint.x + tileWidth / 2f) / tileWidth + centerX);
        int gridY = Mathf.FloorToInt((localPoint.y + tileHeight / 2f) / tileHeight + centerY);

        if (gridX < 0 || gridX >= mazeGenerator.Cols || gridY < 0 || gridY >= mazeGenerator.Rows)
        {
            lastDetectedGrid = null; return;
        }

        Vector2Int currentGrid = new Vector2Int(gridX, gridY);

        if (lastDetectedGrid.HasValue && lastDetectedGrid.Value != currentGrid)
        {
            foreach (Vector2Int cell in GetCellsBetween(lastDetectedGrid.Value, currentGrid)) ProcessCell(cell);
        }
        else ProcessCell(currentGrid);

        lastDetectedGrid = currentGrid;
    }

    private IEnumerable<Vector2Int> GetCellsBetween(Vector2Int from, Vector2Int to)
    {
        int x0 = from.x, y0 = from.y, x1 = to.x, y1 = to.y;
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
        if (grid.x < 0 || grid.x >= mazeGenerator.Cols || grid.y < 0 || grid.y >= mazeGenerator.Rows) return;
        if (mazeGenerator.Grid[grid.y, grid.x] != 0) return;
        if (visitedPath.Count > 0 && visitedPath[visitedPath.Count - 1] == grid) return;

        if (visitedPath.Count == 0)
        {
            if (grid != mazeGenerator.Entry) return;
            visitedPath.Add(grid);
            RefreshPathVisuals();
            return;
        }

        Vector2Int lastCell = visitedPath[visitedPath.Count - 1];
        if (!AreTouching(grid, lastCell)) return;

        int existingIndex = visitedPath.IndexOf(grid);
        if (existingIndex >= 0)
        {
            visitedPath.RemoveRange(existingIndex + 1, visitedPath.Count - existingIndex - 1);
            RefreshPathVisuals();
            return;
        }

        visitedPath.Add(grid);
        RefreshPathVisuals();

        if (grid == mazeGenerator.Exit)
        {
            OnMazeSolved();
        }
    }

    private void RefreshPathVisuals()
    {
        for (int y = 0; y < mazeGenerator.Rows; y++)
        {
            for (int x = 0; x < mazeGenerator.Cols; x++)
            {
                if (mazeGenerator.Grid[y, x] == 0)
                    UpdateTileAppearance(new Vector2Int(x, y), Appearance.Empty);
            }
        }

        UpdateTileAppearance(mazeGenerator.Entry, Appearance.Socket);
        UpdateTileAppearance(mazeGenerator.Exit, Appearance.Exit);

        for (int i = 0; i < visitedPath.Count; i++)
        {
            Vector2Int current = visitedPath[i];

            if (i == visitedPath.Count - 1 && current != mazeGenerator.Exit && current != mazeGenerator.Entry)
            {
                UpdateTileAppearance(current, Appearance.CableHead);

                Vector2Int prev = visitedPath[i - 1];
                Vector2Int dir = current - prev;
                RectTransform rect = TileRects[current.y, current.x];

                float angle = 0f;
                if (dir.x == 1) angle = -90f;
                else if (dir.x == -1) angle = 90f;
                else if (dir.y == 1) angle = 0f;
                else if (dir.y == -1) angle = 180f;

                rect.localEulerAngles = new Vector3(0, 0, angle + cableHeadOffset);
            }
            else if (current != mazeGenerator.Entry && current != mazeGenerator.Exit)
            {
                UpdateTileAppearance(current, Appearance.Cable);
            }
        }
    }

    // YENÝLENEN KISIM: GÖREV BÝTÝNCE ARKA PLANI DA KAPATIYORUZ
    private void OnMazeSolved()
    {
        Debug.Log("Maze Solved!");

        // Eðer dýþarýdan bir backplate objesi atanmýþsa onu kapat
        if (backplateObject != null)
        {
            backplateObject.SetActive(false);
        }

        if (GridObjects != null) foreach (var go in GridObjects) if (go != null) Destroy(go);
        GridObjects = null; TileImages = null; TileRects = null; visitedPath = new List<Vector2Int>();

        // Scriptin olduðu ana paneli de kapat
        transform.gameObject.SetActive(false);
    }

    private void UpdateTileAppearance(Vector2Int cell, Appearance type)
    {
        if (TileImages[cell.y, cell.x] == null) return;
        RawImage img = TileImages[cell.y, cell.x];
        RectTransform rect = TileRects[cell.y, cell.x];

        rect.localScale = Vector3.one;
        rect.localEulerAngles = Vector3.zero;

        switch (type)
        {
            case Appearance.Socket:
                if (entrySocketTexture != null) { img.texture = entrySocketTexture; img.color = Color.white; }
                rect.localScale = new Vector3(socketScale, socketScale, 1f);
                break;
            case Appearance.Exit:
                if (exitSocketTexture != null) { img.texture = exitSocketTexture; img.color = Color.white; }
                rect.localScale = new Vector3(socketScale, socketScale, 1f);
                break;
            case Appearance.Empty:
                if (emptyChannelTexture != null) { img.texture = emptyChannelTexture; img.color = Color.white; }
                else { img.color = new Color(0, 0, 0, 0.5f); }
                break;
            case Appearance.Wall:
                if (wallTexture != null) { img.texture = wallTexture; img.color = Color.white; }
                else { img.color = new Color(0, 0, 0, 0f); }
                break;
            case Appearance.Cable:
                img.texture = null;
                img.color = cableColor;
                rect.localScale = new Vector3(overlapScale, overlapScale, 1f);
                break;
            case Appearance.CableHead:
                if (cableHeadTexture != null) { img.texture = cableHeadTexture; img.color = Color.white; }
                else { img.color = Color.red; }
                rect.localScale = new Vector3(headScale, headScale, 1f);
                break;
        }
    }

    private bool AreTouching(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) == 1;
    }
}