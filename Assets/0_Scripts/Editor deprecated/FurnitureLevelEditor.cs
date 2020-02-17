using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FurnitureLevel))]
[CanEditMultipleObjects]
public class FurnitureLevelEditor : Editor
{
    SerializedProperty row1;
    SerializedProperty row2;
    SerializedProperty row3;

    private void OnEnable()
    {
        // Setup the SerializedProperties.
        row1 = serializedObject.FindProperty("row1");
        row2 = serializedObject.FindProperty("row2");
        row3 = serializedObject.FindProperty("row3");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Show the custom GUI controls.
        for(int i=0; i< row1.arraySize; i++)
        {
        }

        serializedObject.ApplyModifiedProperties();
    }
}
