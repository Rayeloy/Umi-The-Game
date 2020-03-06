using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerCMF : MonoBehaviour
{
    #region Variables
    [Header("Referencias")]
    public PlayerMovementCMF myPlayerMov;
    public Transform myPlayer;
    public Transform cameraFollowObj;
    public Transform myCamera;

    public cameraMode camMode = cameraMode.Fixed;

    [Tooltip("Valor de 0 a 1 que indica la zona muerta del joystick de la camara, siendo 0 una zona muerta nula, y 1 todo el joystick entero.")]
    public float deadZone = 0.2f;
    [Header("FIXED CAMERA")]
    public float clampAngleMaxFixed = 40f;
    public float clampAngleMinFixed = -40f;
    public float rotSpeed = 2.0f;
    [Header("SHOULDER CAMERA")]
    public Vector3 originalCamPosSho;
    public float clampAngleMaxSho = 40f;
    public float clampAngleMinSho = -40f;
    public float rotSpeedSho = 120.0f;
    [Header("FREE CAMERA")] //------------------------ 3rd person FREE CAMERA
    public Vector3 originalCamPosFree;
    public float cameraMoveSpeed = 120.0f;
    public float clampAngleMax = 80f;
    public float clampAngleMin = 80f;
    public float inputSensitivity = 150f;
    public GameObject cameraObj;
    public GameObject placerObj;
    public float camDistanceXToPlayer;
    public float camDistanceYToPlayer;
    public float camDistanceZToPlayer;
    float mouseX = 0;
    float mouseY = 0;
    float finalInputX;
    float finalInputZ;
    public float smoothX;
    public float smoothY;
    private float rotY = 0.0f;
    private float rotX = 0.0f;
    //------------------------

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    Vector3 targetCamPos;
    Vector3 originalPos;
    Vector3 currentCamPos;

    Vector3 targetCamRot;
    Vector3 originalRot;
    Vector3 currentCamRot;

    //For the camera inside CameraBase (the real camera)
    [Header("Camera inside CameraBase")]
    [HideInInspector]
    public Vector3 targetMyCamPos;
    Vector3 currentMyCamPos;
    float smoothMyCamX, smoothMyCamY, smoothMyCamZ;
    public float smoothCamMoveTime = 0.2f;

    [Header("Smoothing")]
    float smoothPosSpeedX, smoothPosSpeedY, smoothPosSpeedZ;
    public float smoothPositioningTime = 0.2f;
    float smoothRotSpeedX, smoothRotSpeedY, smoothRotSpeedZ;
    public float smoothRotationTime = 0.2f;

    //reset camera pos and rot "R3"
    [Header("Reset Camera")]
    bool resetingCamera = false;
    public float cameraResetVerticalAngle;

    //Switch camera
    bool switching = false;
    float timeSwitching;

    //CanSeeHookPoint
    [HideInInspector]
    public bool canSeeHookPoint;

    #endregion

    #region Funciones de MonoBehaviour
    public void KonoAwake()
    {
        originalPos = myCamera.localPosition;
        originalRot = myCamera.localRotation.eulerAngles;
        switch (camMode)
        {
            case cameraMode.Fixed:
                GetComponentInChildren<CameraCollisionsCMF>().enabled = true;
                //myCamera.SetParent(myPlayerMov.rotateObj);
                myCamera.localPosition = originalPos;
                //myCamera.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case cameraMode.Free:
                //myCamera.SetParent(transform);
                GetComponentInChildren<CameraCollisionsCMF>().enabled = true;
                transform.localRotation = myPlayerMov.rotateObj.localRotation;
                //print("I'm " + gameObject.name + " and my local rotation = " + transform.localRotation.eulerAngles);
                myCamera.localPosition = originalCamPosFree;
                Debug.Log("Cam konoAwake, new postion :" + myCamera.localPosition);
                break;
            case cameraMode.FixedFree:
                break;
            case cameraMode.Shoulder:
                GetComponentInChildren<CameraCollisionsCMF>().enabled = true;
                originalPos = originalCamPosSho;
                myCamera.localPosition = originalPos;
                break;
        }
        currentMyCamPos = targetMyCamPos = originalCamPosFree;
        Debug.Log("Awake: targetMyCamPos = " + targetMyCamPos.ToString("F6"));
        currentCamPos = targetCamPos = transform.position;
        currentCamRot = targetCamRot = transform.rotation.eulerAngles;

        myCamera.GetComponent<CameraCollisionsCMF>().KonoAwake();

    }
    // Use this for initialization
    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    public void LateUpdate()
    {
        if (myPlayerMov.gC.playing)
        {
            //Vector3 screenPos = myCamera.GetComponent<Camera>().WorldToScreenPoint((myPlayerMov.gC as GameController_FlagMode).flags[0].transform.position);
            //Debug.Log("Flag is " + screenPos.ToString("F4") + " pixels from the left");

            float inputX = myPlayerMov.actions.RightJoystick.X;//Input.GetAxis(myPlayerMov.contName + "H2");
            float inputZ = -myPlayerMov.actions.RightJoystick.Y;//Input.GetAxis(myPlayerMov.contName + "V2");
            //mouseX = Input.GetAxis("Mouse X");                                                                             
            //mouseY = Input.GetAxis("Mouse Y");
            Vector3 input = new Vector2(inputX, inputZ);
            if (myPlayerMov.actions.Device != null && input.magnitude < deadZone)
            {
                inputX = 0;
                inputZ = 0;
            }
            finalInputX = inputX + mouseX;
            finalInputZ = inputZ + mouseY;

            Quaternion localRotation = Quaternion.Euler(0, 0, 0);
            switch (camMode)
            {
                case cameraMode.Fixed:
                    //yaw += speedH * Input.GetAxis(myPlayerMov.contName + "H2");
                    //pitch -= speedV * Input.GetAxis(myPlayerMov.contName + "V2");

                    Quaternion followObjRot = cameraFollowObj.rotation;
                    rotX += finalInputZ * rotSpeed * Time.deltaTime;
                    rotX = Mathf.Clamp(rotX, clampAngleMinFixed, clampAngleMaxFixed);

                    myPlayerMov.RotateCharacter(rotSpeed * finalInputX);

                    localRotation = followObjRot;
                    localRotation = Quaternion.Euler(rotX, myPlayerMov.rotateObj.localRotation.eulerAngles.y, 0);
                    targetCamRot = localRotation.eulerAngles;
                    //SmoothRot();
                    currentCamRot = targetCamRot;
                    transform.rotation = Quaternion.Euler(currentCamRot.x, currentCamRot.y, currentCamRot.z);

                    //targetCamPos = myPlayerMov.rotateObj.TransformPoint(originalPos);
                    targetCamPos = cameraFollowObj.position;
                    //SmoothPos();
                    currentCamPos = targetCamPos;
                    transform.position = currentCamPos;
                    //print("myCamera.localPosition = " + myCamera.localPosition);
                    break;
                case cameraMode.Shoulder:
                    followObjRot = cameraFollowObj.rotation;
                    rotX += finalInputZ * rotSpeedSho * Time.deltaTime;
                    rotX = Mathf.Clamp(rotX, clampAngleMinSho, clampAngleMaxSho);

                    myPlayerMov.RotateCharacter(rotSpeedSho * finalInputX);

                    localRotation = followObjRot;
                    localRotation = Quaternion.Euler(rotX, myPlayerMov.rotateObj.localRotation.eulerAngles.y, 0);
                    targetCamRot = localRotation.eulerAngles;
                    targetCamPos = cameraFollowObj.position;

                    if (switching)
                    {
                        SmoothRot();
                        SmoothPos();
                        timeSwitching += Time.deltaTime;
                        if (timeSwitching >= smoothPositioningTime + 0.2f)
                        {
                            switching = false;
                        }
                        print("SWITCHING CAMERA: targetCamPos= " + targetCamPos + "; currentCamPos = " + currentCamPos);
                    }
                    else
                    {
                        currentCamPos = targetCamPos;
                        currentCamRot = targetCamRot;
                    }
                    transform.rotation = Quaternion.Euler(currentCamRot.x, currentCamRot.y, currentCamRot.z);
                    transform.position = currentCamPos;
                    //print("myCamera.localPosition = " + myCamera.localPosition);
                    break;
                case cameraMode.Free:
                    if (myPlayerMov.actions.R3.WasPressed)
                    {
                        print("R3 pulsado");
                        StartResetCamera();
                    }
                    if ((finalInputX != 0 || finalInputZ != 0) && resetingCamera)
                    {
                        print("STOP RESETING CAMERA");
                        StopResetCamera();
                    }

                    if (resetingCamera)
                    {
                        print("reseting camera");
                        SmoothRotReset();
                        targetCamPos = cameraFollowObj.position;
                        currentCamPos = targetCamPos;
                        float step = cameraMoveSpeed * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, currentCamPos, step);
                        //SmoothPos();
                        if (currentCamRot == targetCamRot)
                        {
                            print("STOP RESETING CAMERA");
                            StopResetCamera();
                        }
                    }
                    else
                    {
                        rotY += finalInputX * inputSensitivity * Time.deltaTime;
                        rotX += finalInputZ * inputSensitivity * Time.deltaTime;
                        rotX = Mathf.Clamp(rotX, clampAngleMin, clampAngleMax);
                        localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
                        //currentCamRot = targetCamRot;
                        targetCamRot = localRotation.eulerAngles;
                        if (switching)
                        {
                            SmoothRot();
                            SmoothPos();
                            timeSwitching += Time.deltaTime;
                            if (timeSwitching >= smoothPositioningTime + 0.5f)
                            {
                                switching = false;
                            }
                        }
                        else
                        {
                            targetCamPos = cameraFollowObj.position;
                            currentCamPos = targetCamPos;
                            currentCamRot = targetCamRot;
                        }

                        float step = cameraMoveSpeed * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, currentCamPos, step);
                    }
                    transform.rotation = Quaternion.Euler(currentCamRot.x, currentCamRot.y, currentCamRot.z);
                    break;
                case cameraMode.FixedFree:
                    break;
            }
            if (myCamera.GetComponentInChildren<CameraCollisionsCMF>().enabled)
            {
                myCamera.GetComponent<CameraCollisionsCMF>().KonoUpdate();
            }
            SmoothCameraMove();
            //print("I'm " + gameObject.name + " and my local rotation = " + transform.localRotation.eulerAngles);
        }
        Debug.Log("Cam new postion :" + myCamera.localPosition);
    }
    #endregion

    #region Funciones

    #region Instant Position/Rotation
    public void InstantPositioning()
    {
        targetCamPos = cameraFollowObj.position;
        currentCamPos = targetCamPos;
        transform.position = currentCamPos;
    }

    public void InstantRotation()
    {
        if (camMode == cameraMode.Free)
        {
            rotY = myPlayerMov.rotateObj.localRotation.eulerAngles.y;
        }
        targetCamRot = myPlayerMov.rotateObj.localRotation.eulerAngles;
        currentCamRot = targetCamRot;
        transform.localRotation = Quaternion.Euler(currentCamRot.x, currentCamRot.y, currentCamRot.z); ;
        //print("I'm " + gameObject.name + " and my rotateObj.localRotation = " + myPlayerMov.rotateObj.localRotation.eulerAngles+"; my local rotation = "+transform.localRotation.eulerAngles);
        myCamera.localPosition = originalCamPosFree;
    }
    #endregion

    #region Smoothing

    void SmoothPos()
    {
        currentCamPos.x = Mathf.SmoothDamp(currentCamPos.x, targetCamPos.x, ref smoothPosSpeedX, smoothPositioningTime);
        currentCamPos.y = Mathf.SmoothDamp(currentCamPos.y, targetCamPos.y, ref smoothPosSpeedY, smoothPositioningTime);
        currentCamPos.z = Mathf.SmoothDamp(currentCamPos.z, targetCamPos.z, ref smoothPosSpeedZ, smoothPositioningTime);
    }

    void SmoothRot()
    {
        currentCamRot.x = Mathf.SmoothDamp(currentCamRot.x, targetCamRot.x, ref smoothRotSpeedX, smoothRotationTime);
        currentCamRot.y = Mathf.SmoothDamp(currentCamRot.y, targetCamRot.y, ref smoothRotSpeedY, smoothRotationTime);
        currentCamRot.z = Mathf.SmoothDamp(currentCamRot.z, targetCamRot.z, ref smoothRotSpeedZ, smoothRotationTime);
    }

    void SmoothCameraMove()//para la camara de dentro
    {
        currentMyCamPos.x = Mathf.SmoothDamp(currentMyCamPos.x, targetMyCamPos.x, ref smoothMyCamX, smoothCamMoveTime);
        currentMyCamPos.y = Mathf.SmoothDamp(currentMyCamPos.y, targetMyCamPos.y, ref smoothMyCamY, smoothCamMoveTime);
        currentMyCamPos.z = Mathf.SmoothDamp(currentMyCamPos.z, targetMyCamPos.z, ref smoothMyCamZ, smoothCamMoveTime);
        myCamera.localPosition = currentMyCamPos;
    }
    #endregion

    public void SwitchCamera(cameraMode cameraMode)
    {
        camMode = cameraMode;
        switch (camMode)
        {
            case cameraMode.Fixed:
                GetComponentInChildren<CameraCollisionsCMF>().enabled = true;
                targetMyCamPos = originalPos;
                myPlayerMov.rotateObj.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
                break;
            case cameraMode.Free:
                GetComponentInChildren<CameraCollisionsCMF>().enabled = true;
                //transform.localRotation = myPlayerMov.rotateObj.localRotation;
                rotY = myPlayerMov.rotateObj.localRotation.eulerAngles.y;
                targetMyCamPos = originalCamPosFree;
                break;
            case cameraMode.FixedFree:
                break;
            case cameraMode.Shoulder:
                GetComponentInChildren<CameraCollisionsCMF>().enabled = true;
                originalPos = originalCamPosSho;
                targetMyCamPos = originalPos;
                myPlayerMov.rotateObj.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
                break;
        }
        timeSwitching = 0;
        //switching = true;
        myCamera.GetComponent<CameraCollisionsCMF>().ResetData();
    }

    #region Reset Camera
    void StartResetCamera()
    {
        float yRot = myPlayerMov.rotateObj.localRotation.eulerAngles.y;
        targetCamRot = transform.localRotation.eulerAngles;
        targetCamRot.y = TransformTargetRotationForReset(currentCamRot.y, yRot);
        targetCamRot.x = TransformTargetRotationForReset(currentCamRot.x, cameraResetVerticalAngle);
        //targetMyCamPos = originalPos;
        resetingCamera = true;
        print("TARGET CAM ROT = " + targetCamRot.ToString("F4") + "; CURRENT CAM ROT = " + currentCamRot.ToString("F4"));
    }

    void StopResetCamera()
    {
        resetingCamera = false;
        rotY = currentCamRot.y;
        rotX = currentCamRot.x;
    }

    void SmoothRotReset()
    {
        float xRot = TransformCurrentRotationForReset(currentCamRot.x, targetCamRot.x);
        currentCamRot.x = Mathf.SmoothDamp(xRot, targetCamRot.x, ref smoothRotSpeedX, smoothRotationTime);
        float yRot = TransformCurrentRotationForReset(currentCamRot.y, targetCamRot.y);
        currentCamRot.y = Mathf.SmoothDamp(yRot, targetCamRot.y, ref smoothRotSpeedY, smoothRotationTime);
        currentCamRot.z = Mathf.SmoothDamp(currentCamRot.z, targetCamRot.z, ref smoothRotSpeedZ, smoothRotationTime);
        print("NEW CURRENT CAM ROT = " + currentCamRot.x + "; TARGET CAM ROT = " + targetCamRot.ToString("F4"));
    }

    float TransformTargetRotationForReset(float current, float target)
    {
        float angleDif = Mathf.Abs(current - target);
        if (angleDif > 180)
        {
            if (current <= 180)
            {
                return target - 360;
            }
        }
        return target;
    }

    float TransformCurrentRotationForReset(float current, float target)
    {
        float angleDif = current - target;
        if (angleDif > 180)
        {
            if (current > 180)
            {
                print("Transforming current angle : " + current + " -> " + (360 - current));
                return current - 360;
            }
        }
        return current;
    }

    #endregion

    void LookAtPlayer()
    {
        //vector to player
        Vector3 camPoint = transform.position;
        Vector3 playerPoint = myPlayerMov.gameObject.transform.position;
        //Vector3 lookPoint = new Vector3(playerPoint.x, playerPoint.y + 1, playerPoint.z);
        //transform.LookAt(playerPoint);
    }
    #endregion
}
