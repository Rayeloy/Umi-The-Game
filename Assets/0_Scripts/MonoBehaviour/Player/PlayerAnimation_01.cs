﻿using System.Collections;
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



    int idleHash = Animator.StringToHash("Idle_01");
    bool idle_01;

    int pointScoreHash = Animator.StringToHash("PointScore");
    bool pointScore;

    int groundHash = Animator.StringToHash("Ground"); //Done
    bool ground;

    int walkHash = Animator.StringToHash("Walk");
    bool walk;

    int runHash = Animator.StringToHash("Run"); //Done
    bool run;

    int rollHash = Animator.StringToHash("Roll");
    bool roll;

    int airHash = Animator.StringToHash("Air");
    bool air;

    int fallingHash = Animator.StringToHash("Falling"); //Done
    bool falling;

    int startJumpHash = Animator.StringToHash("StartJump"); //Done
    bool startJump;

    int toGroundHash = Animator.StringToHash("ToGround"); //Done
    bool toGround;

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

    int backHitHash = Animator.StringToHash("BackHit");
    bool backHit;

    int bounceHash = Animator.StringToHash("Bounce");
    bool bounce;

    int attackHash = Animator.StringToHash("Attack");
    bool attack;

    int q_TipHash = Animator.StringToHash("Q_Tip");
    bool q_Tip;

    int hammerHash = Animator.StringToHash("Hammer");
    bool hammer;


    private void Update()
    {
        if (myPlayerMovement.inWater && !water)
        {
            water = true;
        }
    }
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

        animator.SetFloat("HorizontalSpeed", myPlayerMovement.currentSpeed);//new Vector2 (playerMovement.currentVel.x, playerMovement.currentVel.z).magnitude);
        animator.SetFloat("VerticalSpeed", myPlayerMovement.currentVel.y);



        animator.SetBool("Ground", myPlayerMovement.controller.collisions.below);



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
        if (ground && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0))
        {
            ground = false;
            animator.SetBool(groundHash, ground);
        }
        if (swimming && !myPlayerMovement.inWater)
        {
            swimming = false;
            animator.SetBool(swimmingHash, swimming);
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

        if (enterWater)
        {
            //TO DO:
            water = true;

            enterWater = false;
        }

        if (exitWater)
        {
            //TO DO:

            water = false;

            exitWater = false;
        }


        if (myPlayerMovement.currentSpeed != 0 && myPlayerMovement.controller.collisions.below)
        {
            run = true;
            animator.SetBool(runHash, run);
        }

        if (startJump || (!myPlayerMovement.controller.collisions.below && myPlayerMovement.controller.collisions.lastBelow)) //Si ya ha empezado el salto ó No hay colisoines abajo y además en el frame anterior si que habían
        {
            //All variables to false in order to secure the state

            if (startJump)
            {
                startJump = false;
                animator.SetBool(startJumpHash, startJump);
            }
            if (swimming)
            {
                swimming = false;
                animator.SetBool(swimmingHash, swimming);
            }
            if (ground)
            {
                ground = false;
                animator.SetBool(groundHash, ground);
            }
            if (noControl)
            {
                noControl = false;
                animator.SetBool(noControlHash, noControl);
            }

            falling = true; //Desde este estado se puede pasar a Ground, Water o No control
            animator.SetBool(fallingHash, falling);
        }

        float timeToLand = myPlayerMovement.controller.collisions.distanceToFloor / Mathf.Abs(myPlayerMovement.currentVel.y);

        //Debug.LogWarning("vel.y = " + playerMovement.currentVel.y + "; below = " + playerMovement.controller.collisions.below + "; distance to floor = " + playerMovement.controller.collisions.distanceToFloor + "; timeToLand = " + timeToLand);

        if ((myPlayerMovement.currentVel.y < 0 && !myPlayerMovement.controller.collisions.below && timeToLand <= maxTimeToLand)
            || (falling && myPlayerMovement.controller.collisions.below))
        {
            if (falling)
            {
                falling = false; //Because it is about to touch the ground
                animator.SetBool(fallingHash, falling);
            }
            //print("SET TRIGGER LAND ");
            //Se activa la animación de empezar a tocar el suelo, desde ésta se puede ir a cualquier otro estado.
            toGround = true;
            animator.SetBool(toGroundHash, toGround);
        }

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
        startJump = _jump;
        animator.SetBool(startJumpHash, startJump);
    }
}