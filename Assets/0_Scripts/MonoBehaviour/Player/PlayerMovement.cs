using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


#region ----[ PUBLIC ENUMS ]----
public enum Team
{
    A,// Blue - Green
    B,// Red - Pink
    none
}
public enum MoveState
{
    None = 0,
    Moving = 1,
    NotMoving = 2,//Not stunned, breaking
    Knockback = 3,//Stunned
    MovingBreaking = 4,//Moving but reducing speed by breakAcc till maxMovSpeed
    Hooked = 5,
    Boost = 6,
    FixedJump = 7,
    NotBreaking = 8
}
public enum JumpState
{
    Jumping,
    Breaking,//Emergency stop
    none,
    wallJumping
}
#endregion

#region ----[ REQUIRECOMPONENT ]----
[RequireComponent(typeof(Controller3D))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerAnimation))]
[RequireComponent(typeof(PlayerWeapons))]
[RequireComponent(typeof(PlayerHook))]
#endregion
public class PlayerMovement : MonoBehaviourPunCallbacks
{

    #region ----[ VARIABLES FOR DESIGNERS ]----
    [Header("Referencias")]
    public PlayerCombat myPlayerCombat;
    public PlayerWeapons myPlayerWeap;
    public PlayerPickups myPlayerPickups;
    public PlayerAnimation myPlayerAnimation;
    public PlayerAnimation_01 myPlayerAnimation_01;
    public PlayerHook myPlayerHook;
    public Controller3D controller;
    public PlayerBody myPlayerBody;
    public PlayerObjectDetection myPlayerObjectDetection;
    public PlayerModel myPlayerModel;
    public PlayerVFX myPlayerVFX;

    public GameControllerBase gC;
    public CameraController myCamera;
    public Transform cameraFollow;
    public Transform rotateObj;



    //CONTROLES
    public PlayerActions Actions { get; set; }

    //[Header("Body and body color")]

    //VARIABLES DE MOVIMIENTO
    [HideInInspector]
    public float maxMoveSpeed = 10;
    float maxMoveSpeed2; // is the max speed from which we aply the joystick sensitivity value
    float currentMaxMoveSpeed = 10.0f; // its the final max speed, after the joyjoystick sensitivity value

    [Header("ROTATION")]
    public float rotationSpeed = 1;
    float currentRotationSpeed = 0;
    [Range(0, 180)]
    public float instantRotationMaxAngle = 130;
    //public float rotationSpeedFixedCam = 250;
    float targetRotation = 0;
    float currentRotation;

    [Header("SPEED")]
    [Tooltip("Maximum speed that you can travel at horizontally when hit by someone")]
    public float maxKnockbackSpeed = 300f;
    public float maxAimingSpeed = 5f;
    public float maxHookingSpeed = 3.5f;
    public float maxGrapplingSpeed = 3.5f;
    public float maxSpeedInWater = 5f;
    public float maxVerticalSpeedInWater = 3f;

    [Header("BOOST")]
    public float boostSpeed = 20f;
    public float boostCapacity = 1f;
    [Tooltip("1 = 1 capacity per second")]
    public float boostDepletingSpeed = 1f;
    [Tooltip("1 = 1 capacity per second")]
    public float boostRechargingSpeed = 0.3f;
    [HideInInspector]
    public float boostCurrentFuel = 1;
    public float boostCDMaxTime = 0.3f;
    float boostCDTime = 0;
    bool boostCDStarted = false;
    [Tooltip("0-> none; 1-> All the fuel")]
    [Range(0, 1)]
    public float boostFuelLostOnStart = 0.15f;
    [Tooltip("0-> none; 1-> All the fuel")]
    [Range(0, 1)]
    public float boostMinFuelNeeded = 0.2f;
    [HideInInspector]
    public bool boostReady
    {
        get
        {
            return ((boostCurrentFuel > boostCapacity * boostMinFuelNeeded) && !boostCDStarted);
        }
    }
    Vector3 boostDir;

    [Header("ACCELERATIONS")]
    public float initialAcc = 30;
    public float breakAcc = -30;
    public float movingAcc = 2.0f;
    public float airMovingAcc = 0.5f;
    [Tooltip("Acceleration used when breaking from a boost.")]
    public float hardBreakAcc = -120f;
    [Tooltip("Breaking negative acceleration that is used under the effects of a knockback (stunned). Value is clamped to not be higher than breakAcc.")]
    public float knockbackBreakAcc = -30f;
    //public float breakAccOnHit = -2.0f;
    [HideInInspector]
    public float gravity;

    [Header("JUMP")]
    public float jumpHeight = 4f;
    public float jumpApexTime = 0.4f;
    float jumpVelocity;
    float timePressingJump = 0.0f;
    float maxTimePressingJump;
    [Tooltip("How fast the 'stop jump early' stops in the air. This value is multiplied by the gravity and then applied to the vertical speed.")]
    public float breakJumpForce = 2.0f;
    [Tooltip("During how much part of the jump (in time to reach the apex) is the player able to stop the jump. 1 is equals to the whole jump, and 0.5 is equals the half of the jump time.")]
    public float pressingJumpActiveProportion = 0.7f;
    public float maxTimeJumpInsurance = 0.2f;
    float timeJumpInsurance = 0;
    bool jumpInsurance;
    bool jumpingFromWater;

    [Header("WALLJUMP")]
    public float wallJumpVelocity = 10f;
    public float wallJumpSlidingAcc = -2.5f;
    public float wallJumpMinHeightPercent = 55;
    public float stopWallMaxTime = 0.5f;
    float stopWallTime = 0;
    bool wallJumping = false;
    Vector3 anchorPoint;
    Vector3 wallNormal;
    [Tooltip("Vertical angle in which the player wall-jumps.")]
    [Range(0, 89)]
    public float wallJumpAngle = 30;
    [Tooltip("Minimum horizontal angle in which the player wall-jumps. This number ranges from 0 to 90. 0 --> parallel to the wall; 90 --> perpendicular to the wall")]
    [Range(0, 89)]
    public float wallJumpMinHorizAngle = 30;

    [Header("----- ONLINE VARIABLES ----")]
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    #endregion

