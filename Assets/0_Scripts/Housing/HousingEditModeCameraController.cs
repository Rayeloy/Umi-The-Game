using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EditCameraMode
{
    ZoomedOut,
    ZoomedIn
}

public enum EditCameraDirection
{
    ZPos=0,
    ZNeg=2,
    XPos=1,
    XNeg=3
}

public class HousingEditModeCameraController : MonoBehaviour
{
    PlayerActions actions;
    [Range(0, 1)]
    public float deadZone = 0.2f;

    JoyStickControls myRightJoyStickControls;


    //POSITION
    Vector3 centerCamPos;
    Vector3 targetCamPos;
    Vector3 currentCamPos;
    [Header(" - Camera Position - ")]
    public float camMoveSpeed = 20;
    float currentCamDist;

    //ROTATION
    [Header(" - Camera Rotation - ")]
    public float cameraRotationSpeed = 150;
    public float clampAngleMax = 80f;
    public float clampAngleMin = 80f;
    public float smoothRotMaxTime = 0.5f;
    float smoothRotTime = 0;
    float rotY, rotX = 0;
    Vector3 targetCamRot;
    Vector3 currentCamRot;
    float startRotY = 0;

    //ZOOM
    [Header(" - Camera Zoom - ")]
    public float zoomedInCamZoom = 2;
    float zoomedOutCamZoom = 0;
    float currentCamZoom;
    float targetCamZoom;
    public float cameraZoomSpeed = 10;

    GameObject myCameraObject;
    [HideInInspector]
    public EditCameraDirection currentCameraDir = EditCameraDirection.ZPos;
    EditCameraMode currentCameraMode = EditCameraMode.ZoomedOut;


    private void Start()
    {
        myCameraObject = GetComponentInChildren<Camera>().gameObject;
        myCameraObject.gameObject.SetActive(false);
        Debug.Log("EditCameraBase: My camera = " + myCameraObject);
        myRightJoyStickControls = new JoyStickControls(GameInfo.instance.myControls.RightJoystick, deadZone);
    }

    public void Activate(Vector3 cameraBasePos, Vector3 houseFloorCenter, float _zoomedOutCamDist)
    {
        if (myCameraObject == null)
        {
            Debug.LogError("EditCameraBase: My camera is null");
            return;
        }
        myCameraObject.SetActive(true);

        currentCamRot = targetCamRot = Vector3.zero;
        transform.rotation = Quaternion.Euler(currentCamRot.x, currentCamRot.y, currentCamRot.z);
        startRotY = currentCamRot.y;
        centerCamPos = targetCamPos = currentCamPos = cameraBasePos;
        transform.position = currentCamPos;
        currentCamZoom = targetCamZoom = zoomedOutCamZoom = _zoomedOutCamDist;
        myCameraObject.transform.localPosition = new Vector3(0, 0, currentCamZoom);

        myCameraObject.transform.LookAt(houseFloorCenter);
        Debug.Log("houseFloorCenter = " + houseFloorCenter.ToString("F4") + "; inside camera Rotation = " + myCameraObject.transform.localRotation.eulerAngles.ToString("F4"));

        actions = GameInfo.instance.myControls;
        currentCameraMode = EditCameraMode.ZoomedOut;

        smoothRotTime = 0;
    }

    public void DeActivate()
    {
        myCameraObject.gameObject.SetActive(false);
        actions = null;
        //currentCamRot = targetCamRot = Vector3.zero;
    }


