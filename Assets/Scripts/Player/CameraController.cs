﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerMovement myPlayerMov;
    public Transform myCamera;
    public Transform myPlayer;
   
    
    public cameraMode camMode = cameraMode.Fixed;
    public enum cameraMode
    {
        Fixed,
        Free,
        FixedFree,
        Shoulder
    }
    public GameObject cameraFollowObj;
    [Tooltip("Valor de 0 a 1 que indica la zona muerta del joystick de la camara, siendo 0 una zona muerta nula, y 1 todo el joystick entero.")]
    public float deadZone = 0.2f;
    [Header("FIXED CAMERA")]
    public float clampAngleMaxFixed=40f;
    public float clampAngleMinFixed = -40f;
    public float rotSpeed = 2.0f;
    [Header("SHOULDER CAMERA")]
    public Vector3 originalCamPosSho;
    public float clampAngleMaxSho= 40f;
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

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    Vector3 targetCamPos;
    Vector3 originalPos;
    Vector3 currentCamPos;

    Vector3 targetCamRot;
    Vector3 originalRot;
    Vector3 currentCamRot;

    public void KonoAwake()
    {
        originalPos = myCamera.localPosition;
        originalRot = myCamera.localRotation.eulerAngles;
        switch (camMode)
        {
            case cameraMode.Fixed:
                GetComponentInChildren<CameraCollisions>().enabled = true;
                //myCamera.SetParent(myPlayerMov.rotateObj);
                myCamera.localPosition = originalPos;
                //myCamera.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case cameraMode.Free:
                //myCamera.SetParent(transform);
                GetComponentInChildren<CameraCollisions>().enabled = true;
                transform.localRotation = myPlayerMov.rotateObj.localRotation;
                //print("I'm " + gameObject.name + " and my local rotation = " + transform.localRotation.eulerAngles);
                myCamera.localPosition = originalCamPosFree;
                break;
            case cameraMode.FixedFree:
                break;
            case cameraMode.Shoulder:
                GetComponentInChildren<CameraCollisions>().enabled = true;
                originalPos = originalCamPosSho;
                myCamera.localPosition = originalPos;
                break;
        }
        currentMyCamPos = targetMyCamPos = myCamera.localPosition;
        currentCamPos = targetCamPos = transform.position;
        currentCamRot = targetCamRot = transform.rotation.eulerAngles;

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
        if (GameController.instance.playing)
        {
            float inputX = myPlayerMov.Actions.CamMovement.X;//Input.GetAxis(myPlayerMov.contName + "H2");
            float inputZ = -myPlayerMov.Actions.CamMovement.Y;//Input.GetAxis(myPlayerMov.contName + "V2");
            //mouseX = Input.GetAxis("Mouse X");                                                                             
            //mouseY = Input.GetAxis("Mouse Y");
            Vector3 input = new Vector2(inputX, inputZ);
            if (myPlayerMov.Actions.Device!=null && input.magnitude < deadZone)
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

                    Quaternion followObjRot = cameraFollowObj.transform.rotation;
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
                    targetCamPos = cameraFollowObj.transform.position;
                    //SmoothPos();
                    currentCamPos = targetCamPos;
                    transform.position = currentCamPos;
                    //print("myCamera.localPosition = " + myCamera.localPosition);
                    break;
                case cameraMode.Shoulder:
                    followObjRot = cameraFollowObj.transform.rotation;
                    rotX += finalInputZ * rotSpeed * Time.deltaTime;
                    rotX = Mathf.Clamp(rotX, clampAngleMinSho, clampAngleMaxSho);

                    myPlayerMov.RotateCharacter(rotSpeedSho * finalInputX);

                    localRotation = followObjRot;
                    localRotation = Quaternion.Euler(rotX, myPlayerMov.rotateObj.localRotation.eulerAngles.y, 0);
                    targetCamRot = localRotation.eulerAngles;
                    targetCamPos = cameraFollowObj.transform.position;

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
                    if (myPlayerMov.Actions.R3.WasPressed)
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
                        targetCamPos = cameraFollowObj.transform.position;
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
                            currentCamPos = targetCamPos;
                            currentCamRot = targetCamRot;
                        }
                        targetCamPos = cameraFollowObj.transform.position;
                        float step = cameraMoveSpeed * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, currentCamPos, step);
                    }
                    transform.rotation = Quaternion.Euler(currentCamRot.x, currentCamRot.y, currentCamRot.z);
                    break;
                case cameraMode.FixedFree:
                    break;
            }
            if (myCamera.GetComponentInChildren<CameraCollisions>().enabled)
            {
                myCamera.GetComponent<CameraCollisions>().KonoUpdate();
            }
            SmoothCameraMove();
            //print("I'm " + gameObject.name + " and my local rotation = " + transform.localRotation.eulerAngles);
        }
    }

    public void InstantPositioning()
    {
        targetCamPos = cameraFollowObj.transform.position;
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

    float smoothPosSpeedX, smoothPosSpeedY, smoothPosSpeedZ;
    public float smoothPositioningTime = 0.2f;
    void SmoothPos()
    {
        currentCamPos.x = Mathf.SmoothDamp(currentCamPos.x, targetCamPos.x, ref smoothPosSpeedX, smoothPositioningTime);
        currentCamPos.y = Mathf.SmoothDamp(currentCamPos.y, targetCamPos.y, ref smoothPosSpeedY, smoothPositioningTime);
        currentCamPos.z = Mathf.SmoothDamp(currentCamPos.z, targetCamPos.z, ref smoothPosSpeedZ, smoothPositioningTime);
    }

    float smoothRotSpeedX, smoothRotSpeedY, smoothRotSpeedZ;
    public float smoothRotationTime = 0.2f;
    void SmoothRot()
    {
        currentCamRot.x = Mathf.SmoothDamp(currentCamRot.x, targetCamRot.x, ref smoothRotSpeedX, smoothRotationTime);
        currentCamRot.y = Mathf.SmoothDamp(currentCamRot.y, targetCamRot.y, ref smoothRotSpeedY, smoothRotationTime);
        currentCamRot.z = Mathf.SmoothDamp(currentCamRot.z, targetCamRot.z, ref smoothRotSpeedZ, smoothRotationTime);
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

    [HideInInspector]
    public Vector3 targetMyCamPos;
    Vector3 currentMyCamPos;
    float smoothMyCamX, smoothMyCamY, smoothMyCamZ;
    public float smoothCamMoveTime = 0.2f;
    void SmoothCameraMove()//para la camara de dentro
    {
        currentMyCamPos.x = Mathf.SmoothDamp(currentMyCamPos.x, targetMyCamPos.x, ref smoothMyCamX, smoothCamMoveTime);
        currentMyCamPos.y = Mathf.SmoothDamp(currentMyCamPos.y, targetMyCamPos.y, ref smoothMyCamY, smoothCamMoveTime);
        currentMyCamPos.z = Mathf.SmoothDamp(currentMyCamPos.z, targetMyCamPos.z, ref smoothMyCamZ, smoothCamMoveTime);
        myCamera.localPosition = currentMyCamPos;
    }

    bool switching = false;
    float timeSwitching;
    public void SwitchCamera(cameraMode cameraMode)
    {
        camMode = cameraMode;
        switch (camMode)
        {
            case cameraMode.Fixed:
                GetComponentInChildren<CameraCollisions>().enabled = true;
                targetMyCamPos = originalPos;
                myPlayerMov.rotateObj.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
                break;
            case cameraMode.Free:
                GetComponentInChildren<CameraCollisions>().enabled = false;
                //transform.localRotation = myPlayerMov.rotateObj.localRotation;
                rotY = myPlayerMov.rotateObj.localRotation.eulerAngles.y;
                targetMyCamPos = originalCamPosFree;
                break;
            case cameraMode.FixedFree:
                break;
            case cameraMode.Shoulder:
                GetComponentInChildren<CameraCollisions>().enabled = true;
                originalPos = originalCamPosSho;
                targetMyCamPos = originalPos;
                myPlayerMov.rotateObj.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
                break;
        }
        timeSwitching = 0;
        //switching = true;
        myCamera.GetComponent<CameraCollisions>().ResetData();
    }

    bool resetingCamera=false;
    [Header("Reset Camera")]
    public float cameraResetVerticalAngle;
    void StartResetCamera()
    {
        float yRot = myPlayerMov.rotateObj.localRotation.eulerAngles.y;
        targetCamRot = transform.localRotation.eulerAngles;
        targetCamRot.y = TransformTargetRotationForReset(currentCamRot.y, yRot);
        targetCamRot.x = TransformTargetRotationForReset(currentCamRot.x, cameraResetVerticalAngle);
        //targetMyCamPos = originalPos;
        resetingCamera = true;
        print("TARGET CAM ROT = " + targetCamRot.ToString("F4")+"; CURRENT CAM ROT = "+currentCamRot.ToString("F4"));
    }
    void StopResetCamera()
    {
        resetingCamera = false;
        rotY = currentCamRot.y;
        rotX = currentCamRot.x;
    }

    float TransformTargetRotationForReset(float current, float target)
    {
        float angleDif = Mathf.Abs(current - target);
        if (angleDif > 180)
        {
            if(current<=180)
            {
                return target-360;
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
                return current-360;
            }
        }
        return current;
    }

    void LookAtPlayer()
    {
        //vector to player
        Vector3 camPoint = transform.position;
        Vector3 playerPoint = myPlayerMov.gameObject.transform.position;
        //Vector3 lookPoint = new Vector3(playerPoint.x, playerPoint.y + 1, playerPoint.z);
        //transform.LookAt(playerPoint);
    }
}
