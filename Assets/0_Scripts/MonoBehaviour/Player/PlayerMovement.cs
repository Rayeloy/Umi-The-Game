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
public class PlayerMovement : MonoBehaviour
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

    [Header("ROTATION")]
    public bool hasCharacterRotation = false;
    public float rotationSpeed = 1;
    float currentRotationSpeed = 0;
    [Tooltip("Min angle needed in a turn to activate the instant rotation of the character (and usually the hardSteer mechanic too)")]
    [Range(0, 180)]
    public float instantRotationMinAngle = 120;
    //public float rotationSpeedFixedCam = 250;
    float targetRotation = 0;
    float currentRotation;

    [Header("SPEED")]
    public float maxFallSpeed = 100;
    public float maxMoveSpeed = 10;
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
            return ((boostCurrentFuel > boostCapacity * boostMinFuelNeeded) && !boostCDStarted && !haveFlag && !inWater);
        }
    }
    Vector3 boostDir;

    [Header("ACCELERATIONS")]
    public float initialAcc = 30;
    public float airInitialAcc = 30;
    public float breakAcc = -30;
    public float airBreakAcc = -5;
    public float hardSteerAcc = -60; 
    public float airHardSteerAcc = -10;
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
    [Range(0, 1)]
    public float wallJumpMinHeightPercent = 0.55f;
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
    //Eloy: FOR ONLINE
    [HideInInspector]
    public int playerNumber; //from 0 to maxPlayers

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
    public PlayerSpawnInfo mySpawnInfo;
    #endregion

    #region ----[ VARIABLES ]----  
    int frameCounter = 0;

    //Movement
    float currentMaxMoveSpeed; // is the max speed from which we aply the joystick sensitivity value
    float finalMaxMoveSpeed = 10.0f; // its the final max speed, after the joyjoystick sensitivity value
    //bool hardSteer = false;

    //JOYSTICK INPUT
    float joystickAngle;
    float deadzone = 0.2f;
    float lastJoystickSens = 0;
    float joystickSens = 0;

    //WALL JUMP
    bool firstWallJumpDone = false;
    float wallJumpCurrentWallAngle = 0;
    GameObject wallJumpCurrentWall = null;
    float lastWallAngle = -500;
    GameObject lastWall = null;
    float wallJumpRadius;
    float walJumpConeHeight = 1;
    Axis wallJumpRaycastAxis = Axis.none;
    int wallJumpCheckRaysRows = 5;
    int wallJumpCheckRaysColumns = 5;
    float wallJumpCheckRaysRowsSpacing;
    float wallJumpCheckRaysColumnsSpacing;
    LayerMask auxLM;//LM = Layer Mask

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
        hardSteerAcc = Mathf.Clamp(hardSteerAcc, hardSteerAcc, breakAcc);
        airHardSteerAcc = Mathf.Clamp(airHardSteerAcc, airHardSteerAcc, airBreakAcc);

        //PLAYER MODEL
        myPlayerModel.SwitchTeam(team);

        //WALLJUMP
        wallJumpCheckRaysRows = 5;
        wallJumpCheckRaysColumns = 5;
        wallJumpCheckRaysRowsSpacing = (controller.coll.bounds.size.y * wallJumpMinHeightPercent) / (wallJumpCheckRaysRows - 1);
        wallJumpCheckRaysColumnsSpacing = controller.coll.bounds.size.x / (wallJumpCheckRaysColumns - 1);
        auxLM = LayerMask.GetMask("Stage");

        PlayerAwakes();

        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        //if (online)
        //{
        //    if (photonV)
        //    {
        //        PlayerMovement.LocalPlayerInstance = this.gameObject;
        //    }
        //}
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

        finalMaxMoveSpeed = currentMaxMoveSpeed = maxMoveSpeed;
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
        //if (controller.collisions.below && !controller.collisions.lastBelow)
        //{
        //    Debug.LogError("LANDING");
        //}
        //Debug.Log("Mis acciones son " + Actions);
        Debug.LogWarning("CurrentVel 0= " + currentVel.ToString("F6"));
        if (Actions.Start.WasPressed) gC.PauseGame(Actions);

        if ((controller.collisions.above || controller.collisions.below) && !hooked)
        {
            currentVel.y = 0;
        }
        //print("FRAME NUMBER " + frameCounter);
        frameCounter++;
        UpdateFlagLightBeam();
        ProcessInputsBuffer();

        //print("CurrentVel 1= " + currentVel.ToString("F6"));
        HorizontalMovement();
        //print("CurrentVel 2= " + currentVel.ToString("F6"));
        //print("vel = " + currentVel.ToString("F4"));
        UpdateFacingDir();
        VerticalMovement();
        //print("vel = " + currentVel.ToString("F4"));

        ProcessWallJump();//IMPORTANTE QUE VAYA ANTES DE LLAMAR A "MOVE"

        //Debug.Log("currentVel = " + currentVel + "; Time.deltaTime = " + Time.deltaTime + "; currentVel * Time.deltaTime = " + (currentVel * Time.deltaTime) + "; Time.fixedDeltaTime = " + Time.fixedDeltaTime);

        print("CurrentVel 3= " + currentVel.ToString("F6"));
        controller.Move(currentVel * Time.deltaTime);
        myPlayerCombat.KonoUpdate();
        controller.collisions.ResetAround();

        //myPlayerAnimation.KonoUpdate();
        myPlayerAnimation_01.KonoUpdate();
        myPlayerWeap.KonoUpdate();
        myPlayerHook.KonoUpdate();
        Debug.Log("CurrentVel End = " + currentVel.ToString("F6") + "; Player position = " + transform.position.ToString("F6"));
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

    public void TeleportPlayer(Vector3 worldPos)
    {
        ExitWater();
        StopBoost();
        StopHooked();
        StopHooking();
        StopWallJump();
        transform.position = worldPos;
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
        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
        currentSpeed = horVel.magnitude;
    }

    public void CalculateMoveDir()
    {
        if (!noInput)
        {
            float horiz = Actions.LeftJoystick.X;//Input.GetAxisRaw(contName + "H");
            float vert = Actions.LeftJoystick.Y;//-Input.GetAxisRaw(contName + "V");
                                                // Check that they're not BOTH zero - otherwise dir would reset because the joystick is neutral.
                                                //if (horiz != 0 || vert != 0)Debug.Log("Actions.LeftJoystick.X = "+ Actions.LeftJoystick.X+ "Actions.LeftJoystick.Y" + Actions.LeftJoystick.Y);
            Vector3 temp = new Vector3(horiz, 0, vert);
            lastJoystickSens = joystickSens;
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
                        currentMovDir = currentInputDir;
                        break;
                }
            }
            else
            {
                joystickSens = 1;//no estoy seguro de que esté bien
                moveSt = MoveState.NotMoving;
                currentInputDir = Vector3.zero;
            }
        }
    }

    bool hardSteerStarted = false;
    void HorizontalMovement()
    {
        float angleDiff = 0;
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
            angleDiff = Vector3.Angle(horizontalVel, currentMovDir);//hard Steer si > 90
            //ProcessHardSteer();
            if (!myPlayerCombat.aiming && Actions.R2.WasPressed)//Input.GetButtonDown(contName + "RB"))
            {
                StartBoost();
            }
            //------------------------------- Max Move Speed -------------------------------
            currentMaxMoveSpeed = maxMoveSpeed;
            ProcessWater();
            ProcessAiming();
            ProcessHooking();
            if (currentSpeed > (currentMaxMoveSpeed + 0.1f) && (moveSt == MoveState.Moving || moveSt == MoveState.NotMoving))
            {
                //Debug.LogWarning("Warning: moveSt set to MovingBreaking!: currentSpeed = "+currentSpeed+ "; maxMoveSpeed2 = " + maxMoveSpeed2 + "; currentVel.magnitude = "+currentVel.magnitude);
                moveSt = MoveState.MovingBreaking;
            }

            finalMaxMoveSpeed = lastJoystickSens > joystickSens && moveSt == MoveState.Moving ? (lastJoystickSens / 1) * currentMaxMoveSpeed : (joystickSens / 1) * currentMaxMoveSpeed;
            //------------------------------- Acceleration -------------------------------
            float finalAcc;
            float finalBreakAcc = controller.collisions.below ? breakAcc : airBreakAcc;
            float finalHardSteerAcc = controller.collisions.below ? hardSteerAcc : airHardSteerAcc;
            float finalInitialAcc = controller.collisions.below ? initialAcc : airInitialAcc;
            finalMovingAcc = controller.collisions.below ? movingAcc : airMovingAcc; //Turning accleration
            //finalBreakAcc = currentSpeed < 0 ? -finalBreakAcc : finalBreakAcc;
            switch (moveSt)
            {
                case MoveState.Moving:
                    if (angleDiff > instantRotationMinAngle)//hard Steer
                    {
                        finalAcc = finalHardSteerAcc;
                    }
                    else //Debug.LogWarning("Moving: angleDiff <= 90");
                    {
                        finalAcc = lastJoystickSens > joystickSens ? finalBreakAcc : finalInitialAcc;
                    }
                    //}
                    break;
                case MoveState.MovingBreaking:
                    finalAcc = hardBreakAcc;//breakAcc * 3;
                    break;
                case MoveState.Knockback:
                    finalAcc = knockbackBreakAcc;
                    break;
                case MoveState.NotMoving:
                    finalAcc = (currentSpeed == 0) ? 0 : finalBreakAcc;
                    break;
                default:
                    finalAcc = finalBreakAcc;
                    break;
            }

            //------------------------------- Speed ------------------------------- 
            switch (moveSt)
            {
                case MoveState.Moving:
                    if (angleDiff > instantRotationMinAngle && Mathf.Sign(currentSpeed) == 1)//hard Steer
                    {
                        if (!hardSteerStarted)
                        {
                            hardSteerStarted = true;
                            Debug.LogError("Current speed: Moving: angleDiff > instantRotationMinAngle -> Calculate velNeg" + "; currentMovDir = " + currentMovDir.ToString("F6"));
                            float angle = 180 - angleDiff;
                            float hardSteerInitialSpeed = Mathf.Cos(angle * Mathf.Deg2Rad) * horizontalVel.magnitude;//cos(angle) = velNeg /horizontalVel.magnitude;
                            currentSpeed = hardSteerInitialSpeed;
                        }
                    }
                    break;
                case MoveState.NotMoving:
                    break;
            }
            Debug.Log("CurrentSpeed 1.1 = " + currentSpeed);
            float currentSpeedB4 = currentSpeed;
            currentSpeed = currentSpeed + finalAcc * Time.deltaTime;
            if (moveSt == MoveState.NotMoving && Mathf.Sign(currentSpeedB4) != Mathf.Sign(currentSpeed))
            {
                currentSpeed = 0;
            }
            if(moveSt==MoveState.Moving && Mathf.Sign(currentSpeed) < 0 && angleDiff > instantRotationMinAngle)
            {
                currentSpeed = -currentSpeed;
                horizontalVel = currentMovDir * currentSpeed;
                currentVel = new Vector3(horizontalVel.x,currentVel.y, horizontalVel.z);
            }
            Debug.Log("CurrentSpeed 1.2 = " + currentSpeed);
            float maxSpeedClamp = stunned || noInput ? maxKnockbackSpeed : finalMaxMoveSpeed;
            float minSpeedClamp = (lastJoystickSens > joystickSens && moveSt == MoveState.Moving) ? (joystickSens / 1) * currentMaxMoveSpeed : 0;
            currentSpeed = Mathf.Clamp(currentSpeed, minSpeedClamp, maxSpeedClamp);
            if(hardSteerStarted && angleDiff<= instantRotationMinAngle)
            {
                hardSteerStarted = false;
            }
        }
        #endregion
        #endregion
        #region//------------------------------------------------ PROCESO EL TIPO DE MOVIMIENTO DECIDIDO ---------------------------------
        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
        print("CurrentVel before processing= " + currentVel.ToString("F6") + "; currentSpeed 1.3 = " +
            currentSpeed + "; MoveState = " + moveSt + "; currentMaxMoveSpeed = " + finalMaxMoveSpeed + "; below = " + controller.collisions.below
            + "; horVel.magnitude = " + horVel.magnitude);
        //print("CurrentVel 1.3= " + currentVel.ToString("F6")+ "MoveState = " + moveSt);
        if (jumpSt != JumpState.wallJumping)
        {
            horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
            switch (moveSt)
            {
                case MoveState.Moving: //MOVING WITH JOYSTICK
                    //angleDiff = Vector3.Angle(horizontalVel, currentMovDir);
                    Vector3 newDir;
                    if (angleDiff>instantRotationMinAngle)//hard Steer
                    {
                        Debug.LogWarning("Moving: angleDiff > instantRotationMinAngle");
                        newDir = horizontalVel;
                    }
                    else
                    {
                        Debug.LogWarning("Moving: angleDiff <= instantRotationMinAngle");
                        newDir = horizontalVel + currentMovDir * finalMovingAcc * Time.deltaTime;
                    }
                    horizontalVel = newDir.normalized * currentSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                    break;
                case MoveState.NotMoving: //NOT MOVING JOYSTICK
                    horizontalVel = horizontalVel.normalized * currentSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
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
                        horizontalVel = horizontalVel.normalized * currentSpeed;
                        currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
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
                    currentSpeed = horizontalVel.magnitude;
                    break;
                case MoveState.FixedJump:
                    currentVel = knockback;
                    horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                    currentSpeed = horizontalVel.magnitude;
                    currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
                    break;
                case MoveState.NotBreaking:
                    currentSpeed = horizontalVel.magnitude;
                    break;

            }
        }
        horVel = new Vector3(currentVel.x, 0, currentVel.z);
        print("CurrentVel after processing= " + currentVel.ToString("F6") + "; CurrentSpeed 1.4 = " + currentSpeed + "; horVel.magnitude = " 
            + horVel.magnitude + "; currentMovDir = " + currentMovDir.ToString("F6"));
        #endregion
    }

    //void StartHardSteer()
    //{
    //    if (!hardSteer && controller.collisions.below)
    //    {
    //        Debug.LogError("START HARD STEER");
    //        hardSteer = true;
    //        currentSpeed = -currentSpeed;
    //    }
    //}

    //void ProcessHardSteer()
    //{
    //    if (hardSteer)
    //    {
    //        if (currentSpeed >= 0 || moveSt != MoveState.Moving || noInput)
    //        {
    //            StopHardSteer();
    //        }
    //    }
    //    else
    //    {
    //        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
    //        float rotationDiffAfterLanding = Vector3.Angle(horVel, currentMovDir);
    //        if (moveSt == MoveState.Moving && controller.collisions.below && !controller.collisions.lastBelow && rotationDiffAfterLanding > instantRotationMinAngle)
    //        {
    //            Debug.LogWarning("Character Rotation 4.1.0");
    //            StartHardSteer();
    //        }
    //    }
    //}

    //void StopHardSteer()
    //{
    //    if (hardSteer)
    //    {
    //        Debug.LogError("STOP HARD STEER");
    //        hardSteer = false;
    //        if (currentSpeed < 0)
    //        {
    //            currentSpeed = -currentSpeed;
    //            //Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
    //            //horVel = -horVel;
    //            //currentVel = new Vector3(horVel.x, currentVel.y, horVel.z);
    //        }
    //    }
    //}

    void VerticalMovement()
    {

        if (!jumpedOutOfWater && !inWater && controller.collisions.below)
        {
            jumpedOutOfWater = true;

            maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;
        }
        if (lastWallAngle != -500 && controller.collisions.below)//RESET WALL JUMP VARIABLE TO MAKE IT READY
        {
            lastWallAngle = -500;
            lastWall = null;
        }
        if (Actions.A.WasPressed)//Input.GetButtonDown(contName + "A"))
        {
            //print("JUMP");
            //Debug.Log("pene");
            StartJump();
        }

        if (moveSt != MoveState.Boost)
        {
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
        }

        if (inWater)
        {
            currentVel.y = Mathf.Clamp(currentVel.y, -maxVerticalSpeedInWater, float.MaxValue);
        }
        ProcessJumpInsurance();
        currentVel.y = Mathf.Clamp(currentVel.y, -maxFallSpeed, maxFallSpeed);

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
                Debug.LogWarning("JUMP!");
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

    /// <summary>
    /// In order to be able to save the input for Input buffering, we return true if the input was successful, 
    /// and false if the input was not successful and should be buffered.
    /// </summary>
    /// <returns></returns>
    bool StartWallJump()
    {
        Debug.LogWarning("Check Wall jump");
        bool result = false;
        if (!controller.collisions.below && !inWater && jumpedOutOfWater && controller.collisions.collisionHorizontal &&
            (!firstWallJumpDone || lastWallAngle != controller.collisions.wallAngle || (lastWallAngle == controller.collisions.wallAngle &&
            lastWall != controller.collisions.horWall)))
        {
            wallJumpCurrentWall = controller.collisions.horWall;
            if (wallJumpCurrentWall.GetComponent<StageScript>() == null || wallJumpCurrentWall.GetComponent<StageScript>().wallJumpable)
            {
                Debug.LogWarning("Wall jumping start");
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
                wallJumpCurrentWallAngle = controller.collisions.wallAngle;
                wallJumpRaycastAxis = controller.collisions.closestHorRaycast.axis;
                Debug.Log("WALL JUMP RAY HEIGHT PERCENTAGE : " + controller.collisions.closestHorRaycast.rayHeightPercentage + "%; wall = " + wallJumpCurrentWall.name);
            }

        }
        else
        {
            Debug.LogWarning("Couldn't wall jump because:  !controller.collisions.below (" + !controller.collisions.below + ") && !inWater(" + !inWater + ") &&" +
                " jumpedOutOfWater(" + jumpedOutOfWater + ") && controller.collisions.collisionHorizontal(" + controller.collisions.collisionHorizontal + ") && " +
                "(!firstWallJumpDone(" + !firstWallJumpDone + ") || lastWallAngle != controller.collisions.wallAngle (" + (lastWallAngle != controller.collisions.wallAngle) + ") || " +
                "(lastWallAngle == controller.collisions.wallAngle (" + (lastWallAngle == controller.collisions.wallAngle) + ")&& " +
                "lastWall != controller.collisions.horWall(" + (lastWall != controller.collisions.horWall) + ")))");
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
                return;
            }

            if (stopWallTime >= stopWallMaxTime)
            {
                StopWallJump();
                return;
            }

            #region --- WALL JUMP CHECK RAYS ---
            //Check continuously if we are still attached to wall
            float rayLength = controller.coll.bounds.extents.x + 0.5f;
            RaycastHit hit;

            //calculamos el origen de todos los rayos y columnas: esquina inferior (izda o dcha,no sé)
            //del personaje. 
            Vector3 paralelToWall = new Vector3(-wallNormal.z, 0, wallNormal.x).normalized;
            Vector3 rowsOrigin = new Vector3(controller.coll.bounds.center.x, controller.coll.bounds.min.y, controller.coll.bounds.center.z);
            rowsOrigin -= paralelToWall * controller.coll.bounds.extents.x;
            Vector3 dir = -wallNormal.normalized;
            bool success = false;
            for (int i = 0; i < wallJumpCheckRaysRows && !success; i++)
            {
                Vector3 rowOrigin = rowsOrigin + Vector3.up * wallJumpCheckRaysRowsSpacing * i;
                for (int j = 0; j < wallJumpCheckRaysColumns && !success; j++)
                {
                    Vector3 rayOrigin = rowOrigin + paralelToWall * wallJumpCheckRaysColumnsSpacing * j;
                    Debug.Log("WallJump: Ray[" + i + "," + j + "] with origin = " + rayOrigin.ToString("F4") + "; rayLength =" + rayLength);
                    Debug.DrawRay(rayOrigin, dir * rayLength, Color.blue, 3);

                    if (Physics.Raycast(rayOrigin, dir, out hit, rayLength, auxLM, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform.gameObject == wallJumpCurrentWall)
                        {
                            success = true;
                            Debug.Log("WallJump: Success! still walljumping!");
                        }
                        else
                        {
                            Debug.Log("WallJump: this wall (" + hit.transform.gameObject + ")is not the same wall that I started walljumping from " +
                                "(" + wallJumpCurrentWall + ").");
                        }
                    }
                }
            }
            #endregion

            if (!success)
            {
                Debug.LogError("STOPPED WALLJUMPING DUE TO NOT DETECTING THE WALL ANYMORE. wallJumpCheckRaysRows = " + wallJumpCheckRaysRows);
                StopWallJump();
            }
        }

    }

    void EndWallJump()
    {
        Debug.LogError("End Wall jump");
        if (!firstWallJumpDone) firstWallJumpDone = true;
        lastWallAngle = wallJumpCurrentWallAngle;
        lastWall = wallJumpCurrentWall;
        wallJumping = false;
        wallJumpAnim = true;
        jumpSt = JumpState.none;
        wallJumpRaycastAxis = Axis.none;
        //CALCULATE JUMP DIR
        //LEFT OR RIGHT ORIENTATION?
        //Angle
        Vector3 circleCenter = anchorPoint + Vector3.up * walJumpConeHeight;
        Vector3 circumfPoint = CalculateReflectPoint(wallJumpRadius, wallNormal, circleCenter);
        Vector3 finalDir = (circumfPoint - anchorPoint).normalized;
        Debug.LogWarning("FINAL DIR= " + finalDir.ToString("F4"));

        SetVelocity(finalDir * wallJumpVelocity);
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

    void StartBoost()
    {
        if (!noInput && boostReady && !inWater)
        {
            //noInput = true;
            //PARA ORTU: Variable para empezar boost

            //myPlayerAnimation_01.dash = true;
            boostCurrentFuel -= boostCapacity * boostFuelLostOnStart;
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
        else
        {
            myPlayerHUD.StartDashHUDCantDoAnimation();
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
        if (moveSt == MoveState.Boost)
        {
            //print("STOP BOOST");
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

    void WallBoost()
    {
        //CALCULATE JUMP DIR
        Vector3 circleCenter = transform.position;
        Vector3 circumfPoint = CalculateReflectPoint(1, controller.collisions.wallNormal, circleCenter);
        Vector3 finalDir = (circumfPoint - circleCenter).normalized;
        Debug.LogWarning("WALL BOOST: FINAL DIR = " + finalDir.ToString("F4"));

        currentVel = finalDir * currentVel.magnitude;
        currentSpeed = currentVel.magnitude;
        boostDir = new Vector3(finalDir.x, 0, finalDir.z);
        RotateCharacter(boostDir);
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

                    float angle = Mathf.Acos(((0 * currentInputDir.x) + (1 * currentInputDir.z)) / (1 * currentInputDir.magnitude)) * Mathf.Rad2Deg;
                    angle = currentInputDir.x < 0 ? 360 - angle : angle;
                    targetRotation = angle;
                    Vector3 targetRotationVector = AngleToVector(targetRotation);
                    Vector3 currentRotationVector = AngleToVector(currentRotation);
                    float rotationDiff = Vector3.Angle(targetRotationVector, currentRotationVector);
                    if (hasCharacterRotation)
                    {
                        #region --- Character Rotation with rotation speed ---
                        if (currentRotation != targetRotation)
                        {
                            if (!myPlayerCombat.isRotationRestricted && rotationDiff > instantRotationMinAngle)//Instant rotation
                            {
                                //Debug.LogError("INSTANT BODY ROTATION, angle = "+targetRotation);
                                RotateCharacterInstantly(targetRotation);
                                //Debug.LogError("INSANT BODYY ROTATION after, angle = " + rotateObj.localRotation.eulerAngles.y);
                            }
                            else
                            {//rotate with speed
                                #region --- Decide rotation direction ---
                                int sign = 1;
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
                                Vector3 newAngleVector = AngleToVector(newAngle);
                                float newAngleDiff = Vector3.Angle(currentRotationVector, newAngleVector);
                                if (newAngleDiff > rotationDiff) newAngle = targetRotation;
                                rotateObj.localRotation = Quaternion.Euler(0, newAngle, 0);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        RotateCharacterInstantly(targetRotation);
                    }
                }
                break;
        }
        //print("current angle = " + rotateObj.rotation.eulerAngles.y + "; currentMovDir = "+currentMovDir);
    }

    void RotateCharacter(Vector3 dir)
    {
        float angle = Mathf.Acos(dir.z / dir.magnitude) * Mathf.Rad2Deg;
        angle = dir.x < 0 ? 360 - angle : angle;
        rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
        currentRotation = targetRotation = angle;
    }

    void RotateCharacterInstantly(float angle)
    {
        rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
        currentRotation = targetRotation = angle;
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
        StopBoost();

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
        //print("STOP HOOKED");
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
            if (currentMaxMoveSpeed > maxHookingSpeed)
            {
                currentMaxMoveSpeed = maxHookingSpeed;
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
        if (myPlayerCombat.aiming && currentMaxMoveSpeed > maxAimingSpeed)
        {
            currentMaxMoveSpeed = maxAimingSpeed;
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
            if (currentMaxMoveSpeed > maxSpeedInWater)
            {
                currentMaxMoveSpeed = maxSpeedInWater;
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
        //Debug.LogWarning("ANGLE = " + angle);
        float offsetAngleDir = Vector3.Angle(wallDirLeft, Vector3.forward) > 90 ? -1 : 1;
        float offsetAngle = Vector3.Angle(Vector3.right, wallDirLeft) * offsetAngleDir;
        angle += offsetAngle;
        //CALCULATE CIRCUMFERENCE POINT
        float px = circleCenter.x + (radius * Mathf.Cos(angle * Mathf.Deg2Rad));
        float pz = circleCenter.z + (radius * Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector3 circumfPoint = new Vector3(px, circleCenter.y, pz);

        //Debug.LogWarning("; circleCenter= " + circleCenter + "; circumfPoint = " + circumfPoint + "; angle = " + angle + "; offsetAngle = " + offsetAngle + "; offsetAngleDir = " + offsetAngleDir
        //+ ";wallDirLeft = " + wallDirLeft);
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
