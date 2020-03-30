using UnityEngine;

namespace FIMSpace.Jiggling
{
    /// <summary>
    /// FM: Class using FJiggle_Simple calculations to animate rect transform jiggling
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Jiggling/FJiggling UI Rect")]
    public class FJiggling_Rect : FJiggling_Simple
    {
        protected RectTransform RectTransformToAnimate;

        protected override void Init()
        {
            if (initialized) return;

            base.Init();

            RectTransformToAnimate = TransformToAnimate.GetComponent<RectTransform>();

            if (!RectTransformToAnimate) Debug.LogError("There is no rectTransform in " + TransformToAnimate.name + "!");
        }

        // To be true component don't need anything changed to animate rectTransforms :D
        // But component created to keep it separated for more controll
    }

#if UNITY_EDITOR
    /// <summary>
    /// FM: Editor class for Jiggle Rect component to check animation from editor level (in playmode)
    /// </summary>
    [UnityEditor.CustomEditor(typeof(FJiggling_Rect))]
    public class FJiggling_RectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FJiggling_Rect targetScript = (FJiggling_Rect)target;
            DrawDefaultInspector();

            GUILayout.Space(10f);

            if (!Application.isPlaying) GUI.color = FColorMethods.ChangeColorAlpha(GUI.color, 0.45f);
            if (GUILayout.Button("Jiggle It")) if (Application.isPlaying) targetScript.StartJiggle(); else Debug.Log("You must be in playmode to run this method!");
        }
    }
#endif

}