    public void KonoLateUpdate()
    {
        //Debug.LogWarning("Edit Camera is in mode " + currentCameraMode);
        switch (currentCameraMode)
        {
            case EditCameraMode.ZoomedOut:
                //Inputs
                if (myRightJoyStickControls.RightWasPressed)
                {
                    RotateCameraRight();
                }
                else if (myRightJoyStickControls.LeftWasPressed)
                {
                    RotateCameraLeft();
                }

                break;
            case EditCameraMode.ZoomedIn:
                float inputX = 0;// actions.RightJoystick.X;
                float inputZ = -actions.RightJoystick.Y;


                Vector3 input = new Vector2(inputX, inputZ);
                if (!actions.isKeyboard && input.magnitude < deadZone) inputX = inputZ = 0;

                Quaternion localRotation = Quaternion.Euler(0, 0, 0);

                rotY += inputX * cameraRotationSpeed * Time.deltaTime;
                rotX += inputZ * cameraRotationSpeed * Time.deltaTime;
                rotX = Mathf.Clamp(rotX, clampAngleMin, clampAngleMax);
                targetCamRot = new Vector3(rotX, rotY, 0.0f);
                break;
        }

        //SMOOTH ROTATION/MOVEMENT/ZOOM
        SmoothCamRot();
        SmoothCamPos();
        SmoothCamZoom();

        //APPLY ROTATION/MOVEMENT/ZOOM
        //Debug.Log(" currentCamRot = " + currentCamRot.ToString("F4"));
        transform.rotation = Quaternion.Euler(currentCamRot.x, currentCamRot.y, currentCamRot.z);
        transform.position = currentCamPos;
        myCameraObject.transform.localPosition = new Vector3(0, 0, currentCamZoom);

        myRightJoyStickControls.ResetJoyStick();
    }

    void SmoothCamPos()
    {
        if (currentCamPos == targetCamPos) return;
        currentCamPos = Vector3.Lerp(currentCamPos, targetCamPos, camMoveSpeed);
    }

    float smoothRotVal = 0;
    void SmoothCamRot()
    {
        //if (smoothRotTime >= smoothRotMaxTime) return;
        if (smoothRotVal >= 1) return;
        smoothRotTime += Time.deltaTime;
        smoothRotVal = Mathf.Clamp01(smoothRotTime / smoothRotMaxTime);
        float y = EasingFunction.EaseOutBack(startRotY, targetCamRot.y, smoothRotVal);
        Debug.Log(" targetCamRot.y= "+ targetCamRot.y + "; value = " + smoothRotVal + "; y = " + y);
currentCamRot = new Vector3(currentCamRot.x, y, currentCamRot.z);
    }

    void SmoothCamZoom()
    {
        if (currentCamZoom == targetCamZoom) return;
        currentCamZoom = Mathf.Lerp(currentCamZoom, targetCamZoom, cameraZoomSpeed);
    }

    void RotateCameraRight()
    {
        Debug.Log("Rotate Camera Right");
        smoothRotTime = 0;
        smoothRotVal = 0;
        startRotY = currentCamRot.y;
        float newRotY = targetCamRot.y - 90;
        //newRotY += newRotY < 0 ? 360 : 0;
        targetCamRot.y = newRotY;
        currentCameraDir--;
        currentCameraDir += (int)currentCameraDir < 0 ? 4 : 0;
    }

    void RotateCameraLeft()
    {
        Debug.Log("Rotate Camera Left");
        smoothRotTime = 0;
        smoothRotVal = 0;
        startRotY = currentCamRot.y;
        float newRotY = targetCamRot.y + 90;
        //newRotY += newRotY > 360 ? -360 : 0;
        targetCamRot.y = newRotY;
        currentCameraDir++;
        currentCameraDir += (int)currentCameraDir > 3 ? -4 : 0;
    }

    void SwitchCameraMode()
    {
        switch (currentCameraMode)
        {
            case EditCameraMode.ZoomedOut:
                currentCameraMode = EditCameraMode.ZoomedIn;
                //targetCamPos = selectedSlotPos;
                targetCamZoom = -zoomedInCamZoom;
                break;
            case EditCameraMode.ZoomedIn:
                currentCameraMode = EditCameraMode.ZoomedOut;
                targetCamPos = centerCamPos;
                targetCamZoom = -zoomedOutCamZoom;
                break;
        }
    }
}
