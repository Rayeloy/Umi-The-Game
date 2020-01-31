using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;




[System.Serializable]
public class MyIntEvent : UnityEvent<int>
{
}

#region ----[ PUBLIC ENUMS ]----
#endregion

#region ----[ REQUIRECOMPONENT ]----
[RequireComponent(typeof(Controller3D))]
#endregion
public class Dummy : MonoBehaviour
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    [Header(" --- Referencias --- ")]
    //Referencias
    public Controller3D controller;

    //public MyIntEvent phaseProgressFunction;

    public float bodyMass;
    [Tooltip("Max number of hits the dummy can take before deflating")]
    public int maxHP;
    int currentHP;

    public bool disableAllDebugs;

    [HideInInspector] public bool startBeingHitAnimation = false;

    //CONTROLES
    public PlayerActions Actions { get; set; }

    //[Header("Body and body color")]

    //VARIABLES DE MOVIMIENTO

    [Header(" --- ROTATION --- ")]
    [Tooltip("Min angle needed in a turn to activate the instant rotation of the character (and usually the hardSteer mechanic too)")]
    [Range(0, 180)]
    public float instantRotationMinAngle = 120;

    [Header("--- SPEED ---")]
    public float maxFallSpeed = 100;
    public float maxMoveSpeed = 10;
    [Tooltip("Maximum speed that you can travel at horizontally when hit by someone")]
    public float maxKnockbackSpeed = 300f;
    public float maxSpeedInWater = 5f;
    public float maxVerticalSpeedInWater = 3f;

    [Header(" --- ACCELERATIONS --- ")]
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

    [Header(" --- JUMP --- ")]
    public float jumpHeight = 4f;
    public float jumpApexTime = 0.4f;
    float jumpVelocity;

    [Header("----- ONLINE VARIABLES ----")]
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    #endregion

    #region ----[ PROPERTIES ]----
    //Referencias que no se asignan en el inspector

    //ONLINE
    [HideInInspector]
    public bool online = false;

    [HideInInspector]
    public PlayerSpawnInfo mySpawnInfo;
    //[Header("Body Mass")]
    //[Tooltip("Body Mass Index. 1 is for normal body mass.")]

    //BOOL PARA PERMITIR O BLOQUEAR INPUTS
    [HideInInspector]
    public bool noInput = false;

    //MOVIMIENTO
    [HideInInspector]
    public MoveState moveSt = MoveState.NotMoving;
    [HideInInspector]
    public Vector3 currentVel;
    Vector3 oldCurrentVel;
    [HideInInspector]
    public float currentSpeed = 0;
    //[HideInInspector]
    //public Vector3 currentMovDir;//= a currentInputDir??? 

    //[HideInInspector]
    //public Vector3 currentInputDir;
    //[HideInInspector]
    //public Vector3 currentFacingDir = Vector3.forward;
    //[HideInInspector]
    //public float facingAngle = 0;
    //[HideInInspector]
    //public Vector3 currentCamFacingDir = Vector3.zero;
    //Vector3 hardSteerDir = Vector3.zero;
    //float hardSteerAngleDiff = 0;
    //bool hardSteerOn = false;
    //bool hardSteerStarted = false;

    ////IMPULSE
    //[HideInInspector]
    //public ImpulseInfo currentImpulse;
    //bool impulseStarted = false;
    //float impulseTime = 0;
    //bool impulseDone = false;



    //SALTO
    [HideInInspector]
    public VerticalMovementState jumpSt = VerticalMovementState.None;
    //[HideInInspector]
    //public bool wallJumpAnim = false;

    ////FLAG
    //[HideInInspector]
    //public bool haveFlag = false;
    //[HideInInspector]
    //public Flag flag = null;

    //WATER
    [HideInInspector]
    public bool inWater = false;

    #endregion

    #region ----[ VARIABLES ]----  
    int frameCounter = 0;

    //Movement
    [HideInInspector]
    public float currentMaxMoveSpeed; // is the max speed from which we aply the joystick sensitivity value
    float finalMaxMoveSpeed = 10.0f; // its the final max speed, after the joyjoystick sensitivity value
    //bool hardSteer = false;


    ////WALL JUMP
    //bool firstWallJumpDone = false;
    //float wallJumpCurrentWallAngle = 0;
    //GameObject wallJumpCurrentWall = null;
    //float lastWallAngle = -500;
    //GameObject lastWall = null;
    //float wallJumpRadius;
    //float walJumpConeHeight = 1;
    //Axis wallJumpRaycastAxis = Axis.none;
    //int wallJumpCheckRaysRows = 5;
    //int wallJumpCheckRaysColumns = 5;
    //float wallJumpCheckRaysRowsSpacing;
    //float wallJumpCheckRaysColumnsSpacing;
    //LayerMask auxLM;//LM = Layer Mask

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
    float noMoveMaxTime;
    float noMoveTime;

    //HOOK
    [HideInInspector]
    public bool hooked = false;


    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region AWAKE

    private void Awake()
    {
        if (breakAcc != knockbackBreakAcc) Debug.LogError("The breakAcceleration and the KnockbackAcceleration should be the same!");
        maxMoveSpeed = 10;
        currentSpeed = 0;
        noInput = false;
        hardSteerAcc = Mathf.Clamp(hardSteerAcc, hardSteerAcc, breakAcc);
        airHardSteerAcc = Mathf.Clamp(airHardSteerAcc, airHardSteerAcc, airBreakAcc);
        jumpSt = VerticalMovementState.None;
    }

    #endregion

    #region START
    private void Start()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);
        jumpVelocity = Mathf.Abs(gravity * jumpApexTime);
        print("Gravity = " + gravity + "; Jump Velocity = " + jumpVelocity);

        finalMaxMoveSpeed = currentMaxMoveSpeed = maxMoveSpeed;
        knockbackBreakAcc = Mathf.Clamp(knockbackBreakAcc, -float.MaxValue, breakAcc);//menos de break Acc lo haría ver raro
        //EquipWeaponAtStart();

    }


    #endregion

    #region UPDATE
    private void Update()
    {
        ResetMovementVariables();

        if (!disableAllDebugs && currentSpeed != 0) Debug.LogWarning("CurrentVel 0= " + currentVel.ToString("F6") + "; currentSpeed =" + currentSpeed.ToString("F4"));

        if ((controller.collisions.above || controller.collisions.below) && !hooked)
        {
            currentVel.y = 0;
        }
        frameCounter++;

        HorizontalMovement();

        VerticalMovement();

        controller.Move(currentVel * Time.deltaTime);
        controller.collisions.ResetAround();
    }
    #endregion

    private void OnTriggerStay(Collider col)
    {
        switch (col.tag)
        {
            case "Water":
                float waterSurface = col.GetComponent<Collider>().bounds.max.y;
                if (transform.GetComponent<Collider>().bounds.min.y <= waterSurface)
                {
                    EnterWater(col);
                }
                else
                {
                    ExitWater(col);
                }
                break;
        }
    }

    #endregion

    #region ----[ CLASS FUNCTIONS ]----

    #region MOVEMENT -----------------------------------------------
    public void SetVelocity(Vector3 vel)
    {
        currentVel = vel;
        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
        currentSpeed = horVel.magnitude;
    }

    void ResetMovementVariables()
    {
        //hardSteerOn = false;

        oldCurrentVel = new Vector3(currentVel.x, 0, currentVel.z);
    }

    //public void CalculateMoveDir()
    //{
    //    if (!noInput)
    //    {
    //        float horiz = Actions.LeftJoystick.X;//Input.GetAxisRaw(contName + "H");
    //        float vert = Actions.LeftJoystick.Y;//-Input.GetAxisRaw(contName + "V");
    //                                            // Check that they're not BOTH zero - otherwise dir would reset because the joystick is neutral.
    //                                            //if (horiz != 0 || vert != 0)Debug.Log("Actions.LeftJoystick.X = "+ Actions.LeftJoystick.X+ "Actions.LeftJoystick.Y" + Actions.LeftJoystick.Y);
    //        Vector3 temp = new Vector3(horiz, 0, vert);
    //        lastJoystickSens = joystickSens;
    //        joystickSens = temp.magnitude;
    //        //print("temp.magnitude = " + temp.magnitude);
    //        if (temp.magnitude >= deadzone)
    //        {
    //            joystickSens = joystickSens >= 0.88f ? 1 : joystickSens;//Eloy: esto evita un "bug" por el que al apretar el joystick 
    //                                                                    //contra las esquinas no da un valor total de 1, sino de 0.9 o así
    //            moveSt = MoveState.Moving;
    //            currentInputDir = temp;
    //            currentInputDir.Normalize();
    //            switch (myCamera.camMode)
    //            {
    //                case cameraMode.Fixed:
    //                    currentInputDir = RotateVector(-facingAngle, temp);
    //                    break;
    //                case cameraMode.Shoulder:
    //                    currentInputDir = RotateVector(-facingAngle, temp);
    //                    break;
    //                case cameraMode.Free:
    //                    Vector3 camDir = (transform.position - myCamera.transform.GetChild(0).position).normalized;
    //                    camDir.y = 0;
    //                    // ANGLE OF JOYSTICK
    //                    joystickAngle = Mathf.Acos(((0 * currentInputDir.x) + (1 * currentInputDir.z)) / (1 * currentInputDir.magnitude)) * Mathf.Rad2Deg;
    //                    joystickAngle = (horiz > 0) ? -joystickAngle : joystickAngle;
    //                    //rotate camDir joystickAngle degrees
    //                    currentInputDir = RotateVector(joystickAngle, camDir);
    //                    //HARD STEER CHECK
    //                    //if(!disableAllDebugs)Debug.LogError(" hardSteerOn = "+ hardSteerOn + "; isRotationRestricted = " + myPlayerCombatNew.isRotationRestricted);
    //                    if (!(!hardSteerOn && myPlayerCombatNew.isRotationRestricted))
    //                    {
    //                        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
    //                        hardSteerAngleDiff = Vector3.Angle(horizontalVel, currentInputDir);//hard Steer si > 90
    //                        hardSteerOn = hardSteerAngleDiff > instantRotationMinAngle ? true : false;
    //                        if (hardSteerOn && !hardSteerStarted)
    //                        {
    //                            //if (!disableAllDebugs && hardSteerOn) Debug.LogError("HARD STEER ON: STARTED");
    //                            hardSteerDir = currentInputDir;
    //                        }
    //                    }
    //                    RotateCharacter();
    //                    break;
    //            }
    //        }
    //        else
    //        {
    //            joystickSens = 1;//no estoy seguro de que esté bien
    //            moveSt = MoveState.NotMoving;
    //        }
    //    }
    //}

    void HorizontalMovement()
    {

        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        #region//------------------------------------------------ DECIDO TIPO DE MOVIMIENTO --------------------------------------------
        #region//----------------------------------------------------- Efecto externo --------------------------------------------

        ReceiveKnockback();
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
        #endregion
        #region //----------------------------------------------------- Efecto interno --------------------------------------------
        if (!hooked && !fixedJumping && moveSt != MoveState.Knockback)
        {
            //------------------------------------------------ Direccion Joystick, aceleracion, maxima velocidad y velocidad ---------------------------------

            #region ------------------------------ Max Move Speed ------------------------------
            currentMaxMoveSpeed = maxMoveSpeed;
            ProcessWater();//only apply if the new max move speed is lower

            if (currentSpeed > (currentMaxMoveSpeed + 0.1f) && (moveSt == MoveState.Moving || moveSt == MoveState.NotMoving) && !knockbackDone)
            {
                //Debug.LogWarning("Warning: moveSt set to MovingBreaking!: currentSpeed = "+currentSpeed+ "; maxMoveSpeed2 = " + maxMoveSpeed2 + "; currentVel.magnitude = "+currentVel.magnitude);
                moveSt = MoveState.MovingBreaking;
            }

            finalMaxMoveSpeed = currentMaxMoveSpeed;
            #endregion

            #region ------------------------------- Acceleration -------------------------------
            float finalAcc = 0;
            float finalBreakAcc = controller.collisions.below ? breakAcc : airBreakAcc;
            float finalHardSteerAcc = controller.collisions.below ? hardSteerAcc : airHardSteerAcc;
            float finalInitialAcc = controller.collisions.below ? initialAcc : airInitialAcc;
            //if (!disableAllDebugs && rotationRestrictedPercentage!=1) Debug.LogWarning("finalMovingAcc = " + finalMovingAcc+ "; rotationRestrictedPercentage = " + rotationRestrictedPercentage+
            //    "; attackStg = " + myPlayerCombatNew.attackStg);
            //finalBreakAcc = currentSpeed < 0 ? -finalBreakAcc : finalBreakAcc;
            if (knockbackDone)
            {
                finalAcc = knockbackBreakAcc;
            }
            else
            {
                switch (moveSt)
                {
                    case MoveState.MovingBreaking:
                        finalAcc = hardBreakAcc;//breakAcc * 3;
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
            switch (moveSt)
            {
                case MoveState.NotMoving:
                    break;
            }
            if (!disableAllDebugs && currentSpeed != 0) Debug.Log("CurrentSpeed 1.2 = " + currentSpeed.ToString("F4") + "; finalAcc = " + finalAcc + "; moveSt = " + moveSt +
                "; currentSpeed =" + currentSpeed.ToString("F4"));
            float currentSpeedB4 = currentSpeed;
            currentSpeed = currentSpeed + finalAcc * Time.deltaTime;
            if (moveSt == MoveState.NotMoving && Mathf.Sign(currentSpeedB4) != Mathf.Sign(currentSpeed))
            {
                currentSpeed = 0;
            }

            //Debug.Log("CurrentSpeed 1.2 = " + currentSpeed);
            float maxSpeedClamp = knockbackDone ? maxKnockbackSpeed : finalMaxMoveSpeed;
            float minSpeedClamp = 0;
            currentSpeed = Mathf.Clamp(currentSpeed, minSpeedClamp, maxSpeedClamp);

            if (knockbackDone && currentSpeed <= maxMoveSpeed)
            {
                knockbackDone = false;
                startBeingHitAnimation = false;
            }
        }
        #endregion
        #endregion
        #endregion
        #region//------------------------------------------------ PROCESO EL TIPO DE MOVIMIENTO DECIDIDO ---------------------------------
        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
        if (!disableAllDebugs && currentSpeed != 0) print("CurrentVel before processing= " + currentVel.ToString("F6") + "; currentSpeed =" + currentSpeed.ToString("F4") +
            "; MoveState = " + moveSt + "; currentMaxMoveSpeed = " + finalMaxMoveSpeed + "; below = " + controller.collisions.below + "; horVel.magnitude = " + horVel.magnitude);
        //print("CurrentVel 1.3= " + currentVel.ToString("F6")+ "MoveState = " + moveSt);

        horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        switch (moveSt)
        {
            case MoveState.NotMoving: //NOT MOVING JOYSTICK
                horizontalVel = horizontalVel.normalized * currentSpeed;
                currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
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
                Vector3 newDir = horizontalVel.normalized;
                horizontalVel = newDir.normalized * currentSpeed;
                currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
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

        horVel = new Vector3(currentVel.x, 0, currentVel.z);
        //print("CurrentVel after processing= " + currentVel.ToString("F6") + "; CurrentSpeed 1.4 = " + currentSpeed + "; horVel.magnitude = " 
        //    + horVel.magnitude + "; currentInputDir = " + currentInputDir.ToString("F6"));
        #endregion
    }

    void VerticalMovement()
    {
        switch (jumpSt)
        {
            case VerticalMovementState.None:
                currentVel.y += gravity * Time.deltaTime;
                break;
        }

        if (inWater)
        {
            currentVel.y = Mathf.Clamp(currentVel.y, -maxVerticalSpeedInWater, float.MaxValue);
        }

        currentVel.y = Mathf.Clamp(currentVel.y, -maxFallSpeed, maxFallSpeed);
    }
    #endregion

    #region RECIEVE HIT AND EFFECTS ---------------------------------------------

    public bool StartRecieveHit(PlayerMovement attacker, Vector3 _knockback, EffectType efecto, float _maxTime = 0)
    {
        startBeingHitAnimation = true;

        if (!disableAllDebugs) print("Recieve hit with knockback= " + _knockback + "; effect = " + efecto + "; maxtime = " + _maxTime);

        sufferingEffect = efecto;
        switch (efecto)
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

        effectMaxTime = _maxTime;
        effectTime = 0;
        if (_knockback != Vector3.zero)
        {
            knockbackDone = false;
            _knockback = _knockback / bodyMass;
            knockback = _knockback;
        }

        if (!disableAllDebugs) print("Dummy " + name + " RECIEVED HIT");
        return true;
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
        //ReceiveKnockback();
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
    public bool StartHooked()
    {
        if (!hooked)
        {
            hooked = true;
            //Stop attacking
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

    #endregion

    #region  WATER ---------------------------------------------

    public void EnterWater(Collider waterTrigger = null)
    {
        if (!inWater)
        {
            inWater = true;

            //myPlayerVFX.ActivateEffect(PlayerVFXType.SwimmingEffect);
            if (waterTrigger != null)
            {
                //Vector3 waterSplashPos = myPlayerVFX.GetEffectGO(PlayerVFXType.WaterSplash).transform.position;//.y = waterTrigger.bounds.max.y;
                //waterSplashPos.y = waterTrigger.bounds.max.y - 0.5f;
                //myPlayerVFX.GetEffectGO(PlayerVFXType.WaterSplash).transform.position = waterSplashPos;
            }
            //myPlayerVFX.ActivateEffect(PlayerVFXType.WaterSplash);
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

    public void ExitWater(Collider waterTrigger = null)
    {
        if (inWater)
        {
            inWater = false;
            //myPlayerVFX.DeactivateEffect(PlayerVFXType.SwimmingEffect);
            if (waterTrigger != null)
            {
                //Vector3 waterSplashPos = myPlayerVFX.GetEffectGO(PlayerVFXType.WaterSplash).transform.position;//.y = waterTrigger.bounds.max.y;
                //waterSplashPos.y = waterTrigger.bounds.max.y - 0.5f;
                //myPlayerVFX.GetEffectGO(PlayerVFXType.WaterSplash).transform.position = waterSplashPos;
            }
            //myPlayerVFX.ActivateEffect(PlayerVFXType.WaterSplash);
        }
    }
    #endregion

    #region  AUXILIAR FUNCTIONS ---------------------------------------------

    public void ResetPlayer()
    {
        ExitWater();
        jumpSt = VerticalMovementState.None;
    }

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
        StopHooked(0);
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


