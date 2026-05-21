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

    // Runtime state (do not modify the inspector fields, as they will be lost on second play)
    private Jackpot[] runtimeColumns;
    private Image runtimeReference;

    private void HandleColumnFinished()
    {
        if (runtimeColumns == null) return;
        foreach (Jackpot col in runtimeColumns)
        {
            if (col != null && col.isSpinning) return; // Wait until all are stopped
        }
        CheckForMatch();
    }

    private void CheckForMatch()
    {
        if (runtimeColumns == null || runtimeColumns.Length == 0 || targetSymbolPrefab == null) return;

        // The GameManager chooses the target name
        string targetName = targetSymbolPrefab.name;
        bool isJackpot = true;

        // Check if ALL columns match the GameManager's chosen target
        for (int i = 0; i < runtimeColumns.Length; i++)
        {
            if (runtimeColumns[i] == null || runtimeColumns[i].CurrentItems == null || runtimeColumns[i].CurrentItems.Count <= winRowIndex)
            {
                isJackpot = false;
                break;
            }

            // We use .Replace to safely ignore the "(Clone)" text Unity adds
            string currentItemName = runtimeColumns[i].CurrentItems[winRowIndex].gameObject.name.Replace("(Clone)", "").Trim();

            if (currentItemName != targetName)
            {
                isJackpot = false;
                break;
            }
        }

        if (isJackpot)
        {
            EndPuzzle();
        }
    }

    protected override void OnStartPuzzle()
    {
        // 1. Find the runtime instance dynamically
        GameObject runtimeInstance = null;
        if (prefab != null)
        {
            // First try reflection to get the exact instance created by Puzzle.cs
            System.Reflection.FieldInfo field = typeof(Puzzle).GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                runtimeInstance = field.GetValue(this) as GameObject;
            }

            // Fallback: search by name
            if (runtimeInstance == null)
            {
                GameObject canvasObj = GameObject.FindGameObjectWithTag("UI");
                if (canvasObj != null)
                {
                    Transform cloneTransform = canvasObj.transform.Find(prefab.name + "(Clone)");
                    if (cloneTransform != null) runtimeInstance = cloneTransform.gameObject;
                }
            }
        }

        // Initialize runtime state without overwriting inspector variables
        if (columns != null)
        {
            runtimeColumns = new Jackpot[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                runtimeColumns[i] = GetRuntimeClone(columns[i], runtimeInstance);
            }
        }

        runtimeReference = GetRuntimeClone(referance, runtimeInstance);

        // Wire events and initialize on the runtime columns
        if (runtimeColumns != null)
        {
            foreach (Jackpot col in runtimeColumns)
            {
                if (col != null)
                {
                    col.OnSpinComplete -= HandleColumnFinished;
                    col.OnSpinComplete += HandleColumnFinished;
                    col.Initialize();
                }
            }
        }

        if (runtimeColumns != null && runtimeColumns.Length > 0 && runtimeColumns[0] != null && runtimeColumns[0].itemPrefabs != null && runtimeColumns[0].itemPrefabs.Length > 0)
        {
            targetSymbolPrefab = runtimeColumns[0].itemPrefabs[UnityEngine.Random.Range(0, runtimeColumns[0].itemPrefabs.Length)];
            if (runtimeReference != null && targetSymbolPrefab != null)
            {
                Image targetImage = targetSymbolPrefab.GetComponent<Image>();
                if (targetImage != null)
                {
                    runtimeReference.sprite = targetImage.sprite;
                }
                RectTransform targetRect = targetSymbolPrefab.GetComponent<RectTransform>();
                RectTransform refRect = runtimeReference.GetComponent<RectTransform>();
                if (targetRect != null && refRect != null)
                {
                    refRect.localScale = targetRect.localScale;
                    refRect.sizeDelta = targetRect.sizeDelta;
                }
            }
        }
    }

    protected override void OnEndPuzzle()
    {
        if (runtimeColumns != null)
        {
            foreach (Jackpot col in runtimeColumns)
            {
                if (col != null) col.OnSpinComplete -= HandleColumnFinished;
            }
        }

        // Clear runtime state so it doesn't hold references to destroyed objects
        runtimeColumns = null;
        runtimeReference = null;
    }

    private T GetRuntimeClone<T>(T prefabComponent, GameObject runtimeInstance) where T : Component
    {
        if (prefabComponent == null) return null;
        if (prefab == null || runtimeInstance == null) return prefabComponent;

        // If the component's gameObject is in a valid scene (not a prefab asset), it is already a runtime clone.
        if (prefabComponent.gameObject.scene.IsValid())
        {
            return prefabComponent;
        }

        // Trace the path from the prefab component up to the prefab root
        System.Collections.Generic.List<string> path = new System.Collections.Generic.List<string>();
        Transform curr = prefabComponent.transform;
        while (curr != null && curr != prefab.transform)
        {
            path.Insert(0, curr.name);
            curr = curr.parent;
        }

        // If it wasn't a child of the prefab root, return the original component
        if (curr == null) return prefabComponent;

        // Resolve the path on the instantiated clone
        Transform target = runtimeInstance.transform;
        foreach (string name in path)
        {
            target = target.Find(name);
            if (target == null)
            {
                Debug.LogWarning($"[JackpotPuzzle] Could not find child '{name}' on cloned instance.");
                return null;
            }
        }

        return target.GetComponent<T>();
    }
}
