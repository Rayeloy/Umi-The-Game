using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControler : MonoBehaviour {
    public PlayerMovement myPlayerMov;
    public Transform camera1;
    public Transform Player1;

    public cameraMode camMode = cameraMode.Fixed;
    public enum cameraMode
    {
        Fixed,
        Free,
        FixedFree
    }

    //------------------------ 3rd person FREE CAMERA
    public float CameraMoveSpeed = 120.0f;
    public GameObject CameraFollowObj;
    Vector3 FollowPOS;
    public float clampAngle = 80f;
    public float inputSensitivity = 150f;
    public GameObject CameraObj;
    public GameObject PlacerObj;
    public float camDistanceXToPlayer;
    public float camDistanceYToPlayer;
    public float camDistanceZToPlayer;
    public float mouseX;
    public float mouseY;
    public float finalInputX;
    public float finalInputZ;
    public float smoothX;
    public float smoothY;
    private float rotY = 0.0f;
    private float rotX = 0.0f;
    //------------------------



    public float speedH = 2.0f;
    public float speedV = 2.0f;
    public float rotSpeed = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;


    private void Awake()
    {
        switch (camMode)
        {
            case cameraMode.Fixed:
                GetComponentInChildren<CameraCollisions>().enabled = false;
                camera1.SetParent(Player1);
                break;
            case cameraMode.Free:
                camera1.SetParent(transform);
                GetComponentInChildren<CameraCollisions>().enabled = true;
                break;
            case cameraMode.FixedFree:
                break;
        }
    }
    // Use this for initialization
    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;


    }

    // Update is called once per frame
    void LateUpdate()
    {
        float inputX = Input.GetAxis(myPlayerMov.contName + "H2");
        float inputZ = Input.GetAxis(myPlayerMov.contName + "V2");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        finalInputX = inputX + mouseX;
        finalInputZ = inputZ + mouseY;
        Quaternion localRotation = Quaternion.Euler(0, 0, 0);
        switch (camMode)
        {
            case cameraMode.Fixed:
                yaw += speedH * Input.GetAxis(myPlayerMov.contName + "H2");
                pitch -= speedV * Input.GetAxis(myPlayerMov.contName + "V2");

                rotX += finalInputZ * rotSpeed * Time.deltaTime;
                rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
                localRotation = camera1.transform.localRotation;
                localRotation =Quaternion.Euler(rotX, localRotation.eulerAngles.y, localRotation.eulerAngles.z);
                camera1.transform.localRotation = localRotation;
                //reset to 0,0 after some time
                //Rotate character with mouse X

                //transform.eulerAngles = new Vector3(pitch, , 0.0f);
                //LookAtPlayer();

                myPlayerMov.RotateCharacter(rotSpeed * finalInputX);
                break;
            case cameraMode.Free:
                rotY += finalInputX * inputSensitivity * Time.deltaTime;
                rotX += finalInputZ * inputSensitivity * Time.deltaTime;

                rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
                localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
                transform.rotation = localRotation;
                FreeCameraUpdate();
                break;
            case cameraMode.FixedFree:
                break;
        }

    }

    void FreeCameraUpdate()
    {
        Transform target = CameraFollowObj.transform;

        float step = CameraMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
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
