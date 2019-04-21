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


    int idleHash = Animator.StringToHash("Idle_1"); //Done
    bool idle_01;

    int pointScoreHash = Animator.StringToHash("PointScore");
    bool pointScore;

    int groundHash = Animator.StringToHash("Ground"); //Done
    bool ground;

    //int walkHash = Animator.StringToHash("Walk");
    //bool walk;

    int runHash = Animator.StringToHash("Run"); //Done
    bool run;

    int rollHash = Animator.StringToHash("Roll");
    bool roll;

    int airHash = Animator.StringToHash("Air"); //Done
    bool air;

    int fallingHash = Animator.StringToHash("Falling"); //Done
    bool falling;

    int startJumpHash = Animator.StringToHash("StartJump"); //Done
    bool startJump;

    int toGroundHash = Animator.StringToHash("ToGround"); //Done
    bool toGround;
    float maxToGroundTime=0.2f;
    float toGroundTime = 0;

    //int toWaterHash = Animator.StringToHash("ToWater");
    //bool toWater;

    int waterHash = Animator.StringToHash("Water"); //Done
    bool water = false;
    [HideInInspector]
    public bool enterWater = false;
    [HideInInspector]
    public bool exitWater = false;

    int idleWHash = Animator.StringToHash("IdleW"); //Done
    bool idleW;

    int swimmingHash = Animator.StringToHash("Swimming"); //Done
    bool swimming;

    int noControlHash = Animator.StringToHash("NoControl");
    bool noControl;

    int dashHash = Animator.StringToHash("Dash"); //Done
    bool dash;

    int hookedHash = Animator.StringToHash("Hooked");
    bool hooked;

    int frontHitHash = Animator.StringToHash("FrontHit");
    bool frontHit;

    //int backHitHash = Animator.StringToHash("BackHit");
    //bool backHit;

    int bounceHash = Animator.StringToHash("Bounce");
    bool bounce;

    int attackHash = Animator.StringToHash("Attack");
    bool attack;

    int q_TipHash = Animator.StringToHash("Q_Tip");
    bool q_Tip;

    int hammerHash = Animator.StringToHash("Hammer");
    bool hammer;

    //All the combat bools to be added



    ////int jumpHash = Animator.StringToHash("Jump");
    ////int jumpingHash = Animator.StringToHash("Jumping");
    ////bool jumpingValue;

    ////int landHash = Animator.StringToHash("Land");
    ////bool landBool;

    ////int jumpStateHash = Animator.StringToHash("Ascender");
    ////int swimmingIdleHash = Animator.StringToHash("SwimmingIdle");
    ////bool swimmingIdle;
    ////int swimmingHash = Animator.StringToHash("Swimming");
    ////bool swimming;
    ////int runningHash = Animator.StringToHash("Running");
    ////bool runningValue;
    ////int basicSwingHash = Animator.StringToHash("BasicSwing");


    ////bool basicSwingValue;
    ////int endGameHash = Animator.StringToHash("EndGame");


    [Tooltip("Distance to floor at which the landing animation will start")]
    public float maxTimeToLand = 1;

    //-------------
    ////bool jump;

    //Churros: Pos: -0.0086, 0.0097, 0.0068; Rot: 45.854,-163.913,-5.477; Scale: 1.599267,1.599267,1.599267

    public void KonoAwake()
    {
        myPlayerCombat = GetComponent<PlayerCombat>();
        myPlayerMovement = GetComponent<PlayerMovement>();
    }

    public void KonoUpdate()
    {
        if (toGround)
        {
            toGroundTime += Time.deltaTime;
        }
        if (myPlayerMovement.gC.gameMode == GameMode.CaptureTheFlag)
        {
            if ((myPlayerMovement.gC as GameController_FlagMode).myScoreManager.End)
                {
                ////animator.SetBool(endGameHash, true);
                //ResetVariables();

                // Water can´t be False

                return;
            }
        }

        //animator.SetFloat("HorizontalSpeed", myPlayerMovement.currentSpeed);//new Vector2 (playerMovement.currentVel.x, playerMovement.currentVel.z).magnitude);
        //animator.SetFloat("VerticalSpeed", myPlayerMovement.currentVel.y);              

        animator.SetBool("Air", !myPlayerMovement.controller.collisions.below);

        animator.SetBool("Water", myPlayerMovement.inWater);

        //animator.SetBool("StartJump", startJump);

        ////if (myPlayerMovement.wallJumpAnim)
        ////{
        ////    animator.SetTrigger("WallJump");
        ////    myPlayerMovement.wallJumpAnim = false;
        ////}

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
        //if (ground && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0))
        //{
        //    ground = false;
        //    animator.SetBool(groundHash, ground);
        //}
        if (toGround && startJump)
        {
            toGround = false;
            animator.SetBool(toGroundHash, toGround);
        }
        if (ground)
        {
            if (!myPlayerMovement.controller.collisions.below)
            {
                ground = false;
                animator.SetBool(groundHash, ground);
            }
            if (toGround && toGroundTime >= maxToGroundTime)
            {
                toGround = false;
                animator.SetBool(toGroundHash, toGround);
            }
            if (falling)
            {
                falling = false;
                animator.SetBool(fallingHash, falling);
            }                    
        }
        if (swimming && !myPlayerMovement.inWater)
        {
            swimming = false;
            animator.SetBool(swimmingHash, swimming);
        }
        if (myPlayerMovement.currentSpeed > 0 || startJump || !myPlayerMovement.controller.collisions.below)
        {
            idle_01 = false;
            animator.SetBool(idleHash, idle_01);
        }
        if (idleW && !myPlayerMovement.inWater)
        {
            idleW = false;
            animator.SetBool(idleWHash, idleW);
        }
        if (run && !(myPlayerMovement.currentSpeed != 0 && myPlayerMovement.controller.collisions.below))
        {
            run = false;
            animator.SetBool(runHash, run);
        }


        //////COMBAT ANIMATIONS
        ////if (basicSwingValue && myPlayerCombat.attackStg == AttackStage.ready)
        ////{
        ////    basicSwingValue = false;
        ////    animator.SetBool(basicSwingHash, basicSwingValue);
        ////}
    }

    public void ProcessVariableValues()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (myPlayerMovement.controller.collisions.below && !ground)
        {
            ground = true;
            animator.SetBool(groundHash, ground);
        }

        if (enterWater)
        {
            //TO DO:
            water = true;

            //Activate particle system

            enterWater = false;
        }

        if (exitWater)
        {
            //TO DO:

            water = false;
            
            //Activate particle system

            exitWater = false;
        }

        if (myPlayerMovement.controller.collisions.below && myPlayerMovement.currentSpeed == 0)
        {
            idle_01 = true;
            animator.SetBool(idleHash, idle_01);
        }

        if (myPlayerMovement.currentSpeed != 0 && myPlayerMovement.controller.collisions.below)
        {
            run = true;
            animator.SetBool(runHash, run);
        }

        if (/*startJump || */!myPlayerMovement.controller.collisions.below && myPlayerMovement.currentVel.y < 0) //Si ya ha empezado el salto ó No hay colisoines abajo y además en el frame anterior si que habían
        {

            if (!falling)
            {
                falling = true;
                animator.SetBool(fallingHash, falling);
            }
            if (startJump)
            {
                startJump = false;
                animator.SetBool(startJumpHash,startJump);
            }

            //if (swimming)
            //{
            //    swimming = false;
            //    animator.SetBool(swimmingHash, swimming);
            //}

        }

        float timeToLand = myPlayerMovement.controller.collisions.distanceToFloor / Mathf.Abs(myPlayerMovement.currentVel.y);

        //Debug.Log("vel.y = " + myPlayerMovement.currentVel.y + "; below = " + myPlayerMovement.controller.collisions.below + "; distance to floor = "
        //    + myPlayerMovement.controller.collisions.distanceToFloor + "; timeToLand = " + timeToLand + "; falling = " + falling + "; below = "
        //    + myPlayerMovement.controller.collisions.below);


        if (myPlayerMovement.currentVel.y < 0 && !myPlayerMovement.controller.collisions.below && timeToLand <= maxTimeToLand)
        {
            //Debug.Log("vel.y = " + myPlayerMovement.currentVel.y + "; below = " + myPlayerMovement.controller.collisions.below 
            //+ "; timeToLand = " + timeToLand + "; falling = " + falling + "; below = " + myPlayerMovement.controller.collisions.below);

            startJump = false;
            animator.SetBool(startJumpHash, startJump);
            toGroundTime = 0;
            toGround = true; 
            animator.SetBool(toGroundHash, toGround);
            

        }

        if ((myPlayerMovement.currentVel.y < 0 && !myPlayerMovement.controller.collisions.below && timeToLand <= maxTimeToLand)
    || (falling && myPlayerMovement.controller.collisions.below))

            if (myPlayerMovement.inWater && myPlayerMovement.currentSpeed > 0)
        {
            if (swimming)
            {
                swimming = false;
                animator.SetBool(swimmingHash, swimming);
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
            idleW = true;
            animator.SetBool(idleWHash, idleW);
        }
        //////COMBAT ANIMATIONS
        ////if (!basicSwingValue && myPlayerCombat.attackStg == AttackStage.startup)
        ////{
        ////    basicSwingValue = true;
        ////    animator.SetBool(basicSwingHash, basicSwingValue);
        ////}
    }

    public void SetJump(bool _jump)
    {

        air = _jump;
        animator.SetBool(airHash, air);
    
        startJump = _jump;
        animator.SetBool(startJumpHash, startJump);

    }
    //int startJumpHash = Animator.StringToHash("StartJump"); //Done
    //bool startJump;

}