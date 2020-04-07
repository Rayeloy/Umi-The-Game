using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationCMF : MonoBehaviour
{
    [Header("Referencias")]
    public Animator animator;
    PlayerMovementCMF myPlayerMovement;
    //PlayerCombat myPlayerCombat;
    PlayerCombatCMF myPlayerCombatNew;

    [Header("Animator Variables")]
    AnimatorStateInfo stateInfo;


    public bool animHit = false;


    int idleHash = Animator.StringToHash("Idle_1"); //Done
    bool idle_01;

    int pointScoreHash = Animator.StringToHash("PointScore");
    bool pointScore;

    int groundHash = Animator.StringToHash("Ground"); //Done
    bool ground;

    int safeBelowHash = Animator.StringToHash("SafeBelow"); //Done
    bool safeBelow;

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
    float maxToGroundTime = 0.2f;
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

    int jumpOutHash = Animator.StringToHash("JumpOut");
    bool jumpout;

    int noControlHash = Animator.StringToHash("NoControl");
    bool noControl;
    //{
    //    get
    //    {
    //        return myPlayerMovement.moveSt == MoveState.Hooked || myPlayerMovement.moveSt == MoveState.Boost || myPlayerMovement.sufferingEffect != EffectType.none;
    //    }
    //}

    int dashHash = Animator.StringToHash("Dash"); //Done
    [HideInInspector]
    public bool dash;

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

    int softHitHash = Animator.StringToHash("SoftHit");
    bool softhit;

    int hardHitHash = Animator.StringToHash("HardHit");
    bool hardhit;

    int spAtHash = Animator.StringToHash("SpAt");
    bool spAt;

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
        myPlayerCombatNew = GetComponent<PlayerCombatCMF>();
        myPlayerMovement = GetComponent<PlayerMovementCMF>();
    }

    public void KonoUpdate()
    {
        if (toGround)
        {
            toGroundTime += Time.deltaTime;
        }
        if (myPlayerMovement.gC.gameMode == GameMode.CaptureTheFlag)
        {
            if ((myPlayerMovement.gC as GameControllerCMF_FlagMode).myScoreManager.End)
            {
                ////animator.SetBool(endGameHash, true);
                //ResetVariables();

                // Water can´t be False

                return;
            }
        }

        //animator.SetFloat("HorizontalSpeed", myPlayerMovement.currentSpeed);//new Vector2 (playerMovement.currentVel.x, playerMovement.currentVel.z).magnitude);
        //animator.SetFloat("VerticalSpeed", myPlayerMovement.currentVel.y);              

        animator.SetBool("Air", !myPlayerMovement.collCheck.below);

        animator.SetBool("Water", myPlayerMovement.vertMovSt == VerticalMovementState.FloatingInWater);

        if (noControl)
        {
            air = false;
            animator.SetBool(airHash, air);
            ground = false;
            animator.SetBool(groundHash, ground);
            run = false;
            animator.SetBool(runHash, run);
            water = false;
            animator.SetBool(waterHash, water);
            safeBelow = false;
            animator.SetBool(safeBelowHash, safeBelow);
            falling = false;
            animator.SetBool(fallingHash, falling);
            Debug.LogWarning("START JUMP FALSE");
            startJump = false;
            animator.SetBool(startJumpHash, startJump);
        }
             
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

        if (toGround)
        {
            if (startJump)
            {
                toGround = false;
            }
            if (falling)
            {
                toGround = false;
            }

            animator.SetBool(toGroundHash, toGround);
        }
        if (ground)
        {
            jumpout = false;
            animator.SetBool(jumpOutHash, jumpout);

            if (!myPlayerMovement.collCheck.below)
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
        if (water)
        {
            if (toGround) { toGround = false; animator.SetBool(toGroundHash, toGround); }
            if (falling) { falling = false; animator.SetBool(fallingHash, falling); }
        }
        if (swimming && myPlayerMovement.vertMovSt != VerticalMovementState.FloatingInWater)
        {
            swimming = false;
            animator.SetBool(swimmingHash, swimming);
        }
        if (myPlayerMovement.currentSpeed > 0 || startJump || !myPlayerMovement.collCheck.below)
        {
            idle_01 = false;
            animator.SetBool(idleHash, idle_01);
        }
        if (idleW && myPlayerMovement.vertMovSt != VerticalMovementState.FloatingInWater)
        {
            idleW = false;
            animator.SetBool(idleWHash, idleW);
        }
        if (run && !(myPlayerMovement.currentSpeed != 0 && myPlayerMovement.collCheck.below))
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
        ///


    }

    public void ProcessVariableValues()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!ground && myPlayerMovement.collCheck.below && myPlayerMovement.vertMovSt != VerticalMovementState.FloatingInWater && !noControl)
        {
            ground = true;
            animator.SetBool(groundHash, ground);
        }

        if (ground && myPlayerMovement.collCheck.below && myPlayerMovement.vertMovSt == VerticalMovementState.FloatingInWater && !noControl)
        {

            ground = false;
            animator.SetBool(groundHash, ground);

            water = true;
            animator.SetBool(waterHash, water);
        }

        if (ground && startJump)
        {
            Debug.LogWarning("START JUMP FALSE");
            startJump = false;
            animator.SetBool(startJumpHash, startJump);
        }

        if (!ground && !air && myPlayerMovement.vertMovSt != VerticalMovementState.FloatingInWater && !noControl)
        {
            air = true;
            animator.SetBool(airHash, air);
        }

        if (!safeBelow && myPlayerMovement.collCheck.safeBelow && !noControl)
        {
            safeBelow = true;
            animator.SetBool(safeBelowHash, safeBelow);
        }
        else if (safeBelow && !myPlayerMovement.collCheck.safeBelow)
        {
            safeBelow = false;
            animator.SetBool(safeBelowHash, safeBelow);
        }

        if (!dash && myPlayerMovement.moveSt == MoveState.Boost)
        {
            noControl = true;
            animator.SetBool(noControlHash, noControl);

            dash = true;
            animator.SetBool(dashHash, dash);
        }
        else if (dash && myPlayerMovement.moveSt != MoveState.Boost)
        {
            noControl = false;
            animator.SetBool(noControlHash, noControl);

            dash = false;
            animator.SetBool(dashHash, dash);
        }


        if (!frontHit && myPlayerMovement.startBeingHitAnimation) /*(moveSt == MoveState.Knockback)*/
        {
            noControl = true;
            animator.SetBool(noControlHash, noControl);

            frontHit = true;
            animator.SetBool(frontHitHash, frontHit);
        }
        else if (frontHit && !myPlayerMovement.startBeingHitAnimation)
        {
            noControl = false;
            animator.SetBool(noControlHash, noControl);

            frontHit = false;
            animator.SetBool(frontHitHash, frontHit);
        }

        if (!hooked && myPlayerMovement.hooked)
        {
            noControl = true;
            animator.SetBool(noControlHash, noControl);

            hooked = true;
            animator.SetBool(hookedHash, hooked);
        }
        else if (hooked && !myPlayerMovement.hooked)
        {
            noControl = false;
            animator.SetBool(noControlHash, noControl);

            hooked = false;
            animator.SetBool(hookedHash, hooked);
        }

        //if (myPlayerCombat.attackStg == AttackStage.ready)
        //{
        //    softhit = false;
        //    animator.SetBool(softHitHash, softhit);
        //    hardhit = false;
        //    animator.SetBool(hardHitHash, hardhit);
        //    spAt = false;
        //    animator.SetBool(spAtHash, spAt);
        //}

        if (myPlayerCombatNew.attackStg != AttackPhaseType.ready && !noControl)//ESTAMOS ATACANDO
        {

            //switch (myPlayerCombat.attackIndex)
            //{
            //    case 0://ataque debil
            //        if (!softhit)
            //        {
            //            softhit = true;
            //            animator.SetBool(softHitHash, softhit);
            //        }
            //        hardhit = false;
            //        animator.SetBool(hardHitHash, hardhit);
            //        spAt = false;
            //        animator.SetBool(spAtHash, spAt);
            //        break;
            //    case 1://ataque fuerte
            //        if (!hardhit)
            //        {
            //            hardhit = true;
            //            animator.SetBool(hardHitHash, hardhit);
            //        }
            //        softhit = false;
            //        animator.SetBool(softHitHash, softhit);
            //        spAt = false;
            //        animator.SetBool(spAtHash, spAt);
            //        break;
            //    case 2://ataque especial
            //        if (!spAt)
            //        {
            //            spAt = true;
            //            animator.SetBool(spAtHash, spAt);
            //        }
            //        hardhit = false;
            //        animator.SetBool(hardHitHash, hardhit);
            //        softhit = false;
            //        animator.SetBool(softHitHash, softhit);
            //        break;
            //}
        }
        else
        {
            softhit = false;
            animator.SetBool(softHitHash, softhit);
            hardhit = false;
            animator.SetBool(hardHitHash, hardhit);
            spAt = false;
            animator.SetBool(spAtHash, spAt);
        }


        if (attack && myPlayerCombatNew.attackStg == AttackPhaseType.ready)
        {
            attack = false;
            animator.SetBool(attackHash, attack);
        }
        else if (!attack && myPlayerCombatNew.attackStg == AttackPhaseType.startup && !noControl)
        {
            attack = true;
            animator.SetBool(attackHash, attack);
        }
        //if (!jumpout && myPlayerMovement.jumpedOutOfWater)
        //{
        //    jumpout = true;
        //    animator.SetBool(jumpOutHash, jumpout);
        //}
        //else if (jumpout && !myPlayerMovement.jumpedOutOfWater)
        //{
        //    jumpout = false;
        //    animator.SetBool(jumpOutHash, jumpout);
        //}

        if (enterWater)
        {
            //TO DO:
            water = true;

            air = false;
            animator.SetBool(airHash, air);

            jumpout = false;
            animator.SetBool(jumpOutHash, jumpout);
            //Activate particle system

            enterWater = false;
        }

        if (exitWater)
        {
            //TO DO:

            water = false;
            animator.SetBool(waterHash, water);
            jumpout = true;
            animator.SetBool(jumpOutHash, jumpout);

            //Activate particle system

            exitWater = false;
        }

        if (myPlayerMovement.collCheck.below && myPlayerMovement.currentSpeed == 0 && myPlayerMovement.vertMovSt != VerticalMovementState.FloatingInWater)
        {
            idle_01 = true;
            animator.SetBool(idleHash, idle_01);
        }

        if (myPlayerMovement.currentSpeed != 0 && myPlayerMovement.collCheck.below && !noControl)
        {
            run = true;
            animator.SetBool(runHash, run);
        }

        if (/*startJump || */!myPlayerMovement.collCheck.below && myPlayerMovement.currentVel.y < 0 && !noControl) //Si ya ha empezado el salto ó No hay colisoines abajo y además en el frame anterior si que habían
        {

            if (!falling)
            {
                falling = true;
                animator.SetBool(fallingHash, falling);
            }
            if (startJump)
            {
                //Debug.LogWarning("START JUMP FALSE");
                startJump = false;
                animator.SetBool(startJumpHash, startJump);
            }
            if (water)
            {


                falling = false;
                animator.SetBool(fallingHash, falling);
            }

            //if (swimming)
            //{
            //    swimming = false;
            //    animator.SetBool(swimmingHash, swimming);
            //}

        }

        float timeToLand = myPlayerMovement.collCheck.distanceToFloor / Mathf.Abs(myPlayerMovement.currentVel.y);

        //Debug.Log("vel.y = " + myPlayerMovement.currentVel.y + "; below = " + myPlayerMovement.collCheck.below + "; distance to floor = "
        //    + myPlayerMovement.controller.collisions.distanceToFloor + "; timeToLand = " + timeToLand + "; falling = " + falling + "; below = "
        //    + myPlayerMovement.collCheck.below);


        if (myPlayerMovement.currentVel.y < 0 && !myPlayerMovement.collCheck.below && timeToLand <= maxTimeToLand && !toGround)
        {
            //Debug.Log("vel.y = " + myPlayerMovement.currentVel.y + "; below = " + myPlayerMovement.collCheck.below
            //+ "; timeToLand = " + timeToLand + "; falling = " + falling + "; below = " + myPlayerMovement.collCheck.below);

            if (startJump)
            {
                Debug.LogWarning("START JUMP FALSE");
                startJump = false;
                animator.SetBool(startJumpHash, startJump);
            }

            if (!toGround)
            {
                toGroundTime = 0;
                toGround = true;
                animator.SetBool(toGroundHash, toGround);
            }
        }

    //    if ((myPlayerMovement.currentVel.y < 0 && !myPlayerMovement.collCheck.below && timeToLand <= maxTimeToLand)
    //|| (falling && myPlayerMovement.collCheck.below))
      if (water)
            if (myPlayerMovement.vertMovSt == VerticalMovementState.FloatingInWater && myPlayerMovement.currentSpeed > 0)
            {

                swimming = true;
                animator.SetBool(swimmingHash, swimming);

                idleW = false;
                animator.SetBool(idleWHash, idleW);
            }
            else if (myPlayerMovement.vertMovSt == VerticalMovementState.FloatingInWater && myPlayerMovement.currentSpeed == 0)
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

        if(!_jump) Debug.LogWarning("START JUMP FALSE");
        startJump = _jump;
        animator.SetBool(startJumpHash, startJump);

    }


    //int startJumpHash = Animator.StringToHash("StartJump"); //Done
    //bool startJump;

}