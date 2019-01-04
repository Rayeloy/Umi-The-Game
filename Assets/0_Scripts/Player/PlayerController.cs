using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is only for variables, Awake, Start and Update of the player.
public class PlayerController : MonoBehaviour
{
    //literalmente no consigo ver otra forma mas adecuada para usar las funciones de PlayerFunctions aqui sin convertir PlayerFunctions en un Monobehaviour,
    //o sin hacer cada clase suya estática. Pero eso a su vez crea otros problemas tediosos en PlayerFunctions
    #region Referencias
    private PlayerFunctions pF;
    [Header("Referencias")]
    public CameraController myCamera;
    public Controller3D controller;
    public PlayerHUD myPlayerHUD;
    #endregion Referencias


    #region PlayerMovement Variables
    [Header("--- MOVEMENT ---")]
    
    //Objeto padre que usamos para rotar todo dentro del jugador
    public Transform rotateObj;

    //Cuerpo del jugador que cambiamos su material para identificar su equipo (rojo o azul)
    public SkinnedMeshRenderer Body;
    public Material teamBlueMat;
    public Material teamRedMat;

    //Objeto que almacena los controles del jugador
    public PlayerActions actions { get; set; }

    #region Enums
    //enum del equipo al que pertenece el jugador

    [HideInInspector]
    public Team team = Team.blue;

    public enum MoveState
    {
        Moving,
        NotMoving,//Not stunned, breaking
        Knockback,//Stunned
        MovingBreaking,//Moving but reducing speed by breakAcc till maxMovSpeed
        Hooked,
        Boost,
        FixedJump,
        NotBreaking
    }
    [HideInInspector]
    public MoveState moveSt = MoveState.NotMoving;

    public enum JumpState
    {
        Jumping,
        Breaking,//Emergency stop
        none
    }
    [HideInInspector]
    public JumpState jumpSt = JumpState.none;
    #endregion Enums

    [HideInInspector]
    public bool noInput = false;//bool que impide recibir inputs al jugador cuando está a true.
    [HideInInspector]
    public Vector3 currentVel;

    #region Speed
    [Header("SPEED")]
    public float maxMoveSpeed = 10.0f;
    float maxMoveSpeed2; // is the max speed from which we aply the joystick sensitivity value
    float currentMaxMoveSpeed = 10.0f; // is the final max speed, after the joyjoystick sensitivity value
    [Tooltip("Maximum speed that you can travel at horizontally when hit by someone")]
    public float maxKnockbackSpeed = 300f;
    public float maxAimingSpeed = 5f;
    public float maxHookingSpeed = 2f;
    [HideInInspector]
    public float currentSpeed = 0;
    public float maxSpeedInWater = 5f;
    public float maxVerticalSpeedInWater = 3f;
    #endregion

    #region Boost
    [Header("BOOST")]
    public float boostSpeed = 20f;
    public float boostCD = 5f;
    public float boostDuration = 1f;
    float _boostTime = 0f;
    public float boostTime
    {
        get { return _boostTime; }
        set
        {
            myPlayerHUD.setBoostUI(_boostTime / boostCD);
            _boostTime = value;
        }
    }
    bool boostReady = true;
    Vector3 boostDir;
    #endregion

    #region Accelerations
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
    float gravity;
    #endregion

    #region Jump
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
    #endregion

    #region WallJump
    [Header("WALLJUMP")]
    public float wallJumpVelocity = 10f;
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
    float wallJumpRadius;
    float walJumpConeHeight = 1;
    float lastWallAngle = -1;
    GameObject lastWall;
    #endregion

    #region Direccion de movimiento e input
    [HideInInspector]
    public Vector3 currentMovDir;
    float joystickAngle;
    float deadzone = 0.2f;
    float joystickSens = 0;
    #endregion

    #endregion PlayerMovement Variables

    #region PlayerCombat Variables
    #endregion PlayerCombat Variables

    #region PlayerWeapons Variables
    #endregion PlayerWeapons Variables

    #region PlayerHook Variables
    #endregion PlayerHook Variables

    #region PlayerAnimations Variables
    #endregion PlayerAnimations Variables

    #region PlayerPickups Variables
    #endregion PlayerPickups Variables

    public void PlayerAwake()
    {
        pF = new PlayerFunctions();
        MovementAwake();
    }
    public void PlayerStart()
    {
        MovementStart();
    }
    public void PlayerUpdate()
    {
        //Pause Menu
        if (actions.Options.WasPressed) GameController.instance.PauseGame(actions);

        //Updates
        pF.MovementUpdate(controller,currentVel);

    }

    #region Awakes
    private void MovementAwake()
    {
        currentSpeed = 0;
        noInput = false;
        controller = GetComponent<Controller3D>();
        lastWallAngle = 0;
        currentMaxMoveSpeed = maxMoveSpeed2 = maxMoveSpeed;
    }
    #endregion Awakes

    #region Starts
    private void MovementStart()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);//calculo la gravedad dependiendo de la altura maxima del salto, y el tiempo en llegar a esa altura
        jumpVelocity = Mathf.Abs(gravity * jumpApexTime);//con la gravedad ya calculada, calculo la velocidad inicial del salto
        print("Gravity = " + gravity + "; Jump Velocity = " + jumpVelocity);

        //calculo la cantidad de tiempo maxima en la cual permito al jugador soltar el boton de salto para parar antes de tiempo,
        //pasado ese tiempo ya no se permite y se hace el salto completo
        maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;

        //calculo el radio de la base del cono invertido que uso para calcular la nueva direccion del salto en pared.
        wallJumpRadius = Mathf.Tan(wallJumpAngle * Mathf.Deg2Rad) * walJumpConeHeight;
        wallJumpMinHorizAngle = Mathf.Clamp(wallJumpMinHorizAngle, 0, 90);
        print("wallJumpRaduis = " + wallJumpRadius + "; tan(wallJumpAngle)= " + Mathf.Tan(wallJumpAngle * Mathf.Deg2Rad));

        //cambio el arma y el color del cuerpo al equipo que le haya sido asignado
        switch (team)
        {
            case Team.blue:
                myPlayerWeap.AttachWeapon("Churro Azul");
                Body.material = teamBlueMat;
                break;
            case Team.red:
                myPlayerWeap.AttachWeapon("Churro Rojo");
                Body.material = teamRedMat;
                break;
        }
        //aceleracion de frenado tras recibir un golpe con knockback
        knockbackBreakAcc = Mathf.Clamp(knockbackBreakAcc, -float.MaxValue, breakAcc);//menos de break Acc lo haría ver raro
    }
    #endregion Starts

}
public enum Team
{
    red,
    blue,
    none
}
