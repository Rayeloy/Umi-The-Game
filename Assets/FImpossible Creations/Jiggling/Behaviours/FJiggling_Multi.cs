using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Jiggling
{
    /// <summary>
    /// FM: Animating multiple transforms rotation and scale to make it look kinda like jelly type animation
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Jiggling/FJiggling Multi")]
    public class FJiggling_Multi : FJiggling_Base
    {
        [HideInInspector]
        public List<FJiggling_Element> ToJiggle;

        [Header("Multi Variables")]
        [Tooltip("Calculating individual randomization variables for each element of chain, less optimal but can provide more interesting effects (it's cheap in cpu anyway)")]
        public bool SeparatedCalculations = true;

        public bool BonesNotAnimatedByAnimator = true;
        [Tooltip("If your animation is scaling bones set this to false")]
        public bool NoScaleKeyframes = true;
        private Vector3[] initialKeyScales;

        [HideInInspector]
        public bool ShowIndividualOptions = false;


        [Header("For more custom animations")]
        public Vector3 ScaleAxesMultiplier = Vector3.one;
        public Vector3 RotationAxesMultiplier = Vector3.one;


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

            initialKeyScales = new Vector3[ToJiggle.Count];

            for (int i = 0; i < ToJiggle.Count; i++)
            {
                if (ToJiggle[i].Transform == null)
                {
                    continue;
                }

                ToJiggle[i].InitPos = ToJiggle[i].Transform.localPosition;
                ToJiggle[i].InitRot = ToJiggle[i].Transform.localRotation;
                ToJiggle[i].InitScale = ToJiggle[i].Transform.localScale;

                initialKeyScales[i] = ToJiggle[i].Transform.localScale;
            }

            easedPowerProgress = 0f;

            base.Init();
        }


        protected override void Update() { /* Don't do anything in Update() we will use LateUpdate() to support also animated models jiggling animation */ }

        protected override void ReJiggle()
        {
            base.ReJiggle();
            reJiggleProgress = 1f;
            RandomizeVariables(ReJigglePower / 4f);
        }

        /// <summary>
        /// Executing right methods for group of jiggle game objects
        /// </summary>
        private void LateUpdate()
        {
            CalculateJiggle();

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

            if (BonesNotAnimatedByAnimator)
            {
                if (!SeparatedCalculations)
                    for (int i = 0; i < ToJiggle.Count; i++)
                    {
                        ToJiggle[i].Transform.localScale = initialKeyScales[i];
                        ToJiggle[i].Transform.localPosition = ToJiggle[i].InitPos;
                        ToJiggle[i].Transform.localRotation = ToJiggle[i].InitRot;
                        CalculateJigglingFor(i);
                    }
                else
                    for (int i = 0; i < ToJiggle.Count; i++)
                    {
                        ToJiggle[i].Transform.localScale = initialKeyScales[i];
                        ToJiggle[i].Transform.localPosition = ToJiggle[i].InitPos;
                        ToJiggle[i].Transform.localRotation = ToJiggle[i].InitRot;
                        CalculateTrigonoFor(i);
                        CalculateJigglingFor(i);
                    }
            }
            else
            {
                if (NoScaleKeyframes)
                {
                    if (!SeparatedCalculations)
                        for (int i = 0; i < ToJiggle.Count; i++)
                        {
                            ToJiggle[i].Transform.localScale = initialKeyScales[i];
                            CalculateJigglingFor(i);
                        }
                    else
                        for (int i = 0; i < ToJiggle.Count; i++)
                        {
                            ToJiggle[i].Transform.localScale = initialKeyScales[i];
                            CalculateTrigonoFor(i);
                            CalculateJigglingFor(i);
                        }
                }
                else
                {
                    if (!SeparatedCalculations)
                        for (int i = 0; i < ToJiggle.Count; i++)
                        {
                            ToJiggle[i].InitScale = ToJiggle[i].Transform.localScale;
                            ToJiggle[i].Transform.localScale = ToJiggle[i].InitScale;
                            CalculateJigglingFor(i);
                        }
                    else
                        for (int i = 0; i < ToJiggle.Count; i++)
                        {
                            ToJiggle[i].InitScale = ToJiggle[i].Transform.localScale;
                            ToJiggle[i].Transform.localScale = ToJiggle[i].InitScale;
                            CalculateTrigonoFor(i);
                            CalculateJigglingFor(i);
                        }
                }
            }

            if (animationFinished)
            {
                OnAnimationFinish();
                enabled = false;
            }
        }


        /// <summary>
        /// Calculating motion for each jiggling element in chain
        /// </summary>
        private void CalculateJigglingFor(int index)
        {
            FJiggling_Element jigglE = ToJiggle[index];
            Transform t = jigglE.Transform;

            float val1 = 0f; float val2 = 0f;

            for (int i = 0; i < RandomLevel * 2; i++)
            {
                if (i % 2 == 0) val1 += jigglE.trigParams[i].Value; else val2 += jigglE.trigParams[i].Value;
            }

            val1 /= RandomLevel; val2 /= RandomLevel;

            // Additional variation to rotating
            float add1 = 0f; float add2 = 0;
            if (RandomLevel > 1)
            {
                add1 = jigglE.trigParams[3].Value;
                add2 = jigglE.trigParams[2].Value;
            }

            t.transform.localRotation *= Quaternion.Euler((val1 + reJiggleValue + add1) * jigglE.RotationAxesMul.x * RotationAxesMultiplier.x, (-val2 + reJiggleValue - add1) * jigglE.RotationAxesMul.y * RotationAxesMultiplier.y, (val2 + reJiggleValue + add2) * jigglE.RotationAxesMul.z * RotationAxesMultiplier.z);
            t.transform.localScale += new Vector3(trigParams[0].Value * jigglE.ScaleAxesMul.x * ScaleAxesMultiplier.x, (((trigParams[0].Value + trigParams[1].Value) / 2f)) * jigglE.ScaleAxesMul.y * ScaleAxesMultiplier.y, trigParams[0].Value * jigglE.ScaleAxesMul.z * ScaleAxesMultiplier.z) * 0.01f;
        }


        protected void CalculateTrigonoFor(int i)
        {
            for (int r = 0; r < RandomLevel; r++)
            {
                for (int j = 0; j < 2; j++)
                {
                    TrigonoParams parameters = ToJiggle[i].trigParams[r + j];

                    float timeValue = time * parameters.RandomTimeMul + parameters.TimeOffset;
                    float multiplyValue = JiggleTiltValue * parameters.Multiplier * currentJigglePower / AdditionalSpeedValue;

                    if (j % 2 == 0) parameters.Value = Mathf.Sin(timeValue) * multiplyValue; else parameters.Value = Mathf.Cos(timeValue) * multiplyValue;
                }
            }
        }

        /// <summary>
        /// Supporting separated trigonometric functions randomization if right toggle enabled
        /// </summary>
        protected override void RandomizeVariables(float transition = 1f)
        {

            if (!SeparatedCalculations)
            {
                base.RandomizeVariables(transition);
                for (int j = 0; j < ToJiggle.Count; j++) ToJiggle[j].trigParams = trigParams;
            }
            else
            {
                base.RandomizeVariables(transition);

                if (transition >= 1f)
                {
                    for (int j = 0; j < ToJiggle.Count; j++)
                    {
                        ToJiggle[j].trigParams = new List<TrigonoParams>();

                        for (int i = 0; i < RandomLevel; i++)
                        {
                            for (int t = 0; t < 2; t++)
                            {
                                TrigonoParams newParams = new TrigonoParams();
                                newParams.Randomize();
                                ToJiggle[j].trigParams.Add(newParams);
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < ToJiggle.Count; j++)
                    {
                        for (int i = 0; i < RandomLevel; i++)
                        {
                            for (int t = 0; t < 2; t++)
                            {
                                TrigonoParams newParams = new TrigonoParams();
                                newParams.Randomize();
                                ToJiggle[j].trigParams[i + t].Multiplier = Mathf.Lerp(ToJiggle[j].trigParams[i + t].Multiplier, newParams.Multiplier, transition);
                                ToJiggle[j].trigParams[i + t].RandomTimeMul = Mathf.Lerp(ToJiggle[j].trigParams[i + t].RandomTimeMul, newParams.RandomTimeMul, transition);
                                ToJiggle[j].trigParams[i + t].TimeOffset = Mathf.Lerp(ToJiggle[j].trigParams[i + t].TimeOffset, newParams.TimeOffset, transition);
                                ToJiggle[j].trigParams[i + t].Value = Mathf.Lerp(ToJiggle[j].trigParams[i + t].Value, newParams.Value, transition);
                            }
                        }
                    }
                }
            }
        }


        #region List Support Methods

        public void AddNewElement(FJiggling_Element el)
        {
            bool canAdd = false;

            if (ToJiggle == null) ToJiggle = new List<FJiggling_Element>();

            if (el.Transform == null)
            {
                canAdd = true;
            }
            else
                if (!ToJiggle.Contains(el))
                canAdd = true;

            if (canAdd)
            {
                ToJiggle.Add(el);
            }
        }

        public void RemoveElement(FJiggling_Element el)
        {
            ToJiggle.Remove(el);
        }

        public void RemoveElement(int index)
        {
            ToJiggle.RemoveAt(index);
        }

        public void ClearElements()
        {
            ToJiggle.Clear();
        }

        public bool ContainsElement(Transform t)
        {
            for (int i = 0; i < ToJiggle.Count; i++)
                if (ToJiggle[i].Transform == t)
                    return true;

            return false;
        }

        #endregion

        [System.Serializable]
        public class FJiggling_Element
        {
            public FJiggling_Element(Transform target)
            {
                Transform = target;
            }

            public Transform Transform;

            public Vector3 RotationAxesMul = Vector3.one;
            public Vector3 ScaleAxesMul = Vector3.one;

            public Vector3 InitPos;
            public Quaternion InitRot;
            public Vector3 InitScale;

            public List<TrigonoParams> trigParams;
        }


#if UNITY_EDITOR

        protected override void OnDrawGizmos()
        {
            if (!drawGizmo) return;

            for (int i = 0; i < ToJiggle.Count; i++)
            {
                if (ToJiggle[i].Transform != null)
                    Gizmos.DrawIcon(ToJiggle[i].Transform.position, "FIMSpace/Jiggling/Jiggling Gizmo.png");
            }
        }

#endif

    }

}