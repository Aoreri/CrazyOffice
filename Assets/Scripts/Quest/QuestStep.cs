using UnityEngine;

public enum QuestObjectiveType
{
    TalkToNPC,
    SolvePuzzle,
    CollectItem,
    ChangeDoorState,
    CustomEvent1,   // Added Custom Event 1
    CustomEvent2    // Added Custom Event 2
}

[System.Serializable]
public class QuestStep
{
    public string stepDescription;
    public QuestObjectiveType objectiveType;

    [Tooltip("Toggle if this step should be counted in the UI step tracker.")]
    public bool shouldCount = true;

    [Header("Talk To NPC Settings")]
    public GameObject[] possibleNPCs;

    [Header("Puzzle Settings")]
    public string[] puzzleNames;

    [Header("Collect Settings")]
    public GameObject[] itemObjects;
    public int amountRequired = 1;

    [Header("Door Settings")]
    public GameObject[] doors;
}