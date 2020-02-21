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
    bool[] copyUpSwitch;
    bool[] copyDownSwitch;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 2;
        SerializedProperty houseLevels = property.FindPropertyRelative("houseLevels");
        for (int k = 0; k < houseLevels.arraySize; k++)//For every house level(height)
        {
            if (levelFoldout != null && k < levelFoldout.Length && !levelFoldout[k])
            {
                lines++;
                continue;
            }
            lines += 4;//space for Level label, instant column switches, and copy up and copy down switches
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

            #region --- Instant Switches ---
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
                                columnsInstaSwitch[k, l] = false;
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
                                rowsInstaSwitch[k, m] = false;
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
                                levelInstaSwitch[k] = false;
                            }
                        }
                    }

                    //COPY UP SWITCHES
                    if (k > 0)
                    {
                        if (copyUpSwitch[k - 1])
                        {
                            for (int i = 0; i < width; i++)
                            {
                                for (int j = 0; j < depth; j++)
                                {
                                    SerializedProperty row = houseLevels.GetArrayElementAtIndex(k).FindPropertyRelative("houseLevelRows").GetArrayElementAtIndex(i).FindPropertyRelative("row");
                                    SerializedProperty auxBool = row.GetArrayElementAtIndex(j);
                                    SerializedProperty rowUpLevel = houseLevels.GetArrayElementAtIndex(k-1).FindPropertyRelative("houseLevelRows").GetArrayElementAtIndex(i).FindPropertyRelative("row");
                                    SerializedProperty auxBoolUpLevel = rowUpLevel.GetArrayElementAtIndex(j);
                                    auxBoolUpLevel.boolValue = auxBool.boolValue;
                                    copyUpSwitch[k-1] = false;
                                }
                            }
                        }
                    }
                    //COPY DOWN SWITCHES
                    if (k < height-1)
                    {
                        if (copyDownSwitch[k])
                        {
                            for (int i = 0; i < width; i++)
                            {
                                for (int j = 0; j < depth; j++)
                                {
                                    SerializedProperty row = houseLevels.GetArrayElementAtIndex(k).FindPropertyRelative("houseLevelRows").GetArrayElementAtIndex(i).FindPropertyRelative("row");
                                    SerializedProperty auxBool = row.GetArrayElementAtIndex(j);
                                    SerializedProperty rowDownLevel = houseLevels.GetArrayElementAtIndex(k+1).FindPropertyRelative("houseLevelRows").GetArrayElementAtIndex(i).FindPropertyRelative("row");
                                    SerializedProperty auxBoolDownLevel = rowDownLevel.GetArrayElementAtIndex(j);
                                    auxBoolDownLevel.boolValue = auxBool.boolValue;
                                    copyDownSwitch[k] = false;
                                }
                            }
                        }
                    }
                }
            }
            if(rowsInstaSwitch == null ||(rowsInstaSwitch != null && (rowsInstaSwitch.GetLength(0) != height || rowsInstaSwitch.GetLength(1) != depth)))
                rowsInstaSwitch = new bool[height, depth];
            if (columnsInstaSwitch == null || (columnsInstaSwitch != null && (columnsInstaSwitch.GetLength(0) != height || columnsInstaSwitch.GetLength(1) != width)))
                columnsInstaSwitch = new bool[height, width];
            if (levelInstaSwitch == null || (levelInstaSwitch != null && levelInstaSwitch.Length != height))
                levelInstaSwitch = new bool[height];
            if (copyDownSwitch == null || (copyDownSwitch != null && copyDownSwitch.Length != height))
                copyDownSwitch = new bool[height - 1];
            if (copyUpSwitch == null || (copyUpSwitch != null && copyUpSwitch.Length != height))
                copyUpSwitch = new bool[height - 1];
            ////Initialize new insantSwitches
            //if (height > 0 && depth > 0 && width > 0) //checking for a false value to do this would maybe reduce cpu cost?
            //{
            //    rowsInstaSwitch = new bool[height, depth];
            //    columnsInstaSwitch = new bool[height, width];
            //    levelInstaSwitch = new bool[height];
            //    copyDownSwitch = new bool[height - 1];
            //    copyUpSwitch = new bool[height - 1];
            //}
            #endregion

            #region --- DRAW LEVELS ---
            float currentHeight = container.y;
            for (int k = 0; k < houseLevels.arraySize; k++)//For every house level(height)
            {
                currentHeight += EditorGUIUtility.singleLineHeight;
                float currentX = container.x +20;
                Rect levelFoldoutRect = new Rect(currentX, currentHeight, container.width, EditorGUIUtility.singleLineHeight);
                levelFoldout[k] = EditorGUI.Foldout(levelFoldoutRect, levelFoldout[k], "Level " + (k + 1));
                if (levelFoldout[k])
                {
                    currentHeight += EditorGUIUtility.singleLineHeight;
                    currentX += 5;

                    //COPY UP SWITCH
                    if (k > 0)
                    {
                        EditorGUI.LabelField(new Rect(currentX + 80,currentHeight, container.width, EditorGUIUtility.singleLineHeight) ,"Copy up");
                        copyUpSwitch[k-1] =  EditorGUI.Toggle(new Rect(currentX + 150, currentHeight, 15, 15), copyUpSwitch[k - 1]);
                        currentHeight += EditorGUIUtility.singleLineHeight;
                    }

                    //INSTANT COLUMN SWITCHES
                    for (int l = 0; l < columnsInstaSwitch.GetLength(1); l++)
                    {

                        Rect switchRect = new Rect(currentX + 50 + (l * padding), currentHeight, 15, 15);
                        columnsInstaSwitch[k, l] = EditorGUI.Toggle(switchRect, columnsInstaSwitch[k, l]);
                        if (l == columnsInstaSwitch.GetLength(1) - 1)
                        {
                            Rect levelSwitchRect = new Rect(currentX + 50 + (l * padding) + (padding * 1.5f), currentHeight, 15, 15);
                            levelInstaSwitch[k] = EditorGUI.Toggle(levelSwitchRect, levelInstaSwitch[k]);
                        }
                    }

                    SerializedProperty houseLevelRows = houseLevels.GetArrayElementAtIndex(k).FindPropertyRelative("houseLevelRows");
                    for (int i = 0; i < houseLevelRows.arraySize; i++)//For every house level's row
                    {
                        currentHeight += i == 0 ? EditorGUIUtility.singleLineHeight * 1.5f : EditorGUIUtility.singleLineHeight;
                        Rect rowRect = new Rect(currentX, currentHeight, 100, 25);
                        EditorGUI.LabelField(rowRect, "Row " + (i + 1));
                        SerializedProperty currentRow = houseLevelRows.GetArrayElementAtIndex(i).FindPropertyRelative("row");
                        for (int j = 0; j < currentRow.arraySize; j++)//
                        {
                            SerializedProperty auxBool = currentRow.GetArrayElementAtIndex(j);
                            Rect boolRect = new Rect(currentX + 50 + (j * padding), currentHeight, 15, 15);
                            auxBool.boolValue = EditorGUI.Toggle(boolRect, auxBool.boolValue);

                            if (j == currentRow.arraySize - 1)
                            {
                                Rect switchRect = new Rect(boolRect.x + padding * 1.5f, currentHeight, 15, 15);
                                rowsInstaSwitch[k, i] = EditorGUI.Toggle(switchRect, rowsInstaSwitch[k, i]);
                            }
                        }
                        if(i== width - 1)
                        {
                            currentHeight += EditorGUIUtility.singleLineHeight;
                            //COPY DOWN SWITCH
                            if (k < height - 1)
                            {
                                EditorGUI.LabelField(new Rect(currentX + 80, currentHeight, container.width, EditorGUIUtility.singleLineHeight), "Copy down");
                                copyDownSwitch[k] = EditorGUI.Toggle(new Rect(currentX + 150, currentHeight, 15, 15), copyDownSwitch[k]);
                            }
                        }
                    }
                }            
            }
            #endregion
        }
        EditorGUI.EndProperty();
    }
}