    #region ----[ PROPERTIES ]----
    //Referencias que no se asignan en el inspector
    [HideInInspector]
    public Camera myUICamera;
    [HideInInspector]
    public PlayerHUD myPlayerHUD;
    //BOOL PARA PERMITIR O BLOQUEAR INPUTS
    [HideInInspector]
    public bool noInput = false;
    //EQUIPO
    [HideInInspector]
    public Team team = Team.A;
    //ESTADO DE MOVIMIENTO Y SALTO
    [HideInInspector]
    public MoveState moveSt = MoveState.NotMoving;
    [HideInInspector]
    public JumpState jumpSt = JumpState.none;
    [HideInInspector]
    public Vector3 currentVel;
    [HideInInspector]
    public float currentSpeed = 0;
    [HideInInspector]
    public Vector3 currentMovDir;
    [HideInInspector]
    Vector3 currentInputDir;
    [HideInInspector]
    public bool wallJumpAnim = false;
    [HideInInspector]
    public Vector3 currentFacingDir = Vector3.forward;
    [HideInInspector]
    public float facingAngle = 0;
    [HideInInspector]
    public Vector3 currentCamFacingDir = Vector3.zero;
    [HideInInspector]
    public float maxTimeStun = 0.6f;
    [HideInInspector]
    public bool haveFlag = false;
    [HideInInspector]
    public Flag flag = null;
    [HideInInspector]
    public bool inWater = false;
    [HideInInspector]
    public bool online = false;
    [HideInInspector]
    public Vector3 spawnPosition;
    [HideInInspector]
    public Quaternion spawnRotation;
    #endregion

    #region ----[ VARIABLES ]----    
    //WALL JUMP
    float wallJumpRadius;
    float walJumpConeHeight = 1;
    float lastWallAngle = -1;
    GameObject lastWall;
    Raycast.Axis wallJumpRaycastAxis = Raycast.Axis.none;
    int rows = 5;
    int columns = 5;
    float rowsSpacing;
    float columnsSpacing;
    LayerMask auxLM;

    int frameCounter = 0;

    //JOYSTICK INPUT
    float joystickAngle;
    float deadzone = 0.2f;
    float joystickSens = 0;

    //KNOCKBACK AND STUN
    float timeStun = 0;
    bool stunned;
    bool knockBackDone;
    Vector3 knockback;

    //FIXED JUMPS (Como el trampolín)
    bool fixedJumping;
    bool fixedJumpDone;
    float noMoveMaxTime;
    float noMoveTime;

    //HOOK
    [HideInInspector]
    public bool hooked;
    bool hooking;

    //GRAPPLE
    bool grappling = false;


    //WATER
    [HideInInspector]
    public bool jumpedOutOfWater = true;

    BufferedInput[] inputsBuffer;//Eloy: para Juan: esta variable iría aquí? o a "Variables", JUAN: A variables mejor
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region AWAKE

    public void Awake()
    {
        online = PhotonNetwork.IsConnected;
    }

    public void KonoAwake()
    {
        maxMoveSpeed = 10;
        currentSpeed = 0;
        currentRotationSpeed = rotationSpeed;
        boostCurrentFuel = boostCapacity;
        noInput = false;
        lastWallAngle = 0;
        SetupInputsBuffer();
        myPlayerHook.myCameraBase = myCamera;

        //PLAYER MODEL
        myPlayerModel.SwitchTeam(team);

        //WALLJUMP
        rows = 5;
        columns = 5;
        rowsSpacing = ((controller.coll.bounds.size.y * wallJumpMinHeightPercent) / 100) / (rows - 1);
        columnsSpacing = controller.coll.bounds.size.x / (columns - 1);
        auxLM = LayerMask.GetMask("Stage");

        PlayerAwakes();

        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (online)
        {
            if (photonView.IsMine)
            {
                PlayerMovement.LocalPlayerInstance = this.gameObject;
            }
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        //DontDestroyOnLoad(this.gameObject);

    }

    //todos los konoAwakes
    void PlayerAwakes()
    {
        myPlayerHUD.KonoAwake();
        myPlayerCombat.KonoAwake();
        //myPlayerAnimation.KonoAwake();
        myPlayerAnimation_01.KonoAwake();
        myPlayerHook.KonoAwake();
        myPlayerWeap.KonoAwake();
        myPlayerBody.KonoAwake();
        myPlayerVFX.KonoAwake();
    }
    #endregion

    #region START
    public void KonoStart()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);
        jumpVelocity = Mathf.Abs(gravity * jumpApexTime);
        maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;
        wallJumpRadius = Mathf.Tan(wallJumpAngle * Mathf.Deg2Rad) * walJumpConeHeight;
        print("wallJumpRaduis = " + wallJumpRadius + "; tan(wallJumpAngle)= " + Mathf.Tan(wallJumpAngle * Mathf.Deg2Rad));
        wallJumpMinHorizAngle = Mathf.Clamp(wallJumpMinHorizAngle, 0, 90);
        print("Gravity = " + gravity + "; Jump Velocity = " + jumpVelocity);

        currentMaxMoveSpeed = maxMoveSpeed2 = maxMoveSpeed;
        knockbackBreakAcc = Mathf.Clamp(knockbackBreakAcc, -float.MaxValue, breakAcc);//menos de break Acc lo haría ver raro

        ShowFlagFlow();
        EquipWeaponAtStart();

