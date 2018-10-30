using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisions : MonoBehaviour {

    CameraController myCamController;
    public float minDistance = 1.0f;
    [Tooltip("CameraController changes this value. ")]
    public float maxDistance = 4.0f;
    public float smooth = 10.0f;
    Vector3 dollyDir;
    public Vector3 dollyDirAdjusted;
    float distance;
    public LayerMask collisionMask;

    private void Awake()
    {
        myCamController = transform.parent.GetComponent<CameraController>();
    }
    private void Start()
    {
        ResetData();
    }

    public void ResetData()
    {
        maxDistance = myCamController.targetMyCamPos.magnitude;
        dollyDir = myCamController.targetMyCamPos.normalized;
        distance = transform.localPosition.magnitude;
    }

    public void KonoUpdate()
    {
        Vector3 desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;
        if (!myCamController.myPlayerMov.controller.disableAllRays)
        {
            Debug.DrawLine(transform.parent.position, desiredCameraPos, Color.yellow, 0.01f);
        }

        if (Physics.Linecast(transform.parent.position, desiredCameraPos, out hit, collisionMask, QueryTriggerInteraction.Ignore))
        {
            //print("CAMARA OBSTRUIDA");
            distance = Mathf.Clamp((hit.distance * 0.87f), minDistance, maxDistance);
            //print("MaxDistanceCamera = " + maxDistance+"; distance = "+distance);
        }
        else
        {
            distance = maxDistance;
        }
        myCamController.targetMyCamPos = dollyDir * distance;
    }



    bool shaking, smoothShakeStart_End;
    float TimeShaking;
    float MaxTimeShaking;
    float ShakingSize;
    float NextShakeTime;
    float MaxNextShakeTime;
    Vector2 shakedPos;
    public void shakeCameraTrial(float time)
    {
        StartShakeCamera(time, 0.5f, 0.08f, true);
    }
    public void StartShakeCamera(float time, float size = 0.3f, float shakeFreq = 0.2f, bool _smoothShakeStart_End = true)
    {
        shaking = true;
        smoothShakeStart_End = _smoothShakeStart_End;
        shakedPos = Vector2.zero;
        TimeShaking = 0;
        MaxTimeShaking = time;
        ShakingSize = size;
        NextShakeTime = MaxNextShakeTime + 1;
    }
    void ShakeCamera()
    {
        if (shaking)
        {
            //Debug.Log("SHAKING CAMERA");
            if (TimeShaking >= MaxTimeShaking)
            {
                shaking = false;
            }
            float actShakingSize = ShakingSize;
            //smoothShake
            if (smoothShakeStart_End)
            {
                if (TimeShaking < MaxTimeShaking / 5)//beggining
                {
                    float prog = TimeShaking / (MaxTimeShaking / 5);
                    actShakingSize = Mathf.Lerp(0, ShakingSize, prog);
                }
                else if (TimeShaking >= (MaxTimeShaking / 5) && TimeShaking <= (MaxTimeShaking - (MaxTimeShaking / 5)))
                {
                    actShakingSize = ShakingSize;
                }
                else if (TimeShaking > (MaxTimeShaking - (MaxTimeShaking / 5)))
                {
                    float timeStartEnd = (MaxTimeShaking - (MaxTimeShaking / 5));
                    float prog = (TimeShaking - timeStartEnd) / (MaxTimeShaking - timeStartEnd);
                    actShakingSize = Mathf.Lerp(ShakingSize, 0, prog);
                }
            }
            else
            {
                actShakingSize = ShakingSize;
            }

            if (NextShakeTime >= MaxNextShakeTime)
            {
                NextShakeTime = 0;
                float shakedX = Random.Range(-actShakingSize, actShakingSize);
                float shakedY = Random.Range(-actShakingSize, actShakingSize);
                shakedPos = new Vector2(shakedX, shakedY);
            }
            //focusPosition = new Vector2(cameraTarget.x + shakedPos.x, cameraTarget.y + shakedPos.y);
            TimeShaking += Time.deltaTime;
            NextShakeTime += Time.deltaTime;
        }
    }
}
