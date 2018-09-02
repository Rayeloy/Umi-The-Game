using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller3D))]
public class PlayerMovement : MonoBehaviour
{
    public CameraControler myCamera;
    public Transform rotateObj;

    public GameController.controllerName contName;
    [HideInInspector]
    public Controller3D controller;

    [HideInInspector]
    public bool noInput = false;
    [Header("SPEED")]
    Vector3 objectiveVel;
    Vector3 currentVel;
    public float maxMoveSpeed = 10.0f;
    [Tooltip("Maximum speed that you can travel at horizontally when hit by someone")]
    public float maxKnockbackSpeed = 300f;
    float currentSpeed = 0;
    bool moving = false;//true if player tries to move with left joystick
    [Header("ACCELERATIONS")]
    public float accel = 2.0f;
    public float breakAcc = -2.0f;
    //public float breakAccOnHit = -2.0f;
    float gravity;
    [Header("JUMP")]
    public float jumpHeight = 4f;
    public float jumpApexTime = 0.4f;
    float jumpVelocity;


    private void Awake()
    {
        currentSpeed = 0;
        noInput = false;
    }
    private void Start()
    {
        controller = GetComponent<Controller3D>();
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);
        jumpVelocity = Mathf.Abs(gravity * jumpApexTime);
        print("Gravity = " + gravity + "; Jump Velocity = " + jumpVelocity);
    }
    int frameCounter = 0;
    private void Update()
    {
        if (controller.collisions.above || controller.collisions.below)
        {
            currentVel.y = 0;
        }
        //print("FRAME NUMBER " + frameCounter);
        frameCounter++;
        ProcessStun();

        HorizontalMovement();
        UpdateFacingDir();
        VerticalMovement();

        //print("CurrentVel = " + currentVel);
        controller.Move(currentVel * Time.deltaTime);
    }

    void VerticalMovement()
    {
        currentVel.y += gravity * Time.deltaTime;
        if (Input.GetButtonDown(contName + "A"))
        {
            print("JUMP");
            startJump();
        }

    }

    void HorizontalMovement()
    {
        CalculateMoveDir();//Movement direction
        float actAccel = moving && currentSpeed < maxMoveSpeed ? accel : breakAcc;
        float finalMaxMoveSpeed = noInput ? float.MaxValue : maxMoveSpeed;

        currentSpeed = currentSpeed + actAccel * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);

        if (hitRecieved)
        {
            print("HIT: Current Vel = " + currentVel);
            currentVel = currentVel + knockback;
            print("HIT: knockback = " + knockback + "; Current Vel = " + currentVel);
            currentSpeed = currentVel.magnitude;
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
            //controlar velocidad máx?
            hitRecieved = false;
        }
        else
        {
            if (moving)//MOVING JOYSTICK
            {
                objectiveVel = new Vector3(currentMovDir.x, 0, currentMovDir.z);
                currentVel = currentVel + objectiveVel;
                Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                if (horizontalVel.magnitude >= maxMoveSpeed)
                {
                    horizontalVel = horizontalVel.normalized * maxMoveSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                }
            }
            else//BREAKING (NOT MOVING JOYSTICK)
            {
                Vector3 aux = currentVel.normalized * currentSpeed;
                currentVel = new Vector3(aux.x, currentVel.y, aux.z);
            }
        }
    }

    void startJump()
    {
        if (controller.collisions.below)
        {
            currentVel.y = jumpVelocity;
        }
    }

    [HideInInspector]
    public Vector3 currentFacingDir = Vector3.forward;
    [HideInInspector]
    public float facingAngle = 0;
    void UpdateFacingDir()
    {
        switch (myCamera.camMode)
        {
            case CameraControler.cameraMode.Fixed:
                facingAngle = transform.localRotation.eulerAngles.y;
                break;
            case CameraControler.cameraMode.Free:
                float theta = rotateObj.localRotation.y * Mathf.Deg2Rad;
                float cs = Mathf.Cos(theta);
                float sn = Mathf.Sin(theta);
                float px = Vector3.forward.x * cs - Vector3.forward.z * sn;
                float py = Vector3.forward.x * sn + Vector3.forward.z * cs;
                currentFacingDir = new Vector3(px, 0, py).normalized;
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                break;
        }

    }
    public void RotateCharacter(float rotSpeed)
    {
        switch (myCamera.camMode)
        {
            case CameraControler.cameraMode.Fixed:
                Vector3 point1 = transform.position;
                Vector3 point2 = new Vector3(point1.x, point1.y + 1, point1.z);
                Vector3 dir = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
                transform.Rotate(dir, rotSpeed * Time.deltaTime);
                break;
            case CameraControler.cameraMode.Free:
                float angle = Mathf.Acos(((0 * currentMovDir.x) + (1 * currentMovDir.z)) / (1 * currentMovDir.magnitude)) * Mathf.Rad2Deg;
                angle = currentMovDir.x < 0 ? -angle : angle;
                //print("ANGULO = " + angle);
                rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
                break;
        }

    }
    [HideInInspector]
    public Vector3 currentMovDir;
    float joystickAngle;
    float deadzone = 0.25f;

    public void CalculateMoveDir()
    {
        float horiz = Input.GetAxis(contName + "H");
        float vert = -Input.GetAxis(contName + "V");
        // Check that they're not BOTH zero - otherwise
        // dir would reset because the joystick is neutral.
        Vector3 temp = new Vector3(horiz, 0, vert);
        if (temp.magnitude >= deadzone && !noInput)
        {
            moving = true;
            currentMovDir = temp;
            currentMovDir.Normalize();
            switch (myCamera.camMode)
            {
                case CameraControler.cameraMode.Free:
                    //Calculate looking dir of camera
                    Vector3 camPos = myCamera.transform.GetChild(0).position;
                    Vector3 myPos = transform.position;
                    Vector3 camDir = new Vector3(myPos.x - camPos.x, 0, myPos.z - camPos.z).normalized;
                    // ANGLE OF JOYSTICK
                    joystickAngle = Mathf.Acos(((0 * currentMovDir.x) + (1 * currentMovDir.z)) / (1 * currentMovDir.magnitude)) * Mathf.Rad2Deg;
                    joystickAngle = (horiz > 0) ? -joystickAngle : joystickAngle;
                    //print("currentMovDir = " + currentMovDir + "; camDir = " + camDir + "; joystickAngle = " + joystickAngle);
                    //rotate camDir joystickAngle degrees
                    float theta = joystickAngle * Mathf.Deg2Rad;
                    float cs = Mathf.Cos(theta);
                    float sn = Mathf.Sin(theta);
                    float px = camDir.x * cs - camDir.z * sn;
                    float py = camDir.x * sn + camDir.z * cs;
                    currentMovDir = new Vector3(px, 0, py).normalized;
                    //print("FINAL currentMovDir = " + currentMovDir);
                    RotateCharacter(0);
                    break;
                case CameraControler.cameraMode.Fixed:
                    break;
            }
            //corrections...
            //rotate angle -90 degrees
            /*float theta = 90 * Mathf.Deg2Rad;
            float cs = Mathf.Cos(theta);
            float sn = Mathf.Sin(theta);
            float px = currentMovDir.x * cs - currentMovDir.z * sn;
            float py = currentMovDir.x * sn + currentMovDir.z * cs;
            currentMovDir = new Vector3(px, 0, py).normalized;*/
        }
        else
        {
            moving = false;
        }
    }

    public float maxTimeStun = 0.6f;
    float timeStun = 0;
    bool hitRecieved = false;
    Vector3 knockback;
    public void StartRecieveHit(Vector3 _knockback)
    {
        timeStun = 0;
        noInput = true;
        hitRecieved = true;
        moving = false;
        knockback = _knockback;
        print("STUNNED");
    }
    void ProcessStun()
    {
        timeStun += Time.deltaTime;
        if (timeStun >= maxTimeStun && noInput)
        {
            noInput = false;
            print("STUN END");
        }
    }

}
