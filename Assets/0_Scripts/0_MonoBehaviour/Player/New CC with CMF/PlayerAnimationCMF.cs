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

    int dashHash = Animator.StringToHash("Dash"); //Done
    [HideInInspector]
    public bool dash;

    int hookedHash = Animator.StringToHash("Hooked");
    bool hooked;

    int frontHitHash = Animator.StringToHash("FrontHit");
    bool frontHit;


    //ALL REFERENCES TO BE ADDED

    //El bounce no tengo claro si necesitamos un estado especial para que haga una animación diferente o lo dejamos con falling, supongo que lo podremos determinar una vez tengamos todo montado y veamos cómo queda.
    int bounceHash = Animator.StringToHash("Bounce");
    public bool bounce;//interruptor

    // Arma equipada, esto va a determinar prácticamente todas las animaciones, asi que será un valor que se determinará al principio del árbol y se quedará ahí, es posible que necesite hacer un animator controller override para cada arma.
    // Si consideras que es una locura el tenerlo todo en el mismo ábrol, dímelo y preparo 3 overrides x 4 personajes. Esta opción creo que es la óptima.


    int weaponTypeHash = Animator.StringToHash("WeaponType");
    public WeaponType weaponType = WeaponType.None;


    // Basic Attack
    // Ahora depende de si queremos separar el golpe final del combo del resto de golpes básicos para que se diferencie, por ejemplo que se quede más tiempo en la pose o que haga una naimación especial.
    // No es necesario diferenciar si es el 1, 2 o 3 golpe del autocombo, pero es una cosa que tendremos que hablar de cara al producto final, dado que es importante que se diferencie el último golpe del combo, pero esto lo podemos solucionar por otros medios que no sean animaciones.

    int basicAttackHash = Animator.StringToHash("BasicAttack");//
    public byte currentBasicAttack =255;//255=none, 0= first hit, 1 = second hit, 2 = third hit .... etc

    // Ahora depende de si queremos separar el golpe final del combo del resto de golpes básicos para que se diferencie, por ejemplo que se quede más tiempo en la pose o que haga una naimación especial.
    // No es necesario diferenciar si es el 1, 2 o 3 golpe del autocombo, pero es una cosa que tendremos que hablar de cara al producto final, dado que es importante que se diferencie el último golpe del combo, pero esto lo podemos solucionar por otros medios que no sean animaciones.


    // ABILITIES
    // Necesito saber si está ejecutando la animación para decirle al animator que empiece a hacer la animación, también necesito una variable que me avise cuándo se acaba la animación para pasar al siguiente estado en el árbol.
    // Me sirve una variable que sea algo como "Habilidad Activada" y la ponemos a false cuando se termine o se corte y así entramso y pasamos de estado con una sola variable.

    int skill1Hash = Animator.StringToHash("Skill1");//Boolean State
    public byte currentSkill=255;//255=none, 0= first skill, 1 = second skill




    // Salto en Pared
    // La animación del salto en pared va a estar dividida en 3 estados:
    // 1er estado = Estamos acercándonos a la pared para hacer un salto en pared.
    // 2do estado = Estamos pegados a la pared y podemos cambiar la dirección en la que vamos a saltar mientras nos escurrimos.
    // 3er estado = Saltamos de la pared propulsados en la dirección elegida.

    int stickWJHash = Animator.StringToHash("StickWJ");
    public bool stickWJ;//interruptor, Stick to wall, 

    int doWJHash = Animator.StringToHash("DoWJ");
    public bool doWJ;//interruptor

    int dropWJHash = Animator.StringToHash("DropWJ");
    public bool dropWJ;//interruptor



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
            //Debug.LogWarning("START JUMP FALSE");
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

        if (stickWJ || !stickWJ)
        {
            animator.SetBool(stickWJHash, stickWJ);
        }
        if (doWJ || !doWJ)
        {
            animator.SetBool(doWJHash, doWJ);
        }
        if (dropWJ || !dropWJ)
        {
            animator.SetBool(dropWJHash, dropWJ);
        }

        if (weaponType >= 0)
        {
            switch((int)weaponType)
            {
                case 1:
                    //animator.SetBool(weaponTypeHash, (int)weaponType);
                    break;

                case 0:

                    break;
            }
        }

        if (currentBasicAttack >= 0)
        {
            switch((int)currentBasicAttack)
            {
                case 1:
                    //animator.SetBool(weaponTypeHash, (int)weaponType);
                    break;

                case 0:

                    animator.SetFloat("BasicAttack", currentBasicAttack);
                    break;

            }

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