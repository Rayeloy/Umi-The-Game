using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(HouseSpace))]
public class HouseSpacePD : PropertyDrawer
{
    float padding = 15;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 1;
        SerializedProperty houseLevels = property.FindPropertyRelative("houseLevels");
        for (int k = 0; k < houseLevels.arraySize; k++)//For every house level(height)
        {
            lines++;
            SerializedProperty houseLevelRows = houseLevels.GetArrayElementAtIndex(k).FindPropertyRelative("houseLevelRows");
            for (int i = 0; i < houseLevelRows.arraySize; i++)//For every house level's row
            {
                lines++;
            }
        }
        return property.isExpanded ? EditorGUIUtility.singleLineHeight * (lines) : EditorGUIUtility.singleLineHeight;
    }
    public override void OnGUI(Rect container, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(container, label, property);
        container.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(container, property.isExpanded, label);
        if (property.isExpanded)
        {
            SerializedProperty houseLevels = property.FindPropertyRelative("houseLevels");
            float currentHeight = container.y;
            for (int k = 0; k < houseLevels.arraySize; k++)//For every house level(height)
            {
                currentHeight += EditorGUIUtility.singleLineHeight;
                Rect levelRect = new Rect(container.x + 50, currentHeight, 100, 25);
                EditorGUI.LabelField(levelRect, "Level " + (k + 1));
                SerializedProperty houseLevelRows = houseLevels.GetArrayElementAtIndex(k).FindPropertyRelative("houseLevelRows");
                for (int i = 0; i < houseLevelRows.arraySize; i++)//For every house level's row
                {
                    currentHeight += EditorGUIUtility.singleLineHeight;
                    Rect rowRect = new Rect(container.x, currentHeight, 100, 25);
                    EditorGUI.LabelField(rowRect, "Row " + (i + 1));
                    SerializedProperty currentRow = houseLevelRows.GetArrayElementAtIndex(i).FindPropertyRelative("row");
                    for (int j = 0; j < currentRow.arraySize; j++)//
                    {
                        SerializedProperty auxBool = currentRow.GetArrayElementAtIndex(j);
                        Rect boolRect = new Rect(rowRect.x + 100 + (j* padding), currentHeight, 15, 15);
                        auxBool.boolValue = EditorGUI.Toggle(boolRect, auxBool.boolValue);
                    }
                }
            }

        }
        EditorGUI.EndProperty();
    }
}
