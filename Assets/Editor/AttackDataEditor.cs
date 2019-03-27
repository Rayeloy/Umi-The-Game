using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AttackData))]
[CanEditMultipleObjects]
public class AttackDataEditor : EditorWithSubEditors<AttackPhaseEditor, AttackPhaseData>
{
    private AttackData attackData;

    SerializedProperty attackName;

    SerializedProperty hasChargingPhase;
    SerializedProperty chargingPhase;
    SerializedProperty startupPhase;
    SerializedProperty activePhase;
    SerializedProperty recoveryPhase;
    AttackPhaseData[] attackPhases = new AttackPhaseData[0];

    SerializedProperty comboDifferentAttackPercent;

    SerializedProperty stunTime;
    SerializedProperty knockbackSpeed;
    SerializedProperty knockbackType;
    SerializedProperty knockbackDirection;

    //bool chargingToggle = false;

    private void OnEnable()
    {  
        attackData = (AttackData)target;
        attackName = serializedObject.FindProperty("attackName");

        hasChargingPhase = serializedObject.FindProperty("hasChargingPhase");

        chargingPhase = serializedObject.FindProperty("chargingPhase");
        startupPhase = serializedObject.FindProperty("startupPhase");
        activePhase = serializedObject.FindProperty("activePhase");
        recoveryPhase = serializedObject.FindProperty("recoveryPhase");

        comboDifferentAttackPercent = serializedObject.FindProperty("comboDifferentAttackPercent");

        stunTime = serializedObject.FindProperty("stunTime");
        knockbackSpeed = serializedObject.FindProperty("knockbackSpeed");
        knockbackType = serializedObject.FindProperty("knockbackType");
        knockbackDirection = serializedObject.FindProperty("knockbackDirection");
        /*
        Debug.Log("0 - attackPhases.Length = " + attackPhases.Length+"; subEditors.Length ="+subEditors.Length);
        CheckAndCreateSubEditors(ref attackPhases);
        Debug.Log("1 - attackPhases.Length = " + attackPhases.Length + "; subEditors.Length =" + subEditors.Length);
        */
    }

    private void OnDisable()
    {
        Debug.Log("2 - attackPhases.Length = " + attackPhases.Length + "; subEditors.Length =" + subEditors.Length);
        CleanupEditors();
    }

    void InitializeArrays()
    {
        if(attackPhases == null)
        attackPhases = new AttackPhaseData[0];

        if (subEditors == null)
        {

        }
    }

    protected override void SubEditorSetup(AttackPhaseEditor editor)
    {

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        /*Debug.Log("1.5 - attackPhases.Length = " + attackPhases.Length + "; subEditors.Length =" + subEditors.Length);
        CheckAndCreateSubEditors(ref attackPhases);
        //base.OnInspectorGUI(); 
        serializedObject.Update();

        //CheckAndCreateSubEditors(attackPhases);

        EditorGUILayout.PropertyField(attackName);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Has Charging phase", GUILayout.Width(120));
        hasChargingPhase.boolValue = EditorGUILayout.Toggle(hasChargingPhase.boolValue);
        GUILayout.EndHorizontal();

        for (int i = 0; i < subEditors.Length; i++)
        {
            if (i == 0 && !hasChargingPhase.boolValue)
                continue;

            subEditors[i].OnInspectorGUI();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(comboDifferentAttackPercent);

        EditorGUILayout.PropertyField(stunTime);
        EditorGUILayout.PropertyField(knockbackSpeed);
        EditorGUILayout.PropertyField(knockbackType);
        if (knockbackType.intValue == 2)
            EditorGUILayout.PropertyField(knockbackDirection);

        serializedObject.ApplyModifiedProperties();
        //EditorUtility.SetDirty(target);
        */
    }

    private bool FillAttackPhasesArray(ref AttackPhaseData[] subEditorTargets)
    {
        /* Debug.Log("1- AttackPhasesArray has length = " + subEditorTargets.Length);
         if (subEditorTargets.Length == 0)
         {
             if (attackData.chargingPhase == null)
             {
                 Debug.Log("chargingPhase created");
                 attackData.chargingPhase = CreateInstance(typeof(AttackPhaseData)) as AttackPhaseData;
                 //attackData.chargingPhase = new AttackPhase();
             }
             if (attackData.startupPhase == null)
             {
                 Debug.Log("startupPhase created");
                 attackData.startupPhase = CreateInstance(typeof(AttackPhaseData)) as AttackPhaseData;
                 //attackData.startupPhase = new AttackPhase();
             }
             if (attackData.activePhase == null)
             {
                 Debug.Log("activePhase created");
                 attackData.activePhase = CreateInstance(typeof(AttackPhaseData)) as AttackPhaseData;
                 //attackData.activePhase = new AttackPhase();
             }
             if (attackData.recoveryPhase == null)
             {
                 Debug.Log("recoveryPhase created");
                 attackData.recoveryPhase = CreateInstance(typeof(AttackPhaseData)) as AttackPhaseData;
                 //attackData.recoveryPhase = new AttackPhase();
             }

             subEditorTargets = new AttackPhaseData[4];
             //To create new scriptableObjects you must use "CreateInstance(Type type)" instead of new scriptableObjectClass(,,);
             subEditorTargets[0] = attackData.chargingPhase;
             subEditorTargets[1] = attackData.startupPhase;
             subEditorTargets[2] = attackData.activePhase;
             subEditorTargets[3] = attackData.recoveryPhase;
             return true;
         }
         else
         {
             return false;
         }*/
        return false;
    }

    protected override void CheckAndCreateSubEditors(ref AttackPhaseData[] subEditorTargets)
    {
        /*if(FillAttackPhasesArray(ref subEditorTargets))
        {
            Debug.Log("2- AttackPhasesArray has length = " + subEditorTargets.Length);
            base.CheckAndCreateSubEditors(ref subEditorTargets);
            Debug.Log("CheckAndCreateSubEditors ends");
            if (subEditors.Length == 4)
            {
                Debug.Log("attackData = " + attackData.ToString() + "; attackData.chargingPhase = " + attackData.chargingPhase.ToString());
                subEditors[0].SetUpEditorData("Charging phase", attackData.chargingPhase);
                if (hasChargingPhase.boolValue)
                {

                }
                subEditors[1].SetUpEditorData("Startup phase", attackData.startupPhase);
                subEditors[2].SetUpEditorData("Active phase", attackData.activePhase);
                subEditors[3].SetUpEditorData("Recovery phase", attackData.recoveryPhase);
            }
            else
            {
                Debug.LogError("Error: There should be 4 attack phases, but there are " + subEditors.Length);
            }
        }*/
    }
}

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

