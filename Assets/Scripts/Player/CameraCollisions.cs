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
}