        PlayerStarts();
    }
    private void PlayerStarts()
    {
        myPlayerCombat.KonoStart();
        myPlayerHUD.KonoStart();
    }

    #endregion

    #region UPDATE
    public void KonoUpdate()
    {
        if (Actions.Start.WasPressed) gC.PauseGame(Actions);

        if ((controller.collisions.above || controller.collisions.below) && !hooked)
        {
            currentVel.y = 0;
        }
        //print("FRAME NUMBER " + frameCounter);
        frameCounter++;
        UpdateFlagLightBeam();
        ProcessInputsBuffer();

        //print("CurrentVel 1= " + currentVel);
        HorizontalMovement();
        //print("CurrentVel 2= " + currentVel);
        //print("vel = " + currentVel.ToString("F4"));
        UpdateFacingDir();
        VerticalMovement();
        //print("vel = " + currentVel.ToString("F4"));

        ProcessWallJump();//IMPORTANTE QUE VAYA ANTES DE LLAMAR A "MOVE"

        //Debug.Log("currentVel = " + currentVel + "; Time.deltaTime = " + Time.deltaTime + "; currentVel * Time.deltaTime = " + (currentVel * Time.deltaTime) + "; Time.fixedDeltaTime = " + Time.fixedDeltaTime);

        //print("CurrentVel 3= " + currentVel);
        controller.Move(currentVel * Time.deltaTime);
        myPlayerCombat.KonoUpdate();
        controller.collisions.ResetAround();

        //myPlayerAnimation.KonoUpdate();
        myPlayerAnimation_01.KonoUpdate();
        myPlayerWeap.KonoUpdate();
        myPlayerHook.KonoUpdate();
    }

    public void KonoFixedUpdate()
    {
    }
    #endregion

    #endregion

    #region ----[ CLASS FUNCTIONS ]----

    public void ResetPlayer()
    {
        ExitWater();
        jumpSt = JumpState.none;
        myPlayerHook.ResetHook();
        if (haveFlag)
        {
            flag.SetAway(false);
        }
        boostCurrentFuel = boostCapacity;
        //myPlayerWeap.DropWeapon();
        //controller.collisionMask = LayerMask.GetMask("Stage", "WaterFloor", "SpawnWall");
    }

    void EquipWeaponAtStart()
    {
        //print("EQUIP WEAPON AT START");
        switch (team)
        {
            case Team.A:
                //print("EQUIP BLUE WEAPON");
                myPlayerWeap.PickupWeapon(gC.startingWeaponBlue);
                break;
            case Team.B:
                //print("EQUIP RED WEAPON");
                myPlayerWeap.PickupWeapon(gC.startingWeaponRed);
                break;
        }
    }

    #region INPUTS BUFFERING

    void SetupInputsBuffer()
    {
        inputsBuffer = new BufferedInput[gC.allBufferedInputs.Length];
        for (int i = 0; i < gC.allBufferedInputs.Length; i++)
        {
            inputsBuffer[i] = new BufferedInput(gC.allBufferedInputs[i]);
        }
    }

    void ProcessInputsBuffer()
    {
        for (int i = 0; i < inputsBuffer.Length; i++)
        {
            if (inputsBuffer[i].buffering)
            {
                switch (inputsBuffer[i].input.inputType)
                {
                    case PlayerInput.Jump:
                        if (StartJump(true))
                        {
                            inputsBuffer[i].StopBuffering();
                        }
                        break;
                }
                inputsBuffer[i].ProcessTime();
            }
        }
    }

    void BufferInput(PlayerInput _inputType)
    {
        bool found = false;
        for (int i = 0; i < inputsBuffer.Length && !found; i++)
        {
            if (inputsBuffer[i].input.inputType == _inputType)
            {
                inputsBuffer[i].StartBuffering();
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogError("Error: Impossible to buffer the input " + _inputType + " because there is no BufferedInput with that type in the inputsBuffered array.");
        }
    }

    #endregion

    #region MOVEMENT -----------------------------------------------
    public void SetVelocity(Vector3 vel)
    {
        currentVel = vel;
        currentSpeed = currentVel.magnitude;
    }

    public void CalculateMoveDir()
    {
        if (!noInput)
        {
            float horiz = Actions.RightJoystick.X;//Input.GetAxisRaw(contName + "H");
            float vert = Actions.RightJoystick.Y;//-Input.GetAxisRaw(contName + "V");
                                                 // Check that they're not BOTH zero - otherwise dir would reset because the joystick is neutral.
            Vector3 temp = new Vector3(horiz, 0, vert);
            joystickSens = temp.magnitude;
            //print("temp.magnitude = " + temp.magnitude);
            if (temp.magnitude >= deadzone)
            {
                joystickSens = joystickSens >= 0.88f ? 1 : joystickSens;//Eloy: esto evita un "bug" por el que al apretar el joystick 
                                                                        //contra las esquinas no da un valor total de 1, sino de 0.9 o así
                moveSt = MoveState.Moving;
                currentInputDir = temp;
                currentInputDir.Normalize();
                switch (myCamera.camMode)
                {
                    case cameraMode.Fixed:
                        currentInputDir = currentMovDir = RotateVector(-facingAngle, temp);
                        break;
                    case cameraMode.Shoulder:
                        currentInputDir = currentMovDir = RotateVector(-facingAngle, temp);
                        break;
                    case cameraMode.Free:
                        Vector3 camDir = (transform.position - myCamera.transform.GetChild(0).position).normalized;
                        camDir.y = 0;
                        // ANGLE OF JOYSTICK
                        joystickAngle = Mathf.Acos(((0 * currentInputDir.x) + (1 * currentInputDir.z)) / (1 * currentInputDir.magnitude)) * Mathf.Rad2Deg;
                        joystickAngle = (horiz > 0) ? -joystickAngle : joystickAngle;
                        //rotate camDir joystickAngle degrees
                        currentInputDir = RotateVector(joystickAngle, camDir);
                        //print("joystickAngle= " + joystickAngle + "; camDir= " + camDir.ToString("F4") + "; currentMovDir = " + currentMovDir.ToString("F4"));
                        RotateCharacter();
                        break;
                }
            }
            else
            {
                joystickSens = 1;
                moveSt = MoveState.NotMoving;
                currentInputDir = Vector3.zero;
            }
        }
    }

    void HorizontalMovement()
    {
        float finalMovingAcc = 0;
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        #region//------------------------------------------------ DECIDO TIPO DE MOVIMIENTO --------------------------------------------
        #region//----------------------------------------------------- Efecto externo --------------------------------------------
        if (stunned)
        {
            ProcessStun();
        }
        else if (hooked)
        {
            ProcessHooked();
        }
        else if (fixedJumping)// FIXED JUMPING
        {

            ProcessFixedJump();
        }

        ProcessBoost();

        #endregion
        #region //----------------------------------------------------- Efecto interno --------------------------------------------
        if (!hooked && !fixedJumping && moveSt != MoveState.Boost)
        {
            //------------------------------------------------ Direccion Joystick, aceleracion, maxima velocidad y velocidad ---------------------------------
            //------------------------------- Joystick Direction -------------------------------
            CalculateMoveDir();//Movement direction
            if (!myPlayerCombat.aiming && Actions.R2.WasPressed)//Input.GetButtonDown(contName + "RB"))
            {
                StartBoost();
            }
            //------------------------------- Max Move Speed -------------------------------
            maxMoveSpeed2 = maxMoveSpeed;
            ProcessWater();
            ProcessAiming();
            ProcessHooking();
            currentMaxMoveSpeed = (joystickSens / 1) * maxMoveSpeed2;
            if (currentSpeed > currentMaxMoveSpeed && (moveSt == MoveState.Moving || moveSt == MoveState.NotMoving))
            {
                moveSt = MoveState.MovingBreaking;
            }
            //------------------------------- Acceleration -------------------------------
            float actAccel;
            switch (moveSt)
            {
                case MoveState.Moving:
                    actAccel = initialAcc;
                    break;
                case MoveState.MovingBreaking:
                    actAccel = hardBreakAcc;//breakAcc * 3;
                    break;
                case MoveState.Knockback:
                    actAccel = knockbackBreakAcc;
                    break;
                default:
                    actAccel = breakAcc;
                    break;
            }
            finalMovingAcc = controller.collisions.below ? movingAcc : airMovingAcc; //Turning accleration
            //------------------------------- Speed ------------------------------ -
            currentSpeed = currentSpeed + actAccel * Time.deltaTime;
            float maxSpeedClamp = moveSt == MoveState.Moving ? currentMaxMoveSpeed : maxKnockbackSpeed;
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeedClamp);
        }
        #endregion
        #endregion
        #region//------------------------------------------------ PROCESO EL TIPO DE MOVIMIENTO DECIDIDO ---------------------------------
        //print("MoveState = " + moveSt + "; currentSpeed = " + currentSpeed + "; currentMaxMoveSpeed = " + currentMaxMoveSpeed);
        if (jumpSt != JumpState.wallJumping)
        {
            switch (moveSt)
            {
                case MoveState.Moving: //MOVING WITH JOYSTICK
                    currentVel = currentVel + currentMovDir * finalMovingAcc;
                    horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                    if (horizontalVel.magnitude > currentMaxMoveSpeed)
                    {
                        horizontalVel = horizontalVel.normalized * currentMaxMoveSpeed;
                        SetVelocity(new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z));
                    }
                    //print("Speed = " + currentSpeed+"; currentMaxMoveSpeed = "+currentMaxMoveSpeed);
                    break;
                case MoveState.NotMoving: //NOT MOVING JOYSTICK
                    Vector3 aux = currentVel.normalized * currentSpeed;
                    currentVel = new Vector3(aux.x, currentVel.y, aux.z);
                    break;
                case MoveState.Boost:
                    if (controller.collisions.collisionHorizontal)//BOOST CONTRA PARED
                    {
                        WallBoost();
                    }
                    else//BOOST NORMAL
                    {
                        //boostDir: dirección normalizada en la que quieres hacer el boost
                        horizontalVel = new Vector3(boostDir.x, 0, boostDir.z);
                        horizontalVel = horizontalVel.normalized * boostSpeed;
                        SetVelocity(new Vector3(horizontalVel.x, 0, horizontalVel.z));
                    }
                    break;
                case MoveState.Knockback:
                    if (!knockBackDone)
                    {
                        print("KNOCKBACK");
                        currentVel = currentVel + knockback;
                        horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                        currentSpeed = horizontalVel.magnitude;
                        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
                        knockBackDone = true;
                    }
                    else
                    {
                        aux = currentVel.normalized * currentSpeed;
                        currentVel = new Vector3(aux.x, currentVel.y, aux.z);
                    }
                    //print("vel.y = " + currentVel.y);

                    break;
                case MoveState.MovingBreaking://FRENADA FUERTE
                    Vector3 finalDir = currentVel + currentMovDir * finalMovingAcc;
                    horizontalVel = new Vector3(finalDir.x, 0, finalDir.z);
                    currentVel = horizontalVel.normalized * currentSpeed;
                    currentVel.y = finalDir.y;
                    break;
                case MoveState.Hooked:
                    horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                    currentSpeed = horizontalVel.magnitude;
                    break;
                case MoveState.FixedJump:
                    currentVel = knockback;
                    horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                    currentSpeed = horizontalVel.magnitude;
                    currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
                    break;
                case MoveState.NotBreaking:
                    horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                    currentSpeed = horizontalVel.magnitude;
                    break;

            }
        }
        #endregion
    }

    void VerticalMovement()
    {

        if (!jumpedOutOfWater && !inWater && controller.collisions.below)
        {
            jumpedOutOfWater = true;

            maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;
        }
        if (lastWallAngle >= 0 && controller.collisions.below)
        {
            lastWallAngle = -1;
        }
        if (Actions.A.WasPressed)//Input.GetButtonDown(contName + "A"))
        {
            //print("JUMP");
            //Debug.Log("pene");
            StartJump();
        }

        switch (jumpSt)
        {
            case JumpState.none:
                currentVel.y += gravity * Time.deltaTime;
                break;
            case JumpState.Jumping:
                currentVel.y += gravity * Time.deltaTime;
                timePressingJump += Time.deltaTime;
                if (timePressingJump >= maxTimePressingJump)
                {
                    StopJump();
                }
                else
                {
                    if (Actions.A.WasReleased)//Input.GetButtonUp(contName + "A"))
                    {
                        jumpSt = JumpState.Breaking;
                    }
                }
                break;
            case JumpState.Breaking:
                currentVel.y += (gravity * breakJumpForce) * Time.deltaTime;
                if (currentVel.y <= 0)
                {
                    jumpSt = JumpState.none;
                }

                break;
            case JumpState.wallJumping:
                currentVel.y += wallJumpSlidingAcc * Time.deltaTime;
                break;


        }
        if (inWater)
        {
            currentVel.y = Mathf.Clamp(currentVel.y, -maxVerticalSpeedInWater, float.MaxValue);
        }
        ProcessJumpInsurance();

    }
    #endregion

    #region JUMP ---------------------------------------------------
    bool StartJump(bool calledFromBuffer = false)
    {
        bool result = false;
        if (!noInput && moveSt != MoveState.Boost)
        {
            if ((controller.collisions.below || jumpInsurance) && (!inWater || (inWater && controller.collisions.around &&
                ((gC.gameMode == GameMode.CaptureTheFlag && !(gC as GameController_FlagMode).myScoreManager.prorroga) ||
                (gC.gameMode != GameMode.CaptureTheFlag)))))
            {
                //PlayerAnimation_01.startJump = true;
                myPlayerAnimation_01.SetJump(true);
                result = true;
                currentVel.y = jumpVelocity;
                jumpSt = JumpState.Jumping;
                timePressingJump = 0;
                //myPlayerAnimation.SetJump(true);

            }
            else
            {
                /*Debug.LogWarning("Warning: Can't jump because: controller.collisions.below || jumpInsurance (" + (controller.collisions.below || jumpInsurance) +
                    ") / !inWater || (inWater && controller.collisions.around && ((gC.gameMode == GameMode.CaptureTheFlag && !ScoreManager.instance.prorroga) || (gC.gameMode != GameController.GameMode.CaptureTheFlag))) (" +
                    (!inWater || (inWater && controller.collisions.around &&
                ((gC.gameMode == GameMode.CaptureTheFlag && !(gC as GameController_FlagMode).myScoreManager.prorroga) ||
                (gC.gameMode != GameMode.CaptureTheFlag)))) + ")");*/
                result = StartWallJump();
            }
        }
        else
        {
            Debug.LogWarning("Warning: Can't jump because: player is in noInput mode(" + !noInput + ") / moveSt != Boost (" + moveSt != MoveState.Boost + ")");
        }

        if (!result && !calledFromBuffer)
        {
            BufferInput(PlayerInput.Jump);
        }
        return result;
    }

    void StopJump()
    {
        myPlayerAnimation_01.SetJump(false);
        jumpSt = JumpState.none;
        timePressingJump = 0;
    }

    void ProcessJumpInsurance()
    {
        if (!jumpInsurance)
        {
            if (controller.collisions.lastBelow && !controller.collisions.below && jumpSt == JumpState.none && jumpedOutOfWater)
            {
                //print("Jump Insurance");
                jumpInsurance = true;
                timeJumpInsurance = 0;
            }
        }
        else
        {
            timeJumpInsurance += Time.deltaTime;
            if (timeJumpInsurance >= maxTimeJumpInsurance || jumpSt == JumpState.Jumping)
            {
                jumpInsurance = false;
            }
        }

    }

    bool StartWallJump()
    {
        bool result = false;
        if (!controller.collisions.below && (!inWater || inWater && controller.collisions.around) && controller.collisions.collisionHorizontal &&
            (lastWallAngle != controller.collisions.wallAngle || lastWallAngle == controller.collisions.wallAngle && lastWall != controller.collisions.horWall) && jumpedOutOfWater)
        {
            GameObject wall = controller.collisions.horWall;
            if (wall.GetComponent<StageScript>() == null || wall.GetComponent<StageScript>().wallJumpable)
            {
                print("Wall jump");
                //PARA ORTU: PlayerAnimation_01.startJump = true;
                jumpSt = JumpState.wallJumping;
                result = true;
                //wallJumped = true;
                stopWallTime = 0;
                SetVelocity(Vector3.zero);
                wallJumping = true;
                anchorPoint = transform.position;
                wallNormal = controller.collisions.wallNormal;
                wallNormal.y = 0;
                lastWallAngle = controller.collisions.wallAngle;
                lastWall = wall;
                wallJumpRaycastAxis = controller.collisions.closestHorRaycast.axis;
                Debug.Log("WALL JUMP RAY HEIGHT PERCENTAGE : " + controller.collisions.closestHorRaycast.rayHeightPercentage + "%; wall = " + wall.name);
            }

        }
        return result;
    }

    void ProcessWallJump()//IMPORTANTE QUE VAYA ANTES DE LLAMAR A "MOVE"
    {
        if (wallJumping)
        {
            stopWallTime += Time.deltaTime;
            if (Actions.A.WasReleased || !Actions.A.IsPressed)
            {
                EndWallJump();
            }
            if (stopWallTime >= stopWallMaxTime)
            {
                StopWallJump();
            }

            float rayLength = controller.coll.bounds.extents.x + 0.3f;
            RaycastHit hit;

            Vector3 paralelToWall = new Vector3(-wallNormal.z, 0, wallNormal.x).normalized;
            Vector3 rowsOrigin = new Vector3(controller.coll.bounds.center.x, controller.coll.bounds.min.y, controller.coll.bounds.center.z);
            rowsOrigin -= paralelToWall * controller.coll.bounds.extents.x;
            Vector3 dir = -wallNormal.normalized;
            bool success = false;
            for (int i = 0; i < rows && !success; i++)
            {
                Vector3 rowOrigin = rowsOrigin + Vector3.up * rowsSpacing * i;
                for (int j = 0; j < columns && !success; j++)
                {
                    Vector3 rayOrigin = rowOrigin + paralelToWall * rowsSpacing * j;
                    Debug.Log("WallJump: Ray[" + i + "," + j + "] with origin = " + rayOrigin.ToString("F4") + "; rayLength =" + rayLength);
                    Debug.DrawRay(rayOrigin, dir * rayLength, Color.blue, 3);

                    if (Physics.Raycast(rayOrigin, dir, out hit, rayLength, auxLM, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform.gameObject == lastWall)
                        {
                            success = true;
                            Debug.Log("WallJump: Success! still walljumping!");
                        }
                        else
                        {
                            Debug.Log("WallJump: this wall (" + hit.transform.gameObject + ")is not the same wall that I started walljumping from (" + lastWall + ").");
                        }
                    }
                    else
                    {
                        Debug.Log("WallJump: no hit on ray [" + i + "," + j + "] when checking for wall!");
                    }
                }

            }
            if (!success)
            {
                StopWallJump();
            }
        }

    }

    void EndWallJump()
    {
        wallJumping = false;
        wallJumpAnim = true;
        jumpSt = JumpState.none;
        wallJumpRaycastAxis = Raycast.Axis.none;
        //CALCULATE JUMP DIR
        //LEFT OR RIGHT ORIENTATION?
        //Angle
        Vector3 circleCenter = anchorPoint + Vector3.up * walJumpConeHeight;
        Vector3 circumfPoint = CalculateReflectPoint(wallJumpRadius, wallNormal, circleCenter);
        Vector3 finalDir = (circumfPoint - anchorPoint).normalized;
        Debug.LogWarning("FINAL DIR= " + finalDir.ToString("F4"));

        currentVel = finalDir * wallJumpVelocity;
        currentSpeed = currentVel.magnitude;
        currentMovDir = new Vector3(finalDir.x, 0, finalDir.z);
        RotateCharacter(currentMovDir);

        //myPlayerAnimation.SetJump(true);

        Debug.DrawLine(anchorPoint, circleCenter, Color.white, 20);
        Debug.DrawLine(anchorPoint, circumfPoint, Color.yellow, 20);
    }

    public void StopWallJump()
    {
        print("STOP WALLJUMP");
        wallJumping = false;
        wallJumpAnim = true;
        jumpSt = JumpState.none;
    }
    #endregion

    #region  DASH ---------------------------------------------
    void WallBoost()
    {
        //CALCULATE JUMP DIR
        Vector3 circleCenter = transform.position;
        Vector3 circumfPoint = CalculateReflectPoint(1, controller.collisions.wallNormal, circleCenter);
        Vector3 finalDir = (circumfPoint - circleCenter).normalized;
        Debug.LogWarning("FINAL DIR= " + finalDir.ToString("F4"));

        currentVel = finalDir * currentVel.magnitude;
        currentSpeed = currentVel.magnitude;
        boostDir = new Vector3(finalDir.x, 0, finalDir.z);
        RotateCharacter();
    }

    void StartBoost()
    {
        if (!noInput && boostReady && !haveFlag && !inWater)
        {
            //noInput = true;
            //PARA ORTU: Variable para empezar boost

            //myPlayerAnimation_01.dash = true;
            boostCurrentFuel -= boostCapacity*boostFuelLostOnStart;
            boostCurrentFuel = Mathf.Clamp(boostCurrentFuel, 0, boostCapacity);
            moveSt = MoveState.Boost;
            if (currentInputDir != Vector3.zero)//Usando el joystick o dirección
            {
                boostDir = currentMovDir;
            }
            else//sin tocar ninguna dirección/joystick
            {
                currentMovDir = boostDir = new Vector3(currentCamFacingDir.x, 0, currentCamFacingDir.z);//nos movemos en la dirección en la que mire la cámara
                RotateCharacter(currentMovDir);
            }
            myPlayerHUD.StartCamVFX(CameraVFXType.Dash);
        }

    }

    void ProcessBoost()
    {
        if (moveSt == MoveState.Boost)
        {
            //print("PROCESS BOOST: boostTime = " + boostCurrentFuel + "; boostDuration = "+ boostCapacity);
            boostCurrentFuel -= boostDepletingSpeed * Time.deltaTime;
            boostCurrentFuel = Mathf.Clamp(boostCurrentFuel, 0, boostCapacity);
            if (boostCurrentFuel > 0)
            {
                if (Actions.R2.WasReleased)
                {
                    StopBoost();
                }
                //moveSt = MoveState.Boost;
            }
            else
            {
                StopBoost();
            }
        }
        else
        {
            if (boostCurrentFuel < boostCapacity)
            {
                ProcessBoostRecharge();
            }
            ProcessBoostCD();
        }
    }

    public void StopBoost()
    {
        if(moveSt == MoveState.Boost)
        {
            print("STOP BOOST");
            moveSt = MoveState.None;
            StartBoostCD();
            myPlayerHUD.StopCamVFX(CameraVFXType.Dash);
        }
        //noInput = false;
        //PARA ORTU: Variable para terminar boost

        //myPlayerAnimation_01.dash = false;
    }

    void ProcessBoostRecharge()
    {
        if (moveSt != MoveState.Boost && boostCurrentFuel < boostCapacity)
        {
            boostCurrentFuel += boostRechargingSpeed * Time.deltaTime;
            boostCurrentFuel = Mathf.Clamp(boostCurrentFuel, 0, boostCapacity);
        }
    }

    void StartBoostCD()
    {
        if (!boostCDStarted)
        {
            boostCDStarted = true;
            boostCDTime = 0;
        }
    }

    void ProcessBoostCD()
    {
        if (boostCDStarted)
        {
            boostCDTime += Time.deltaTime;
            if (boostCDTime >= boostCDMaxTime)
            {
                StopBoostCD();
            }
        }
    }

    void StopBoostCD()
    {
        if (boostCDStarted)
        {
            boostCDStarted = false;
        }
    }

    #endregion

    #region  FACING DIR AND ANGLE & BODY ROTATION---------------------------------------------

    public void SetPlayerRotationSpeed(float rotSpeed)
    {
        currentRotationSpeed = rotSpeed;
    }

    public void ResetPlayerRotationSpeed()
    {
        currentRotationSpeed = rotationSpeed;
    }

    void UpdateFacingDir()//change so that only rotateObj rotates, not whole body
    {
        switch (myCamera.camMode)
        {
            case cameraMode.Fixed:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                //Calculate looking dir of camera
                Vector3 camPos = myCamera.transform.GetChild(0).position;
                Vector3 myPos = transform.position;
                currentFacingDir = new Vector3(myPos.x - camPos.x, 0, myPos.z - camPos.z).normalized;
                currentCamFacingDir = myCamera.myCamera.transform.forward.normalized;
                break;
            case cameraMode.Shoulder:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                currentFacingDir = RotateVector(-myCamera.transform.localRotation.eulerAngles.y, Vector3.forward).normalized;
                currentCamFacingDir = myCamera.myCamera.transform.forward.normalized;
                //print("CurrentFacingDir = " + currentFacingDir);
                break;
            case cameraMode.Free:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                currentFacingDir = RotateVector(-rotateObj.localRotation.eulerAngles.y, Vector3.forward).normalized;
                currentCamFacingDir = (cameraFollow.position - myCamera.myCamera.transform.position).normalized;
                break;
        }
        //print("currentFacingDir = " + currentFacingDir + "; currentCamFacingDir = " + currentCamFacingDir);

    }

    public void RotateCharacter(float rotSpeed = 0)
    {
        switch (myCamera.camMode)
        {
            case cameraMode.Fixed:
                Vector3 point1 = transform.position;
                Vector3 point2 = new Vector3(point1.x, point1.y + 1, point1.z);
                Vector3 dir = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
                rotateObj.Rotate(dir, rotSpeed * Time.deltaTime);
                break;
            case cameraMode.Shoulder:
                point1 = transform.position;
                point2 = new Vector3(point1.x, point1.y + 1, point1.z);
                dir = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
                rotateObj.Rotate(dir, rotSpeed * Time.deltaTime);
                break;
            case cameraMode.Free:
                if (currentInputDir != Vector3.zero)
                {
                    currentRotation = rotateObj.localRotation.eulerAngles.y;
                    //currentRotation = currentRotation > 180 ? currentRotation - 360 : currentRotation;

                    // angle = Arcos((x + y) / magnitude)
                    float angle = Mathf.Acos(((0 * currentInputDir.x) + (1 * currentInputDir.z)) / (1 * currentInputDir.magnitude)) * Mathf.Rad2Deg;
                    angle = currentInputDir.x < 0 ? 360 - angle : angle;
                    targetRotation = angle;

                    if (currentRotation != targetRotation)
                    {
                        if (!myPlayerCombat.isRotationRestricted && Mathf.Abs(currentRotation - targetRotation) > instantRotationMaxAngle)//Instant rotation
                        {
                            RotateCharacter(currentInputDir);
                        }
                        else//rotate with speed
                        {
                            //rotateObj.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                            #region --- Decide rotation direction ---
                            float sign = 1;
                            bool currentRotOver180 = currentRotation > 180 ? true : false;
                            bool targetRotOver180 = targetRotation > 180 ? true : false;
                            if (currentRotOver180 == targetRotOver180)
                            {
                                sign = currentRotation > targetRotation ? -1 : 1;
                            }
                            else
                            {
                                float oppositeAngle = currentRotation + 180;
                                oppositeAngle = oppositeAngle > 360 ? oppositeAngle - 360 : oppositeAngle;
                                //print("oppositeAngle = " + oppositeAngle);
                                sign = oppositeAngle < targetRotation ? -1 : 1;
                            }
                            #endregion

                            float newAngle = currentRotation + (sign * currentRotationSpeed * Time.deltaTime);

                            if (currentRotation < targetRotation && newAngle > targetRotation)
                            {
                                newAngle = targetRotation;
                            }
                            else if (currentRotation > targetRotation && newAngle < targetRotation)
                            {
                                newAngle = targetRotation;
                            }
                            rotateObj.localRotation = Quaternion.Euler(0, newAngle, 0);
                        }
                    }

                    //print("currentRotation = " + currentRotation + "; targetRotation = " + targetRotation);
                    //rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
                }
                break;
        }
        currentMovDir = AngleToVector(rotateObj.rotation.eulerAngles.y);
        //print("current angle = " + rotateObj.rotation.eulerAngles.y + "; currentMovDir = "+currentMovDir);
    }

    void RotateCharacter(Vector3 dir)
    {
        float angle = Mathf.Acos(dir.z / dir.magnitude) * Mathf.Rad2Deg;
        angle = dir.x < 0 ? 360 - angle : angle;
        rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
    }

    #endregion

    #region RECIEVE HIT AND STUN ---------------------------------------------

    public void StartRecieveHit(Vector3 _knockback, PlayerMovement attacker, float _maxTimeStun)
    {
        print("Recieve hit");
        myPlayerHook.FinishAutoGrapple();
        myPlayerHook.StopHook();

        maxTimeStun = _maxTimeStun;
        timeStun = 0;
        noInput = true;
        stunned = true;
        knockBackDone = false;
        knockback = _knockback;

        //Give FLAG
        if (haveFlag)
        {
            flag.DropFlag();
        }

        print("STUNNED");
    }

    void ProcessStun()
    {
        if (stunned)
        {
            moveSt = MoveState.Knockback;//Eloy: ESTO CREO QUE ESTÁ MAL PUESTO, el knockback SOLO SE APLICA UNA VEZ, no todo el tiempo de stun...pero me da miedo cambiarlo ahora de repente
            timeStun += Time.deltaTime;
            if (timeStun >= maxTimeStun)
            {
                StopStun();
            }
        }
    }

    void StopStun()
    {
        noInput = false;
        stunned = false;
        print("STUN END");
    }

    #endregion

    #region FIXED JUMP ---------------------------------------------------

    public void StartFixedJump(Vector3 vel, float _noMoveMaxTime)
    {
        fixedJumping = true;
        fixedJumpDone = false;
        noInput = true;
        noMoveMaxTime = _noMoveMaxTime;
        noMoveTime = 0;
        knockback = vel;

    }

    void ProcessFixedJump()
    {
        if (fixedJumping)
        {
            if (!fixedJumpDone)
            {
                moveSt = MoveState.FixedJump;
                fixedJumpDone = true;
            }
            else
            {
                //print("notBreaking on");
                moveSt = MoveState.NotBreaking;
            }
            noMoveTime += Time.deltaTime;
            if (noMoveTime >= noMoveMaxTime)
            {
                StopFixedJump();
            }
        }
    }

    void StopFixedJump()
    {
        if (fixedJumping)
        {
            fixedJumping = false;
            noInput = false;
        }
    }

    #endregion

    #region HOOKING/HOOKED ---------------------------------------------
    public void StartHooked()
    {
        if (!hooked)
        {
            noInput = true;
            hooked = true;
            if (jumpSt == JumpState.wallJumping)
            {
                StopWallJump();
            }
            if (moveSt == MoveState.Boost)
            {
                StopBoost();
            }
            //To Do:
            //Stop attacking
        }
    }

    void ProcessHooked()
    {
        if (hooked)
        {
            moveSt = MoveState.Hooked;
        }
    }

    public void StopHooked()
    {
        print("STOP HOOKED");
        if (hooked)
        {
            noInput = false;
            hooked = false;
            SetVelocity(currentVel / 2);
        }
    }

    public void StartHooking()
    {
        if (!hooking)
        {
            hooking = true;
        }
    }

    void ProcessHooking()
    {
        if (hooking)
        {
            if (maxMoveSpeed2 > maxHookingSpeed)
            {
                maxMoveSpeed2 = maxHookingSpeed;
            }
        }
    }

    public void StopHooking()
    {
        if (hooking)
        {
            hooking = false;
        }
    }

    void ProcessAiming()
    {
        if (myPlayerCombat.aiming && maxMoveSpeed2 > maxAimingSpeed)
        {
            maxMoveSpeed2 = maxAimingSpeed;
        }
    }
    #endregion

    #region PICKUP / FLAG / DEATH ---------------------------------------------

    public void PutOnFlag(Flag _flag)
    {
        flag = _flag;
        flag.transform.SetParent(rotateObj);
        flag.transform.localPosition = new Vector3(0, 0, -0.5f);
        flag.transform.localRotation = Quaternion.Euler(0, -135, 0);
        HideFlagFlow();
        StopBoost();
    }

    public void LoseFlag()
    {
        haveFlag = false;
        flag = null;
        ShowFlagFlow();
        (gC as GameController_FlagMode).HideFlagHomeLightBeam(team);
    }

    //En desuso
    public void Die()
    {
        if (haveFlag)
        {
            flag.SetAway(false);
        }
        gC.RespawnPlayer(this);
    }
    #endregion

    #region  WATER ---------------------------------------------


    public void EnterWater()
    {
        if (!inWater)
        {
            myPlayerAnimation_01.enterWater = true;
            inWater = true;
            jumpedOutOfWater = false;
            maxTimePressingJump = 0f;
            myPlayerWeap.AttachWeaponToBack();
            if (haveFlag)
            {
                //gC.RespawnFlag(flag.GetComponent<Flag>());
                flag.SetAway(false);
            }
            //Desactivar al jugadro si se esta en la prorroga.
            if (gC.gameMode == GameMode.CaptureTheFlag)
            {
                (gC as GameController_FlagMode).myScoreManager.PlayerEliminado();
            }
            myPlayerVFX.ActivateEffect(PlayerVFXType.SwimmingEffect);
            myPlayerVFX.ActivateEffect(PlayerVFXType.WaterSplash);
        }
    }

    void ProcessWater()
    {
        if (inWater)
        {
            controller.AroundCollisions();
            if (maxMoveSpeed2 > maxSpeedInWater)
            {
                maxMoveSpeed2 = maxSpeedInWater;
            }
        }
    }

    public void ExitWater()
    {
        if (inWater)
        {
            myPlayerAnimation_01.exitWater = true;
            inWater = false;
            myPlayerWeap.AttatchWeapon();
            myPlayerVFX.DeactivateEffect(PlayerVFXType.SwimmingEffect);
        }
    }
    #endregion

    #region  CHECK WIN ---------------------------------------------
    public void CheckScorePoint(FlagHome flagHome)
    {
        if (haveFlag && team == flagHome.team && this.moveSt != MoveState.Hooked)
        {
            (gC as GameController_FlagMode).ScorePoint(team);
            if (flag != null)
            {
                flag.SetAway(true);
            }
        }
    }
    #endregion

    #region LIGHT BEAM ---------------------------------------------
    float distanceToFlag;
    void UpdateDistanceToFlag()
    {
        distanceToFlag = ((gC as GameController_FlagMode).flags[0].transform.position - transform.position).magnitude;
    }

    void UpdateFlagLightBeam()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)
        {
            if (!haveFlag)
            {
                UpdateDistanceToFlag();
                //print("distanceToFlag = " + distanceToFlag);
                if (distanceToFlag >= (gC as GameController_FlagMode).minDistToSeeBeam)
                {
                    ShowFlagLightBeam();
                }
                else
                {
                    StopFlagLightBeam();
                }
            }
            else
            {
                StopFlagLightBeam();
            }
        }
    }

    void ShowFlagLightBeam()
    {
        //print("SHOW FLAG LIGHT BEAM");
        myCamera.myCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("FlagLightBeam");
        //int mask= myCamera.myCamera.GetComponent<Camera>().cullingMask;
        //myCamera.myCamera.GetComponent<Camera>().cullingMask = LayerMask.GetMask();
    }

    void StopFlagLightBeam()
    {
        //print("STOP FLAG LIGHT BEAM");
        myCamera.myCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("FlagLightBeam"));
    }

    void ShowFlagFlow()
    {
        myCamera.myCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("FlagGlow");
    }

    void HideFlagFlow()
    {
        myCamera.myCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("FlagGlow"));
    }

    public void ShowFlagHomeLightBeam(Team team)
    {
        switch (team)
        {
            case Team.A:
                myCamera.myCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("BlueLightBeam");
                break;
            case Team.B:
                myCamera.myCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("RedLightBeam");
                break;
        }
    }

    public void HideFlagHomeLightBeam(Team team)
    {
        switch (team)
        {
            case Team.A:
                print("HIDE BLUE TEAM LIGHT BEAM");
                myCamera.myCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("BlueLightBeam"));
                break;
            case Team.B:
                print("HIDE RED TEAM LIGHT BEAM");
                myCamera.myCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("RedLightBeam"));
                break;
        }
    }
    #endregion

    #region  AUXILIAR FUNCTIONS ---------------------------------------------
    Vector3 AngleToVector(float angle)
    {
        angle = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
    }
    public Vector3 RotateVector(float angle, Vector3 vector)
    {
        //rotate angle -90 degrees
        float theta = angle * Mathf.Deg2Rad;
        float cs = Mathf.Cos(theta);
        float sn = Mathf.Sin(theta);
        float px = vector.x * cs - vector.z * sn;
        float py = vector.x * sn + vector.z * cs;
        return new Vector3(px, 0, py).normalized;
    }
    public Vector3 CalculateReflectPoint(float radius, Vector3 _wallNormal, Vector3 circleCenter)//needs wallJumpRadius and wallNormal
    {
        //LEFT OR RIGHT ORIENTATION?
        Vector3 wallDirLeft = Vector3.Cross(_wallNormal, Vector3.down).normalized;
        float ang = Vector3.Angle(wallDirLeft, currentFacingDir);
        float direction = ang > 90 ? -1 : 1;
        //Angle
        float angle = Vector3.Angle(currentFacingDir, _wallNormal);
        if (angle >= 90)
        {
            angle -= 90;
        }
        else
        {
            angle = 90 - angle;
        }
        angle = Mathf.Clamp(angle, wallJumpMinHorizAngle, 90);
        if (direction == -1)
        {
            float complementaryAng = 90 - angle;
            angle += complementaryAng * 2;
        }
        Debug.LogWarning("ANGLE = " + angle);
        float offsetAngleDir = Vector3.Angle(wallDirLeft, Vector3.forward) > 90 ? -1 : 1;
        float offsetAngle = Vector3.Angle(Vector3.right, wallDirLeft) * offsetAngleDir;
        angle += offsetAngle;
        //CALCULATE CIRCUMFERENCE POINT
        float px = circleCenter.x + (radius * Mathf.Cos(angle * Mathf.Deg2Rad));
        float pz = circleCenter.z + (radius * Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector3 circumfPoint = new Vector3(px, circleCenter.y, pz);

        Debug.LogWarning("; circleCenter= " + circleCenter + "; circumfPoint = " + circumfPoint + "; angle = " + angle + "; offsetAngle = " + offsetAngle + "; offsetAngleDir = " + offsetAngleDir
    + ";wallDirLeft = " + wallDirLeft);
        Debug.DrawLine(circleCenter, circumfPoint, Color.white, 20);

        return circumfPoint;
    }
    #endregion

    #endregion

    #region ----[ PUN CALLBACKS ]----
    #endregion

    #region ----[ RPC ]----
    #endregion

    #region ----[ NETWORK FUNCTIONS ]----
    #endregion

    #region ----[ IPUNOBSERVABLE ]----
    #endregion
}
