using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator
{
    public int[,] Grid { get; private set; }  // 0 = yol, 1 = duvar
    public int Cols { get; private set; }
    public int Rows { get; private set; }
    public Vector2Int Entry { get; private set; }
    public Vector2Int Exit { get; private set; }

    public void Generate(int cols, int rows)
    {
       
        Cols = cols % 2 == 0 ? cols + 1 : cols;
        Rows = rows % 2 == 0 ? rows + 1 : rows;

        Grid = new int[Rows, Cols];

        for (int y = 0; y < Rows; y++)
            for (int x = 0; x < Cols; x++)
                Grid[y, x] = 1;

        Carve(1, 1);

        Entry = new Vector2Int(1, 0);
        Exit = new Vector2Int(Cols - 2, Rows - 1);
        Grid[Entry.y, Entry.x] = 0;
        Grid[Exit.y, Exit.x] = 0;
    }

    void Carve(int x, int y)
    {
        Grid[y, x] = 0;

        var dirs = new List<Vector2Int>
        {
            new Vector2Int( 2,  0),
            new Vector2Int(-2,  0),
            new Vector2Int( 0,  2),
            new Vector2Int( 0, -2)
        };

        // Yönleri karıştır
        for (int i = dirs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = dirs[i]; dirs[i] = dirs[j]; dirs[j] = tmp;
        }

        foreach (var d in dirs)
        {
            int nx = x + d.x;
            int ny = y + d.y;
            if (nx > 0 && ny > 0 && nx < Cols - 1 && ny < Rows - 1 && Grid[ny, nx] == 1)
            {
                Grid[y + d.y / 2, x + d.x / 2] = 0; 
                Carve(nx, ny);
            }
        }
    }
}