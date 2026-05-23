using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public string questId;
    public string questName;
    [TextArea]
    public string description;

    public List<QuestStep> steps;
}