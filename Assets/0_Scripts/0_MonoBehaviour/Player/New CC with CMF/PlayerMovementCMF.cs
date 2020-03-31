using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region ----[ PUBLIC ENUMS ]----
public enum Team
{
    A = 1,// Blue - Green
    B = 2,// Red - Pink
    none = 0
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
    NotBreaking = 8,
    Impulse = 9
}

public enum VerticalMovementState
{
    None,
    Jumping,
    JumpBreaking,//Emergency stop
    Falling,
    WallJumping,
    WallJumped,
    ChargeJumping,
    BounceJumping,
    FloatingInWater,
    Sliding // on a water Slide
}

public enum ChargedJumpMode
{
    Fixed,
    Proportional,
    Proportional_With_Min
}

public enum ForceType
{
    Additive,
    Forced
}

#endregion

#region ----[ REQUIRECOMPONENT ]----
#endregion
public class PlayerMovementCMF : Bolt.EntityBehaviour<IPlayerState>
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    [Header(" --- Debugging ---")]
    public bool disableAllDebugs;
    public bool updateDebugsOn = false;
    public bool startDebugsOn = false;
    public bool charContDebugsOn = false;
    public bool horMovementDebugsOn = false;
    public bool vertMovementDebugsOn = false;
    public bool waterDebugsOn = false;

    [Header(" --- Referencias --- ")]
    //public PlayerCombat myPlayerCombat;
    public CollisionsCheck collCheck;
    public Mover mover;
    public PlayerCombatCMF myPlayerCombat;
    public PlayerWeaponsCMF myPlayerWeap;
    //public PlayerAnimation myPlayerAnimation;
    public PlayerAnimationCMF myPlayerAnimation;
    public PlayerHookCMF myPlayerHook;
    //public Controller3D controller;
    public PlayerBodyCMF myPlayerBody;
    public PlayerObjectDetectionCMF myPlayerObjectDetection;
    public PlayerModel myPlayerModel;
    public PlayerVFXCMF myPlayerVFX;

    public bool startBeingHitAnimation = false;

    public GameControllerCMF gC;
    public CameraControllerCMF myCamera;
    public Transform cameraFollow;
    public Transform rotateObj;


    //CONTROLES
    public PlayerActions actions { get; set; }
    public InputsInfo inputsInfo;


    //GROUND VARIABLES


    //VARIABLES DE MOVIMIENTO

    [Header(" --- ROTATION --- ")]
    [Tooltip("Min angle needed in a turn to activate the instant rotation of the character (and usually the hardSteer mechanic too)")]
    [Range(0, 180)]
    public float instantRotationMinAngle = 120;
    float rotationRestrictedPercentage = 1;
    [HideInInspector]
    public float targetRotAngle;
    float rotationVelocity = 2;

    [Header("--- SPEED ---")]
    public float maxFallSpeed = 40;
    public float maxAscendSpeed = 40;
    public float maxMoveSpeed = 10;
    float maxAttackingMoveSpeed = 10;
    [Tooltip("Maximum speed that you can travel at horizontally when hit by someone")]
    public float maxKnockbackSpeed = 300f;
    public float maxAimingSpeed = 5f;
    public float maxHookingSpeed = 3.5f;
    public float maxGrapplingSpeed = 3.5f;
    public float maxSpeedInWater = 5f;
    //public float maxVerticalSpeedInWater = 10f; 
    public float maxFloatingSpeed = 2f;

    [Header(" - IMPULSE - ")]
    [Range(0, 1)]
    public float airImpulsePercentage = 0.5f;
    [Range(0, 180)]
    public float maxImpulseAngle = 60;

    [Header("--- BOOST ---")]
    public float boostSpeed = 20f;
    public float boostCapacity = 1f;
    [Tooltip("1 -> 1 capacity per second")]
    public float boostDepletingSpeed = 1f;
    [Tooltip("1 -> 1 capacity per second")]
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
            return ((boostCurrentFuel > boostCapacity * boostMinFuelNeeded) && !boostCDStarted 
                && !haveFlag && vertMovSt != VerticalMovementState.FloatingInWater && moveSt != MoveState.Boost);
        }
    }
    Vector3 boostDir;

    [Header(" --- ACCELERATIONS --- ")]
    public float initialAcc = 30;
    public float airInitialAcc = 30;
    public float wallJumpInitialAcc = 30;
    public float fixedJumpInitialAcc = 20f;
    public float breakAcc = -30;
    public float airBreakAcc = -5;
    public float wallJumpBreakAcc = -5;
    public float fixedJumpBreakAcc = -2.5f;
    public float hardSteerAcc = -60;
    public float airHardSteerAcc = -10;
    public float wallJumpHardSteerAcc = -10;
    public float fixedJumpHardSteerAcc = -5f;
    public float movingAcc = 11.5f;
    public float airMovingAcc = 0.5f;
    public float wallJumpMovingAcc = 0.5f;
    public float fixedJumpMovingAcc = 0.25f;
    [Tooltip("Acceleration used when breaking from a boost.")]
    public float hardBreakAcc = -120f;
    [Tooltip("Breaking negative acceleration that is used under the effects of a knockback (stunned). Value is clamped to not be higher than breakAcc.")]
    public float knockbackBreakAcc = -30f;
    //public float breakAccOnHit = -2.0f;
    float gravity;
    [HideInInspector]
    public float currentGravity;

    [Header(" --- JUMP --- ")]
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

    [Header(" --- WALLJUMP --- ")]
    public float wallJumpVelocity = 10f;
    public float wallJumpSlidingAcc = -2.5f;
    [Range(0, 1)]
    public float wallJumpMinHeightPercent = 0.55f;
    public float stopWallMaxTime = 0.5f;
    float stopWallTime = 0;
    [HideInInspector]
    public bool wallJumping = false;
    Vector3 anchorPoint;
    Vector3 wallNormal;
    [Tooltip("Vertical angle in which the player wall-jumps.")]
    [Range(0, 89)]
    public float wallJumpAngle = 30;
    [Tooltip("Minimum horizontal angle in which the player wall-jumps. This number ranges from 0 to 90. 0 --> parallel to the wall; 90 --> perpendicular to the wall")]
    [Range(0, 89)]
    public float wallJumpMinHorizAngle = 30;


    [Header(" --- CHARGE JUMP ---")]
    //[Tooltip("If true, any charge jump will always the same fixed amount, which is equal to the chargeJumpMaxJumpHeight.")]
    //public bool chargeJumpFixedAmount = false;
    public ChargedJumpMode chargedJumpMode = ChargedJumpMode.Proportional;
    [Tooltip("The bigger the multiplier, the faster we will adquire height by ChargedJumping. Normal value would be = 1 ~")]
    [Range(0, 6)]
    public float chargedJumpFallenHeightMultiplier = 1f;
    [Range(0, 2)]
    public float chargedJumpReleaseTimeBeforeLand = 0.5f;
    [Range(0, 2)]
    public float chargedJumpReleaseTimeAfterLand = 0.5f;
    public float chargedJumpMaxJumpHeight = 100;
    [Range(0, 1)]
    public float chargedJumpMinPercentageNeeded = 0.3f;
    float chargedJumpMaxHeight = 80;//clouds height
    bool chargedJumpChargingJump = false;
    float chargedJumpLastApexHeight;
    float chargedJumpCurrentJumpMaxHeight;
    float chargedJumpCurrentReleaseTime = 0;
    bool chargedJumpButtonReleased;
    bool chargedJumpLanded = false;
    float chargedJumpLandedTime = 0;
    float chargedJumpChargingStartHeight;
    bool failedJump = false;
    bool isChargedJump//for StartJump only
    {
        get
        {
            bool result = false;
            result = collCheck.below && chargedJumpChargingJump && isFloorChargeJumpable && (vertMovSt == VerticalMovementState.Falling || vertMovSt == VerticalMovementState.None);
            if (!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("isChargeJump = " + result);
            return result;
        }
    }
    bool isFloorChargeJumpable
    {
        get
        {
            StageScript stageScript = collCheck.floor != null ? collCheck.floor.GetComponent<StageScript>() : null;
            return stageScript != null && stageScript.chargeJumpable;
        }
    }


    [Header(" --- BOUNCE JUMP ---")]
    [Range(0, 1)]
    public float bounceJumpMultiplier = 0.5f;
    public float bounceJumpMinHeight = 2.5f;
    bool isBounceJump//for StartJump only
    {
        get
        {
            bool result = false;
            float totalFallenHeight = chargedJumpLastApexHeight - transform.position.y;
            float auxBounceJumpCurrentMaxHeight = totalFallenHeight * bounceJumpMultiplier;
            result = collCheck.below && isFloorBounceJumpable && auxBounceJumpCurrentMaxHeight >= bounceJumpMinHeight
                && vertMovSt == VerticalMovementState.Falling;
            if (!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("isBounceJump = " + result + "; currentFloor = " + collCheck.floor + "; collCheck.below = " + collCheck.below
                + "; auxBounceJumpCurrentMaxHeight >= bounceJumpMinHeight = " + (auxBounceJumpCurrentMaxHeight >= bounceJumpMinHeight));
            return result;
        }
    }
    bool isFloorBounceJumpable
    {
        get
        {
            StageScript stageScript = collCheck.floor != null ? collCheck.floor.GetComponent<StageScript>() : null;
            if (!disableAllDebugs && vertMovementDebugsOn && stageScript != null) Debug.Log("stageScript = " + stageScript + "; stageScript.bounceJumpable = "
     + stageScript.bounceJumpable);
            return (stageScript == null || (stageScript != null && stageScript.bounceJumpable));
        }
    }

    [Header(" --- SLIDES ---")]
    public float comeFromSlideBreakAcc = -20f;
    float maxSlidingSpeed = 25;
    bool comeFromSlide = false;
    //[Range(0, 2)]
    //public float slideAcc = 0.5f;
    //[Tooltip("Acceleration that keeps you in the slide's direction. The bigger the value, the more difficult it is to fly out of the slide.")]
    //public float slideMovingAcc = 2.5f;
    //public float slideInputMovingAcc = 5f;
    //public bool slideAlgorithmToggle = true;


    [Header("TEAM COLORS")]
    public Gradient StinrayGradient;
    public Gradient OktiromeGradient;

    [Header("----- ONLINE VARIABLES ----")]
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    #endregion

    #region ----[ PROPERTIES ]----
    //Referencias que no se asignan en el inspector
    [HideInInspector]
    public Camera myUICamera;
    [HideInInspector]
    public PlayerHUDCMF myPlayerHUD;

    ////ONLINE
    //[HideInInspector]
    //public bool online = false;

    //Eloy: FOR ONLINE
    [HideInInspector]
    public int playerNumber; //from 0 to maxPlayers

    //INFO GENERAL
    [HideInInspector]
    public Team team = Team.A;
    [HideInInspector]
    public PlayerSpawnInfo mySpawnInfo;
    //[Header("Body Mass")]
    //[Tooltip("Body Mass Index. 1 is for normal body mass.")]
    [HideInInspector] public float bodyMass;

    //BOOL PARA PERMITIR O BLOQUEAR INPUTS
    [HideInInspector]
    public bool noInput = false;

    //MOVIMIENTO
    [HideInInspector]
    public MoveState moveSt = MoveState.NotMoving;
    [HideInInspector]
    public Vector3 currentVel;
    Vector3 oldCurrentVel;
    Vector3 finalVel;

    [HideInInspector]
    public float currentSpeed = 0;
    //[HideInInspector]
    //public Vector3 currentMovDir;//= a currentInputDir??? 
    [HideInInspector]
    public Vector3 currentInputDir;
    [HideInInspector]
    public Vector3 currentFacingDir = Vector3.forward;
    [HideInInspector]
    public float facingAngle = 0;
    [HideInInspector]
    public Vector3 currentCamFacingDir = Vector3.zero;
    Vector3 hardSteerDir = Vector3.zero;
    float hardSteerAngleDiff = 0;
    bool hardSteerOn = false;
    bool hardSteerStarted = false;

    //IMPULSE
    [HideInInspector]
    public ImpulseInfo currentImpulse;
    bool impulseStarted = false;
    float impulseTime = 0;
    bool impulseDone = false;



    //JUMP
    [HideInInspector]
    public VerticalMovementState vertMovSt = VerticalMovementState.None;
    [HideInInspector]
    public bool wallJumpAnim = false;

    //FLAG
    [HideInInspector]
    public bool haveFlag = false;
    [HideInInspector]
    public FlagCMF flag = null;

    #endregion

    #region ----[ VARIABLES ]----  
    int frameCounter = 0;

    //Movement
    [HideInInspector]
    public float currentMaxMoveSpeed; // is the max speed from which we aply the joystick sensitivity value
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
    int wallJumpCheckRaysRows = 5;
    int wallJumpCheckRaysColumns = 5;
    float wallJumpCheckRaysRowsSpacing;
    float wallJumpCheckRaysColumnsSpacing;
    LayerMask auxLM;//LM = Layer Mask

    //Attack effect suffering
    EffectType sufferingEffect;
    float effectTime = 0;
    float effectMaxTime = 0;
    bool knockbackDone;
    Vector3 knockback;
    bool sufferingStunLikeEffect
    {
        get
        {
            return (sufferingEffect == EffectType.softStun);//|| sufferingEffect == EffectType.stun || sufferingEffect == EffectType.knockdown
        }
    }

    //FIXED JUMPS (Como el trampolín)
    bool fixedJumping;
    bool fixedJumpDone;
    float fixedJumpMaxTime;
    float noMoveMaxTime;
    float noMoveTime;
    bool fixedJumpBounceEnabled = false;

    //HOOK
    [HideInInspector]
    public bool hooked = false;
    bool hooking = false;
    [HideInInspector] public bool aimingAndTouchedGroundOnce = false;
    [HideInInspector] public bool hookingAndTouchedGroundOnce = false;

    //ROTATION



    ////GRAPPLE
    //bool grappling = false;


    //WATER
    [HideInInspector]
    public bool jumpedOutOfWater = true;

    BufferedInput[] inputsBuffer;//Eloy: para Juan: esta variable iría aquí? o a "Variables", JUAN: A variables mejor
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region AWAKE

    public void KonoAwake(bool isMyCharacter = false)
    {
        if (MasterManager.GameSettings.online)
        {
        }
        else
        {
            mover = GetComponent<Mover>();
            collCheck.KonoAwake(mover.capsuleCollider);//we use capsule collider in our example


            if (actions == null) Debug.LogError("Error: PlayerActions have not been yet assigned to Player");
            else inputsInfo = new InputsInfo(actions);

            if (breakAcc != knockbackBreakAcc) Debug.LogError("The breakAcceleration and the KnockbackAcceleration should be the same!");
            maxMoveSpeed = 10;
            currentSpeed = 0;
            boostCurrentFuel = boostCapacity;
            noInput = false;
            lastWallAngle = 0;
            SetupInputsBuffer();
            myPlayerHook.myCameraBase = myCamera;
            hardSteerAcc = Mathf.Clamp(hardSteerAcc, hardSteerAcc, breakAcc);
            airHardSteerAcc = Mathf.Clamp(airHardSteerAcc, airHardSteerAcc, airBreakAcc);

            //PLAYER MODEL
            SwitchTeam(team);

            //WALLJUMP
            wallJumpCheckRaysRows = 5;
            wallJumpCheckRaysColumns = 5;
            wallJumpCheckRaysRowsSpacing = (collCheck.myCollider.bounds.size.y * wallJumpMinHeightPercent) / (wallJumpCheckRaysRows - 1);
            wallJumpCheckRaysColumnsSpacing = collCheck.myCollider.bounds.size.x / (wallJumpCheckRaysColumns - 1);
            auxLM = LayerMask.GetMask("Stage", "Slide");

            PlayerAwakes();
        }
    }

    //todos los konoAwakes
    void PlayerAwakes()
    {
        myPlayerHUD.KonoAwake();
        myPlayerCombat.KonoAwake();
        //myPlayerAnimation.KonoAwake();
        myPlayerAnimation.KonoAwake();
        myPlayerHook.KonoAwake();
        myPlayerWeap.KonoAwake();
        myPlayerBody.KonoAwake();
        myPlayerVFX.KonoAwake();
    }
    #endregion

    #region START
    public void KonoStart()
    {
        if (!disableAllDebugs && startDebugsOn) Debug.Log("PlayerMovementCMF ("+name+")-> KonoStart Start");
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);
        currentGravity = gravity;
        jumpVelocity = Mathf.Abs(gravity * jumpApexTime);
        maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;
        wallJumpRadius = Mathf.Tan(wallJumpAngle * Mathf.Deg2Rad) * walJumpConeHeight;
        if (!disableAllDebugs && startDebugsOn) Debug.Log("wallJumpRaduis = " + wallJumpRadius + "; tan(wallJumpAngle)= " + Mathf.Tan(wallJumpAngle * Mathf.Deg2Rad));
        wallJumpMinHorizAngle = Mathf.Clamp(wallJumpMinHorizAngle, 0, 90);
        if(!disableAllDebugs && startDebugsOn) Debug.Log("Gravity = " + gravity + "; Jump Velocity = " + jumpVelocity);

        finalMaxMoveSpeed = currentMaxMoveSpeed = maxMoveSpeed;
        knockbackBreakAcc = Mathf.Clamp(knockbackBreakAcc, -float.MaxValue, breakAcc);//menos de break Acc lo haría ver raro

        ShowFlagFlow();
        //EquipWeaponAtStart();

        PlayerStarts();
    }
    private void PlayerStarts()
    {
        collCheck.KonoStart();
        myPlayerCombat.KonoStart();
        myPlayerHUD.KonoStart();
        myPlayerWeap.KonoStart();
        myPlayerVFX.KonoStart();
    }

    #endregion

    #region UPDATE
    public void KonoUpdate()
    {
        if (actions.Start.WasPressed) gC.PauseGame(actions);

        //POLL INPUTS
        inputsInfo.PollInputs();
        SmoothRotateCharacter();
        myPlayerCombat.KonoUpdate();
        myPlayerWeap.KonoUpdate();
        myPlayerHook.KonoUpdate();
    }

    public void KonoFixedUpdate()
    {

        if (!disableAllDebugs && updateDebugsOn) Debug.LogWarning("PLAYER FIXED UPDATE: ");
        //Debug.LogWarning("Current pos = " + transform.position.ToString("F8"));
        lastPos = transform.position;


        Vector3 platformMovement = collCheck.ChangePositionWithPlatform(mover.instantPlatformMovement);

        collCheck.ResetVariables();
        ResetMovementVariables();

        collCheck.UpdateCollisionVariables(mover, vertMovSt, (fixedJumping && noInput));

        collCheck.UpdateCollisionChecks(currentVel);

        if (vertMovSt == VerticalMovementState.FloatingInWater) collCheck.AroundCollisions();

        if (!disableAllDebugs && horMovementDebugsOn && currentSpeed != 0) Debug.LogWarning("CurrentVel 0= " + currentVel.ToString("F6") + "; currentSpeed =" + currentSpeed.ToString("F4"));


        frameCounter++;
        UpdateFlagLightBeam();
        ProcessInputsBuffer();

        #region --- Calculate Movement ---
        //Debug.Log("Pre Hor Mov: currentVel = " + currentVel.ToString("F6"));
        HorizontalMovement();
        //Debug.Log("Post Hor Mov: currentVel = " + currentVel.ToString("F6"));
        UpdateFacingDir();

        VerticalMovement();
        //Debug.Log("Post Vert Mov: currentVel = " + currentVel.ToString("F6"));

        finalVel = currentVel;
        HandleSlopes();

        ProcessWallJump();//IMPORTANTE QUE VAYA ANTES DE LLAMAR A "mover.SetVelocity"

        #endregion
        //Debug.Log("Movement Fin: currentVel = " + currentVel.ToString("F6") + "; below = " + collCheck.below);
        //If the character is grounded, extend ground detection sensor range;
        mover.SetExtendSensorRange(collCheck.below);
        //Set mover velocity;
        //Debug.Log("currentVel B4 mover = " + currentVel.ToString("F6"));

        //Debug.Log("Pre mover.SetVelocity: finalVel = " + finalVel.ToString("F6"));
        mover.SetVelocity(finalVel, platformMovement);
        //Debug.Log("Mover SetVel Fin: currentVel = " + currentVel.ToString("F6") + "; below = " + collCheck.below);


        //Ocean Renderer floating
        myPlayerBody.KonoFixedUpdate();

        // RESET InputsInfo class to get new possible inputs during the next Update frames
        inputsInfo.ResetInputs();

        collCheck.SavePlatformPoint();
    }

    Vector3 lastPos;
    public void KonoLateUpdate()
    {
        if (!disableAllDebugs && updateDebugsOn) Debug.LogWarning(" --- NEW LATE UPDATE ---");
        Vector3 currentTotalMovement = transform.position - lastPos;
        //Debug.Log("rb velocity = " + GetComponent<Rigidbody>().velocity.ToString("F8") + "; currentTotalMovement = "+ currentTotalMovement.ToString("F8"));
        myPlayerAnimation.KonoUpdate();
    }
    #endregion

    #endregion

    #region ----[ CLASS FUNCTIONS ]----

    void SwitchTeam(Team team)
    {
        myPlayerModel.SwitchTeam(team);
        TrailRenderer dashTR = myPlayerVFX.GetEffectGO(PlayerVFXType.DashTrail).GetComponent<TrailRenderer>();
        dashTR.colorGradient = team == Team.A ? StinrayGradient : OktiromeGradient;
        //dashTR.endColor = team == Team.A ? StinrayColors [1]: OktiromeColors[1];
    }

    #region INPUTS BUFFERING -------------------------------------

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
                    case PlayerInput.WallJump:
                        //TO REDO
                        if (StartWallJump(true))
                        {
                            inputsBuffer[i].StopBuffering();
                        }
                        break;
                    case PlayerInput.StartChargingChargeJump:
                        if (StartChargingChargedJump(true))
                        {
                            inputsBuffer[i].StopBuffering();
                        }
                        break;
                    case PlayerInput.Autocombo:
                        if (!disableAllDebugs) Debug.Log("Trying to input autocombo from buffer... Time left = " + inputsBuffer[i].time);
                        if (myPlayerCombat.StartOrContinueAutocombo(true))
                        {
                            inputsBuffer[i].StopBuffering();
                        }
                        break;

                }
                inputsBuffer[i].ProcessTime();
            }
        }
    }

    public void BufferInput(PlayerInput _inputType)
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

    public void StopBufferedInput(PlayerInput _inputType)
    {
        for (int i = 0; i < inputsBuffer.Length; i++)
        {
            if (inputsBuffer[i].input.inputType == _inputType)
            {
                inputsBuffer[i].StopBuffering();
            }
        }
    }

    #endregion

    #region MOVEMENT -----------------------------------------------

    #region --- HORIZONTAL MOVEMENT ---
    public void SetVelocity(Vector3 vel)
    {
        currentVel = vel;
        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
        currentSpeed = horVel.magnitude;
    }

    void ResetMovementVariables()
    {
        //hardSteerOn = false;
        hardSteerAngleDiff = 0;
        currentInputDir = Vector3.zero;
        oldCurrentVel = new Vector3(currentVel.x, 0, currentVel.z);
    }

    public void CalculateMoveDir()
    {
        if (!noInput)
        {
            float horiz = actions.LeftJoystick.X;//Input.GetAxisRaw(contName + "H");
            float vert = actions.LeftJoystick.Y;//-Input.GetAxisRaw(contName + "V");
                                                // Check that they're not BOTH zero - otherwise dir would reset because the joystick is neutral.
                                                //if (horiz != 0 || vert != 0)Debug.Log("actions.LeftJoystick.X = "+ actions.LeftJoystick.X+ "actions.LeftJoystick.Y" + actions.LeftJoystick.Y);
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
                    case CameraMode.Fixed:
                        currentInputDir = RotateVector(-facingAngle, temp);
                        break;
                    case CameraMode.Shoulder:
                        currentInputDir = RotateVector(-facingAngle, temp);
                        break;
                    case CameraMode.Free:
                        Vector3 camDir = (transform.position - myCamera.transform.GetChild(0).position).normalized;
                        camDir.y = 0;
                        // ANGLE OF JOYSTICK
                        joystickAngle = Mathf.Acos(((0 * currentInputDir.x) + (1 * currentInputDir.z)) / (1 * currentInputDir.magnitude)) * Mathf.Rad2Deg;
                        joystickAngle = (horiz > 0) ? -joystickAngle : joystickAngle;
                        //rotate camDir joystickAngle degrees
                        currentInputDir = RotateVector(joystickAngle, camDir);
                        //HARD STEER CHECK
                        //if(!disableAllDebugs)Debug.LogError(" hardSteerOn = "+ hardSteerOn + "; isRotationRestricted = " + myPlayerCombat.isRotationRestricted);
                        if (!(!hardSteerOn && myPlayerCombat.isRotationRestricted))
                        {
                            Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                            hardSteerAngleDiff = Vector3.Angle(horizontalVel, currentInputDir);//hard Steer si > 90
                            hardSteerOn = hardSteerAngleDiff > instantRotationMinAngle ? true : false;
                            if (hardSteerOn && !hardSteerStarted)
                            {
                                //if (!disableAllDebugs && hardSteerOn) Debug.LogError("HARD STEER ON: STARTED");
                                hardSteerDir = currentInputDir;
                                RotateCharacterInstantly(hardSteerDir);
                            }
                        }
                        break;
                }
            }
            else
            {
                joystickSens = 1;//no estoy seguro de que esté bien
                moveSt = MoveState.NotMoving;
            }
        }
    }

    void HorizontalMovement()
    {
        float finalMovingAcc = 0;
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        #region//------------------------------------------------ DECIDO TIPO DE MOVIMIENTO --------------------------------------------
        #region//----------------------------------------------------- Efecto externo --------------------------------------------
        if (sufferingStunLikeEffect)
        {
            ProcessSufferingEffect();
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
        if (!hooked && moveSt != MoveState.Boost && moveSt != MoveState.Knockback && moveSt != MoveState.Impulse && moveSt != MoveState.FixedJump && moveSt != MoveState.NotBreaking)
        {
            //------------------------------------------------ Direccion Joystick, aceleracion, maxima velocidad y velocidad ---------------------------------
            //------------------------------- Joystick Direction -------------------------------
            CalculateMoveDir();//Movement direction
            //ProcessHardSteer();
            if (!myPlayerCombat.aiming && (inputsInfo.R2WasPressed || actions.R2.IsPressed))//Input.GetButtonDown(contName + "RB"))
            {
                StartBoost();
            }

            #region ------------------------------ Max Move Speed ------------------------------
            currentMaxMoveSpeed = myPlayerCombat.attackStg != AttackPhaseType.ready && myPlayerCombat.landedSinceAttackStarted ? maxAttackingMoveSpeed : maxMoveSpeed;//maxAttackingMoveSpeed == maxMoveSpeed if not attacking
            ProcessWaterMaxSpeed();//only apply if the new max move speed is lower
            ProcessAiming();//only apply if the new max move speed is lower
            ProcessHooking();//only apply if the new max move speed is lower
            if (currentSpeed > (currentMaxMoveSpeed + 0.1f) && (moveSt == MoveState.Moving || moveSt == MoveState.NotMoving) && !knockbackDone && !impulseDone)
            {
                //Debug.LogWarning("Warning: moveSt set to MovingBreaking!: currentSpeed = "+currentSpeed+ "; maxMoveSpeed2 = " + maxMoveSpeed2 + "; currentVel.magnitude = "+currentVel.magnitude);
                moveSt = MoveState.MovingBreaking;
            }

            finalMaxMoveSpeed = comeFromSlide? maxSlidingSpeed : moveSt == MoveState.MovingBreaking? float.MaxValue :
                //Moving while reducing joystick angle? If not normal parameters.
                lastJoystickSens > joystickSens && moveSt == MoveState.Moving ? (lastJoystickSens / 1) * currentMaxMoveSpeed : (joystickSens / 1) * currentMaxMoveSpeed;

            ProcessImpulse(currentMaxMoveSpeed);
            #endregion

            #region ------------------------------- Acceleration -------------------------------
            float finalAcc = 0;
            float finalBreakAcc = comeFromSlide? comeFromSlideBreakAcc : collCheck.below ? breakAcc : vertMovSt == VerticalMovementState.WallJumped ? wallJumpBreakAcc : fixedJumping ? fixedJumpBreakAcc : airBreakAcc;
            float finalHardSteerAcc = collCheck.below ? hardSteerAcc : vertMovSt == VerticalMovementState.WallJumped ? wallJumpHardSteerAcc : fixedJumping ? fixedJumpHardSteerAcc : airHardSteerAcc;
            float finalInitialAcc = collCheck.below ? initialAcc : vertMovSt == VerticalMovementState.WallJumped ? wallJumpInitialAcc : fixedJumping ? fixedJumpInitialAcc : airInitialAcc;
            finalMovingAcc = (collCheck.below ? movingAcc : vertMovSt == VerticalMovementState.WallJumped ? wallJumpMovingAcc : fixedJumping ? fixedJumpMovingAcc : airMovingAcc) * rotationRestrictedPercentage; //Turning accleration
            //if (!disableAllDebugs && rotationRestrictedPercentage!=1) Debug.LogWarning("finalMovingAcc = " + finalMovingAcc+ "; rotationRestrictedPercentage = " + rotationRestrictedPercentage+
            //    "; attackStg = " + myPlayerCombat.attackStg);
            //finalBreakAcc = currentSpeed < 0 ? -finalBreakAcc : finalBreakAcc;
            if (knockbackDone && impulseDone)
            {
                Debug.LogError("ERROR, they should not happen at the same time!");
                StopImpulse();
            }
            else if (knockbackDone)
            {
                finalAcc = knockbackBreakAcc;
            }
            else if (impulseDone)
            {
                finalAcc = currentImpulse.impulseAcc;
            }
            else
            {
                switch (moveSt)
                {
                    case MoveState.Moving:
                        if (hardSteerOn)//hard Steer
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
                        finalAcc = comeFromSlide? comeFromSlideBreakAcc: hardBreakAcc;//breakAcc * 3;
                        break;
                    case MoveState.NotMoving:
                        finalAcc = (currentSpeed == 0) ? 0 : finalBreakAcc;
                        break;
                    default:
                        finalAcc = finalBreakAcc;
                        break;
                }
            }
            #endregion

            #region ----------------------------------- Speed ---------------------------------- 
            if (!fixedJumping && vertMovSt != VerticalMovementState.Sliding)
            {
                switch (moveSt)
                {
                    case MoveState.Moving:
                        if (hardSteerOn && Mathf.Sign(currentSpeed) == 1)//hard Steer
                        {
                            if (!hardSteerStarted)
                            {
                                hardSteerStarted = true;
                                //Debug.Log("Current speed: Moving: angleDiff > instantRotationMinAngle -> Calculate velNeg" + "; currentInputDir = " + currentInputDir.ToString("F6"));
                                float angle = 180 - hardSteerAngleDiff;
                                float hardSteerInitialSpeed = Mathf.Cos(angle * Mathf.Deg2Rad) * horizontalVel.magnitude;//cos(angle) = velNeg /horizontalVel.magnitude;
                                currentSpeed = hardSteerInitialSpeed;
                            }
                        }
                        break;
                    case MoveState.NotMoving:
                        break;
                }
                if (!disableAllDebugs && horMovementDebugsOn && currentSpeed != 0) Debug.Log("CurrentSpeed 1.2 = " + currentSpeed.ToString("F4") + "; finalAcc = " + finalAcc + "; moveSt = " + moveSt +
                    "; currentSpeed =" + currentSpeed.ToString("F4"));
                float currentSpeedB4 = currentSpeed;
                currentSpeed = currentSpeed + finalAcc * Time.deltaTime;
                //Hard Steer
                if (moveSt == MoveState.NotMoving && Mathf.Sign(currentSpeedB4) != Mathf.Sign(currentSpeed))
                {
                    currentSpeed = 0;
                }
                if (moveSt == MoveState.Moving && Mathf.Sign(currentSpeed) < 0 && hardSteerOn)
                {
                    currentSpeed = -currentSpeed;
                    horizontalVel = hardSteerDir * currentSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                }

                float maxSpeedClamp = knockbackDone || impulseDone ? maxKnockbackSpeed : finalMaxMoveSpeed;
                float minSpeedClamp = (lastJoystickSens > joystickSens && moveSt == MoveState.Moving) ? (joystickSens / 1) * currentMaxMoveSpeed : 0;
                currentSpeed = Mathf.Clamp(currentSpeed, minSpeedClamp, maxSpeedClamp);
                if (hardSteerStarted && !hardSteerOn)
                {
                    hardSteerStarted = false;
                }
                if (knockbackDone && currentSpeed <= maxMoveSpeed)
                {
                    knockbackDone = false;
                    startBeingHitAnimation = false;
                }

                if (!disableAllDebugs && horMovementDebugsOn && currentSpeed != 0) Debug.Log("moveSt = "+moveSt+"; currentSpeed = " + currentSpeed.ToString("F4") +"; finalAcc = " +
                    finalAcc.ToString("F4")+ "; maxSpeedClamp = " + maxSpeedClamp.ToString("F4"));
            }
        } 
        #endregion
        #endregion
        #endregion
        #region//------------------------------------------------ PROCESO EL TIPO DE MOVIMIENTO DECIDIDO ---------------------------------
        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
        if (!disableAllDebugs && horMovementDebugsOn && currentSpeed != 0) print("CurrentVel before processing= " + currentVel.ToString("F6") + "; currentSpeed =" + currentSpeed.ToString("F4") +
            "; MoveState = " + moveSt + "; currentMaxMoveSpeed = " + finalMaxMoveSpeed + "; below = " + collCheck.below + "; horVel.magnitude = " + horVel.magnitude+ "; finalMovingAcc = " + finalMovingAcc.ToString("F4"));
        if ((vertMovSt != VerticalMovementState.WallJumping || (vertMovSt == VerticalMovementState.WallJumping && moveSt == MoveState.Knockback)) && vertMovSt != VerticalMovementState.Sliding)
        {
            horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
            switch (moveSt)
            {
                case MoveState.Moving: //MOVING WITH JOYSTICK
                    Vector3 newDir;
                    if (hardSteerOn)//hard Steer
                    {
                        newDir = horizontalVel;
                    }
                    else
                    {
                        Vector3 oldDir = horizontalVel.magnitude == 0 && myPlayerCombat.attackStg != AttackPhaseType.ready ? rotateObj.forward.normalized : horizontalVel.normalized;
                        newDir = oldDir + (currentInputDir * (finalMovingAcc * Time.deltaTime));
                        float auxAngle = Vector3.Angle(oldCurrentVel, newDir);
                        if (!disableAllDebugs && horMovementDebugsOn && currentSpeed!=0) Debug.LogWarning("HorizontalMovement: finalMovingAcc2 = " + finalMovingAcc + ";  auxAngle = " + auxAngle + "; (currentInputDir * finalMovingAcc * Time.deltaTime).magnitude = "
                             + (currentInputDir * finalMovingAcc * Time.deltaTime).magnitude + "; (currentInputDir * finalMovingAcc * Time.deltaTime) = "
                             + (currentInputDir * finalMovingAcc * Time.deltaTime) + "; newDir = " + newDir);
                    }
                    horizontalVel = newDir.normalized * currentSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                    if (!disableAllDebugs && horMovementDebugsOn && currentSpeed != 0) Debug.LogWarning("HorizontalMovement: CurrentVel.normalized = " + currentVel.normalized.ToString("F6"));
                    break;
                case MoveState.NotMoving: //NOT MOVING JOYSTICK
                    horizontalVel = horizontalVel.normalized * currentSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                    break;
                case MoveState.Boost:
                    horizontalVel = new Vector3(boostDir.x, 0, boostDir.z);
                    horizontalVel = horizontalVel.normalized * boostSpeed;
                    SetVelocity(new Vector3(horizontalVel.x, 0, horizontalVel.z));
                    //}
                    break;
                case MoveState.Knockback:
                    if (!knockbackDone)
                    {
                        if (!disableAllDebugs) print("KNOCKBACK");
                        currentVel = knockback;
                        horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                        currentSpeed = horizontalVel.magnitude;
                        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
                        knockbackDone = true;
                        knockback = Vector3.zero;
                        moveSt = MoveState.NotMoving;
                    }
                    else
                    {
                        Debug.LogError("Error: knockback was already done!");
                    }
                    //print("vel.y = " + currentVel.y);
                    break;
                case MoveState.MovingBreaking://FRENADA FUERTE
                    newDir = horizontalVel.normalized + (currentInputDir * finalMovingAcc * Time.deltaTime);
                    horizontalVel = newDir.normalized * currentSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                    break;
                case MoveState.Hooked:
                    currentSpeed = horizontalVel.magnitude;
                    break;
                case MoveState.FixedJump:
                    if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("FIXED JUMP DO");
                    currentVel = knockback;
                    horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                    currentSpeed = horizontalVel.magnitude;
                    currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
                    fixedJumpDone = true;
                    RotateCharacter();
                    myCamera.StartResetCamera(targetRotAngle, 0.2f);
                    moveSt = MoveState.NotBreaking;
                    break;
                case MoveState.NotBreaking:
                    currentSpeed = horizontalVel.magnitude;
                    break;
                case MoveState.Impulse:
                    if (!disableAllDebugs && horMovementDebugsOn) Debug.LogWarning("DOING IMPULSE");
                    impulseDone = true;
                    Vector3 finalImpulse = new Vector3(currentImpulse.impulseVel.x, 0, currentImpulse.impulseVel.z);
                    float maxMag = finalImpulse.magnitude;
                    finalImpulse = horizontalVel + finalImpulse;
                    float finalMag = Mathf.Clamp(finalImpulse.magnitude, 0, maxMag);
                    finalImpulse = finalImpulse.normalized * finalMag;
                    finalImpulse = new Vector3(finalImpulse.x, currentVel.y, finalImpulse.z);
                    SetVelocity(finalImpulse);
                    moveSt = MoveState.NotMoving;
                    break;
            }
        }
        horVel = new Vector3(currentVel.x, 0, currentVel.z);
        //print("CurrentVel after processing= " + currentVel.ToString("F6") + "; CurrentSpeed 1.4 = " + currentSpeed + "; horVel.magnitude = " 
        //    + horVel.magnitude + "; currentInputDir = " + currentInputDir.ToString("F6"));
        #endregion
        RotateCharacter();
    }

    public void SetPlayerAttackMovementSpeed(float movementPercent)
    {
        maxAttackingMoveSpeed = movementPercent * maxMoveSpeed;
    }

    //not in use?
    public void ResetPlayerAttackMovementSpeed()
    {
        maxAttackingMoveSpeed = maxMoveSpeed;
    }

    public void SetPlayerRotationSpeed(float rotSpeedPercentage)
    {
        rotationRestrictedPercentage = rotSpeedPercentage;
    }

    public void ResetPlayerRotationSpeed()
    {
        rotationRestrictedPercentage = 1;
    }

    #region -- Combat Impulse --
    public void StartImpulse(ImpulseInfo _impulse)
    {
        if (_impulse.impulseDistance != 0)
        {
            currentImpulse = _impulse;
            currentImpulse.initialPlayerPos = transform.position;

            if (!collCheck.below)
            {
                currentImpulse.impulseVel *= airImpulsePercentage;
                //IMPORTANT TO DO: 
                //RECALCULATE ALL IMPULSE PARAMETERS
            }
            impulseStarted = true;
            impulseDone = false;
            impulseTime = 0;
            moveSt = MoveState.Impulse;

            //Character rotation
            float angle = Mathf.Acos(((0 * currentImpulse.impulseDir.x) + (1 * currentImpulse.impulseDir.z)) / (1 * currentImpulse.impulseDir.magnitude)) * Mathf.Rad2Deg;
            angle = currentImpulse.impulseDir.x < 0 ? 360 - angle : angle;
            RotateCharacterInstantly(angle);
            if (!disableAllDebugs && horMovementDebugsOn) Debug.LogWarning("Impulse = " + currentImpulse.impulseVel + "; impulseMaxTime = " + currentImpulse.impulseMaxTime +
                "; impulse.impulseInitialVel = " + currentImpulse.impulseInitialSpeed + "; currentImpulse.impulseDistance = " + currentImpulse.impulseDistance);
        }
    }

    void ProcessImpulse(float currentMaxMoveSpeed)
    {
        if (impulseStarted)
        {
            impulseTime += Time.deltaTime;
            //if (!disableAllDebugs && currentSpeed <= maxMoveSpeed) Debug.LogError("CurrentSpeed = "+ currentSpeed + "; maxMoveSpeed = " + maxMoveSpeed);
            if (impulseTime >= currentImpulse.impulseMaxTime || currentSpeed <= currentMaxMoveSpeed)
            {
                StopImpulse();
            }
            else
            {
                moveSt = MoveState.NotMoving;
            }
        }
    }

    void StopImpulse()
    {
        if (impulseStarted)
        {
            if (!disableAllDebugs && horMovementDebugsOn) Debug.LogWarning("STOP IMPULSE: impulseTime = " + impulseTime + "; currentImpulse.impulseMaxTime = " + currentImpulse.impulseMaxTime +
                  ";currentSpeed  = " + currentSpeed + "; currentMaxMoveSpeed = " + currentMaxMoveSpeed);
            //moveSt = MoveState.NotMoving;
            impulseStarted = false;
            impulseDone = false;
            moveSt = MoveState.NotMoving;
        }
    }

    public float MissingImpulseTime()
    {
        return Mathf.Clamp(currentImpulse.impulseMaxTime - impulseTime, 0, float.MaxValue);
    }
    #endregion

    #endregion

    #region --- VERTICAL MOVEMENT ---

    void VerticalMovement()
    {
        //Debug.Log("Vert Mov Inicio: currentVel = " + currentVel.ToString("F6") + "; below = " + collCheck.below);

        if (!jumpedOutOfWater && vertMovSt != VerticalMovementState.FloatingInWater && collCheck.below)
        {
            jumpedOutOfWater = true;

            maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;
        }

        if (lastWallAngle != -500 && collCheck.below)//RESET WALL JUMP VARIABLE TO MAKE IT READY
        {
            lastWallAngle = -500;
            lastWall = null;
        }

        if (inputsInfo.JumpWasPressed)//Input.GetButtonDown(contName + "A"))
        {
            PressA();
        }

        ProcessSlides();

        if (moveSt != MoveState.Boost && !hooked)
        {
            if (vertMovSt == VerticalMovementState.None || vertMovSt == VerticalMovementState.Falling)
            {
                ProcessChargingChargedJump();
            }
            //Debug.Log("Vel y pre = " + currentVel.y.ToString("F6"));
            switch (vertMovSt)
            {
                case VerticalMovementState.None:
                    if (!mover.stickToGround && currentVel.y <= 0)
                    {
                        if(!disableAllDebugs && charContDebugsOn)Debug.Log("VertMovSt None: StickToGround On");
                        mover.stickToGround = true;
                    }
                    if (currentVel.y < 0 && (!collCheck.below || collCheck.sliping))
                    {
                        if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("VertMovSt None: JumpSt = Falling");
                        vertMovSt = VerticalMovementState.Falling;
                        if(!(fixedJumping && !fixedJumpBounceEnabled))
                        chargedJumpLastApexHeight = transform.position.y;
                        if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("VertMovSt None: START FALLING");
                    }
                    currentVel.y += currentGravity * Time.deltaTime;
                    break;
                case VerticalMovementState.Falling:
                    
                    //to avoid bounce jump at the end of a slide
                    if(collCheck.onSlide) chargedJumpLastApexHeight = transform.position.y;

                    if (!chargedJumpChargingJump && collCheck.below && !collCheck.sliping)
                    {
                        if (StartBounceJump())
                            break;
                    }

                    if (currentVel.y >= 0 || (collCheck.below && !collCheck.sliping))
                    {
                        //Debug.Log("STOP FALLING");
                        if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("VertMovSt Falling: JumpSt = None");
                        vertMovSt = VerticalMovementState.None;
                    }

                    currentVel.y += currentGravity * Time.deltaTime;
                    break;
                case VerticalMovementState.Jumping:
                    currentVel.y += currentGravity * Time.deltaTime;
                    timePressingJump += Time.deltaTime;
                    if (timePressingJump >= maxTimePressingJump)
                    {
                        StopJump();
                    }
                    else
                    {
                        //This condition is actually correct, first one is for release stored on buffer from Update.
                        //Second one is for when you tap too fast the jump button
                        if (inputsInfo.JumpWasReleased || !actions.A.IsPressed)                          
                        {
                            vertMovSt = VerticalMovementState.JumpBreaking;
                        }
                    }
                    break;
                case VerticalMovementState.JumpBreaking:
                    if (currentVel.y <= 0)
                    {
                        ResetJumpState();
                    }
                    currentVel.y += (currentGravity * breakJumpForce) * Time.deltaTime;
                    break;
                case VerticalMovementState.WallJumping:
                    currentVel.y += wallJumpSlidingAcc * Time.deltaTime;
                    break;
                case VerticalMovementState.WallJumped:
                    if (currentVel.y <= 0 && (!collCheck.below || collCheck.sliping))
                    {
                        ResetJumpState();
                    }
                    currentVel.y += gravity * Time.deltaTime;
                    break;
                case VerticalMovementState.ChargeJumping:
                    if (currentVel.y <= 0)
                    {
                        ResetJumpState();
                    }
                    currentVel.y += currentGravity * Time.deltaTime;
                    break;
                case VerticalMovementState.BounceJumping:
                    if (currentVel.y <= 0)
                    {
                        ResetJumpState();
                    }
                    currentVel.y += currentGravity * Time.deltaTime;
                    break;
                case VerticalMovementState.FloatingInWater:
                    currentVel.y += currentGravity * Time.deltaTime;
                    currentVel += myPlayerBody.buoyancy * Time.deltaTime;
                    // apply drag relative to water
                    currentVel += myPlayerBody.verticalDrag * Time.fixedDeltaTime;
                    currentVel.y = Mathf.Clamp(currentVel.y, float.MinValue, maxFloatingSpeed);
                    break;
                case VerticalMovementState.Sliding:
                    SlideAlgorithm();
                    break;
            }
        }
        ProcessJumpInsurance();

        if (!fixedJumping)
        {
            currentVel.y = Mathf.Clamp(currentVel.y, -maxFallSpeed, maxAscendSpeed);
        }

        //TO REDO
        if ((/*controller.collisions.above ||*/ collCheck.below) && !hooked && !collCheck.sliping && mover.stickToGround && !collCheck.onSlide)
        {
            currentVel.y = 0;
            //if (controller.collisions.above) StopJump();
        }
    }

    #region -- Slides --

    void ProcessSlides()
    {
        if (comeFromSlide)
        {
            if (currentSpeed <= currentMaxMoveSpeed || (moveSt != MoveState.None && moveSt != MoveState.Moving && moveSt != MoveState.MovingBreaking) || hooked)
            {
                StopSlides();
            }
        }

        if (collCheck.below && collCheck.onSlide && moveSt != MoveState.Boost && moveSt != MoveState.Hooked && moveSt != MoveState.Knockback &&
            moveSt != MoveState.FixedJump && moveSt != MoveState.Impulse && !hooked)
        {
            //noInput = true;
            vertMovSt = VerticalMovementState.Sliding;
            comeFromSlide = true;
        }
        else if (vertMovSt == VerticalMovementState.Sliding)
        {
            //noInput = false;
            vertMovSt = VerticalMovementState.None;
        }
    }

    void StopSlides()
    {
        vertMovSt = VerticalMovementState.None;
        comeFromSlide = false;
    }

    void SlideAlgorithm()
    {
        //if (slideAlgorithmToggle)
        //{                    
        //New algorithm
        UmiSlide slide = collCheck.floor.GetComponentInParent<UmiSlide>();
        maxSlidingSpeed = slide.maxSlidingSpeed;
        currentVel.y += currentGravity * Time.deltaTime;
        Vector3 targetSlideDir = Vector3.ProjectOnPlane(Vector3.down, mover.GetGroundNormal());
        float gravityMagnitude = targetSlideDir.magnitude * slide.slideAcc;
        Vector3 currentSlideDir = Vector3.ProjectOnPlane(currentVel, mover.GetGroundNormal());
        Vector3 finalSlideDir = currentSlideDir.normalized + (targetSlideDir.normalized * slide.slideMovingAcc * Time.deltaTime) + (currentInputDir * (slide.slideInputMovingAcc * Time.deltaTime));
        finalSlideDir = finalSlideDir.normalized * Mathf.Clamp(currentSlideDir.magnitude + (-gravityMagnitude * Mathf.Sign(currentVel.y)), -slide.maxSlidingSpeed, slide.maxSlidingSpeed);
        SetVelocity(finalSlideDir);

        Debug.DrawRay(mover.GetGroundPoint(), targetSlideDir.normalized * 2, Color.yellow);
        Debug.DrawRay(mover.GetGroundPoint(), currentSlideDir.normalized * 2, Color.cyan);
        Debug.DrawRay(mover.GetGroundPoint(), finalSlideDir.normalized * 2, Color.green);
        //}
        //else
        //{
        //    //Old Algorithm
        //    currentVel.y += currentGravity * Time.deltaTime;
        //    Vector3 targetSlideDir = Vector3.ProjectOnPlane(Vector3.down * -currentGravity * 3 * Time.deltaTime, mover.GetGroundNormal());
        //    Debug.DrawRay(mover.GetGroundPoint(), targetSlideDir.normalized * 2, Color.yellow);
        //    Vector3 currentSlideDir = Vector3.ProjectOnPlane(currentVel, mover.GetGroundNormal());
        //    Debug.DrawRay(mover.GetGroundPoint(), currentSlideDir.normalized * 2, Color.cyan);
        //    Vector3 finalSlideDir = currentSlideDir + (targetSlideDir);
        //    float speed = Mathf.Clamp(finalSlideDir.magnitude, 0, maxSlidingSpeed);
        //    Debug.DrawRay(mover.GetGroundPoint(), finalSlideDir.normalized * 2, Color.green);
        //    SetVelocity(finalSlideDir.normalized * speed);
        //    //Debug.Log("targetSlideDir = "+ targetSlideDir.ToString("F6")+ "; currentSlideDir = "+ currentSlideDir.ToString("F6") + "; finalSlideDir = " + finalSlideDir.ToString("F6"));
        //}
    }

    #endregion

    #region -- VerticalImpulse --

    public void StartVerticalImpulse(float ySpeed, ForceType forceType)
    {
        switch (forceType)
        {
            default:
            case ForceType.Forced:
                currentVel.y = ySpeed;
                break;
            case ForceType.Additive:
                currentVel.y += ySpeed;
                break;
        }
        mover.stickToGround = false;
        collCheck.below = false;
        vertMovSt = VerticalMovementState.None;
        if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("StartVerticalImpulse: ySpeed = "+ ySpeed + "; forceType = " + forceType+ "; currentVel.y = " + currentVel.y);
    }
    #endregion

    void ResetJumpState()
    {
        if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("ResetJumpState: JumpSt = None");
        vertMovSt = VerticalMovementState.None;
        mover.stickToGround = true;
        if (!disableAllDebugs && charContDebugsOn)
            Debug.LogWarning("stickToGround On");
    }

    void HandleSlopes()
    {
        if (collCheck.sliping)
        {
            // HORIZONTAL VELOCITY
            Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
            //Vector3 wallNormal = mover.GetGroundNormal();
            //wallNormal.y = 0;
            //wallNormal = -wallNormal.normalized;
            //float angle = Vector3.Angle(wallNormal, horVel);
            //float a = Mathf.Sin(angle * Mathf.Deg2Rad) * horVel.magnitude;
            //Vector3 slideVel = Vector3.Cross(wallNormal, Vector3.up).normalized;
            //Debug.DrawRay(mover.GetGroundPoint(), slideVel * 2, Color.green);
            ////LEFT OR RIGHT ORIENTATION?
            //float ang = Vector3.Angle(slideVel, horVel);
            //slideVel = ang > 90 ? -slideVel : slideVel;
            ////print("SLIDE ANGLE= " + angle + "; vel = " + vel + "; slideVel = " + slideVel.ToString("F4") + "; a = " + a + "; wallAngle = " + wallAngle + "; distanceToWall = " + rayCast.distance);
            //slideVel *= a;

            //horVel = new Vector3(slideVel.x, 0, slideVel.z);

            //VERTICAL VELOCITY
            //Apply slide gravity along ground normal, if controller is sliding;
            if (currentVel.y < 0)
            {
                Vector3 _slideDirection = Vector3.ProjectOnPlane(-Vector3.up, mover.GetGroundNormal()).normalized;
                Debug.DrawRay(mover.GetGroundPoint(), _slideDirection * 2, Color.yellow);
                Vector3 slipVel = _slideDirection * -currentVel.y;
                finalVel = slipVel + horVel;
            }
            if(!disableAllDebugs && horMovementDebugsOn) Debug.Log("Handle Slopes Fin: currentVel = " + currentVel.ToString("F6") + "; below = " + collCheck.below);
        }
    }

    #endregion

    #endregion

    #region JUMP ---------------------------------------------------
    void PressA()
    {
        if(!disableAllDebugs && vertMovementDebugsOn) Debug.Log("TRY JUMP");
        if (!StartJump())
        {
            if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("FAILED JUMP... TRY WALLJUMP");
            //TO REDO
            if (!StartWallJump())
            {
                if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("FAILED WALLJUMP... Try CHARGING CHARGE JUMP ");
                StartChargingChargedJump();
            }
        }
    }

    #region --- Jump ---
    bool StartJump(bool calledFromBuffer = false)
    {
        bool result = false;
        if (!noInput && moveSt != MoveState.Boost)
        {
            if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("START JUMP: below = " + collCheck.below + "; jumpInsurance = " + jumpInsurance + "; sliding = " + collCheck.sliping + "; inWater = " + (vertMovSt == VerticalMovementState.FloatingInWater));
            if ((vertMovSt != VerticalMovementState.FloatingInWater && ((collCheck.below && !collCheck.sliping) || jumpInsurance) && !collCheck.tooSteepSlope && !isChargedJump && !isBounceJump) ||
                ((vertMovSt == VerticalMovementState.FloatingInWater && collCheck.around &&
                ((gC.gameMode == GameMode.CaptureTheFlag && !(gC as GameControllerCMF_FlagMode).myScoreManager.prorroga) || (gC.gameMode != GameMode.CaptureTheFlag)))))
            {
                if (!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("JUMP!");
                ExitWater();
                mover.stickToGround = false;
                /*if (!disableAllDebugs) */
                //Debug.LogWarning("stickToGround Off");
                myPlayerAnimation.SetJump(true);
                result = true;
                currentVel.y = jumpVelocity;
                vertMovSt = VerticalMovementState.Jumping;
                timePressingJump = 0;
                collCheck.StartJump();
                StopBufferedInput(PlayerInput.WallJump);
            }
            #region Jump debug
            else
            {
                string reason = "";
                if (vertMovSt != VerticalMovementState.FloatingInWater)
                {
                    if (!((collCheck.below && !collCheck.sliping) || jumpInsurance))
                    {
                        reason += "((collCheck.below && !collCheck.sliping) || jumpInsurance) results in false, when it should be true. collCheck.below = "
                            + collCheck.below + "; collCheck.sliping = " + collCheck.sliping + "; jumpInsurance = " + jumpInsurance;
                    }
                    else if (isChargedJump)
                    {
                        reason += "isChargedJump is true, and it should be false";
                    }
                    else if (isBounceJump)
                    {
                        reason += "isBounceJump is true, and it should be false";
                    }
                    else
                    {
                        reason += "This makes no sense. If you are reading this the world has gone mad.";
                    }
                }
                else if (!collCheck.around)
                {
                    reason += "; collCheck.around is false, and it should be true";
                }
                else if (!((gC.gameMode == GameMode.CaptureTheFlag && !(gC as GameControllerCMF_FlagMode).myScoreManager.prorroga) || (gC.gameMode != GameMode.CaptureTheFlag)))
                {
                    if (gC.gameMode == GameMode.CaptureTheFlag && (gC as GameControllerCMF_FlagMode).myScoreManager.prorroga)
                    {
                        reason += "We are in Game Mode Capture The Flag and we have entered the overtime!";
                    }
                    else
                    {
                        reason += "This makes no sense. If you are reading this the world has gone mad.";
                    }
                }
                if(!disableAllDebugs && vertMovementDebugsOn) Debug.Log("Jump failed because reason: " + reason);
            }
        }
        else
        {
            if (!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("Warning: Can't jump because: player is in noInput mode(" + !noInput + ") / moveSt != Boost (" + (moveSt != MoveState.Boost) + ")");
        }
        #endregion

        if (!result && !calledFromBuffer)
        {
            BufferInput(PlayerInput.Jump);
        }
        return result;
    }

    void StopJump()
    {
        myPlayerAnimation.SetJump(false);
        if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("StopJump: JumpSt = None");
        vertMovSt = VerticalMovementState.None;
        timePressingJump = 0;
    }

    void ProcessJumpInsurance()
    {
        if (!jumpInsurance)
        {
            //Debug.LogWarning(" collCheck.lastBelow = " + (collCheck.lastBelow) + "; collCheck.below = " + (collCheck.below) +
            //   "; jumpSt = " + jumpSt+"; jumpedOutOfWater = "+jumpedOutOfWater);
            if ((collCheck.lastBelow && !collCheck.lastSliping) && (!collCheck.below || collCheck.sliping) && (vertMovSt == VerticalMovementState.None || vertMovSt == VerticalMovementState.Falling) &&
                (vertMovSt != VerticalMovementState.BounceJumping && vertMovSt != VerticalMovementState.ChargeJumping) && jumpedOutOfWater)
            {
                //print("Jump Insurance");
                jumpInsurance = true;
                timeJumpInsurance = 0;
            }
        }
        else
        {
            timeJumpInsurance += Time.deltaTime;
            if (timeJumpInsurance >= maxTimeJumpInsurance || vertMovSt == VerticalMovementState.Jumping)
            {
                jumpInsurance = false;
            }
        }

    }
    #endregion

    #region --- Wall Jump ---
    /// <summary>
    /// In order to be able to save the input for Input buffering, we return true if the input was successful, 
    /// and false if the input was not successful and should be buffered.
    /// </summary>
    /// <returns></returns>
    bool StartWallJump(bool calledFromBuffer = false)
    {
        if (!disableAllDebugs && vertMovementDebugsOn && calledFromBuffer) Debug.Log("TRY WALLJUMP FROM BUFFER");
        collCheck.HorizontalCollisions();
        collCheck.WallJumpCollisions(currentVel);

        if (!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("Check Wall jump: wall real normal = " + collCheck.wallSlopeAngle);
        bool result = false;
        float slopeAngle = collCheck.wallSlopeAngle;
        bool goodWallAngle = !(slopeAngle >= 110 && slopeAngle <= 180);
        wallJumpCurrentWall = collCheck.wall;

        if (!collCheck.below && vertMovSt != VerticalMovementState.FloatingInWater && jumpedOutOfWater && wallJumpCurrentWall != null && goodWallAngle &&
            (!firstWallJumpDone || lastWallAngle != collCheck.wallAngle || lastWall != collCheck.wall) && (wallJumpCurrentWall.tag == "Stage" || wallJumpCurrentWall.tag == "Slide"))
        {
            if(!disableAllDebugs && vertMovementDebugsOn) Debug.Log("WallJump Stage script check...");
            if (wallJumpCurrentWall.GetComponent<StageScript>() == null || wallJumpCurrentWall.GetComponent<StageScript>().wallJumpable)
            {
                /*if (!disableAllDebugs)*/
                if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("Wall jumping start");
                //PARA ORTU: PlayerAnimation_01.startJump = true;
                vertMovSt = VerticalMovementState.WallJumping;
                result = true;
                //wallJumped = true;
                stopWallTime = 0;
                SetVelocity(Vector3.zero);
                wallJumping = true;
                anchorPoint = transform.position;
                wallNormal = collCheck.wallNormal;
                wallNormal.y = 0;
                wallJumpCurrentWallAngle = collCheck.wallAngle;
                //Debug.Log("WALL JUMP RAY HEIGHT PERCENTAGE : " + controller.collisions.closestHorRaycast.rayHeightPercentage + "%; wall = " + wallJumpCurrentWall.name);

                //STOP OTHER JUMPS BUFFERINGS AND PROCESSES
                StopBufferedInput(PlayerInput.Jump);
                StopBufferedInput(PlayerInput.StartChargingChargeJump);
                StopChargingChargedJump();
            }

        }
        else if (!disableAllDebugs && vertMovementDebugsOn)
        {
            string error = "Couldn't wall jump because:  ";
            if (collCheck.below)
            {
                error += "We are grounded (below = true); ";
            }
            else if(vertMovSt == VerticalMovementState.FloatingInWater)
            {
                error += "We are in the water (vertMovSt == VerticalMovementState.FloatingInWater); ";
            }else if (!jumpedOutOfWater)
            {
                error += "jumpedOutOfWater = false; ";
            }
            else if (wallJumpCurrentWall == null)
            {
                error += "wallJumpCurrentWall = "+ wallJumpCurrentWall+"; ";
            }
            else if (!goodWallAngle)
            {
                error += "Wall angle is not adecuate for walljumping (goodWallAngle = false); ";
            }
            else if (!(!firstWallJumpDone || lastWallAngle != collCheck.wallAngle || lastWall != collCheck.wall))
            {
                if(firstWallJumpDone) error += "firstWallJumpDone = "+ firstWallJumpDone + "; ";
                if (lastWallAngle == collCheck.wallAngle) error += "Wall Angle is different (lastWallAngle == collCheck.wallAngle); ";
                if (lastWall == collCheck.wall) error += "Wall Object is different (lastWall == collcheck.wall); ";
            }else if (!(wallJumpCurrentWall.tag == "Stage" || wallJumpCurrentWall.tag == "Slide"))
            {
                error+= "wallJumpCurrentWall.tag = "+ wallJumpCurrentWall.tag+", when it should be either Stage or Slide; ";
            }
            else
            {
                error += "Something is wrong, this shouldn't be happening; ";
            }
            Debug.Log(error);
        }
        if (!result && !calledFromBuffer)
        {
            BufferInput(PlayerInput.WallJump);
        }
        return result;
    }

    void ProcessWallJump()//IMPORTANTE QUE VAYA ANTES DE LLAMAR A "MOVE"
    {
        if (wallJumping)
        {
            stopWallTime += Time.deltaTime;
            if (actions.A.WasReleased || !actions.A.IsPressed)
            {
                EndWallJump();
                return;
            }

            if (stopWallTime >= stopWallMaxTime)
            {
                StopWallJump();
                return;
            }

            #region --- WALL JUMP CHECK COLLISSION ---
            bool success = false;
            collCheck.HorizontalCollisions();
            for(int i = 0; i < collCheck.horizontalCollHits.Count && !success; i++)
            {
                if (collCheck.horizontalCollHits[i].collider.gameObject == wallJumpCurrentWall)
                    success = true;
            }
            #endregion

            #region --- WALL JUMP CHECK RAYS OLD --- (Deprecated)
            /*//Check continuously if we are still attached to wall
            float rayLength = collCheck.myCollider.bounds.extents.x + 0.5f;
            RaycastHit hit;

            //calculamos el origen de todos los rayos y columnas: esquina inferior (izda o dcha,no sé)
            //del personaje. 
            Vector3 paralelToWall = new Vector3(-wallNormal.z, 0, wallNormal.x).normalized;
            Vector3 rowsOrigin = new Vector3(collCheck.myCollider.bounds.center.x, collCheck.myCollider.bounds.min.y, collCheck.myCollider.bounds.center.z);
            rowsOrigin -= paralelToWall * collCheck.myCollider.bounds.extents.x;
            Vector3 dir = -wallNormal.normalized;
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
                            if(!disableAllDebugs)Debug.Log("WallJump: Success! still walljumping!");
                        }
                        else
                        {
                            if (!disableAllDebugs) Debug.Log("WallJump: this wall (" + hit.transform.gameObject + ")is not the same wall that I started walljumping from " +
                                "(" + wallJumpCurrentWall + ").");
                        }
                    }
                }
            }*/
            #endregion

            if (!success)
            {
                if (!disableAllDebugs) Debug.LogError("STOPPED WALLJUMPING DUE TO NOT DETECTING THE WALL ANYMORE. wallJumpCheckRaysRows = " + wallJumpCheckRaysRows);
                StopWallJump();
            }
        }

    }

    void EndWallJump()
    {
        /*if (!disableAllDebugs)*/
        if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("End Wall jump Start");
        if (!firstWallJumpDone) firstWallJumpDone = true;
        lastWallAngle = wallJumpCurrentWallAngle;
        lastWall = wallJumpCurrentWall;
        wallJumping = false;
        wallJumpAnim = true;
        vertMovSt = VerticalMovementState.WallJumped;
        //CALCULATE JUMP DIR
        //LEFT OR RIGHT ORIENTATION?
        //Angle
        Vector3 circleCenter = anchorPoint + Vector3.up * walJumpConeHeight;
        Vector3 circumfPoint = CalculateReflectPoint(wallJumpRadius, wallNormal, circleCenter);
        Vector3 finalDir = (circumfPoint - anchorPoint).normalized;
        if (!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("End Wall Jump: FINAL DIR= " + finalDir.ToString("F4"));

        SetVelocity(finalDir * wallJumpVelocity);
        Vector3 newMoveDir = new Vector3(finalDir.x, 0, finalDir.z);
        RotateCharacter(newMoveDir);

        //myPlayerAnimation.SetJump(true);

        Debug.DrawLine(anchorPoint, circleCenter, Color.white, 20);
        Debug.DrawLine(anchorPoint, circumfPoint, Color.yellow, 20);
    }

    public void StopWallJump()
    {
        if(vertMovSt == VerticalMovementState.WallJumping)
        {
            if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("STOP WALLJUMP");
            wallJumping = false;
            wallJumpAnim = true;
            vertMovSt = VerticalMovementState.None;
        }
    }
    #endregion

    #region --- Charged Jump ---
    bool StartChargingChargedJump(bool calledFromBuffer = false)
    {
        bool result = false;
        //Debug.Log("Start Charging Charge Jump ? chargeJumpChargingJump = " + chargedJumpChargingJump);
        if (!chargedJumpChargingJump && !chargedJumpButtonReleased && vertMovSt == VerticalMovementState.Falling && vertMovSt != VerticalMovementState.FloatingInWater && !noInput && !jumpInsurance)
        {
            result = true;
            chargedJumpChargingJump = true;
            chargedJumpLanded = false;
            //chargeJumpButtonReleased = false;
            chargedJumpCurrentReleaseTime = 0;
            chargedJumpLandedTime = 0;
            chargedJumpChargingStartHeight = transform.position.y;
            failedJump = false;
            //StopBufferedInput(PlayerInput.Jump);
            if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("Start Charging Jump");
        }

        if (!result && !calledFromBuffer)
        {
            BufferInput(PlayerInput.StartChargingChargeJump);
        }
        return result;
    }

    void ProcessChargingChargedJump()
    {
        if (chargedJumpChargingJump)
        {
            if (!chargedJumpButtonReleased && inputsInfo.JumpWasReleased)
            {
                chargedJumpButtonReleased = true;
            }

            if (!chargedJumpLanded && collCheck.below)
            {
                if (!isFloorChargeJumpable)
                {
                    StopChargingChargedJump();
                    StartBounceJump();
                    return;
                }
                chargedJumpLanded = true;
            }

            //Debug.Log("charging jump: button released = " + chargedJumpButtonReleased);
            if (chargedJumpButtonReleased)
            {
                //Debug.Log("charging jump: chargeJumpLanded = " + chargedJumpLanded);
                if (chargedJumpLanded)
                {
                    StopChargingChargedJump();
                    if (failedJump)
                    {
                        StartBounceJump();
                    }
                    else
                    {
                        StartChargedJump();
                    }
                    return;
                }

                //FAIL?
                if (chargedJumpCurrentReleaseTime >= chargedJumpReleaseTimeBeforeLand)//FAIL: Released too early
                {
                    if(!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("Charged Jump-> FAILED JUMP: RELEASED TOO EARLY");
                    failedJump = true;
                    return;
                }

                chargedJumpCurrentReleaseTime += Time.deltaTime;
            }

            //FAIL?
            if (chargedJumpLanded && !chargedJumpButtonReleased)
            {
                if (chargedJumpLandedTime >= chargedJumpReleaseTimeAfterLand)//FAIL: Released too late
                {
                    if (!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("Charged Jump-> FAILED JUMP: RELEASED TOO LATE");
                    StopChargingChargedJump();
                    StartBounceJump();
                    return;
                }
                //Debug.Log("chargeJumpLandedTime  = " + chargedJumpLandedTime);
                chargedJumpLandedTime += Time.deltaTime;
            }
        }
    }

    void StopChargingChargedJump()
    {
        if (chargedJumpChargingJump)
        {
            chargedJumpChargingJump = false;
            chargedJumpLanded = false;
            chargedJumpButtonReleased = false;
            chargedJumpCurrentReleaseTime = 0;
            chargedJumpLandedTime = 0;
            if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("Stop Charging Jump");
        }
    }

    void StartChargedJump()
    {
        //Debug.Log("Start Charge Jump?");
        if (isFloorChargeJumpable && vertMovSt != VerticalMovementState.FloatingInWater && !noInput)
        {
            float totalFallenHeight = chargedJumpLastApexHeight - transform.position.y;
            if (totalFallenHeight == 0) Debug.LogError("Charge Jump Error: totalFallenHeight = 0");
            float totalChargedHeight = chargedJumpChargingStartHeight - transform.position.y;
            float percentageCharged = totalChargedHeight / totalFallenHeight;
            if (percentageCharged >= chargedJumpMinPercentageNeeded)
            {
                Debug.Log("DO CHARGE JUMP!");
                float distToCloudHeight = Mathf.Abs(chargedJumpMaxHeight - transform.position.y);
                float currentMaxHeight = Mathf.Min(chargedJumpMaxJumpHeight, distToCloudHeight);
                StageScript stageScript = collCheck.floor != null ? collCheck.floor.GetComponent<StageScript>() : null;

                switch (chargedJumpMode)
                {
                    case ChargedJumpMode.Fixed:
                        bool isFixedJump = stageScript != null && stageScript.chargeJumpAmount >= 0 ? true : false;
                        if (!isFixedJump)
                        {
                            //Proportional Mode Copy
                            float auxChargeJumpCurrentJumpMaxHeight1 = totalFallenHeight + (totalFallenHeight * percentageCharged * chargedJumpFallenHeightMultiplier);
                            chargedJumpCurrentJumpMaxHeight = Mathf.Clamp(auxChargeJumpCurrentJumpMaxHeight1, jumpHeight, currentMaxHeight);
                        }
                        else
                        {
                            float chargeJumpHeight = stageScript != null && stageScript.chargeJumpAmount >= 0 ? stageScript.chargeJumpAmount : chargedJumpMaxJumpHeight;
                            chargedJumpCurrentJumpMaxHeight = chargeJumpHeight;
                        }
                        break;
                    case ChargedJumpMode.Proportional:
                        float auxChargeJumpCurrentJumpMaxHeight2 = totalFallenHeight + (totalFallenHeight * percentageCharged * chargedJumpFallenHeightMultiplier);
                        chargedJumpCurrentJumpMaxHeight = Mathf.Clamp(auxChargeJumpCurrentJumpMaxHeight2, jumpHeight, currentMaxHeight);
                        break;
                    case ChargedJumpMode.Proportional_With_Min:
                        bool hasMinHeight = stageScript != null && stageScript.chargeJumpAmount >= 0 ? true : false;
                        float chargedJumpMinJumpHeight = hasMinHeight ? stageScript.chargeJumpAmount : jumpHeight;

                        float auxChargeJumpCurrentJumpMaxHeight3 = totalFallenHeight + (totalFallenHeight * percentageCharged * chargedJumpFallenHeightMultiplier);
                        chargedJumpCurrentJumpMaxHeight = Mathf.Clamp(auxChargeJumpCurrentJumpMaxHeight3, chargedJumpMinJumpHeight, currentMaxHeight);
                        break;
                }

                //bool isFixedJump = stageScript != null && stageScript.chargeJumpAmount>0 ? true : false;
                //if (!isFixedJump)
                //{
                //    float auxChargeJumpCurrentJumpMaxHeight = totalFallenHeight + (totalFallenHeight * percentageCharged * chargedJumpFallenHeightMultiplier);
                //    chargedJumpCurrentJumpMaxHeight = Mathf.Clamp(auxChargeJumpCurrentJumpMaxHeight, jumpHeight, currentMaxHeight);
                //}
                //else
                //{
                //    float chargeJumpHeight = stageScript != null && stageScript.chargeJumpAmount > 0 ? stageScript.chargeJumpAmount : chargedJumpMaxJumpHeight;
                //    chargedJumpCurrentJumpMaxHeight = chargeJumpHeight;
                //}
                float chargeJumpApexTime = Mathf.Sqrt((2 * chargedJumpCurrentJumpMaxHeight) / Mathf.Abs(currentGravity));
                float chargeJumpJumpVelocity = Mathf.Abs(currentGravity * chargeJumpApexTime);

                mover.stickToGround = false;
                /*if (!disableAllDebugs) */
                Debug.LogWarning("stickToGround Off");

                collCheck.below = false;
                currentVel.y = chargeJumpJumpVelocity;

                vertMovSt = VerticalMovementState.ChargeJumping;
                //Debug.Log("percentageCharged = " + percentageCharged + "; totalFallenHeight = " + totalFallenHeight + "; chargeJumpMaxHeight = " + chargedJumpMaxHeight + "; transform.position.y = "
                //    + transform.position.y + "; distToCloudHeight = " + distToCloudHeight + "; currentMaxHeight = " + currentMaxHeight
                //    + "; chargeJumpCurrentJumpMaxHeight = " + chargedJumpCurrentJumpMaxHeight + "; chargeJumpApexTime = "
                //    + chargeJumpApexTime + "; chargeJumpJumpVelocity = " + chargeJumpJumpVelocity);
            }
            else
            {
                Debug.LogWarning("Charged Jump-> Failed Jump: Charged less than a " + (chargedJumpMinPercentageNeeded * 100) + "% of the fall");
                StartBounceJump();
            }
        }
    }
    #endregion

    #region --- Bounce Jump ---
    bool StartBounceJump()//WHEN CHARGEJUMP FAILS
    {
        if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("Check for Start Bounce Jump");
        bool result = false;
        if (vertMovSt != VerticalMovementState.FloatingInWater && isFloorBounceJumpable)
        {
            float totalFallenHeight = chargedJumpLastApexHeight - transform.position.y;
            chargedJumpCurrentJumpMaxHeight = totalFallenHeight * bounceJumpMultiplier;
            if (chargedJumpCurrentJumpMaxHeight >= bounceJumpMinHeight)
            {
                /*if (!disableAllDebugs)*/
                if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("DO BOUNCE JUMP");
                float chargeJumpApexTime = Mathf.Sqrt((2 * chargedJumpCurrentJumpMaxHeight) / Mathf.Abs(currentGravity));
                float chargeJumpJumpVelocity = Mathf.Abs(currentGravity * chargeJumpApexTime);

                mover.stickToGround = false;
                /*if (!disableAllDebugs) */
                if (!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("stickToGround Off");

                collCheck.below = false;
                currentVel.y = chargeJumpJumpVelocity;
                vertMovSt = VerticalMovementState.BounceJumping;
                StopBufferedInput(PlayerInput.Jump);
                StopBufferedInput(PlayerInput.WallJump);

                if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("JumpSt = BounceJumping" + "; chargeJumpJumpVelocity = " + chargeJumpJumpVelocity);
                result = true;
            }
            else
            {
                if (!disableAllDebugs && vertMovementDebugsOn) Debug.LogWarning("Bounce Jump -> bounceJump height was too low (min = " + bounceJumpMinHeight + ")");
            }
        }
        return result;
    }
    #endregion

    #endregion

    #region FIXED JUMP ---------------------------------------------------

    public void StartFixedJump(Vector3 vel, float _noMoveMaxTime, float maxTime, bool _bounceEnabled = false)
    {
        ExitWater();
        StopJump();
        StopFixedJump();

        if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("FIXED JUMP START");
        StopBoost();
        myPlayerCombat.StopDoingCombat();
        StopSufferingEffect();


        fixedJumping = true;
        fixedJumpDone = false;
        noInput = true;
        noMoveMaxTime = _noMoveMaxTime;
        fixedJumpMaxTime = maxTime;
        noMoveTime = 0;
        fixedJumpBounceEnabled = _bounceEnabled;
        knockback = vel;

        moveSt = MoveState.FixedJump;
    }

    void ProcessFixedJump()
    {
        if (fixedJumping)
        {
            if (!fixedJumpDone)
            {
                //Debug.Log("ProcessFixedJump: fixedJumpDone = false");
                collCheck.below = false;
                mover.stickToGround = false;
            }
            else
            {
                noMoveTime += Time.deltaTime;
                if (noInput && (noMoveTime >= noMoveMaxTime))
                {
                    noInput = false;
                    moveSt = MoveState.None;
                }
                //Debug.Log("ProcessFixedJump: noMoveTime = " + noMoveTime + "; fixedJumpMaxTime = " + fixedJumpMaxTime+ "; collCheck.below = "+ collCheck.below +
                //    "; (vertMovSt == VerticalMovementState.FloatingInWater) = " +(vertMovSt == VerticalMovementState.FloatingInWater));
                if (noMoveTime >= fixedJumpMaxTime || collCheck.below || vertMovSt == VerticalMovementState.FloatingInWater)
                {
                    StopFixedJump();
                }
            }
        }
    }

    void StopFixedJump()
    {
        if (fixedJumping)
        {
            if (!disableAllDebugs && vertMovementDebugsOn) Debug.Log("STOP FIXED JUMP");
            fixedJumping = false;
            noInput = false;
            moveSt = MoveState.None;
        }
    }

    #endregion

    #region  DASH ---------------------------------------------

    void StartBoost()
    {
        if (!noInput && boostReady && vertMovSt != VerticalMovementState.FloatingInWater && myPlayerCombat.attackStg == AttackPhaseType.ready)
        {
            //IMPORTANT: Don't change order, call this first as it changes the moveSt
            StopImpulse();
            StopFixedJump();

            mover.stickToGround = false;
            /*if (!disableAllDebugs)*/
            if (!disableAllDebugs && charContDebugsOn) Debug.LogWarning("stickToGround Off");
            boostCurrentFuel -= boostCapacity * boostFuelLostOnStart;
            boostCurrentFuel = Mathf.Clamp(boostCurrentFuel, 0, boostCapacity);
            moveSt = MoveState.Boost;
            if (currentInputDir != Vector3.zero)//Usando el joystick o dirección
            {
                boostDir = currentInputDir;
                RotateCharacter(boostDir);
            }
            else//sin tocar ninguna dirección/joystick
            {
                Vector3 newMoveDir = boostDir = new Vector3(currentCamFacingDir.x, 0, currentCamFacingDir.z);//nos movemos en la dirección en la que mire la cámara
                RotateCharacter(newMoveDir);
            }
            myPlayerHUD.StartCamVFX(CameraVFXType.Dash);
            myPlayerVFX.ActivateEffect(PlayerVFXType.DashWaterImpulse);
            myPlayerVFX.ActivateEffect(PlayerVFXType.DashTrail);
            myPlayerCombat.StopDoingCombat();
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
                if (inputsInfo.R2WasReleased)
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
            mover.stickToGround = true;
            if (!disableAllDebugs && charContDebugsOn)
                Debug.LogWarning("stickToGround On");
            moveSt = MoveState.None;
            StartBoostCD();
            myPlayerHUD.StopCamVFX(CameraVFXType.Dash);
            myPlayerVFX.DeactivateEffect(PlayerVFXType.DashWaterImpulse);
            myPlayerVFX.DeactivateEffect(PlayerVFXType.DashTrail);
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

    //void WallBoost(GameObject wall)
    //{
    //    if (wall.tag == "Stage")
    //    {
    //        //CALCULATE JUMP DIR
    //        Vector3 circleCenter = transform.position;
    //        Vector3 circumfPoint = CalculateReflectPoint(1, collInfo.wallNormal, circleCenter);
    //        Vector3 finalDir = (circumfPoint - circleCenter).normalized;
    //        Debug.LogWarning("WALL BOOST: FINAL DIR = " + finalDir.ToString("F4"));

    //        currentVel = finalDir * currentVel.magnitude;
    //        currentSpeed = currentVel.magnitude;
    //        boostDir = new Vector3(finalDir.x, 0, finalDir.z);
    //        RotateCharacter(boostDir);
    //    }
    //}

    #endregion

    #region  FACING DIR AND ANGLE & BODY ROTATION ------------------------------------

    void UpdateFacingDir()//change so that only rotateObj rotates, not whole body
    {
        switch (myCamera.camMode)
        {
            case CameraMode.Fixed:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                //Calculate looking dir of camera
                Vector3 camPos = myCamera.transform.GetChild(0).position;
                Vector3 myPos = transform.position;
                //currentFacingDir = new Vector3(myPos.x - camPos.x, 0, myPos.z - camPos.z).normalized;
                currentCamFacingDir = myCamera.myCamera.transform.forward.normalized;
                break;
            case CameraMode.Shoulder:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                //currentFacingDir = RotateVector(-myCamera.transform.localRotation.eulerAngles.y, Vector3.forward).normalized;
                currentCamFacingDir = myCamera.myCamera.transform.forward.normalized;
                //print("CurrentFacingDir = " + currentFacingDir);
                break;
            case CameraMode.Free:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                //currentFacingDir = RotateVector(-rotateObj.localRotation.eulerAngles.y, Vector3.forward).normalized;
                currentCamFacingDir = (cameraFollow.position - myCamera.myCamera.transform.position).normalized;
                break;
        }
        currentFacingDir = rotateObj.forward;
        //print("currentFacingDir = " + currentFacingDir + "; currentCamFacingDir = " + currentCamFacingDir);
    }

    public void RotateCharacter(float rotSpeed = 0)
    {
        switch (myCamera.camMode)
        {
            case CameraMode.Fixed:
                Vector3 point1 = transform.position;
                Vector3 point2 = new Vector3(point1.x, point1.y + 1, point1.z);
                Vector3 dir = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
                rotateObj.Rotate(dir, rotSpeed * Time.deltaTime);
                break;
            case CameraMode.Shoulder:
                point1 = transform.position;
                point2 = new Vector3(point1.x, point1.y + 1, point1.z);
                dir = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
                rotateObj.Rotate(dir, rotSpeed * Time.deltaTime);
                break;
            case CameraMode.Free:
                Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
                Vector3 lookingDir = wallJumping ? currentInputDir : hardSteerOn ? hardSteerDir : horVel; lookingDir.Normalize();
                if (lookingDir != Vector3.zero)
                {
                    float angle = Mathf.Acos(((0 * lookingDir.x) + (1 * lookingDir.z)) / (1 * lookingDir.magnitude)) * Mathf.Rad2Deg;
                    angle = lookingDir.x < 0 ? 360 - angle : angle;
                    targetRotAngle = angle;
                    //if (hardSteerStarted)
                    //{
                    //    rotateObj.localRotation = Quaternion.Euler(0, targetRotAngle, 0);
                    //}
                }
                break;
        }
        //print("current angle = " + rotateObj.rotation.eulerAngles.y + "; currentInputDir = " + currentInputDir);
    }

    void SmoothRotateCharacter()
    {
        if (myCamera.camMode == CameraMode.Free )//TO DO: Bloquear en knockback?
        {
            float currentAngle = rotateObj.rotation.eulerAngles.y;
            //if(currentAngle != targetRotAngle)
            //{
            float result1 = currentAngle - targetRotAngle;
            float result2 = currentAngle - (targetRotAngle + (360 * Mathf.Sign(result1)));
            bool normal = Mathf.Abs(result1) <= Mathf.Abs(result2);
            float newAngle = normal ? targetRotAngle : targetRotAngle >= 0 && targetRotAngle < 180 ? targetRotAngle + 360 : targetRotAngle - 360;
            currentAngle = Mathf.Lerp(currentAngle, newAngle, 0.4f);
            //Debug.Log("currentAngle = " + rotateObj.rotation.eulerAngles.y + "; targetRotAngle = " + targetRotAngle + "; newAngle = " + newAngle +
            //    "; result1 = " + result1 + "; result2 = " + result2 + "; normal = " + normal);
            rotateObj.localRotation = Quaternion.Euler(0, currentAngle, 0);
            //}
        }
    }

    void RotateCharacter(Vector3 dir)
    {
        float angle = Mathf.Acos(dir.z / dir.magnitude) * Mathf.Rad2Deg;
        angle = dir.x < 0 ? 360 - angle : angle;
        rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
    }

    void RotateCharacterInstantly(float angle)
    {
        targetRotAngle = angle;
        rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
        if(!disableAllDebugs && horMovementDebugsOn)Debug.Log("RotateCharacterInstantly: angle = " + angle);
    }

    void RotateCharacterInstantly(Vector3 direction)
    {
        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
        Vector3 lookingDir = wallJumping ? currentInputDir : hardSteerOn ? hardSteerDir : horVel; lookingDir.Normalize();
        if (lookingDir != Vector3.zero)
        {
            float angle = Mathf.Acos(((0 * lookingDir.x) + (1 * lookingDir.z)) / (1 * lookingDir.magnitude)) * Mathf.Rad2Deg;
            angle = lookingDir.x < 0 ? 360 - angle : angle;
            targetRotAngle = angle;
            rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
            //Debug.Log("RotateCharacterInstantly: direction = " + direction + "; angle = " + angle);
        }
    }

    #endregion

    #region RECIEVE HIT AND EFFECTS ---------------------------------------------
    PlayerMovementCMF lastAttacker = null;
    int lastAutocomboIndex = -1;
    bool stunProtectionOn = false;
    public bool StartReceiveHit(PlayerMovementCMF attacker, Vector3 _knockback, EffectType effect, float _maxTime = 0, byte autocomboIndex = 255)
    {
        if (!myPlayerCombat.invulnerable)
        {
            //Variable para Ortu
            startBeingHitAnimation = true;

            if (!disableAllDebugs) print("Recieve hit with knockback= " + _knockback + "; effect = " + effect + "; maxtime = " + _maxTime);
            myPlayerHook.FinishAutoGrapple(true);
            myPlayerHook.StopHook();
            StopWallJump();
            StopBoost();
            myPlayerCombat.StopDoingCombat();
            StopImpulse();
            StopFixedJump();

            //if (sufferingEffect == EffectType.stun)
            //{
            //    efecto = EffectType.knockdown;
            //    _maxTime = AttackEffect.knockdownTime;
            //}
            #region STUN PROTECTION

            if (sufferingEffect == EffectType.softStun && (lastAttacker != attacker || (lastAttacker == attacker && autocomboIndex == 0)))
            {
                Debug.Log("Stun protection ON");
                stunProtectionOn = true;
            }

            if (!stunProtectionOn)
            {
                Debug.Log("FULL EFFECT TIME");
                effectMaxTime = _maxTime;
            }
            else
            {
                Debug.Log("THIRD EFFECT TIME");
                effectMaxTime = _maxTime / 3;
            }
            #endregion

            sufferingEffect = effect;
            switch (effect)
            {
                case EffectType.softStun:
                    noInput = true;
                    break;
                //case EffectType.stun:
                //    if (disableAllDebugs) Debug.LogError("STUN !!!!!!!!");
                //    noInput = true;
                //    break;
                //case EffectType.knockdown:
                //    if (disableAllDebugs) Debug.LogError("KNOCKDOWN !!!!!!!!");
                //    noInput = true;
                //    //myPlayerCombat.StartInvulnerabilty(_maxTime);
                //    break;
                case EffectType.none:
                    break;
                default:
                    Debug.LogError("Error: cannot have a 'sufferingEffect' of type " + sufferingEffect);
                    break;
            }

            //Debug.Log("_maxTime = " + _maxTime + "; effectMaxTime = " + effectMaxTime);
            effectTime = 0;
            if (_knockback != Vector3.zero)
            {
                knockbackDone = false;
                _knockback = _knockback / bodyMass;
                knockback = _knockback;
            }

            //Give FLAG
            if (haveFlag)
            {
                flag.DropFlag();
            }
            lastAttacker = attacker;
            lastAutocomboIndex = autocomboIndex;
            if (!disableAllDebugs) print("Player " + playerNumber + " RECIEVED HIT");
            return true;
        }
        else return false;
    }

    public bool StartReceiveHit(Vector3 _knockback, EffectType effect, float _maxTime = 0)
    {
        if (!myPlayerCombat.invulnerable)
        {
            //Variable para Ortu
            startBeingHitAnimation = true;

            if (!disableAllDebugs) print("Recieve hit with knockback= " + _knockback + "; effect = " + effect + "; maxtime = " + _maxTime);
            myPlayerHook.FinishAutoGrapple(true);
            myPlayerHook.StopHook();
            StopWallJump();
            StopBoost();
            myPlayerCombat.StopDoingCombat();
            StopImpulse();
            StopFixedJump();

            sufferingEffect = effect;
            switch (effect)
            {
                case EffectType.softStun:
                    noInput = true;
                    break;
                case EffectType.none:
                    break;
                default:
                    Debug.LogError("Error: cannot have a 'sufferingEffect' of type " + sufferingEffect);
                    break;
            }

            //Debug.Log("_maxTime = " + _maxTime + "; effectMaxTime = " + effectMaxTime);
            effectTime = 0;
            if (_knockback != Vector3.zero)
            {
                knockbackDone = false;
                _knockback = _knockback / bodyMass;
                knockback = _knockback;
            }

            //Give FLAG
            if (haveFlag)
            {
                flag.DropFlag();
            }
            if (!disableAllDebugs) print("Player " + playerNumber + " RECIEVED HIT");
            return true;
        }
        else return false;
    }

    //ES RECEIVE
    public void StartReceiveParry(PlayerMovementCMF enemy, AttackEffect effect = null)
    {
        if (!disableAllDebugs) Debug.Log("PARRY!!!");
        float knockbackMag = effect == null ? 10 : effect.knockbackMagnitude;
        float maxStunTime = effect != null && effect.parryStunTime > 0 ? effect.parryStunTime : 0.5f;

        Vector3 enemyPos = enemy.transform.position;
        //Reduce Recovery Time Player 1
        enemy.myPlayerCombat.HitParry();

        //StartRecieveParry Player2
        //Knockback outwards
        Vector3 parryMyPos = enemyPos;
        Vector3 parryColPos = transform.position;
        Vector3 resultKnockback = new Vector3(parryColPos.x - parryMyPos.x, 0, parryColPos.z - parryMyPos.z).normalized;
        //resultKnockback = Quaternion.Euler(0, effect.knockbackYAngle, 0) * resultKnockback;
        if (!disableAllDebugs) Debug.Log("resultKnockback 1 = " + resultKnockback);
        resultKnockback = HitboxCMF.CalculateYAngle(enemyPos, resultKnockback, 25f);
        if (!disableAllDebugs) Debug.Log("resultKnockback 2 = " + resultKnockback);
        resultKnockback = resultKnockback * knockbackMag;
        if (!disableAllDebugs) Debug.Log("resultKnockback 3 = " + resultKnockback + "; maxStunTime = " + maxStunTime);
        StartReceiveHit(enemy, resultKnockback, EffectType.softStun, maxStunTime);
    }

    void ReceiveKnockback()
    {
        if (knockback != Vector3.zero)
        {
            moveSt = MoveState.Knockback;
        }
    }

    void ProcessSufferingEffect()
    {
        ReceiveKnockback();
        if (sufferingEffect == EffectType.softStun)//|| sufferingEffect == EffectType.stun || sufferingEffect == EffectType.knockdown
        {
            effectTime += Time.deltaTime;
            if (effectTime >= effectMaxTime)
            {
                StopSufferingEffect();
            }
        }
    }

    void StopSufferingEffect()
    {
        if (!disableAllDebugs) print("Suffering effect " + sufferingEffect + " END");
        switch (sufferingEffect)
        {
            case EffectType.softStun:
                noInput = false;
                sufferingEffect = EffectType.none;
                effectTime = 0;
                stunProtectionOn = false;
                Debug.Log("Stun protection OFF");
                break;
            //case EffectType.stun:
            //    noInput = false;
            //    sufferingEffect = EffectType.none;
            //    effectTime = 0;
            //    break;
            //case EffectType.knockdown:
            //    noInput = false;
            //    sufferingEffect = EffectType.none;
            //    effectTime = 0;
            //    break;
            case EffectType.none:
                break;
            default:
                Debug.LogError("Error: cannot have a 'sufferingEffect' of type " + sufferingEffect);
                break;
        }
        startBeingHitAnimation = false;
    }

    #endregion

    #region HOOKING/HOOKED ---------------------------------------------
    public bool StartHooked()
    {
        if (!hooked && !myPlayerCombat.invulnerable)
        {
            StopWallJump();
            StopBoost();
            //Stop attacking
            myPlayerCombat.StopDoingCombat();
            StopImpulse();
            StopFixedJump();

            noInput = true;
            hooked = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    void ProcessHooked()
    {
        if (hooked)
        {
            moveSt = MoveState.Hooked;
        }
    }

    public void StopHooked(float reducedSpeedPercentage = 0)
    {
        //print("STOP HOOKED");
        if (hooked)
        {
            noInput = false;
            hooked = false;
            SetVelocity(currentVel * reducedSpeedPercentage);
        }
    }

    public void StartHooking()
    {
        if (!hooking)
        {
            hooking = true;
            myPlayerCombat.StopDoingCombat();
            hookingAndTouchedGroundOnce = false;
            if (collCheck.below) hookingAndTouchedGroundOnce = true;
        }
    }

    void ProcessHooking()
    {
        if (hooking)
        {
            if (!hookingAndTouchedGroundOnce)
            {
                if (collCheck.below) hookingAndTouchedGroundOnce = true;
            }
            else if (currentMaxMoveSpeed > maxHookingSpeed)
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
        if (!aimingAndTouchedGroundOnce)
        {
            if (collCheck.below)
            {
                aimingAndTouchedGroundOnce = true;
            }
        }
        else if (myPlayerCombat.aiming && currentMaxMoveSpeed > maxAimingSpeed)
        {
            currentMaxMoveSpeed = maxAimingSpeed;
        }
    }
    #endregion

    #region PICKUP / FLAG / DEATH ---------------------------------------------

    public void PutOnFlag(FlagCMF _flag)
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
        (gC as GameControllerCMF_FlagMode).HideFlagHomeLightBeam(team);
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
        if (vertMovSt != VerticalMovementState.FloatingInWater)
        {
            if(!disableAllDebugs && waterDebugsOn) Debug.Log("ENTER WATER");
            myPlayerAnimation.enterWater = true;
            vertMovSt = VerticalMovementState.FloatingInWater;
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
                (gC as GameControllerCMF_FlagMode).myScoreManager.PlayerEliminado();
            }
            myPlayerVFX.ActivateMoveWaves();
            myPlayerVFX.ActivateEffect(PlayerVFXType.SwimmingEffect);

            Vector3 waterSplashPos = myPlayerVFX.GetEffectGO(PlayerVFXType.WaterSplash).transform.position;
            waterSplashPos.y = myPlayerBody.waterSurfaceHeight;
            myPlayerVFX.GetEffectGO(PlayerVFXType.WaterSplash).transform.position = waterSplashPos;
            myPlayerVFX.ActivateEffect(PlayerVFXType.WaterSplash);

            myPlayerCombat.StopDoingCombat();
            myPlayerHook.StopHook();
            mover.stickToGround = false;
        }
    }

    void ProcessWaterMaxSpeed()
    {
        if (vertMovSt == VerticalMovementState.FloatingInWater)
        {
            //TO REDO
            //controller.AroundCollisions();
            if (currentMaxMoveSpeed > maxSpeedInWater)
            {
                currentMaxMoveSpeed = maxSpeedInWater;
            }
        }
    }

    public void ExitWater()
    {
        if (vertMovSt == VerticalMovementState.FloatingInWater)
        {
            if (!disableAllDebugs && waterDebugsOn) Debug.Log("EXIT WATER");
            vertMovSt = VerticalMovementState.None;
            myPlayerAnimation.exitWater = true;
            //vertMovSt = ;
            myPlayerWeap.AttatchWeapon();

            myPlayerVFX.DeactivateMoveWaves();
            myPlayerVFX.DeactivateEffect(PlayerVFXType.SwimmingEffect);

            Vector3 waterSplashPos = myPlayerVFX.GetEffectGO(PlayerVFXType.WaterSplash).transform.position;
            waterSplashPos.y = myPlayerBody.waterSurfaceHeight;
            myPlayerVFX.GetEffectGO(PlayerVFXType.WaterSplash).transform.position = waterSplashPos;
            myPlayerVFX.ActivateEffect(PlayerVFXType.WaterSplash);
            mover.stickToGround = true;
        }
    }
    #endregion

    #region  CHECK WIN && GAME OVER ---------------------------------------------
    public void CheckScorePoint(FlagHome flagHome)
    {
        if (haveFlag && team == flagHome.team && this.moveSt != MoveState.Hooked)
        {
            (gC as GameControllerCMF_FlagMode).ScorePoint(team);
            if (flag != null)
            {
                flag.SetAway(true);
            }
        }
    }

    public void DoGameOver()
    {
        Debug.Log("GAME OVER");
        currentSpeed = 0;
        currentVel = Vector3.zero;
        mover.SetVelocity(currentVel, Vector3.zero);
        //TO REDO
        //controller.Move(currentVel * Time.deltaTime);
    }
    #endregion

    #region LIGHT BEAM ---------------------------------------------
    float distanceToFlag;
    void UpdateDistanceToFlag()
    {
        //if (gC.online)
        //{
        //}
        //else
        //{
        distanceToFlag = ((gC as GameControllerCMF_FlagMode).flags[0].transform.position - transform.position).magnitude;
        //}
    }

    void UpdateFlagLightBeam()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)
        {
            if (!haveFlag)
            {
                UpdateDistanceToFlag();
                //print("distanceToFlag = " + distanceToFlag);
                if (distanceToFlag >= (gC as GameControllerCMF_FlagMode).minDistToSeeBeam)
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

    public void ResetPlayer()
    {
        //Stop doing everything (only stopped a few things, I don't know which more I need to stop)
        StopBoost();
        StopImpulse();
        StopFixedJump();
        
        ExitWater();
        vertMovSt = VerticalMovementState.None;
        myPlayerHook.ResetHook();
        if (haveFlag)
        {
            flag.SetAway(false);
        }
        boostCurrentFuel = boostCapacity;
        //myPlayerWeap.DropWeapon();
        //controller.collisionMask = LayerMask.GetMask("Stage", "WaterFloor", "SpawnWall");
    }

    //void EquipWeaponAtStart()
    //{
    //    //print("EQUIP WEAPON AT START");
    //    switch (team)
    //    {
    //        case Team.A:
    //            //print("EQUIP BLUE WEAPON");
    //            myPlayerWeap.PickupWeapon(gC.startingWeaponA);
    //            break;
    //        case Team.B:
    //            //print("EQUIP RED WEAPON");
    //            myPlayerWeap.PickupWeapon(gC.startingWeaponB);
    //            break;
    //    }
    //}

    public float SignedRelativeAngle(Vector3 referenceForward, Vector3 newDirection, Vector3 referenceUp)
    {
        // the vector perpendicular to referenceForward (90 degrees clockwise)
        // (used to determine if angle is positive or negative)
        Vector3 referenceRight = Vector3.Cross(referenceUp, referenceForward);
        // Get the angle in degrees between 0 and 180
        float angle = Vector3.Angle(newDirection, referenceForward);
        // Determine if the degree value should be negative.  Here, a positive value
        // from the dot product means that our vector is on the right of the reference vector   
        // whereas a negative value means we're on the left.
        float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
        return (sign * angle);//final angle
    }

    public void TeleportPlayer(Vector3 worldPos)
    {
        ExitWater();
        StopBoost();
        StopHooked(0);
        StopHooking();
        StopWallJump();
        transform.position = worldPos;
    }

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

    #region ----[ BOLT CALLBACKS ]----

    #endregion

    #region ----[ NETWORK FUNCTIONS ]----

    #endregion
}

#region --- [ STRUCTS & CLASSES ] ---
public class InputsInfo
{
    PlayerActions actions;

    public InputsInfo(PlayerActions _actions)
    {
        actions = _actions;
    }

    public bool JumpWasPressed;//A / Spacebar
    public bool JumpWasReleased;//A / Spacebar

    public bool R2WasPressed;//Dash / throw hook / Shift
    public bool R2WasReleased;//Dash / throw hook / Shift

    public void ResetInputs()
    {
        ResetJump();
        ResetR2();
    }

    public void PollInputs()
    {
        PollJump();
        PollR2();//dash
    }

    public void PollJump()
    {
        if (!JumpWasPressed && !JumpWasReleased)
        {
            if (actions.A.WasPressed)
            {
                JumpWasPressed = true;
                JumpWasReleased = false;
            }
            else if (actions.A.WasReleased)
            {
                JumpWasPressed = false;
                JumpWasReleased = true;
            }
        }
    }

    public void ResetJump()
    {
        JumpWasPressed = false;
        JumpWasReleased = false;
    }

    public void PollR2()//dash
    {
        if (!R2WasPressed && !R2WasReleased)
        {
            if (actions.R2.WasPressed)
            {
                R2WasPressed = true;
                R2WasReleased = false;
            }
            else if (actions.R2.WasReleased)
            {
                R2WasPressed = false;
                R2WasReleased = true;
            }
        }
    }

    public void ResetR2()
    {
        R2WasPressed = false;
        R2WasReleased = false;
    }
}

//public class CollisionInfo
//{
//    public Collider collider;
//    public GameObject floor;
//    public bool isGrounded;
//    public bool lastIsGrounded;
//    public Vector3 wallNormal;

//    public CollisionInfo(Collider _collider)
//    {
//        collider = _collider;
//        floor = null;
//        isGrounded = false;
//        lastIsGrounded = false;
//        wallNormal = Vector3.zero;
//    }

//    public void ResetVariables()
//    {
//        lastIsGrounded = isGrounded;
//        lastIsGrounded = false;
//        floor = null;
//        wallNormal = Vector3.zero;
//    }
//}
#endregion
