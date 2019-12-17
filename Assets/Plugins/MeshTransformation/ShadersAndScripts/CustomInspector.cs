using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace MeshTransformation
{
    [CustomEditor(typeof(TransformationMesh))]
    public class CustomInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            TransformationMesh ed = (TransformationMesh)target;
            if (GUILayout.Button("CalculateMesh"))
            {
                ed.Start_();
            }

        }

    }
}
#endif
