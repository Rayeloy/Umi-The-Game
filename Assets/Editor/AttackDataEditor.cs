using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AttackData))]
[CanEditMultipleObjects]
public class AttackDataEditor : Editor
{
    SerializedProperty attackName;
    SerializedProperty chargingPhase;
    SerializedProperty startupPhase;
    SerializedProperty activePhase;
    SerializedProperty recoveryPhase;

    SerializedProperty comboDifferentAttackPercent;

    SerializedProperty stunTime;
    SerializedProperty knockbackSpeed;
    SerializedProperty knockbackType;
    SerializedProperty knockbackDirection;


    AttackData myScript = null;
    bool chargingToggle = false;

    private void OnEnable()
    {
        myScript = (AttackData)target;
        attackName = serializedObject.FindProperty("attackName");

        chargingPhase = serializedObject.FindProperty("chargingPhase");
        startupPhase = serializedObject.FindProperty("startupPhase");
        activePhase = serializedObject.FindProperty("activePhase");
        recoveryPhase = serializedObject.FindProperty("recoveryPhase");

        comboDifferentAttackPercent = serializedObject.FindProperty("comboDifferentAttackPercent");

        stunTime = serializedObject.FindProperty("stunTime");
        knockbackSpeed = serializedObject.FindProperty("knockbackSpeed");
        knockbackType = serializedObject.FindProperty("knockbackType");
        knockbackDirection = serializedObject.FindProperty("knockbackDirection");

    }
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI(); 
        serializedObject.Update();
        EditorGUILayout.PropertyField(attackName);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Has Charging phase", GUILayout.Width(120));
        chargingToggle = EditorGUILayout.Toggle(chargingToggle);
        GUILayout.EndHorizontal();
        if (chargingToggle)
        {
            EditorGUILayout.PropertyField(chargingPhase);
        }

        EditorGUILayout.PropertyField(startupPhase);
        EditorGUILayout.PropertyField(activePhase);
        EditorGUILayout.PropertyField(recoveryPhase);

        EditorGUILayout.PropertyField(comboDifferentAttackPercent);

        EditorGUILayout.PropertyField(stunTime);
        EditorGUILayout.PropertyField(knockbackSpeed);
        EditorGUILayout.PropertyField(knockbackType);
        //if(knockbackType  == KnockbackType.customDir)
        EditorGUILayout.PropertyField(knockbackDirection);

        serializedObject.ApplyModifiedProperties();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("charging", GUILayout.Width(70));
        //chargingToggle = EditorGUILayout.Toggle(chargingToggle);
        //GUILayout.EndHorizontal();
        //if (chargingToggle)
        //{
        //    //GUILayout.Space(5);
        //    //GUILayout.BeginHorizontal();
        //    //GUILayout.Label("This Var", GUILayout.Width(70));
        //    //_evCtrl.ThisVar = EditorGUILayout.TextField(_evCtrl.ThisVar);
        //    //GUILayout.EndHorizontal();
        //    //GUILayout.Space(5);
        //    //GUILayout.BeginHorizontal();
        //    //GUILayout.Label("And This One", GUILayout.Width(70));
        //    //_evCtrl.AndThisOne = EditorGUILayout.TextField(_evCtrl.AndThisOne);
        //    //GUILayout.EndHorizontal();
        //    //GUILayout.Space(5);
        //    //GUILayout.BeginHorizontal();
        //    //GUILayout.Label("And This Can Be Slider", GUILayout.Width(70));
        //    //_evCtrl.AndThisCanBeSlider = EditorGUILayout.Slider(_evCtrl.AndThisCanBeSlider, 0f, 100f);
        //    //GUILayout.EndHorizontal();
        //}

        //myScript.chargingPhase.restrictRotation = GUILayout.Toggle(myScript.flag, "Flag");

        //if (myScript.flag)
        //    myScript.i = EditorGUILayout.IntSlider("I field:", myScript.i, 0, 100000);

    }
}
