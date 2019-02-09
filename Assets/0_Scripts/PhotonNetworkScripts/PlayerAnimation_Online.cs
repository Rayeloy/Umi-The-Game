using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


///Juan:
///
/// 
///PhotonAnimatorView and Triggers
///If you use Trigger parameters in animator controllers and want to synchronize them using "PhotonAnimatorView", it's important to consider how this is handled to avoid issues.
///
///·Due to the nature of the Trigger(s), it is only enabled when the animation event starts and disabled immediately before the next Update()
///·Components on a GameObject are executed in the same order as they are declared
///·Editing the Order of Execution Settings will affect execution order on the GameObject's components
///
///It essential that the "PhotonAnimatorView" component is executed after the code that raises the Trigger(s). So it's safer to put it at the bottom of the stack, or at least
///below the component(s) that will be responsible for raising the Trigger(s) using Animator.SetTrigger(...).
///
///The "PhotonAnimatorView" Inspector shows the various paramaters current values.A good way to check even before publishing is that the Trigger is properly raised to true
///when it should.If you don't see it happening, chances are this particular Trigger won't be synchronized over the network.
///


public class PlayerAnimation_Online : MonoBehaviourPun
{
    #region Referencias
    [Header("Referencias")]
	public Animator animator;
	public PlayerMovement myPlayerMovement;
    PlayerCombat myPlayerCombat;
    #endregion

    #region Animator Variables
    [Header("Animator Variables")]
    AnimatorStateInfo stateInfo;
    int jumpHash = Animator.StringToHash("Jump");
    int jumpingHash = Animator.StringToHash("Jumping");
    bool jumpingValue;
    int landHash = Animator.StringToHash("Land");
    int jumpStateHash = Animator.StringToHash("Ascender");
    bool landing;
    int swimmingIdleHash = Animator.StringToHash("SwimmingIdle");
    bool swimmingIdle;
    int swimmingHash = Animator.StringToHash("Swimming");
    bool swimming;
    int runningHash = Animator.StringToHash("Running");
    bool runningValue;
    int basicSwingHash = Animator.StringToHash("BasicSwing");
    bool basicSwingValue;
    int endGameHash = Animator.StringToHash ( "EndGame" );
    [Tooltip("Distance to floor at which the landing animation will start")]
    public float maxTimeToLand = 1;
    //-------------
    bool jump;
    #endregion

    #region Funciones de MonoBehaviour

    //Churros: Pos: -0.0086, 0.0097, 0.0068; Rot: 45.854,-163.913,-5.477; Scale: 1.599267,1.599267,1.599267

    private void Awake()
    {
        myPlayerCombat = GetComponent<PlayerCombat>();
    }

    public void KonoUpdate()
    {
        /// Juan: añadido para hacer que cuando "no es mi jugador" y pero el jugador está conectado no mueva al personaje
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        if(myPlayerMovement.gC.gameMode==GameMode.CaptureTheFlag && (myPlayerMovement.gC as GameController_FlagMode).myScoreManager.End){
            animator.SetBool(endGameHash, true);
            //ResetVariables();
            return;
        }

		animator.SetFloat("HorizontalSpeed", myPlayerMovement.currentSpeed);//new Vector2 (playerMovement.currentVel.x, playerMovement.currentVel.z).magnitude);
        animator.SetFloat("VerticalSpeed", myPlayerMovement.currentVel.y);
		animator.SetBool("OnGround", myPlayerMovement.controller.collisions.below);
		if (myPlayerMovement.wallJumpAnim){
			animator.SetTrigger("WallJump");
            myPlayerMovement.wallJumpAnim = false;
		}
        ResetVariables();
        ProcessVariableValues();

    }

    #endregion

    #region Funciones Locales

    public void RestartAnimation()
    {
        //print("RESETING ANIMATOR");
        animator.Rebind();
    }

    public void SetJump(bool _jump)
    {
        jump = _jump;
        animator.SetBool(jumpHash, jump);
    }
    #endregion

    #region Gestión de Variables

    void ResetVariables()
    {
        if (landing && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0))
        {
            landing = false;
            animator.SetBool(landHash, landing);
        }
        if (swimming && !myPlayerMovement.inWater)
        {
            swimming = false;
            animator.SetBool(swimmingHash, swimming);
        }
        if(swimmingIdle && !myPlayerMovement.inWater)
        {
            swimmingIdle = false;
            animator.SetBool(swimmingIdleHash, swimmingIdle);
        }
        if(runningValue && !(myPlayerMovement.currentSpeed != 0 && myPlayerMovement.controller.collisions.below))
        {
            runningValue = false;
            animator.SetBool(runningHash, runningValue);
        }
        //COMBAT ANIMATIONS
        if (basicSwingValue && myPlayerCombat.attackStg == AttackStage.ready)
        {
            basicSwingValue = false;
            animator.SetBool(basicSwingHash, basicSwingValue);
        }
    }

    public void ProcessVariableValues()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);



        if (myPlayerMovement.currentSpeed != 0 && myPlayerMovement.controller.collisions.below)
        {
            runningValue = true;
            animator.SetBool(runningHash, runningValue);
        }

        if (jump || (!myPlayerMovement.controller.collisions.below && myPlayerMovement.controller.collisions.lastBelow))
        {
            if (jump)
            {
                jump = false;
                animator.SetBool(jumpHash, jump);
            }
            if (swimming)
            {
                swimming = false;
                animator.SetBool(swimmingHash, swimming);
            }
            if (landing)
            {
                landing = false;
                animator.SetBool(landHash, landing);
            }
            jumpingValue = true;
            animator.SetBool(jumpingHash, jumpingValue);
        }

        float timeToLand = myPlayerMovement.controller.collisions.distanceToFloor / Mathf.Abs(myPlayerMovement.currentVel.y);
        //Debug.LogWarning("vel.y = " + playerMovement.currentVel.y + "; below = " + playerMovement.controller.collisions.below + "; distance to floor = " + playerMovement.controller.collisions.distanceToFloor + "; timeToLand = " + timeToLand);
        if ((myPlayerMovement.currentVel.y<0 && !myPlayerMovement.controller.collisions.below && timeToLand <= maxTimeToLand) 
            || (jumpingValue && myPlayerMovement.controller.collisions.below))
        {
            if (jumpingValue)
            {
                jumpingValue = false;
                animator.SetBool(jumpingHash, jumpingValue);
            }
            //print("SET TRIGGER LAND ");
            landing = true;
            animator.SetBool(landHash,landing);
        }

        if(myPlayerMovement.inWater && myPlayerMovement.currentSpeed > 0)
        {
            if (swimmingIdle)
            {
                swimmingIdle = false;
                animator.SetBool(swimmingIdleHash, swimmingIdle);
            }
            swimming = true;
            animator.SetBool(swimmingHash, swimming);
        }

        if (myPlayerMovement.inWater && myPlayerMovement.currentSpeed == 0)
        {
            if (swimming)
            {
                swimming = false;
                animator.SetBool(swimmingHash, swimming);
            }
            swimmingIdle = true;
            animator.SetBool(swimmingIdleHash, swimmingIdle);
        }
        //COMBAT ANIMATIONS
        if (!basicSwingValue && myPlayerCombat.attackStg == AttackStage.startup)
        {
            basicSwingValue = true;
            animator.SetBool(basicSwingHash, basicSwingValue);
        }
    }

    #endregion
}