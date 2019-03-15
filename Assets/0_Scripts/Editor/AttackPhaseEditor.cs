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
    protected AttackPhase myAttackPhase;

    SerializedProperty isFoldedInEditor;

    SerializedProperty duration;
    SerializedProperty restrictRotation;
    SerializedProperty rotationSpeed;
    SerializedProperty hasHitbox;
    SerializedProperty hitboxPrefab;



    private void OnEnable()
    {
        //reaction = (Reaction)target;
        Init();
    }


    protected virtual void Init()
    {
        myAttackPhase = (AttackPhase)target;

        isFoldedInEditor = serializedObject.FindProperty("isFoldedInEditor");

        duration = serializedObject.FindProperty("duration");
        restrictRotation = serializedObject.FindProperty("restrictRotation");
        rotationSpeed = serializedObject.FindProperty("rotationSpeed");
        hasHitbox = serializedObject.FindProperty("hasHitbox");
        hitboxPrefab = serializedObject.FindProperty("hitboxPrefab");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();

        isFoldedInEditor.boolValue = EditorGUILayout.Foldout(isFoldedInEditor.boolValue, phaseName);

        /*if (GUILayout.Button("-", GUILayout.Width(buttonWidth)))
        {
            reactionsProperty.RemoveFromObjectArray(reaction);
        }*/
        EditorGUILayout.EndHorizontal();

        if (isFoldedInEditor.boolValue)
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
        //Debug.Log("Draw Attack Phase inspector");
        //DrawDefaultInspector();
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Duration", GUILayout.Width(70));
        //_evCtrl.ThisVar = EditorGUILayout.FloatField(_evCtrl.ThisVar);
        //GUILayout.EndHorizontal();
        //EditorGUILayout.LabelField("Duration",)
        EditorGUILayout.PropertyField(duration);
        EditorGUILayout.PropertyField(restrictRotation);
        if (restrictRotation.boolValue)
        {
            EditorGUILayout.PropertyField(rotationSpeed);
        }

        EditorGUILayout.BeginHorizontal();
        hasHitbox.boolValue = EditorGUILayout.Toggle(hasHitbox.boolValue, GUILayout.Width(25));
        if (hasHitbox.boolValue)
        {
            hitboxPrefab.objectReferenceValue = EditorGUILayout.ObjectField(
                new GUIContent("Hitbox Prefab", "Add the hitbox prefab"), hitboxPrefab.objectReferenceValue, typeof(GameObject), false);
        }
        else
        {
            GUILayout.Space(12);
            GUILayout.Label("Hitbox Prefab", GUILayout.Width(100));

        }
        EditorGUILayout.EndHorizontal();
    }

    public void SetUpEditorData(string name, AttackPhase attackPhase)
    {
        if(phaseName == null){
            phaseName = name;
        }
        //myAttackPhase.duration = attackPhase.duration;
        //myAttackPhase.restrictRotation = attackPhase.restrictRotation;
        //myAttackPhase.rotationSpeed = attackPhase.rotationSpeed;
        //myAttackPhase.hitboxPrefab = attackPhase.hitboxPrefab;
        //duration = serializedObject.FindProperty("duration");
        //restrictRotation = serializedObject.FindProperty("restrictRotation");
        //rotationSpeed = serializedObject.FindProperty("rotationSpeed");
        //hitboxPrefab = serializedObject.FindProperty("hitboxPrefab");
    }

}
