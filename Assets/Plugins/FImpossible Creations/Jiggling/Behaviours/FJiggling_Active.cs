using UnityEngine;

namespace FIMSpace.Jiggling
{
    /// <summary>
    /// FM: Animating transform's rotation and scale without ending animation
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Jiggling/FJiggling Active")]
    public class FJiggling_Active : FJiggling_Base
    {
        [Header("Left empty - uses component's transform")]
        public Transform TransformToAnimate;

        [Header("For more custom animations")]
        public Vector3 ScaleAxesMultiplier = Vector3.one;
        public Vector3 RotationAxesMultiplier = new Vector3(1f, 0f, 1f);

        /* Remembering initial state of transform */
        protected Quaternion initRotation;
        protected Vector3 initScale;

        protected override void Init()
        {
            if (initialized) return;

            base.Init();

            if (!TransformToAnimate) TransformToAnimate = transform;

            initRotation = TransformToAnimate.localRotation;
            initScale = TransformToAnimate.localScale;

            enabled = true;
        }

        /// <summary>
        /// Using variables calculated in base class to animate transform
        /// </summary>
        protected override void CalculateJiggle()
        {
            base.CalculateJiggle();

            easedPowerProgress = 1f;

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

            val1 /= RandomLevel;
            val2 /= RandomLevel;

            // Additional variation to rotating
            float add1 = 0f;
            float add2 = 0;
            if ( RandomLevel > 1 )
            {
                add1 = trigParams[3].Value;
                add2 = trigParams[2].Value;
            }

            t.localRotation = initRotation * Quaternion.Euler((val1 + add1) * RotationAxesMultiplier.x, (-val2 - add1) * RotationAxesMultiplier.y, (val2 + add2) * RotationAxesMultiplier.z);

            t.localScale = initScale + new Vector3(trigParams[0].Value * ScaleAxesMultiplier.x, (((trigParams[0].Value + trigParams[1].Value) / 2f) ) * ScaleAxesMultiplier.y, trigParams[0].Value * ScaleAxesMultiplier.z) * 0.01f;
        }
    }
}