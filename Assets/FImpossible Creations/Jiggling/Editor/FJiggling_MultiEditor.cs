using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Jiggling
{
    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(FJiggling_Multi))]
    public class FJiggling_MultiEditor : UnityEditor.Editor
    {
        private static bool showTransforms = false;

        //private SerializedProperty sp_sep;

        //private void OnEnable()
        //{
        //    sp_sep = serializedObject.FindProperty("SeparatedCalculations");
        //}

        public override void OnInspectorGUI()
        {
            FJiggling_Multi targetScript = (FJiggling_Multi)target;

            DrawDefaultInspector();

            GUILayout.Space(5);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            Color preCol = GUI.color;
            GUI.color = new Color(0.5f, 1f, 0.5f, 0.9f);

            var drop = GUILayoutUtility.GetRect(0f, 22f, new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
            GUI.Box(drop, "Drag & Drop your GameObjects here", new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter, fixedHeight = 22 });
            var dropEvent = Event.current;

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 166;
            targetScript.ShowIndividualOptions = EditorGUILayout.Toggle(new GUIContent(" Show Individual Options", "If single stimulated transforms don't have animated tracks from individual tracks like rotation or scale, also with this you can chcange intensity of effect for each element separately"), targetScript.ShowIndividualOptions);
            EditorGUIUtility.labelWidth = 0;
            EditorGUI.indentLevel++;

            if (ActiveEditorTracker.sharedTracker.isLocked) GUI.color = new Color(0.44f, 0.44f, 0.44f, 0.8f); else GUI.color = preCol;
            if (GUILayout.Button(new GUIContent("Lock Inspector", "Locking Inspector Window to help Drag & Drop operations"), new GUILayoutOption[2] { GUILayout.Width(106), GUILayout.Height(16) })) ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;

            GUI.color = preCol;

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            switch (dropEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop.Contains(dropEvent.mousePosition)) break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (dropEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var dragged in DragAndDrop.objectReferences)
                        {
                            GameObject draggedObject = dragged as GameObject;

                            if (draggedObject)
                            {
                                targetScript.AddNewElement(new FJiggling_Multi.FJiggling_Element(draggedObject.transform));
                                EditorUtility.SetDirty(target);
                            }
                        }

                    }

                    Event.current.Use();
                    break;
            }

            if (targetScript.ToJiggle == null) targetScript.ToJiggle = new List<FJiggling_Multi.FJiggling_Element>();

            GUILayout.BeginHorizontal();
            showTransforms = EditorGUILayout.Foldout(showTransforms, "To Stimulate (" + targetScript.ToJiggle.Count + ")", true);

            if (GUILayout.Button("All", new GUILayoutOption[2] { GUILayout.MaxWidth(48), GUILayout.MaxHeight(14) }))
            {
                targetScript.ToJiggle.Clear();

                foreach (Transform tr in FTransformMethods.FindComponentsInAllChildren<Transform>(targetScript.transform))
                {
                    targetScript.AddNewElement(new FJiggling_Multi.FJiggling_Element(tr));
                }

                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("+", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(14) }))
            {
                targetScript.AddNewElement(new FJiggling_Multi.FJiggling_Element(null));
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("-", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(14) }))
                if (targetScript.ToJiggle.Count > 0)
                {
                    targetScript.RemoveElement(targetScript.ToJiggle.Count - 1);
                    EditorUtility.SetDirty(target);
                }

            if (GUILayout.Button("C", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(14) }))
            {
                targetScript.ClearElements();
                EditorUtility.SetDirty(target);
            }

            GUILayout.EndHorizontal();
            GUI.color = preCol;

            if (showTransforms)
            {
                GUILayout.Space(3);

                if (!targetScript.ShowIndividualOptions)
                {
                    for (int i = 0; i < targetScript.ToJiggle.Count; i++)
                    {
                        GUILayout.BeginHorizontal();

                        string name;
                        if (!targetScript.ToJiggle[i].Transform)
                        {
                            name = "Assign Object";
                            GUI.color = new Color(0.9f, 0.4f, 0.4f, 0.9f);
                        }
                        else
                        {
                            name = targetScript.ToJiggle[i].Transform.name;
                            if (name.Length > 12) name = targetScript.ToJiggle[i].Transform.name.Substring(0, 7) + "...";
                        }

                        targetScript.ToJiggle[i].Transform = (Transform)EditorGUILayout.ObjectField("  [" + i + "] " + name, targetScript.ToJiggle[i].Transform, typeof(Transform), true);

                        GUI.color = preCol;
                        if (GUILayout.Button("X", new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(14) }))
                        {
                            targetScript.ToJiggle.RemoveAt(i);
                            EditorUtility.SetDirty(target);
                        }

                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    for (int i = 0; i < targetScript.ToJiggle.Count; i++)
                    {
                        GUILayout.BeginHorizontal();

                        string name;
                        if (!targetScript.ToJiggle[i].Transform)
                        {
                            name = "Assign Object";
                            GUI.color = new Color(0.9f, 0.4f, 0.4f, 0.9f);
                        }
                        else
                        {
                            name = targetScript.ToJiggle[i].Transform.name;
                            if (name.Length > 12) name = targetScript.ToJiggle[i].Transform.name.Substring(0, 7) + "...";
                        }

                        targetScript.ToJiggle[i].Transform = (Transform)EditorGUILayout.ObjectField("  [" + i + "] " + name, targetScript.ToJiggle[i].Transform, typeof(Transform), true);
                        GUI.color = preCol;

                        if (GUILayout.Button("X", new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(14) }))
                        {
                            targetScript.ToJiggle.RemoveAt(i);
                            EditorUtility.SetDirty(target);
                        }

                        GUILayout.EndHorizontal();

                        targetScript.ToJiggle[i].RotationAxesMul = EditorGUILayout.Vector3Field(new GUIContent("     Rot. Mul.", "Individual Rotation Axes Multiplier"), targetScript.ToJiggle[i].RotationAxesMul);
                        targetScript.ToJiggle[i].ScaleAxesMul = EditorGUILayout.Vector3Field(new GUIContent("     Scale Mul.", "Individual Rotation Axes Multiplier"), targetScript.ToJiggle[i].ScaleAxesMul);

                        GUILayout.Space(7);
                    }
                }
            }

            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            GUILayout.Space(5f);

            if (!Application.isPlaying) GUI.color = FColorMethods.ChangeColorAlpha(GUI.color, 0.45f); else GUI.color = preCol;
            if (GUILayout.Button("Jiggle It")) if (Application.isPlaying) targetScript.StartJiggle(); else Debug.Log("You must be in playmode to run this method!");
        }
    }
}