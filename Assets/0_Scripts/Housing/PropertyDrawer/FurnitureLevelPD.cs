using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FurnitureLevel))]
public class FurnitureLevelPD : PropertyDrawer
{
    float padding = 15;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return property.isExpanded? EditorGUIUtility.singleLineHeight * (property.FindPropertyRelative("spaces").arraySize +1) : EditorGUIUtility.singleLineHeight;
    }
    public override void OnGUI(Rect container, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(container, label, property);
        container.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(container, property.isExpanded, label);
        if (property.isExpanded)
        {
            SerializedProperty spaces = property.FindPropertyRelative("spaces");
            //SerializedProperty rows = property.FindPropertyRelative("rows");
            //SerializedProperty columns = property.FindPropertyRelative("columns");

            for (int i = 0; i < spaces.arraySize; i++)
            {
                Rect rowRect = new Rect(container.x, container.y + EditorGUIUtility.singleLineHeight * (i+1), 100, 25);
                EditorGUI.LabelField(rowRect, "Row "+(i+1));
                SerializedProperty currentRow = spaces.GetArrayElementAtIndex(i).FindPropertyRelative("row");
                for (int j = 0; j < currentRow.arraySize; j++)
                {
                    SerializedProperty auxBool = currentRow.GetArrayElementAtIndex(j);
                    Rect boolRect = new Rect(rowRect.x + 100 + (padding * j), rowRect.y, 30, 15);
                    auxBool.boolValue = EditorGUI.Toggle(boolRect, auxBool.boolValue);                //auxBool.boolValue = EditorGUILayout.Toggle(auxBool.boolValue);
                }
            }


           
        }
        EditorGUI.EndFoldoutHeaderGroup();
        EditorGUI.EndProperty();
    }
}
