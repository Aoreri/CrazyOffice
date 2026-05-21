using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrinterScript : Puzzle
{
    [Header("Animation Settings")]
    [SerializeField] private float fillAnimationDuration = 0.25f; // How long the initial fill takes

    private TankFill[] allTanks;
    private bool isFinished = false;
    private bool isSpawning = false; // Blocks completion checks during the start animation

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
        int numEmpty = Random.Range(1, 3);
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
            availableIndices.RemoveAt(randomIndex);
        }

        // 5. Start the animated fill process
        StartCoroutine(AnimateInitialFill(emptyIndices));
    }

    private IEnumerator AnimateInitialFill(List<int> emptyIndices)
    {
        isSpawning = true;
        float elapsedTime = 0f;

        // Array to hold the target fill goals for each tank
        float[] targetFills = new float[allTanks.Length];

        // First, set EVERY tank to minimum (empty) visually
        for (int i = 0; i < allTanks.Length; i++)
        {
            TankFill tank = allTanks[i];

            // Determine if this tank is supposed to be full or empty at the end
            targetFills[i] = emptyIndices.Contains(i) ? tank.minFill : tank.maxFill;

            // Force visual to empty to start the animation
            tank.fillAmount = tank.minFill;
            tank.AddFill(0f);
        }

        // Animate the fill over time
        while (elapsedTime < fillAnimationDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate interpolation factor (0 to 1)
            float t = elapsedTime / fillAnimationDuration;

            // Add SmoothStep so the fill starts and ends nicely instead of linearly snapping
            t = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < allTanks.Length; i++)
            {
                // We only need to animate tanks that are actually supposed to fill up
                if (!emptyIndices.Contains(i))
                {
                    TankFill tank = allTanks[i];
                    tank.fillAmount = Mathf.Lerp(tank.minFill, targetFills[i], t);
                    tank.AddFill(0f); // Force visual update
                }
            }

            // Wait until next frame
            yield return null;
        }

        // Ensure all tanks are exactly at their final target values just in case
        for (int i = 0; i < allTanks.Length; i++)
        {
            TankFill tank = allTanks[i];
            tank.fillAmount = targetFills[i];
            tank.AddFill(0f);
        }

        // Allow the puzzle to be played and checked
        isSpawning = false;
    }

    void Update()
    {
        // Do not check for win condition if we are still playing the opening animation
        if (isFinished || isSpawning || allTanks == null || allTanks.Length == 0) return;

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