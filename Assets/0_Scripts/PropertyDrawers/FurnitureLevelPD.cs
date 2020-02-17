using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FurnitureLevel))]
public class FurnitureLevelPD : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 100;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
        if (property.isExpanded)
        {
            SerializedProperty row1 = property.FindPropertyRelative("row1");
            //Rect row1Rect = EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Row 1");
            //float offset1 = 0;
            //for (int i = 0; i < row1.arraySize; i++)
            //{
            //    SerializedProperty auxBool = row1.GetArrayElementAtIndex(i);
            //    Rect boolRect = new Rect(row1Rect.position.x + 100 + offset1, row1Rect.position.y, 4, 4);
            //    EditorGUI.PropertyField(boolRect, auxBool, GUIContent.none);
            //    //auxBool.boolValue = EditorGUILayout.Toggle(auxBool.boolValue);
            //    offset1 += 15;
            //}
            //EditorGUILayout.PropertyField(row1);
            //EditorGUILayout.EndHorizontal();

            //SerializedProperty row2 = property.FindPropertyRelative("row2");
            //Rect row2Rect = EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Row 2");
            //float offset2 = 0;
            //for (int i = 0; i < row1.arraySize; i++)
            //{
            //    SerializedProperty auxBool = row2.GetArrayElementAtIndex(i);
            //    Rect boolRect = new Rect(row2Rect.position.x + 100 + offset2, row2Rect.position.y, 4, 4);
            //    EditorGUI.PropertyField(boolRect, auxBool, GUIContent.none);
            //    //auxBool.boolValue = EditorGUILayout.Toggle(auxBool.boolValue);
            //    offset2 += 15;
            //}
            ////EditorGUILayout.PropertyField(row1);
            //EditorGUILayout.EndHorizontal();

            //SerializedProperty row3 = property.FindPropertyRelative("row3");
            //Rect row3Rect = EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Row 3");
            //float offset3 = 0;
            //for (int i = 0; i < row1.arraySize; i++)
            //{
            //    SerializedProperty auxBool = row3.GetArrayElementAtIndex(i);
            //    Rect boolRect = new Rect(row3Rect.position.x + 100 + offset3, row3Rect.position.y, 4, 4);
            //    EditorGUI.PropertyField(boolRect, auxBool, GUIContent.none);
            //    //auxBool.boolValue = EditorGUILayout.Toggle(auxBool.boolValue);
            //    offset3 += 15;
            //}
            ////EditorGUILayout.PropertyField(row1);
            //EditorGUILayout.EndHorizontal();
        }
        EditorGUI.EndFoldoutHeaderGroup();
        EditorGUI.EndProperty();
    }
}
