using System;
using UnityEngine;
using UnityEngine.UI;

public class JackpotPuzzle : Puzzle
{
    [Header("Slot Machine Setup")]
    public Jackpot[] columns;
    public int winRowIndex = 1;

    public GameObject targetSymbolPrefab;


    public Image referance;

    void Start()
    {
        foreach (Jackpot col in columns)
        {
            if (col != null) col.OnSpinComplete += HandleColumnFinished;
        }

        targetSymbolPrefab = columns[0].itemPrefabs[UnityEngine.Random.Range(0, columns[0].itemPrefabs.Length)];
        referance.sprite = targetSymbolPrefab.GetComponent<Image>().sprite;
        referance.gameObject.GetComponent<RectTransform>().localScale = targetSymbolPrefab.GetComponent<RectTransform>().localScale;
        referance.gameObject.GetComponent<RectTransform>().sizeDelta = targetSymbolPrefab.GetComponent<RectTransform>().sizeDelta;
    }

    private void HandleColumnFinished()
    {
        foreach (Jackpot col in columns)
        {
            if (col.isSpinning) return; // Wait until all are stopped
        }
        CheckForMatch();
    }

    private void CheckForMatch()
    {
        if (columns.Length == 0 || targetSymbolPrefab == null) return;

        // The GameManager chooses the target name
        string targetName = targetSymbolPrefab.name;
        bool isJackpot = true;

        // Check if ALL columns match the GameManager's chosen target
        for (int i = 0; i < columns.Length; i++)
        {
            // We use .Replace to safely ignore the "(Clone)" text Unity adds
            string currentItemName = columns[i].CurrentItems[winRowIndex].gameObject.name.Replace("(Clone)", "").Trim();

            if (currentItemName != targetName)
            {
                isJackpot = false;
                break;
            }
        }

        if (isJackpot)
        {
            EndPuzzle();
           // Debug.Log("🎉 JACKPOT! All columns matched the Target Symbol!");

            //foreach (Jackpot col in columns)
            // {
            //    GameObject winningItem = col.CurrentItems[winRowIndex].gameObject;
            //    Destroy(winningItem);
            //    col.RemoveAndRespawnItems();
            // }
        }
        else
        {
         //   Debug.Log("Columns stopped. They did not match the target.");
        }
    }

    void OnDestroy()
    {
        foreach (Jackpot col in columns)
            if (col != null) col.OnSpinComplete -= HandleColumnFinished;
    }

    protected override void OnStartPuzzle()
    {
  
    }

    protected override void OnEndPuzzle()
    {
        
    }
}
