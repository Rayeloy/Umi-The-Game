using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation_01 : MonoBehaviour
{
    [Header("Referencias")]
    public Animator animator;
    PlayerMovement myPlayerMovement;
    PlayerCombat myPlayerCombat;

    [Header("Animator Variables")]
    AnimatorStateInfo stateInfo;
    int jumpHash = Animator.StringToHash("Jump");
    int jumpingHash = Animator.StringToHash("Jumping");
    bool jumpingValue;

    int landHash = Animator.StringToHash("Land");
    bool landBool;

    int jumpStateHash = Animator.StringToHash("Ascender");
    int swimmingIdleHash = Animator.StringToHash("SwimmingIdle");
    bool swimmingIdle;
    int swimmingHash = Animator.StringToHash("Swimming");
    bool swimming;
    int runningHash = Animator.StringToHash("Running");
    bool runningValue;
    int basicSwingHash = Animator.StringToHash("BasicSwing");
    bool basicSwingValue;
    int endGameHash = Animator.StringToHash("EndGame");
    [Tooltip("Distance to floor at which the landing animation will start")]
    public float maxTimeToLand = 1;
    //-------------
    bool jump;

    //Churros: Pos: -0.0086, 0.0097, 0.0068; Rot: 45.854,-163.913,-5.477; Scale: 1.599267,1.599267,1.599267

    public void KonoAwake()
    {
        myPlayerCombat = GetComponent<PlayerCombat>();
        myPlayerMovement = GetComponent<PlayerMovement>();
    }

    public void KonoUpdate()
    {
        if (myPlayerMovement.gC.gameMode == GameMode.CaptureTheFlag)
        {
            if ((myPlayerMovement.gC as GameController_FlagMode).myScoreManager.End)
                {
                animator.SetBool(endGameHash, true);
                //ResetVariables();
                return;
            }
        }

        animator.SetFloat("HorizontalSpeed", myPlayerMovement.currentSpeed);//new Vector2 (playerMovement.currentVel.x, playerMovement.currentVel.z).magnitude);
        animator.SetFloat("VerticalSpeed", myPlayerMovement.currentVel.y);
        animator.SetBool("OnGround", myPlayerMovement.controller.collisions.below);
        if (myPlayerMovement.wallJumpAnim)
        {
            animator.SetTrigger("WallJump");
            myPlayerMovement.wallJumpAnim = false;
        }
        ResetVariables();
        ProcessVariableValues();

    }

    public void RestartAnimation()
    {
        //print("RESETING ANIMATOR");
        animator.Rebind();
    }

    void ResetVariables()
    {
        if (landBool && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0))
        {
            landBool = false;
            animator.SetBool(landHash, landBool);
        }
        if (swimming && !myPlayerMovement.inWater)
        {
            swimming = false;
            animator.SetBool(swimmingHash, swimming);
        }
        if (swimmingIdle && !myPlayerMovement.inWater)
        {
            swimmingIdle = false;
            animator.SetBool(swimmingIdleHash, swimmingIdle);
        }
        if (runningValue && !(myPlayerMovement.currentSpeed != 0 && myPlayerMovement.controller.collisions.below))
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
            if (landBool)
            {
                landBool = false;
                animator.SetBool(landHash, landBool);
            }
            jumpingValue = true;
            animator.SetBool(jumpingHash, jumpingValue);
        }

        float timeToLand = myPlayerMovement.controller.collisions.distanceToFloor / Mathf.Abs(myPlayerMovement.currentVel.y);
        //Debug.LogWarning("vel.y = " + playerMovement.currentVel.y + "; below = " + playerMovement.controller.collisions.below + "; distance to floor = " + playerMovement.controller.collisions.distanceToFloor + "; timeToLand = " + timeToLand);
        if ((myPlayerMovement.currentVel.y < 0 && !myPlayerMovement.controller.collisions.below && timeToLand <= maxTimeToLand)
            || (jumpingValue && myPlayerMovement.controller.collisions.below))
        {
            if (jumpingValue)
            {
                jumpingValue = false;
                animator.SetBool(jumpingHash, jumpingValue);
            }
            //print("SET TRIGGER LAND ");
            landBool = true;
            animator.SetBool(landHash, landBool);
        }

        if (myPlayerMovement.inWater && myPlayerMovement.currentSpeed > 0)
        {
            if (swimmingIdle)
            {
                swimmingIdle = false;
                animator.SetBool(swimmingIdleHash, swimmingIdle);
            }
            swimming = true;
            animator.SetBool(swimmingHash, swimming);
        }
        else if (myPlayerMovement.inWater && myPlayerMovement.currentSpeed == 0)
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

    public void SetJump(bool _jump)
    {
        jump = _jump;
        animator.SetBool(jumpHash, jump);
    }
}