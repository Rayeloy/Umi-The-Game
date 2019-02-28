using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AttackPhase))]
[CanEditMultipleObjects]
public class AttackPhaseEditor : Editor
{
    public bool showPhase;

    private string phaseName = null;

    SerializedProperty duration;
    SerializedProperty restrictRotation;
    SerializedProperty rotationSpeed;
    SerializedProperty hitboxPrefab;


    //private Reaction reaction;


    //private const float buttonWidth = 30f;


    private void OnEnable()
    {
        //reaction = (Reaction)target;
        Init();
    }


    protected virtual void Init()
    {
        duration = serializedObject.FindProperty("duration");
        restrictRotation = serializedObject.FindProperty("restrictRotation");
        rotationSpeed = serializedObject.FindProperty("rotationSpeed");
        hitboxPrefab = serializedObject.FindProperty("hitboxPrefab");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();

        showPhase = EditorGUILayout.Foldout(showPhase, phaseName);

        /*if (GUILayout.Button("-", GUILayout.Width(buttonWidth)))
        {
            reactionsProperty.RemoveFromObjectArray(reaction);
        }*/
        EditorGUILayout.EndHorizontal();

        if (showPhase)
        {
            DrawPhase();
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }


    /*public static Reaction CreateReaction(Type reactionType)
    {
        return (Reaction)CreateInstance(reactionType);
    }*/


    protected virtual void DrawPhase()
    {
        Debug.Log("Draw Attack Phase inspector");
        //DrawDefaultInspector();
        EditorGUILayout.PropertyField(duration);
        EditorGUILayout.PropertyField(restrictRotation);
        EditorGUILayout.PropertyField(rotationSpeed);
        EditorGUILayout.PropertyField(hitboxPrefab);
    }

    public void SetUpEditorData(string name)
    {
        if(phaseName == null){
            phaseName = name;
        }
    }

}
