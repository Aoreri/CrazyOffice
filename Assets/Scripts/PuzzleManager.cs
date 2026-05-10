using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static Puzzle[] puzzles;

    [SerializeField] private Puzzle[] puzzleArray;


    void Awake()
    {
        puzzles = puzzleArray;
    }

    public static void StartPuzzle(string name)
    {
        for(int i = 0; i < puzzles.Length; i++)
        {
            if (puzzles[i].puzzleName.Equals(name))
            {
                puzzles[i].StartPuzzle();
                break;
            }
        }
    }
}
