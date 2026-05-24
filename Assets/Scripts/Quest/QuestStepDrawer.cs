using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(QuestStep))]
public class QuestStepDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded)
        {
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("stepDescription")) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("objectiveType")) + EditorGUIUtility.standardVerticalSpacing;

            // Should Count toggle height (always visible)
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("shouldCount")) + EditorGUIUtility.standardVerticalSpacing;

            int enumValue = property.FindPropertyRelative("objectiveType").enumValueIndex;

            if (enumValue == (int)QuestObjectiveType.TalkToNPC)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("possibleNPCs"), true) + EditorGUIUtility.standardVerticalSpacing;
            }
            else if (enumValue == (int)QuestObjectiveType.SolvePuzzle)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("puzzleNames"), true) + EditorGUIUtility.standardVerticalSpacing;
            }
            else if (enumValue == (int)QuestObjectiveType.CollectItem)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("itemObjects"), true) + EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("amountRequired")) + EditorGUIUtility.standardVerticalSpacing;
            }
            else if (enumValue == (int)QuestObjectiveType.ChangeDoorState)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("doors"), true) + EditorGUIUtility.standardVerticalSpacing;
            }
            else if (enumValue == (int)QuestObjectiveType.ShowDialogue)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("target")) + EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("dialogueArea")) + EditorGUIUtility.standardVerticalSpacing;
            }
            // CustomEvent1 and CustomEvent2 do not need extra height calculations
        }
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            Rect currentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, 0);

            // Draw Description
            SerializedProperty stepDesc = property.FindPropertyRelative("stepDescription");
            currentRect.height = EditorGUI.GetPropertyHeight(stepDesc);
            EditorGUI.PropertyField(currentRect, stepDesc);
            currentRect.y += currentRect.height + EditorGUIUtility.standardVerticalSpacing;

            // Draw Objective Type
            SerializedProperty objType = property.FindPropertyRelative("objectiveType");
            currentRect.height = EditorGUI.GetPropertyHeight(objType);
            EditorGUI.PropertyField(currentRect, objType);
            currentRect.y += currentRect.height + EditorGUIUtility.standardVerticalSpacing;

            // Draw Should Count Toggle
            SerializedProperty shouldCountProp = property.FindPropertyRelative("shouldCount");
            currentRect.height = EditorGUI.GetPropertyHeight(shouldCountProp);
            EditorGUI.PropertyField(currentRect, shouldCountProp);
            currentRect.y += currentRect.height + EditorGUIUtility.standardVerticalSpacing;

            // Draw Conditional Fields
            int enumValue = objType.enumValueIndex;

            if (enumValue == (int)QuestObjectiveType.TalkToNPC)
            {
                SerializedProperty npcs = property.FindPropertyRelative("possibleNPCs");
                currentRect.height = EditorGUI.GetPropertyHeight(npcs, true);
                EditorGUI.PropertyField(currentRect, npcs, true);
            }
            else if (enumValue == (int)QuestObjectiveType.SolvePuzzle)
            {
                SerializedProperty puzzles = property.FindPropertyRelative("puzzleNames");
                currentRect.height = EditorGUI.GetPropertyHeight(puzzles, true);
                EditorGUI.PropertyField(currentRect, puzzles, true);
            }
            else if (enumValue == (int)QuestObjectiveType.CollectItem)
            {
                SerializedProperty items = property.FindPropertyRelative("itemObjects");
                currentRect.height = EditorGUI.GetPropertyHeight(items, true);
                EditorGUI.PropertyField(currentRect, items, true);
                currentRect.y += currentRect.height + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty amount = property.FindPropertyRelative("amountRequired");
                currentRect.height = EditorGUI.GetPropertyHeight(amount);
                EditorGUI.PropertyField(currentRect, amount);
            }
            else if (enumValue == (int)QuestObjectiveType.ChangeDoorState)
            {
                SerializedProperty doors = property.FindPropertyRelative("doors");
                currentRect.height = EditorGUI.GetPropertyHeight(doors, true);
                EditorGUI.PropertyField(currentRect, doors, true);
            }
            else if (enumValue == (int)QuestObjectiveType.ShowDialogue)
            {
                SerializedProperty targetProp = property.FindPropertyRelative("target");
                currentRect.height = EditorGUI.GetPropertyHeight(targetProp);
                EditorGUI.PropertyField(currentRect, targetProp);
                currentRect.y += currentRect.height + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty dialogueAreaProp = property.FindPropertyRelative("dialogueArea");
                currentRect.height = EditorGUI.GetPropertyHeight(dialogueAreaProp);
                EditorGUI.PropertyField(currentRect, dialogueAreaProp);
            }
            // CustomEvent1 and CustomEvent2 do not draw any extra fields

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }
}