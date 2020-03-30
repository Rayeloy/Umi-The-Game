using UnityEngine;

namespace FIMSpace.Jiggling
{
    /// <summary>
    /// FM: Animating transform's rotation and scale to make it look kinda like jelly
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Jiggling/FJiggling Simple")]
    public class FJiggling_Simple : FJiggling_Base
    {
        //[Header("Left empty - uses component's transform")]
        public Transform TransformToAnimate;

        [Header("For more custom animations")]
        public Vector3 ScaleAxesMultiplier = Vector3.one;
        protected float scaleMul = 1f;
        public Vector3 RotationAxesMultiplier = new Vector3(1f, 0f, 1f);
        protected float rotationMul = 1f;

        /* Remembering initial state of transform */
        protected Quaternion initRotation;
        protected Vector3 initScale;

        /* When jiggling is executed second time during animating, this variables making effect like object is hitted again */
        [Space(5f)]
        [Tooltip("How big should be tilt impact when jiggling is triggered another time during animating")]
        public float ReJigglePower = 2f;
        protected float reJiggleProgress = 0f;
        protected float reJiggleValue = 0f;
        protected float reJiggleRandomOffset;

        protected override void Init()
        {
            if (initialized) return;

            base.Init();

            if (!TransformToAnimate) TransformToAnimate = transform;

            initRotation = TransformToAnimate.localRotation;
            initScale = TransformToAnimate.localScale;

            if (ConstantJiggleValue <= 0f) enabled = false;
        }

        protected virtual void Reset()
        {
            TransformToAnimate = transform;
        }

        public override void StartJiggle(float powerMultiplier = 1f)
        {
            base.StartJiggle(powerMultiplier);

            easedPowerProgress = 1f;
            reJiggleRandomOffset = Random.Range(-0.2f, 0.2f);
        }


        protected override void ReJiggle()
        {
            base.ReJiggle();
            reJiggleProgress = 1f * powerMultiplier;
            RandomizeVariables((ReJigglePower * powerMultiplier) / 4f);
        }


        /// <summary>
        /// Using variables calculated in base class to animate transform
        /// </summary>
        protected override void CalculateJiggle()
        {
            base.CalculateJiggle();

            Transform t = TransformToAnimate;

            float val1 = 0f;
            float val2 = 0f;

            for (int i = 0; i < RandomLevel * 2; i++)
            {
                if (i % 2 == 0)
                    val1 += trigParams[i].Value;
                else
                    val2 += trigParams[i].Value;
            }

            val1 /= (float)RandomLevel;
            val2 /= (float)RandomLevel;

            if (reJiggled)
            {
                if (reJiggleProgress > 0f)
                {
                    reJiggleProgress -= Time.deltaTime * JiggleDecelerate * 1.75f;
                    if (reJiggleProgress < 0.001f) reJiggleProgress = 0.001f;
                    float targetValue = Mathf.Sin(time * 1.45f * trigParams[1].RandomTimeMul + reJiggleRandomOffset + trigParams[0].TimeOffset + trigParams[1].TimeOffset) * JiggleTiltValue * trigParams[0].Multiplier * reJiggleProgress * ReJigglePower;
                    reJiggleValue = Mathf.Lerp(reJiggleValue, targetValue, Time.deltaTime * 12f);
                }
                else
                {
                    reJiggleProgress = 0f;
                    reJiggleValue = 0f;
                }
            }
            else
            {
                reJiggleProgress = 0f;
                reJiggleValue = 0f;
            }

            // Additional variation to rotating
            float add1 = 0f;
            float add2 = 0;
            if (RandomLevel > 1)
            {
                add1 = trigParams[3].Value;
                add2 = trigParams[2].Value;
            }

            float xAngle = (val1 + reJiggleValue + add1) * RotationAxesMultiplier.x * rotationMul;
            float yAngle = (-val2 + reJiggleValue - add1) * RotationAxesMultiplier.y * rotationMul;
            float zAngle = (val2 + reJiggleValue + add2) * RotationAxesMultiplier.z * rotationMul;

            t.localRotation = initRotation * Quaternion.Euler(xAngle, yAngle, zAngle);

            float xScale = trigParams[0].Value * ScaleAxesMultiplier.x * scaleMul;
            float yScale = (((trigParams[0].Value + trigParams[1].Value) / 2f)) * ScaleAxesMultiplier.y * scaleMul;
            float zScale = trigParams[0].Value * ScaleAxesMultiplier.z * scaleMul;

            t.localScale = initScale + new Vector3(xScale, yScale, zScale) * 0.01f;

            if (animationFinished) OnAnimationFinish();
        }


        protected override void OnAnimationFinish()
        {
            TransformToAnimate.localRotation = initRotation;
            TransformToAnimate.localScale = initScale;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// FM: Editor class for Jiggle Simple component to check animation from editor level (in playmode)
    /// </summary>
    [UnityEditor.CustomEditor(typeof(FJiggling_Simple))]
    [UnityEditor.CanEditMultipleObjects]
    public class FJiggling_SimpleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FJiggling_Simple targetScript = (FJiggling_Simple)target;
            DrawDefaultInspector();

            GUILayout.Space(10f);

            if (!Application.isPlaying) GUI.color = FColorMethods.ChangeColorAlpha(GUI.color, 0.45f);
            if (GUILayout.Button("Jiggle It")) if (Application.isPlaying) targetScript.StartJiggle(); else Debug.Log("You must be in playmode to run this method!");
        }
    }
#endif
}