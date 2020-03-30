using UnityEngine;

namespace FIMSpace.Jiggling
{
    /// <summary>
    /// FM: Class which is using FJiggle_Simple to animate tilting but also scalling transform up / down
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Jiggling/FJiggling Grow")]
    public class FJiggling_Grow : FJiggling_Simple
    {
        /// <summary> Multiplies deltaTime </summary>
        public float GrowShrinkSpeed = 1f;
        [Range(0f, 2.5f)]
        public float GrowFinishTilt = 1f;

        protected float growProgress = 1f;
        protected bool shrinking = false;

        public void ToggleGrowShrink()
        {
            if (shrinking) StartGrowing();
            else
                StartShrinking();
        }

        public virtual void StartGrowing()
        {
            shrinking = false;
            StartJiggle();
        }

        public virtual void StartShrinking()
        {
            shrinking = true;
            StartJiggle();
        }

        protected override void ReJiggle()
        {
            // Dont do anything
        }

        protected override void CalculateJiggle()
        {
            base.CalculateJiggle();

            float sign = 1f;
            if (shrinking) sign = -1f;

            growProgress = Mathf.Clamp(growProgress + Time.deltaTime * sign * GrowShrinkSpeed, 0f, 1f);

            if (!shrinking)
            {
                if (growProgress > 0.5f)
                {
                    if (growProgress < 0.85f)
                    {
                        addTilt = Mathf.Lerp(addTilt,
                                  Mathf.Lerp(0f, 4f * JiggleTiltValue * GrowFinishTilt, Mathf.InverseLerp(0.5f, 0.85f, growProgress)),
                                  Time.deltaTime * 7f);
                    }
                    else
                    {
                        addTilt = Mathf.Lerp(addTilt,
                                  Mathf.Lerp(4f * JiggleTiltValue * GrowFinishTilt, 0f, Mathf.InverseLerp(0.85f, 1f, growProgress)),
                                  Time.deltaTime * 7f);
                    }
                }
                else
                    addTilt = Mathf.Lerp(addTilt, 0f, Time.deltaTime * 15f);
            }
            else
                addTilt = Mathf.Lerp(addTilt, 0f, Time.deltaTime * 15f);

            Transform t = TransformToAnimate;
            t.localScale = t.localScale * growProgress;

            if (shrinking)
            {
                easedPowerProgress = Mathf.Lerp(0.7f, 1f, growProgress);

                if (growProgress == 0) animationFinished = true;
            }
            else // When object is growing
            {
                easedPowerProgress = Mathf.Lerp(1f, 0.15f, growProgress);

                if (growProgress == 1) animationFinished = true;
            }

            if (animationFinished) OnAnimationFinish();
        }

        protected override void OnAnimationFinish()
        {
            addTilt = 0f;
            if (!shrinking) if (growProgress == 1) ResetInitialPosRot();
        }

        protected void ResetInitialPosRot()
        {
            base.OnAnimationFinish();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// FM: Editor class for Grow component to check animation from editor level (in playmode)
    /// </summary>
    [UnityEditor.CustomEditor(typeof(FJiggling_Grow))]
    [UnityEditor.CanEditMultipleObjects]
    public class FJiggling_GrowEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FJiggling_Grow targetScript = (FJiggling_Grow)target;
            DrawDefaultInspector();

            GUILayout.Space(10f);

            if (!Application.isPlaying) GUI.color = FColorMethods.ChangeColorAlpha(GUI.color, 0.45f);
            if (GUILayout.Button("Animate Growing")) if (Application.isPlaying) targetScript.StartGrowing(); else Debug.Log("You must be in playmode to run this method!");
            if (GUILayout.Button("Animate Shrink")) if (Application.isPlaying) targetScript.StartShrinking(); else Debug.Log("You must be in playmode to run this method!");
        }
    }
#endif
}