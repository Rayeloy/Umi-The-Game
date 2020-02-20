using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(HouseSpace))]
public class HouseSpacePD : PropertyDrawer
{
    float padding = 16;
    bool[,] rowsInstaSwitch;
    bool[,] columnsInstaSwitch;
    bool[] levelInstaSwitch;
    bool[] levelFoldout;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 2;
        SerializedProperty houseLevels = property.FindPropertyRelative("houseLevels");
        for (int k = 0; k < houseLevels.arraySize; k++)//For every house level(height)
        {
            lines += 2;//space for Level label and instant column switches
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
            int height = property.FindPropertyRelative("height").intValue;
            int width = property.FindPropertyRelative("width").intValue;
            int depth = property.FindPropertyRelative("depth").intValue;

            if (levelFoldout == null)
            {
                levelFoldout = new bool[height];
            }
            else if(levelFoldout.Length!= height)
            {
                bool[] levelFoldoutCopy = levelFoldout;
                levelFoldout = new bool[height];
                for (int i = 0; i < levelFoldout.Length; i++)
                {
                    levelFoldout[i] = levelFoldoutCopy[i];
                }

            }

            if (rowsInstaSwitch != null && columnsInstaSwitch != null && levelInstaSwitch != null && rowsInstaSwitch.GetLength(0) == height && columnsInstaSwitch.GetLength(0) == height &&
                levelInstaSwitch.Length == height && rowsInstaSwitch.GetLength(1) == depth && columnsInstaSwitch.GetLength(1) == width)
            {
                //CHECK FOR switches
                for (int k = 0; k < height; k++)
                {
                    SerializedProperty houseLevelRows = houseLevels.GetArrayElementAtIndex(k).FindPropertyRelative("houseLevelRows");
                    //COLUMNS
                    for (int l = 0; l < width; l++)
                    {
                        if (columnsInstaSwitch[k, l])
                        {
                            bool prevValue = houseLevelRows.GetArrayElementAtIndex(0).FindPropertyRelative("row").GetArrayElementAtIndex(l).boolValue;
                            for (int i = 0; i < depth; i++)// for every row
                            {
                                SerializedProperty auxBool = houseLevelRows.GetArrayElementAtIndex(i).FindPropertyRelative("row").GetArrayElementAtIndex(l);
                                auxBool.boolValue = !prevValue;
                            }
                        }
                    }

                    //ROWS
                    for (int m = 0; m < depth; m++)
                    {
                        if (rowsInstaSwitch[k, m])
                        {
                            SerializedProperty row = houseLevels.GetArrayElementAtIndex(k).FindPropertyRelative("houseLevelRows").GetArrayElementAtIndex(m).FindPropertyRelative("row");
                            bool prevValue = row.GetArrayElementAtIndex(0).boolValue;
                            for (int i = 0; i < width; i++)// for every column
                            {
                                SerializedProperty auxBool = row.GetArrayElementAtIndex(i);
                                auxBool.boolValue = !prevValue;
                            }
                        }
                    }

                    //WHOLE LEVEL
                    if (levelInstaSwitch[k])
                    {
                        bool prevValue = houseLevelRows.GetArrayElementAtIndex(0).FindPropertyRelative("row").GetArrayElementAtIndex(0).boolValue;
                        for (int i = 0; i < depth; i++)//for every row
                        {
                            for (int j = 0; j < width; j++)//for every column
                            {
                                SerializedProperty row = houseLevels.GetArrayElementAtIndex(k).FindPropertyRelative("houseLevelRows").GetArrayElementAtIndex(i).FindPropertyRelative("row");
                                SerializedProperty auxBool = row.GetArrayElementAtIndex(j);
                                auxBool.boolValue = !prevValue;
                            }
                        }
                    }
                }
            }

            //Initialize new insantSwitches
            if (height > 0 && depth > 0 && width > 0)
            {
                rowsInstaSwitch = new bool[height, depth];
                columnsInstaSwitch = new bool[height, width];
                levelInstaSwitch = new bool[height];
            }

            float currentHeight = container.y;
            for (int k = 0; k < houseLevels.arraySize; k++)//For every house level(height)
            {
                currentHeight += EditorGUIUtility.singleLineHeight;
                Rect levelFoldoutRect = new Rect(container.x+25, currentHeight, container.width, EditorGUIUtility.singleLineHeight);
                levelFoldout[k] = EditorGUI.Foldout(levelFoldoutRect, levelFoldout[k], "Level " + (k + 1));
                if (levelFoldout[k])
                {
                    //Rect levelRect = new Rect(container.x + 50, currentHeight, 100, 25);
                    //EditorGUI.LabelField(levelRect, "Level " + (k + 1));

                    //INSTANT COLUMN SWITCHES
                    currentHeight += EditorGUIUtility.singleLineHeight;
                    for (int l = 0; l < columnsInstaSwitch.GetLength(1); l++)
                    {
                        Rect switchRect = new Rect(container.x + 100 + (l * padding), currentHeight, 15, 15);
                        columnsInstaSwitch[k, l] = EditorGUI.Toggle(switchRect, columnsInstaSwitch[k, l]);
                        if (l == columnsInstaSwitch.GetLength(1) - 1)
                        {
                            Rect levelSwitchRect = new Rect(container.x + 100 + (l * padding) + (padding * 1.5f), currentHeight, 15, 15);
                            levelInstaSwitch[k] = EditorGUI.Toggle(levelSwitchRect, levelInstaSwitch[k]);
                        }
                    }

                    SerializedProperty houseLevelRows = houseLevels.GetArrayElementAtIndex(k).FindPropertyRelative("houseLevelRows");
                    for (int i = 0; i < houseLevelRows.arraySize; i++)//For every house level's row
                    {
                        currentHeight += i == 0 ? EditorGUIUtility.singleLineHeight * 1.5f : EditorGUIUtility.singleLineHeight;
                        Rect rowRect = new Rect(container.x, currentHeight, 100, 25);
                        EditorGUI.LabelField(rowRect, "Row " + (i + 1));
                        SerializedProperty currentRow = houseLevelRows.GetArrayElementAtIndex(i).FindPropertyRelative("row");
                        for (int j = 0; j < currentRow.arraySize; j++)//
                        {
                            SerializedProperty auxBool = currentRow.GetArrayElementAtIndex(j);
                            Rect boolRect = new Rect(rowRect.x + 100 + (j * padding), currentHeight, 15, 15);
                            auxBool.boolValue = EditorGUI.Toggle(boolRect, auxBool.boolValue);

                            if (j == currentRow.arraySize - 1)
                            {
                                Rect switchRect = new Rect(boolRect.x + padding * 1.5f, currentHeight, 15, 15);
                                rowsInstaSwitch[k, i] = EditorGUI.Toggle(switchRect, rowsInstaSwitch[k, i]);
                            }
                        }
                    }
                }            
            }
        }
        EditorGUI.EndProperty();
    }
}
