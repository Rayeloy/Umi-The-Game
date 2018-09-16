using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller3D))]
public class PlayerMovement : MonoBehaviour
{
    public CameraControler myCamera;
    public Transform rotateObj;
    public MeshRenderer Body;
    public Material teamBlueMat;
    public Material teamRedMat;
    PlayerCombat myPlayerCombat;

    public GameController.controllerName contName;
    public Team team = Team.blue;
    [HideInInspector]
    public Controller3D controller;

    public enum Team
    {
        red,
        blue
    }
    [HideInInspector]
    public MoveState moveSt = MoveState.NotMoving;
    public enum MoveState
    {
        Moving,
        NotMoving,//Not stunned, breaking
        Knockback,//Stunned
        MovingBreaking,//Moving but reducing speed by breakAcc till maxMovSpeed
        Boost
    }
    [HideInInspector]
    public bool noInput = false;
    Vector3 objectiveVel;
    Vector3 currentVel;
    [Header("SPEED")]
    public float maxMoveSpeed = 10.0f;
    float maxMoveSpeed2; // is the max speed from which we aply the joystick sensitivity value
    float currentMaxMoveSpeed = 10.0f; // its the final max speed, after the joyjoystick sensitivity value
    [Tooltip("Maximum speed that you can travel at horizontally when hit by someone")]
    public float maxKnockbackSpeed = 300f;
    float currentSpeed = 0;
    public float maxSpeedInWater = 5f;
    public float maxVerticalSpeedInWater = 3f;
    [Header("BOOST")]
    public float boostSpeed = 20f;
    public float boostCD = 5f;
    public float boostDuration = 1f;
    float boostTime = 0f;
    bool boostReady = true;
    [Header("ACCELERATIONS")]
    public float initialAcc = 2.0f;
    public float breakAcc = -2.0f;
    public float movingAcc = 2.0f;
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
        controller = GetComponent<Controller3D>();
        myPlayerCombat = GetComponent<PlayerCombat>();
    }
    private void Start()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);
        jumpVelocity = Mathf.Abs(gravity * jumpApexTime);
        print("Gravity = " + gravity + "; Jump Velocity = " + jumpVelocity);
        Body.material = team == Team.blue ? teamBlueMat : teamRedMat;
        currentMaxMoveSpeed = maxMoveSpeed2 = maxMoveSpeed;
    }
    int frameCounter = 0;
    public void KonoUpdate()
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
        myPlayerCombat.KonoUpdate();
        controller.collisions.ResetAround();
    }

    void VerticalMovement()
    {
        currentVel.y += gravity * Time.deltaTime;
        if (inWater)
        {
            currentVel.y = Mathf.Clamp(currentVel.y, -maxVerticalSpeedInWater, float.MaxValue);
        }
        if (Input.GetButtonDown(contName + "A"))
        {
            //print("JUMP");
            StartJump();
        }

    }
    
    void HorizontalMovement()
    {
        if (moveSt != MoveState.Knockback)
        {
            CalculateMoveDir();//Movement direction
        }
        if (Input.GetButtonDown(contName+"Y"))
        {
            StartBoost();
        }
        ProcessBoostCD();

        maxMoveSpeed2 = maxMoveSpeed;
        ProcessWater();
        if (joystickSens >= 0.88 || joystickSens > 1) joystickSens = 1;
        currentMaxMoveSpeed = (joystickSens / 1) * maxMoveSpeed2;
        float actAccel = moveSt==MoveState.Moving && currentSpeed < currentMaxMoveSpeed ? initialAcc : breakAcc;

        currentSpeed = currentSpeed + actAccel * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        if (moveSt == MoveState.Moving && horizontalVel.magnitude > currentMaxMoveSpeed)
        {
            //print("horizontalVel.magnitude = " + horizontalVel.magnitude + "; currentMaxMoveSpeed = " + currentMaxMoveSpeed);
            moveSt = MoveState.MovingBreaking;
        }
        //print("MoveState = " + moveSt);
        switch (moveSt)
        {
            case MoveState.Moving:
                currentVel = currentVel + currentMovDir * movingAcc;
                horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                if (horizontalVel.magnitude > currentMaxMoveSpeed)
                {
                    horizontalVel = horizontalVel.normalized * currentMaxMoveSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                }
                break;
            case MoveState.NotMoving:
                Vector3 aux = currentVel.normalized * currentSpeed;
                currentVel = new Vector3(aux.x, currentVel.y, aux.z);
                break;
            case MoveState.Boost:
                currentVel = currentVel + currentMovDir * movingAcc;
                horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                horizontalVel = horizontalVel.normalized * boostSpeed;
                currentVel = new Vector3(horizontalVel.x, 0, horizontalVel.z);
                currentSpeed = boostSpeed;
                //print("BOOST -> CURRENT VEL = " + currentVel.ToString("F4"));
                break;
            case MoveState.Knockback:
                currentVel = currentVel + knockback;
                currentSpeed = currentVel.magnitude;
                currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
                //print("currentVel = " + currentVel.ToString("F4"));
                moveSt = MoveState.NotMoving;
                break;
            case MoveState.MovingBreaking:
                Vector3 finalDir = currentVel + currentMovDir * movingAcc;
                horizontalVel = new Vector3(finalDir.x, 0, finalDir.z);
                currentVel = horizontalVel.normalized * currentSpeed;
                currentVel.y = finalDir.y;
                //print("CURRENT SPEED = " + currentSpeed);
                break;
        }
    }
    [HideInInspector]
    public Vector3 currentMovDir;
    float joystickAngle;
    float deadzone = 0.15f;
    float joystickSens = 0;
    public void CalculateMoveDir()
    {
        float horiz = Input.GetAxisRaw(contName + "H");
        float vert = -Input.GetAxisRaw(contName + "V");
        //print("H = " + horiz + "; V = " + vert);
        // Check that they're not BOTH zero - otherwise
        // dir would reset because the joystick is neutral.
        Vector3 temp = new Vector3(horiz, 0, vert);
        joystickSens = temp.magnitude;
        //print("temp.magnitude = " + temp.magnitude);
        if (temp.magnitude >= deadzone && !noInput)
        {
            moveSt = MoveState.Moving;
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
                    float angle = -facingAngle * Mathf.Deg2Rad;
                    float cos = Mathf.Cos(angle);
                    float sin = Mathf.Sin(angle);
                    float x = temp.x * cos - temp.z * sin;
                    float y = temp.x * sin + temp.z * cos;
                    currentMovDir = new Vector3(x, 0, y).normalized;
                    //print("facingAngle ="+facingAngle+"; currentMovDir= " + currentMovDir);
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
            moveSt = MoveState.NotMoving;
        }
    }

    void StartJump()
    {
        if (controller.collisions.below && (!inWater || inWater && controller.collisions.around))
        {
            currentVel.y = jumpVelocity;
        }
    }

    void StartBoost()
    {
        if (!noInput && boostReady && !haveFlag&& !inWater)
        {
            moveSt = MoveState.Boost;
            boostTime = 0f;
            boostReady = false;
        }

    }

    void ProcessBoostCD()
    {
        if (!boostReady)
        {
            boostTime += Time.deltaTime;
            if(boostTime < boostDuration)
            {
                moveSt = MoveState.Boost;
            }
            if (boostTime >= boostCD)
            {
                boostReady = true;
            }
        }
    }

    [HideInInspector]
    public Vector3 currentFacingDir = Vector3.forward;
    [HideInInspector]
    public float facingAngle = 0;
    void UpdateFacingDir()//change so that only rotateObj rotates, not whole body
    {
        switch (myCamera.camMode)
        {
            case CameraControler.cameraMode.Fixed:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
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
                rotateObj.Rotate(dir, rotSpeed * Time.deltaTime);
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
    public float maxTimeStun = 0.6f;
    float timeStun = 0;
    Vector3 knockback;
    public void StartRecieveHit(Vector3 _knockback, PlayerMovement attacker, float _maxTimeStun)
    {
        moveSt = MoveState.Knockback;
        maxTimeStun = _maxTimeStun;
        timeStun = 0;
        noInput = true;
        knockback = _knockback;

        //Give FLAG
        if (haveFlag)
        {
            attacker.PickFlag(flag);
            flag = null;
            haveFlag = false;
        }
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

    [HideInInspector]
    public bool haveFlag = false;
    [HideInInspector]
    public GameObject flag = null;
    public void PickFlag(GameObject _flag)
    {
        flag = _flag;
        flag.transform.SetParent(rotateObj);
        flag.transform.localPosition = new Vector3(0, 0, -1);
        haveFlag = true;
    }

    void Die()
    {
        if (haveFlag)
        {
            GameController.instance.RespawnFlag(flag.GetComponent<Flag>());
            flag = null;
            haveFlag = false;
        }
        GameController.instance.RespawnPlayer(this);
    }

    bool inWater = false;
    void EnterWater()
    {
        inWater = true;
        if (haveFlag)
        {
            GameController.instance.RespawnFlag(flag.GetComponent<Flag>());
            flag = null;
            haveFlag = false;
        }
    }
    void ExitWater()
    {
        inWater = false;
    }
    void ProcessWater()
    {
        if (inWater)
        {
            controller.AroundCollisions();
            maxMoveSpeed2 = maxSpeedInWater;
        }
    }

    void CheckWinGame(Respawn respawn)
    {
        if (haveFlag && team == respawn.team)
        {
            GameController.instance.GameOver(team);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        switch (col.tag)
        {
            case "KillTrigger":
                Die();
                break;
            case "Flag":
                PickFlag(col.gameObject);
                break;
            case "Respawn":
                //print("I'm " + name + " and I touched a respawn");
                CheckWinGame(col.GetComponent<Respawn>());
                break;
            case "Water":
                EnterWater();
                break;
        }
    }
    private void OnTriggerExit(Collider col)
    {
        switch (col.tag)
        {
            case "Water":
                ExitWater();
                break;
        }
    }
}
