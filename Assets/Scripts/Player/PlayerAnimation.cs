using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
	[Header("Referencias")]
	public Animator animator;
	public PlayerMovement playerMovement;

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
    int basicSwingHash = Animator.StringToHash("BasicSwingHash");
    bool basicSwingValue;
    int endGameHash = Animator.StringToHash ( "EndGame" );
    [Tooltip("Distance to floor at which the landing animation will start")]
    public float maxTimeToLand = 1;
    //-------------
    bool jump;

    //Churros: Pos: -0.0086, 0.0097, 0.0068; Rot: 45.854,-163.913,-5.477; Scale: 1.599267,1.599267,1.599267

    private void Awake()
    {
        
    }

    public void KonoUpdate()
    {
        if(ScoreManager.instance.End){
            animator.SetBool(endGameHash, true);
            //ResetVariables();
            return;
        }

		animator.SetFloat("HorizontalSpeed", playerMovement.currentSpeed);//new Vector2 (playerMovement.currentVel.x, playerMovement.currentVel.z).magnitude);
        animator.SetFloat("VerticalSpeed", playerMovement.currentVel.y);
		animator.SetBool("OnGround", playerMovement.controller.collisions.below);
		if (playerMovement.wallJumpAnim){
			animator.SetTrigger("WallJump");
			playerMovement.wallJumpAnim = false;
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
        if (landing && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0))
        {
            landing = false;
            animator.SetBool(landHash, landing);
        }
        if (swimming && !playerMovement.inWater)
        {
            swimming = false;
            animator.SetBool(swimmingHash, swimming);
        }
        if(swimmingIdle && !playerMovement.inWater)
        {
            swimmingIdle = false;
            animator.SetBool(swimmingIdleHash, swimmingIdle);
        }
    }

    public void ProcessVariableValues()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (playerMovement.currentSpeed != 0 && playerMovement.controller.collisions.below)
        {
            runningValue = true;
            animator.SetBool(runningHash, runningValue);
        }

        if (jump || (!playerMovement.controller.collisions.below && playerMovement.controller.collisions.lastBelow))
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

        float timeToLand = playerMovement.controller.collisions.distanceToFloor / Mathf.Abs(playerMovement.currentVel.y);
        //Debug.LogWarning("vel.y = " + playerMovement.currentVel.y + "; below = " + playerMovement.controller.collisions.below + "; distance to floor = " + playerMovement.controller.collisions.distanceToFloor + "; timeToLand = " + timeToLand);
        if ((playerMovement.currentVel.y<0 && !playerMovement.controller.collisions.below && timeToLand <= maxTimeToLand) 
            || (jumpingValue && playerMovement.controller.collisions.below))
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

        if(playerMovement.inWater && playerMovement.currentSpeed > 0)
        {
            if (swimmingIdle)
            {
                swimmingIdle = false;
                animator.SetBool(swimmingIdleHash, swimmingIdle);
            }
            swimming = true;
            animator.SetBool(swimmingHash, swimming);
        }

        if (playerMovement.inWater && playerMovement.currentSpeed == 0)
        {
            if (swimming)
            {
                swimming = false;
                animator.SetBool(swimmingHash, swimming);
            }
            swimmingIdle = true;
            animator.SetBool(swimmingIdleHash, swimmingIdle);
        }

    }

    public void SetJump(bool _jump)
    {
        jump = _jump;
        animator.SetBool(jumpHash, jump);
    }
}