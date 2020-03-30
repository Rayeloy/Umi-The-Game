using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Jiggling
{
    /// <summary>
    /// FM: Class with methods which calculating variables helpful to create animations like jiggling
    /// </summary>
    public abstract class FJiggling_Base : MonoBehaviour, UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {
        public string EditorIconPath { get { return "Jiggling/Jiggling Icon"; } }
        public void OnDrop(UnityEngine.EventSystems.PointerEventData data) { }

        [Tooltip("How much jiggle should wave")]
        [Range(0f,50f)]
        public float JiggleTiltValue = 12f;
        [Tooltip("How fast should be jiggling")]
        [Range(0f,60f)]
        public float JiggleFrequency = 22f;
        [Tooltip("How fast animation should slow down")]
        [Range(0f,10f)]
        public float JiggleDecelerate = 1.5f;

        [Header("Additional Parameters")]
        [Tooltip("Optional value to change effect a bit")]
        [Range(0.5f, 2f)]
        public float AdditionalSpeedValue = 1f;

        [Tooltip("Change this value up to make object jiggle all the time and work with hit jiggling")]
        [Range(0.0f, 0.5f)]
        public float ConstantJiggleValue = 0f;

        /// <summary> Time for trigonometric functions (not using Time.time because we want more controll) </summary>
        protected float time;

        /// <summary> If animation finished </summary>
        protected bool animationFinished = false;

        /* Variables for trigonometric functions, values, variations */
        protected List<TrigonoParams> trigParams;

        /// <summary> How many trigonometric functions should be used to create jiggle animation </summary>
        [Tooltip("Higher random lever - jiggling more randomized")]
        [Range(1, 3)]
        public int RandomLevel = 1;

        /// <summary> Variables to change intensity of animation </summary>
        protected float targetPowerValue = 1.2f;
        protected float easedPowerProgress = 1f;
        protected float currentJigglePower = 0.15f;
        protected bool reJiggled = false;
        protected float addTilt = 0f;
        protected float timeMul = 1f;
        protected float powerMultiplier = 1f;

        /// <summary> Initialization method controll flag </summary>
		protected bool initialized = false;

        /// <summary>
        /// Method to initialize component, to have more controll than waiting for Start() method, init can be executed before or after start, as programmer need it.
        /// </summary>
        protected virtual void Init()
        {
            if (initialized) return;

            RandomizeVariables();

            if (ConstantJiggleValue > 0f)
            {
                easedPowerProgress = 0.1f;
                targetPowerValue = 0.1f;
            }

            initialized = true;
        }

        void Awake()
        {
            Init();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) RandomizeVariables();
        }

        /// <summary>
        /// Executing jiggle calculating and animating methods
        /// </summary>
        protected virtual void Update()
        {
            CalculateJiggle();
            if (animationFinished) enabled = false;
        }

        /// <summary>
        /// Place to put custom action when animation finishes
        /// </summary>
        protected virtual void OnAnimationFinish()
        {
        }


        /// <summary>
        /// Enabling component and resetting base variables to start jiggle animation
        /// </summary>
        public virtual void StartJiggle(float powerMultiplier = 1f)
        {
            Init();
            enabled = true;
            this.powerMultiplier = powerMultiplier;

            if (animationFinished)
            {
                animationFinished = false;

                targetPowerValue = 1.15f * powerMultiplier;
                easedPowerProgress = 1f;
                currentJigglePower = 0.15f * powerMultiplier;

                RandomizeVariables();

                reJiggled = false;
            }
            else
            {
                easedPowerProgress = 1.1f * powerMultiplier;
                ReJiggle();
            }
        }

        public virtual void StartJiggle()
        {
            StartJiggle(1f);
        }


        /// <summary>
        /// When jiggling is executted again during animation
        /// </summary>
        protected virtual void ReJiggle()
        {
            reJiggled = true;
        }


        /// <summary>
        /// Resetting trigonometric variables for animation to look different every time
        /// </summary>
        protected virtual void RandomizeVariables(float transition = 1f)
        {
            if (transition >= 1f)
            {
                time = Random.Range(-Mathf.PI * 5f, Mathf.PI * 5f);

                trigParams = new List<TrigonoParams>();

                // Each random level have 2 trigonometric functions (sinus and cosinus)
                for (int i = 0; i < RandomLevel; i++)
                {
                    for (int t = 0; t < 2; t++)
                    {
                        TrigonoParams newParams = new TrigonoParams();
                        newParams.Randomize();
                        trigParams.Add(newParams);
                    }
                }
            }
            else
            {
                time = Mathf.Lerp(time, Random.Range(-Mathf.PI * 5f, Mathf.PI * 5f), transition);

                // Each random level have 2 trigonometric functions (sinus and cosinus)
                for (int i = 0; i < RandomLevel; i++)
                {
                    for (int t = 0; t < 2; t++)
                    {
                        TrigonoParams newParams = new TrigonoParams();
                        newParams.Randomize();
                        trigParams[i+t].Multiplier = Mathf.Lerp(trigParams[i+t].Multiplier, newParams.Multiplier, transition);
                        trigParams[i+t].RandomTimeMul = Mathf.Lerp(trigParams[i+t].RandomTimeMul, newParams.RandomTimeMul, transition);
                        trigParams[i+t].TimeOffset = Mathf.Lerp(trigParams[i+t].TimeOffset, newParams.TimeOffset, transition);
                        trigParams[i+t].Value = Mathf.Lerp(trigParams[i+t].Value, newParams.Value, transition);
                    }
                }
            }
        }


        /// <summary>
        /// Calculating trigonometric variables to simulate jiggling animation
        /// </summary>
        protected virtual void CalculateTrigonometricVariables(float timeMultiplier = 1f)
        {
            float finishingSafeRange = currentJigglePower - 0.01f; if (finishingSafeRange <= 0f) finishingSafeRange = 0f;

            time += Time.deltaTime * (JiggleFrequency * timeMultiplier) * timeMul;

            currentJigglePower = Mathf.Lerp(currentJigglePower, targetPowerValue, Time.deltaTime * 20f);

            easedPowerProgress -= Time.deltaTime * JiggleDecelerate * AdditionalSpeedValue;
            easedPowerProgress = Mathf.Max(ConstantJiggleValue, easedPowerProgress);

            targetPowerValue = EaseInOutCubic(-0.001f, 1f, easedPowerProgress);

            for (int i = 0; i < RandomLevel; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    TrigonoParams parameters = trigParams[i + j];

                    float timeValue = time * parameters.RandomTimeMul + parameters.TimeOffset;
                    float multiplyValue = (JiggleTiltValue * powerMultiplier + addTilt ) * parameters.Multiplier * finishingSafeRange / AdditionalSpeedValue;

                    if (j % 2 == 0)
                        parameters.Value = Mathf.Sin(timeValue) * multiplyValue;
                    else
                        parameters.Value = Mathf.Cos(timeValue) * multiplyValue;
                }
            }
        }


        /// <summary>
        /// Calculating trigonometric values and defining if animation is finished
        /// place for custom use of calculated trigonometric values
        /// </summary>
        protected virtual void CalculateJiggle()
        {
            CalculateTrigonometricVariables();

            if (currentJigglePower <= 0f)
            {
                if (ConstantJiggleValue <= 0f) animationFinished = true;
            }
        }


        /// <summary>
        /// FM: Container class to hold trigonometric parameters for animating jiggle parameters for animations
        /// </summary>
        [System.Serializable]
        public class TrigonoParams
        {
            public float Value;
            public float Multiplier;
            public float TimeOffset;
            public float RandomTimeMul;

            /// <summary>
            /// Getting random valuse for variables
            /// </summary>
            public void Randomize()
            {
                Value = 0f;
                Multiplier = Random.Range(0.85f, 1.15f);

                TimeOffset = Random.Range(-Mathf.PI, Mathf.PI);
                RandomTimeMul = Random.Range(0.8f, 1.2f);
            }
        }

        public static float EaseInOutCubic(float start, float end, float value)
        {
            value /= .5f;
            end -= start;

            if (value < 1) return end * 0.5f * value * value * value + start;

            value -= 2;

            return end * 0.5f * (value * value * value + 2) + start;
        }

#if UNITY_EDITOR

        public bool drawGizmo = true;
        protected virtual void OnDrawGizmos()
        {
            if (!drawGizmo) return;
            Gizmos.DrawIcon(transform.position, "FIMSpace/Jiggling/Jiggling Gizmo.png");
        }

#endif

    }
}
