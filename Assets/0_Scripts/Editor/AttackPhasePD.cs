using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AttackPhase))]
public class AttackPhasePD : PropertyDrawer
{
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
        /*Debug.Log("I'm being executed now; Target = " + target.ToString() + "; Application.isPlaying = " + Application.isPlaying);
        if (target == null) return;
        myAttackPhase = (AttackPhaseData)target;

        isFoldedInEditor = serializedObject.FindProperty("isFoldedInEditor");

        duration = serializedObject.FindProperty("duration");
        restrictRotation = serializedObject.FindProperty("restrictRotation");
        rotationSpeed = serializedObject.FindProperty("rotationSpeed");
        hasHitbox = serializedObject.FindProperty("hasHitbox");
        hitboxPrefab = serializedObject.FindProperty("hitboxPrefab");
        */
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.BeginProperty(position, label, property);
        //EditorGUI.indentLevel++;

        /*EditorGUILayout.BeginHorizontal();

        isFoldedInEditor.boolValue = EditorGUILayout.Foldout(isFoldedInEditor.boolValue, "Test Phase");

        EditorGUILayout.EndHorizontal();

        if (isFoldedInEditor.boolValue)
        {
            DrawPhase();
        }*/


        //base.OnGUI(position, property, label);



        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        Rect durationRect = new Rect(position.x, position.y, 50, position.height);
        Rect restrictRotationRect = new Rect(position.x, position.y+15, 50, position.height);
        Rect rotationSpeedRect = new Rect(position.x, position.y+30, 50, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(durationRect, property.FindPropertyRelative("duration"));
        EditorGUI.PropertyField(restrictRotationRect, property.FindPropertyRelative("restrictRotation"));
        EditorGUI.PropertyField(rotationSpeedRect, property.FindPropertyRelative("rotationSpeed"));

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;


        //EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
        EditorGUILayout.EndVertical();
    }

    protected virtual void DrawPhase()
    {/*
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
        */
    }
    
}


/*
 * [CustomPropertyDrawer( typeof( Test ) )]
public class Ed : PropertyDrawer {
 
    public override float GetPropertyHeight ( SerializedProperty property, GUIContent label ) {
        return base.GetPropertyHeight( property, label );
    }
 
    public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
        GUI.Label( position, "This is a Test" );
    }
 
}
*/