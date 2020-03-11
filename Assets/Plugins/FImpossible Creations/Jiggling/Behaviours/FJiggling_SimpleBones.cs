using System.Collections;
using UnityEngine;

namespace FIMSpace.Jiggling
{
    /// <summary>
    /// FM: Animating transform's rotation and scale to make it look kinda like jelly
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Jiggling/FJiggling Simple - Bones")]
    public class FJiggling_SimpleBones : FJiggling_Simple
    {
        public bool NoRotationKeyframes = false;

        [Tooltip("If your animation is scaling bones set this to false")]
        public bool NoScaleKeyframes = true;

        [Tooltip("Toggle it to true if you animator is using 'Animated Physics' update mode")]
        public bool AnimatePhysics = false;
        private bool animatePhysicsWorking = false;
        private bool triggerAnimatePhysics = false;

        private Quaternion initialKeyRotation;
        private Vector3 initialKeyScale;

        protected override void Init()
        {
            if (initialized) return;

            base.Init();
            initialKeyRotation = TransformToAnimate.localRotation;
            initialKeyScale = TransformToAnimate.localScale;
        }

        protected override void Update()
        {
            // Erasing all actions made in Update() 
        }

        protected virtual void LateUpdate()
        {
            if (AnimatePhysics)
            {
                if (!animatePhysicsWorking) StartCoroutine(AnimatePhysicsClock());
                if (!triggerAnimatePhysics) return; else triggerAnimatePhysics = false;
            }

            if (NoRotationKeyframes) TransformToAnimate.localRotation = initialKeyRotation;
            if (NoScaleKeyframes) TransformToAnimate.localScale = initialKeyScale;

            // Every beginning of late update rotations are the same as in animation played by Animator component
            initRotation = TransformToAnimate.localRotation;// initialKeyRotation;
            initScale = TransformToAnimate.localScale;

            // Doing update calculations in LateUpdate() to override Animator's work
            base.Update();
        }

        /// <summary>
        /// Support for 'animate physics' option inside unity's Animator
        /// </summary>
        private IEnumerator AnimatePhysicsClock()
        {
            animatePhysicsWorking = true;

            while (true)
            {
                yield return new WaitForFixedUpdate();
                triggerAnimatePhysics = true;
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// FM: Editor class for Jiggle Bones component to check animation from editor level (in playmode)
    /// </summary>
    [UnityEditor.CustomEditor(typeof(FJiggling_SimpleBones))]
    [UnityEditor.CanEditMultipleObjects]
    public class FJiggling_SimpleBonesEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FJiggling_SimpleBones targetScript = (FJiggling_SimpleBones)target;
            DrawDefaultInspector();

            GUILayout.Space(10f);

            if (!Application.isPlaying) GUI.color = FColorMethods.ChangeColorAlpha(GUI.color, 0.45f);
            if (GUILayout.Button("Jiggle It")) if (Application.isPlaying) targetScript.StartJiggle(); else Debug.Log("You must be in playmode to run this method!");
        }
    }
#endif
}