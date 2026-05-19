using System.Collections.Generic;
using UnityEngine;

public class PrinterScript : Puzzle
{

    private TankFill[] allTanks;
    private bool isFinished = false;

    protected override void OnEndPuzzle()
    {
   
    }

    protected override void OnStartPuzzle()
    {
       
    }

    void Start()
    {
        // 1. Find all TankFill objects active in the scene
        allTanks = FindObjectsByType<TankFill>(FindObjectsInactive.Exclude);
        if (allTanks == null || allTanks.Length == 0) return;

        // 2. Decide randomly if 1 or 2 tanks will be empty
        // Random.Range(1, 3) returns either 1 or 2 (the max value is exclusive for integers)
        int numEmpty = Random.Range(1, 3);

        // Safety check: ensure we don't try to empty more tanks than actually exist
        numEmpty = Mathf.Min(numEmpty, allTanks.Length);

        // 3. Create a list of available tank indices to pick from
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < allTanks.Length; i++)
        {
            availableIndices.Add(i);
        }

        // 4. Randomly select which indices will represent the empty tanks
        List<int> emptyIndices = new List<int>();
        for (int i = 0; i < numEmpty; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            emptyIndices.Add(availableIndices[randomIndex]);

            // Remove the chosen index so we don't pick the same tank twice
            availableIndices.RemoveAt(randomIndex);
        }

        // 5. Apply the fill amounts to the tanks
        for (int i = 0; i < allTanks.Length; i++)
        {
            TankFill tank = allTanks[i];

            if (emptyIndices.Contains(i))
            {
                // Make the chosen tanks empty (using minFill as your "zero" limit)
                tank.fillAmount = tank.minFill;
            }
            else
            {
                // Make all the other tanks completely full
                tank.fillAmount = tank.maxFill;
            }

            // Force visual update without changing your original TankFill script
            tank.AddFill(0f);
        }
    }

    void Update()
    {
        if (isFinished || allTanks == null || allTanks.Length == 0) return;

        bool allTanksFull = true;

        // Check if every tank has reached its maximum fill limit
        foreach (TankFill tank in allTanks)
        {
            if (tank.fillAmount < tank.maxFill - 0.001f)
            {
                allTanksFull = false;
                break;
            }
        }

        // If all are full, destroy the target object
        if (allTanksFull)
        {
            isFinished = true;

            EndPuzzle();
        }
    }
}
