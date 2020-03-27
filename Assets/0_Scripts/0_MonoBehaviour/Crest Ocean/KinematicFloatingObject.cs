// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

// Thanks to @VizzzU for contributing this.

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Applies simple approximation of buoyancy force - force based on submerged depth and torque based on alignment
    /// to water normal.
    /// </summary>
    public class KinematicFloatingObject : FloatingObjectBase
    {
        public float gravity = 0.05f;

        [Header("Buoyancy Force")]
        [Tooltip("Offsets center of object to raise it (or lower it) in the water."), SerializeField]
        float _raiseObject = 1f;
        [Tooltip("Strength of buoyancy force per meter of submersion in water."), SerializeField]
        float _buoyancyCoeff = 3f;
        [Tooltip("Strength of torque applied to match boat orientation to water normal."), SerializeField]
        public float buoyancyRotationSpeed = 0.02f;

        [Header("Wave Response")]
        [Tooltip("Diameter of object, for physics purposes. The larger this value, the more filtered/smooth the wave response will be."), SerializeField]
        float _objectWidth = 3f;
        public override float ObjectWidth { get { return _objectWidth; } }

        [Header("Drag")]

        [SerializeField] float _dragInWaterUp = 3f;
        [SerializeField] float _dragInWaterRight = 2f;
        [SerializeField] float _dragInWaterForward = 1f;

        [Header("Debug")]
        [SerializeField]
        bool _debugDraw = false;
        [SerializeField] bool _debugValidateCollision = false;

        bool _inWater;
        public override bool InWater { get { return _inWater; } }

        Vector3 _displacementToObject = Vector3.zero;
        public override Vector3 CalculateDisplacementToObject() { return _displacementToObject; }

        public override Vector3 Velocity => currentVelocity;

        Rigidbody _rb;
        Vector3 currentVelocity;

        SampleHeightHelper _sampleHeightHelper = new SampleHeightHelper();
        SampleFlowHelper _sampleFlowHelper = new SampleFlowHelper();

        void Start()
        {
            _rb = GetComponent<Rigidbody>();

            if (OceanRenderer.Instance == null)
            {
                enabled = false;
                return;
            }
        }

        void FixedUpdate()
        {
            //Debug.Log("KinFloatObject (" + name + "): currentVelocity PRE MOVE = " + currentVelocity.ToString("F6") + "; currentPos = " + transform.position.ToString("F6"));
            transform.position += currentVelocity * Time.fixedDeltaTime;
            //Debug.Log("KinFloatObject (" + name + "): movement applied = " + (currentVelocity * Time.deltaTime).ToString("F6") + "; currentPos = " + transform.position.ToString("F6"));

            AddForce(Vector3.down * gravity);

            UnityEngine.Profiling.Profiler.BeginSample("KinematicFloatingObject.FixedUpdate");

            if (OceanRenderer.Instance != null)
            {

                // Trigger processing of displacement textures that have come back this frame. This will be processed
                // anyway in Update(), but FixedUpdate() is earlier so make sure it's up to date now.
                if (OceanRenderer.Instance._simSettingsAnimatedWaves.CollisionSource == SimSettingsAnimatedWaves.CollisionSources.OceanDisplacementTexturesGPU && GPUReadbackDisps.Instance)
                {
                    GPUReadbackDisps.Instance.ProcessRequests();
                }

                var collProvider = OceanRenderer.Instance.CollisionProvider;
                var position = transform.position;

                var normal = Vector3.up; var waterSurfaceVel = Vector3.zero;
                _sampleHeightHelper.Init(transform.position, _objectWidth);
                _sampleHeightHelper.Sample(ref _displacementToObject, ref normal, ref waterSurfaceVel);

                var undispPos = transform.position - _displacementToObject;
                undispPos.y = OceanRenderer.Instance.SeaLevel;

                if (_debugDraw) VisualiseCollisionArea.DebugDrawCross(undispPos, 1f, Color.red);

                if (QueryFlow.Instance)
                {
                    _sampleFlowHelper.Init(transform.position, ObjectWidth);

                    Vector2 surfaceFlow = Vector2.zero;
                    _sampleFlowHelper.Sample(ref surfaceFlow);
                    waterSurfaceVel += new Vector3(surfaceFlow.x, 0, surfaceFlow.y);
                }

                if (_debugDraw)
                {
                    Debug.DrawLine(transform.position + 5f * Vector3.up, transform.position + 5f * Vector3.up + waterSurfaceVel,
                        new Color(1, 1, 1, 0.6f));
                }

                var velocityRelativeToWater = currentVelocity - waterSurfaceVel;

                var dispPos = undispPos + _displacementToObject;
                if (_debugDraw) VisualiseCollisionArea.DebugDrawCross(dispPos, 4f, Color.white);

                float height = dispPos.y;

                float bottomDepth = height - transform.position.y + _raiseObject;

                Vector3 waterSufacePos = new Vector3(transform.position.x, dispPos.y, transform.position.z);
                Vector3 floatingObjectPos = new Vector3(transform.position.x, transform.position.y + _raiseObject, transform.position.z);

                Debug.DrawLine(waterSufacePos, floatingObjectPos, Color.red);
                //Debug.Log("KinematicFloatingObject " + name + " : Checking if inWater -> bottomDepth = " + bottomDepth.ToString("F6")+ "; waterHeight = "+ height.ToString("F6")+
                //    "; object pos = " +(transform.position.y + _raiseObject).ToString("F6"));
                _inWater = bottomDepth > 0f;
                if (_inWater)
                {
                    var buoyancy = Vector3.up * _buoyancyCoeff * bottomDepth * bottomDepth * bottomDepth;
                    //buoyancy.y = Mathf.Clamp(buoyancy.y, float.MinValue, maxFloatingYSpeed);

                    AddForce(buoyancy);
                    //Debug.Log("KinematicFloatingObject " + name + " : After buoyancy -> currentVelocity = " + currentVelocity.ToString("F6") + "; buoyancy = " + buoyancy.ToString("F6"));

                    // apply drag relative to water
                    Vector3 verticalDrag = Vector3.up * Vector3.Dot(Vector3.up, -velocityRelativeToWater) * _dragInWaterUp;
                    Vector3 sidewaysDrag = transform.right * Vector3.Dot(transform.right, -velocityRelativeToWater) * _dragInWaterRight;
                    Vector3 fowardDrag = transform.forward * Vector3.Dot(transform.forward, -velocityRelativeToWater) * _dragInWaterForward;

                    AddForce(verticalDrag);
                    AddForce(sidewaysDrag);
                    AddForce(fowardDrag);

                    //Debug.Log("KinematicFloatingObject " + name + " : After vertical drag -> currentVelocity = " + currentVelocity.ToString("F6") + "; verticalDrag = " + verticalDrag.ToString("F6"));


                    FixedUpdateOrientation(normal);

                    //collProvider.ReturnSamplingData(_samplingData);
                }
            }

            UnityEngine.Profiling.Profiler.EndSample();

        }

        void AddForce(Vector3 force)
        {
            currentVelocity += force*Time.fixedDeltaTime;
        }

        /// <summary>
        /// Align to water normal. One normal by default, but can use a separate normal based on boat length vs width. This gives
        /// varying rotations based on boat dimensions.
        /// </summary>
        void FixedUpdateOrientation(Vector3 normal)
        {
            if (_debugDraw) Debug.DrawLine(transform.position, transform.position + 5f * normal, Color.green);

            transform.up = Vector3.Lerp(transform.up, normal, buoyancyRotationSpeed);
        }

#if UNITY_EDITOR
        //private void Update()
        //{
        //    UpdateDebugDrawSurroundingColl();
        //}

        //private void UpdateDebugDrawSurroundingColl()
        //{
        //    var r = 5f;
        //    var steps = 10;

        //    var collProvider = OceanRenderer.Instance.CollisionProvider;
        //    var thisRect = new Rect(transform.position.x - r * steps / 2f, transform.position.z - r * steps / 2f, r * steps / 2f, r * steps / 2f);
        //    if (!collProvider.GetSamplingData(ref thisRect, _objectWidth, _samplingData))
        //    {
        //        return;
        //    }

        //    for (float i = 0; i < steps; i++)
        //    {
        //        for (float j = 0; j < steps; j++)
        //        {
        //            Vector3 pos = new Vector3(((i + 0.5f) - steps / 2f) * r, 0f, ((j + 0.5f) - steps / 2f) * r);
        //            pos.x += transform.position.x;
        //            pos.z += transform.position.z;

        //            Vector3 disp;
        //            if (collProvider.SampleDisplacement(ref pos, _samplingData, out disp))
        //            {
        //                DebugDrawCross(pos + disp, 1f, Color.green);
        //            }
        //            else
        //            {
        //                DebugDrawCross(pos, 0.25f, Color.red);
        //            }
        //        }
        //    }

        //    collProvider.ReturnSamplingData(_samplingData);
        //}
#endif

        void DebugDrawCross(Vector3 pos, float r, Color col)
        {
            Debug.DrawLine(pos - Vector3.up * r, pos + Vector3.up * r, col);
            Debug.DrawLine(pos - Vector3.right * r, pos + Vector3.right * r, col);
            Debug.DrawLine(pos - Vector3.forward * r, pos + Vector3.forward * r, col);
        }
    }
}
