using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingEditModeCameraController : MonoBehaviour
{
    PlayerActions actions;
    [Range(0,1)]
    public float deadZone = 0.2f;
    public float cameraRotationSpeed = 150;
    public float clampAngleMax = 80f;
    public float clampAngleMin = 80f;

    float rotY, rotX = 0;
    Vector3 targetCamRot;
    Vector3 currentCamRot;

    Camera myCamera;


    private void Awake()
    {
        myCamera = GetComponentInChildren<Camera>();
        myCamera.gameObject.SetActive(false);
    }

    public void Activate(Vector3 cameraBasePos)
    {
        transform.position = cameraBasePos;
        myCamera.gameObject.SetActive(true);
        actions = GameInfo.instance.myControls;
        currentCamRot = targetCamRot = Vector3.zero;
    }

    public void DeActivate()
    {
        myCamera.gameObject.SetActive(false);
        actions = null;
        //currentCamRot = targetCamRot = Vector3.zero;
    }

    public void RotateCamera()
    {
        float inputX = actions.RightJoystick.X;
        float inputZ = -actions.RightJoystick.Y;
                                                                                                                       
                                               
        Vector3 input = new Vector2(inputX, inputZ);
        if (!actions.isKeyboard && input.magnitude < deadZone) inputX = inputZ =0;

        Quaternion localRotation = Quaternion.Euler(0, 0, 0);

        rotY += inputX * cameraRotationSpeed * Time.deltaTime;
        rotX += inputZ * cameraRotationSpeed * Time.deltaTime;
        rotX = Mathf.Clamp(rotX, clampAngleMin, clampAngleMax);
        //localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        //currentCamRot = targetCamRot;
        targetCamRot = new Vector3(rotX, rotY, 0.0f); ;
        currentCamRot = targetCamRot;


        //float step = cameraMoveSpeed * Time.deltaTime;
        //transform.position = Vector3.MoveTowards(transform.position, currentCamPos, step);
        transform.rotation = Quaternion.Euler(currentCamRot.x, currentCamRot.y, currentCamRot.z);
    }
}
