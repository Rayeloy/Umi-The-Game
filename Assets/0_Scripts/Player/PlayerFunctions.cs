using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is only for functions of the player.
public class PlayerFunctions
{
    #region PlayerMovement

    public static void MovementUpdate(Controller3D controller, Vector3 currentVel, bool noInput, bool hooked)
    {
        if ((controller.collisions.above || controller.collisions.below) && !hooked)
        {
            currentVel.y = 0;
        }

        HorizontalMovement();

        UpdateFacingDir();
        VerticalMovement();

        ProcessWallJump();//IMPORTANTE QUE VAYA ANTES DE LLAMAR A "MOVE"
        controller.Move(currentVel * Time.deltaTime);
        //myPlayerCombat.KonoUpdate();
        //myPlayerAnimation.KonoUpdate();
    }

    #region MOVEMENT -----------------------------------------------
    public void CalculateMoveDir(PlayerActions actions, CameraController myCamera, Transform transform, ref PlayerController.MoveState moveSt, ref Vector3 currentMovDir,
        float deadzone, bool noInput, ref float joystickSens, ref float joystickAngle)
    {
        if (!noInput)
        {
            float horiz = actions.Movement.X;//Input.GetAxisRaw(contName + "H");
            float vert = actions.Movement.Y;//-Input.GetAxisRaw(contName + "V");
                                            //print("H = " + horiz + "; V = " + vert);
                                            // Check that they're not BOTH zero - otherwise
                                            // dir would reset because the joystick is neutral.
            Vector3 temp = new Vector3(horiz, 0, vert);
            joystickSens = temp.magnitude;
            //print("temp.magnitude = " + temp.magnitude);
            if (temp.magnitude >= deadzone)
            {
                if (joystickSens >= 0.88 || joystickSens > 1) joystickSens = 1;
                moveSt = PlayerController.MoveState.Moving;
                currentMovDir = temp;
                currentMovDir.Normalize();
                switch (myCamera.camMode)
                {
                    case CameraController.cameraMode.Fixed:
                        currentMovDir = RotateVector(-facingAngle, temp);
                        break;
                    case CameraController.cameraMode.Shoulder:
                        currentMovDir = RotateVector(-facingAngle, temp);
                        break;
                    case CameraController.cameraMode.Free:
                        Vector3 camDir = (transform.position - myCamera.transform.GetChild(0).position).normalized;
                        camDir.y = 0;
                        // ANGLE OF JOYSTICK
                        joystickAngle = Mathf.Acos(((0 * currentMovDir.x) + (1 * currentMovDir.z)) / (1 * currentMovDir.magnitude)) * Mathf.Rad2Deg;
                        joystickAngle = (horiz > 0) ? -joystickAngle : joystickAngle;
                        //rotate camDir joystickAngle degrees
                        currentMovDir = RotateVector(joystickAngle, camDir);
                        //print("joystickAngle= " + joystickAngle + "; camDir= " + camDir.ToString("F4") + "; currentMovDir = " + currentMovDir.ToString("F4"));
                        RotateCharacter();
                        break;
                }
            }
            else
            {
                joystickSens = 1;
                moveSt = PlayerController.MoveState.NotMoving;
                currentMovDir = Vector3.zero;
            }
        }
    }

    void HorizontalMovement()
    {
        float finalMovingAcc = 0;
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        #region//------------------------------------------------ DECIDO TIPO DE MOVIMIENTO --------------------------------------------
        #region//----------------------------------------------------- Efecto externo --------------------------------------------
        if (stunned)
        {
            ProcessStun();
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
        if (!hooked && !fixedJumping)
        {
            //------------------------------------------------ Direccion Joystick, aceleracion, maxima velocidad y velocidad ---------------------------------
            //------------------------------- Joystick Direction -------------------------------
            CalculateMoveDir();//Movement direction
            if (!myPlayerCombat.aiming && Actions.Boost.WasPressed)//Input.GetButtonDown(contName + "RB"))
            {
                StartBoost();
            }
            ProcessBoost();
            //------------------------------- Max Move Speed -------------------------------
            maxMoveSpeed2 = maxMoveSpeed;
            ProcessWater();
            ProcessAiming();
            ProcessHooking();
            currentMaxMoveSpeed = (joystickSens / 1) * maxMoveSpeed2;
            if (currentSpeed > currentMaxMoveSpeed && moveSt == MoveState.Moving)
            {
                moveSt = MoveState.MovingBreaking;
            }
            //------------------------------- Acceleration -------------------------------
            float actAccel;
            switch (moveSt)
            {
                case MoveState.Moving:
                    actAccel = initialAcc;
                    break;
                case MoveState.MovingBreaking:
                    actAccel = hardBreakAcc;//breakAcc * 3;
                    break;
                case MoveState.Knockback:
                    actAccel = knockbackBreakAcc;
                    break;
                default:
                    actAccel = breakAcc;
                    break;
            }
            finalMovingAcc = controller.collisions.below ? movingAcc : airMovingAcc; //Turning accleration
                                                                                     //------------------------------- Speed ------------------------------ -
            currentSpeed = currentSpeed + actAccel * Time.deltaTime;
            float maxSpeedClamp = moveSt == MoveState.Moving ? currentMaxMoveSpeed : maxKnockbackSpeed;
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeedClamp);
        }
        #endregion
        #endregion
        #region//------------------------------------------------ PROCESO EL TIPO DE MOVIMIENTO DECIDIDO ---------------------------------
        //print("MoveState = " + moveSt+"; speed = "+currentSpeed);
        switch (moveSt)
        {
            case MoveState.Moving:
                currentVel = currentVel + currentMovDir * finalMovingAcc;
                horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                if (horizontalVel.magnitude > currentMaxMoveSpeed)
                {
                    horizontalVel = horizontalVel.normalized * currentMaxMoveSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                    currentSpeed = currentVel.magnitude;
                }
                //print("Speed = " + currentSpeed+"; currentMaxMoveSpeed = "+currentMaxMoveSpeed);
                break;
            case MoveState.NotMoving:
                Vector3 aux = currentVel.normalized * currentSpeed;
                currentVel = new Vector3(aux.x, currentVel.y, aux.z);
                break;
            case MoveState.Boost:
                if (controller.collisions.collisionHorizontal)//BOOST CONTRA PARED
                {
                    WallBoost();
                }
                else//BOOST NORMAL
                {
                    currentVel = boostDir * finalMovingAcc;
                    horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                    horizontalVel = horizontalVel.normalized * boostSpeed;
                    currentVel = new Vector3(horizontalVel.x, 0, horizontalVel.z);
                    currentSpeed = boostSpeed;
                }
                break;
            case MoveState.Knockback:
                if (!knockBackDone)
                {
                    print("KNOCKBACK");
                    currentVel = currentVel + knockback;
                    horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                    currentSpeed = horizontalVel.magnitude;
                    currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
                    knockBackDone = true;
                }
                else
                {
                    aux = currentVel.normalized * currentSpeed;
                    currentVel = new Vector3(aux.x, currentVel.y, aux.z);
                }
                //print("vel.y = " + currentVel.y);

                break;
            case MoveState.MovingBreaking://FRENADA FUERTE
                Vector3 finalDir = currentVel + currentMovDir * finalMovingAcc;
                horizontalVel = new Vector3(finalDir.x, 0, finalDir.z);
                currentVel = horizontalVel.normalized * currentSpeed;
                currentVel.y = finalDir.y;
                break;
            case MoveState.Hooked:
                horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                currentSpeed = horizontalVel.magnitude;
                break;
            case MoveState.FixedJump:
                currentVel = knockback;
                horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                currentSpeed = horizontalVel.magnitude;
                currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
                break;
            case MoveState.NotBreaking:
                horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                currentSpeed = horizontalVel.magnitude;
                break;

        }
        #endregion
    }

    void VerticalMovement()
    {
        if (!jumpedOutOfWater && !inWater && controller.collisions.below)
        {
            jumpedOutOfWater = true;
            maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;
        }
        if (lastWallAngle >= 0 && controller.collisions.below)
        {
            lastWallAngle = -1;
        }
        if (Actions.Jump.WasPressed)//Input.GetButtonDown(contName + "A"))
        {
            //print("JUMP");
            StartJump();
        }

        switch (jumpSt)
        {
            case JumpState.none:
                currentVel.y += gravity * Time.deltaTime;
                break;
            case JumpState.Jumping:
                currentVel.y += gravity * Time.deltaTime;
                timePressingJump += Time.deltaTime;
                if (timePressingJump >= maxTimePressingJump)
                {
                    StopJump();
                }
                else
                {
                    if (Actions.Jump.WasReleased)//Input.GetButtonUp(contName + "A"))
                    {
                        jumpSt = JumpState.Breaking;
                    }
                }
                break;
            case JumpState.Breaking:
                currentVel.y += (gravity * breakJumpForce) * Time.deltaTime;
                if (currentVel.y <= 0)
                {
                    jumpSt = JumpState.none;
                }

                break;


        }
        if (inWater)
        {
            currentVel.y = Mathf.Clamp(currentVel.y, -maxVerticalSpeedInWater, float.MaxValue);
        }
        ProcessJumpInsurance();

    }
    #endregion

    #region JUMP ---------------------------------------------------
    void StartJump()
    {
        if (!noInput && moveSt != MoveState.Boost)
        {
            if ((controller.collisions.below || jumpInsurance) && (!inWater || (inWater && controller.collisions.around &&
                ((GameController.instance.gameMode == GameController.GameMode.CaptureTheFlag && !ScoreManager.instance.prorroga) ||
                (GameController.instance.gameMode != GameController.GameMode.CaptureTheFlag)))))
            {
                currentVel.y = jumpVelocity;
                jumpSt = JumpState.Jumping;
                timePressingJump = 0;
                myPlayerAnimation.SetJump(true);
            }
            else
            {
                Debug.LogWarning("Warning: Can't jump because: controller.collisions.below || jumpInsurance (" + (controller.collisions.below || jumpInsurance) +
                    ") / !inWater || (inWater && controller.collisions.around && ((GameController.instance.gameMode == GameController.GameMode.CaptureTheFlag && !ScoreManager.instance.prorroga) || (GameController.instance.gameMode != GameController.GameMode.CaptureTheFlag))) (" +
                    (!inWater || (inWater && controller.collisions.around &&
                ((GameController.instance.gameMode == GameController.GameMode.CaptureTheFlag && !ScoreManager.instance.prorroga) ||
                (GameController.instance.gameMode != GameController.GameMode.CaptureTheFlag)))) + ")");
                StartWallJump();
            }
        }
        else
        {
            Debug.LogWarning("Warning: Can't jump because: player is in noInput mode(" + !noInput + ") / moveSt != Boost (" + moveSt != MoveState.Boost + ")");
        }
    }

    void StopJump()
    {
        jumpSt = JumpState.none;
        timePressingJump = 0;
    }

    void ProcessJumpInsurance()
    {
        if (!jumpInsurance)
        {
            if (controller.collisions.lastBelow && !controller.collisions.below && jumpSt == JumpState.none && jumpedOutOfWater)
            {
                //print("Jump Insurance");
                jumpInsurance = true;
                timeJumpInsurance = 0;
            }
        }
        else
        {
            timeJumpInsurance += Time.deltaTime;
            if (timeJumpInsurance >= maxTimeJumpInsurance || jumpSt == JumpState.Jumping)
            {
                jumpInsurance = false;
            }
        }

    }

    void StartWallJump()
    {
        if (!controller.collisions.below && (!inWater || inWater && controller.collisions.around) && controller.collisions.collisionHorizontal &&
            (lastWallAngle != controller.collisions.wallAngle || lastWallAngle == controller.collisions.wallAngle && lastWall != controller.collisions.wall) && jumpedOutOfWater)
        {
            GameObject wall = controller.collisions.wall;
            if (wall.GetComponent<StageScript>() == null || wall.GetComponent<StageScript>().wallJumpable)
            {
                print("Wall jump");
                //wallJumped = true;
                stopWallTime = 0;
                currentVel = Vector3.zero;
                wallJumping = true;
                anchorPoint = transform.position;
                wallNormal = controller.collisions.wallNormal;
                wallNormal.y = 0;
                lastWallAngle = controller.collisions.wallAngle;
                lastWall = wall;
            }

        }
    }

    void ProcessWallJump()
    {
        if (wallJumping)
        {
            currentVel = Vector3.zero;
            currentSpeed = 0;
            stopWallTime += Time.deltaTime;
            if (stopWallTime >= stopWallMaxTime)
            {
                EndWallJump();
            }
        }
    }

    [HideInInspector]
    public bool wallJumpAnim = false;

    void EndWallJump()
    {
        wallJumping = false;
        wallJumpAnim = true;
        //CALCULATE JUMP DIR
        //LEFT OR RIGHT ORIENTATION?
        //Angle
        Vector3 circleCenter = anchorPoint + Vector3.up * walJumpConeHeight;
        Vector3 circumfPoint = CalculateReflectPoint(wallJumpRadius, wallNormal, circleCenter);
        Vector3 finalDir = (circumfPoint - anchorPoint).normalized;
        Debug.LogWarning("FINAL DIR= " + finalDir.ToString("F4"));

        currentVel = finalDir * wallJumpVelocity;
        currentSpeed = currentVel.magnitude;
        currentMovDir = new Vector3(finalDir.x, 0, finalDir.z);
        RotateCharacter();

        myPlayerAnimation.SetJump(true);

        Debug.DrawLine(anchorPoint, circleCenter, Color.white, 20);
        Debug.DrawLine(anchorPoint, circumfPoint, Color.yellow, 20);
    }
    #endregion

    #region  DASH ---------------------------------------------
    void WallBoost()
    {
        //CALCULATE JUMP DIR
        Vector3 circleCenter = transform.position;
        Vector3 circumfPoint = CalculateReflectPoint(1, controller.collisions.wallNormal, circleCenter);
        Vector3 finalDir = (circumfPoint - circleCenter).normalized;
        Debug.LogWarning("FINAL DIR= " + finalDir.ToString("F4"));

        currentVel = finalDir * currentVel.magnitude;
        currentSpeed = currentVel.magnitude;
        boostDir = new Vector3(finalDir.x, 0, finalDir.z);
        RotateCharacter();
    }

    void StartBoost()
    {
        if (!noInput && boostReady && !haveFlag && !inWater)
        {
            boostReady = false;
            moveSt = MoveState.Boost;
            boostTime = 0f;
            if (currentMovDir != Vector3.zero)
            {
                boostDir = currentMovDir;
            }
            else
            {
                currentMovDir = boostDir = new Vector3(currentCamFacingDir.x, 0, currentCamFacingDir.z);
                RotateCharacter();
            }
        }

    }

    void ProcessBoost()
    {
        if (!boostReady)
        {
            boostTime += Time.deltaTime;
            if (boostTime < boostDuration)
            {
                if (Actions.Jump.WasPressed)
                {
                    StopBoost();
                }
                moveSt = MoveState.Boost;
            }
            if (boostTime >= boostCD)
            {
                boostReady = true;
            }
        }
    }

    void StopBoost()
    {
        boostTime = boostDuration;
    }
    #endregion

    #region  FACING DIR AND ANGLE & BODY ROTATION---------------------------------------------
    [HideInInspector]
    public Vector3 currentFacingDir = Vector3.forward;
    [HideInInspector]
    public float facingAngle = 0;
    [HideInInspector]
    public Vector3 currentCamFacingDir = Vector3.zero;

    void UpdateFacingDir()//change so that only rotateObj rotates, not whole body
    {
        switch (myCamera.camMode)
        {
            case CameraController.cameraMode.Fixed:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                //Calculate looking dir of camera
                Vector3 camPos = myCamera.transform.GetChild(0).position;
                Vector3 myPos = transform.position;
                currentFacingDir = new Vector3(myPos.x - camPos.x, 0, myPos.z - camPos.z).normalized;
                currentCamFacingDir = myCamera.myCamera.transform.forward.normalized;
                break;
            case CameraController.cameraMode.Shoulder:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                currentFacingDir = RotateVector(-myCamera.transform.localRotation.eulerAngles.y, Vector3.forward).normalized;
                currentCamFacingDir = myCamera.myCamera.transform.forward.normalized;
                //print("CurrentFacingDir = " + currentFacingDir);
                break;
            case CameraController.cameraMode.Free:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                currentFacingDir = RotateVector(-rotateObj.localRotation.eulerAngles.y, Vector3.forward).normalized;
                currentCamFacingDir = (myCamera.cameraFollowObj.transform.position - myCamera.myCamera.transform.position).normalized;
                break;
        }
        //print("currentFacingDir = " + currentFacingDir + "; currentCamFacingDir = " + currentCamFacingDir);

    }

    public void RotateCharacter(float rotSpeed = 0)
    {
        switch (myCamera.camMode)
        {
            case CameraController.cameraMode.Fixed:
                Vector3 point1 = transform.position;
                Vector3 point2 = new Vector3(point1.x, point1.y + 1, point1.z);
                Vector3 dir = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
                rotateObj.Rotate(dir, rotSpeed * Time.deltaTime);
                break;
            case CameraController.cameraMode.Shoulder:
                point1 = transform.position;
                point2 = new Vector3(point1.x, point1.y + 1, point1.z);
                dir = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
                rotateObj.Rotate(dir, rotSpeed * Time.deltaTime);
                break;
            case CameraController.cameraMode.Free:
                if (currentMovDir != Vector3.zero)
                {
                    float angle = Mathf.Acos(((0 * currentMovDir.x) + (1 * currentMovDir.z)) / (1 * currentMovDir.magnitude)) * Mathf.Rad2Deg;
                    angle = currentMovDir.x < 0 ? -angle : angle;
                    rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
                }
                break;
        }

    }
    #endregion

    #region RECIEVE HIT AND STUN ---------------------------------------------
    [HideInInspector]
    public float maxTimeStun = 0.6f;
    float timeStun = 0;
    bool stunned;
    bool knockBackDone;
    Vector3 knockback;

    public void StartRecieveHit(Vector3 _knockback, PlayerMovement attacker, float _maxTimeStun)
    {
        print("Recieve hit");
        maxTimeStun = _maxTimeStun;
        timeStun = 0;
        noInput = true;
        stunned = true;
        knockBackDone = false;
        knockback = _knockback;

        //Give FLAG
        if (haveFlag)
        {
            flag.DropFlag();
        }

        print("STUNNED");
    }

    void ProcessStun()
    {
        if (stunned)
        {
            moveSt = MoveState.Knockback;
            timeStun += Time.deltaTime;
            if (timeStun >= maxTimeStun)
            {
                StopStun();
            }
        }
    }

    void StopStun()
    {
        noInput = false;
        stunned = false;
        print("STUN END");
    }

    #endregion

    #region FIXED JUMP ---------------------------------------------------
    bool fixedJumping;
    bool fixedJumpDone;
    float noMoveMaxTime;
    float noMoveTime;

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
                print("notBreaking on");
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

    #region HOOKING/HOOK ---------------------------------------------
    bool hooked;
    public void StartHooked()
    {
        if (!hooked)
        {
            noInput = true;
            hooked = true;
        }
    }

    void ProcessHooked()
    {
        if (hooked)
        {
            moveSt = MoveState.Hooked;
        }
    }

    public void StopHooked()
    {
        if (hooked)
        {
            noInput = false;
            hooked = false;
            currentVel = Vector3.zero;
            currentSpeed = 0;
        }
    }

    bool hooking;
    public void StartHooking()
    {
        if (!hooking)
        {
            hooking = true;
        }
    }

    void ProcessHooking()
    {
        if (hooking)
        {
            if (maxMoveSpeed2 > maxHookingSpeed)
            {
                maxMoveSpeed2 = maxHookingSpeed;
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
        if (myPlayerCombat.aiming && maxMoveSpeed2 > maxAimingSpeed)
        {
            maxMoveSpeed2 = maxAimingSpeed;
        }
    }
    #endregion

    #region PICKUP / FLAG / DEATH ---------------------------------------------
    [HideInInspector]
    public bool haveFlag = false;
    [HideInInspector]
    public Flag flag = null;

    public void PutOnFlag(Flag _flag)
    {
        flag = _flag;
        flag.transform.SetParent(rotateObj);
        flag.transform.localPosition = new Vector3(0, 0, -0.5f);
        flag.transform.localRotation = Quaternion.Euler(0, -90, 0);
    }

    public void LoseFlag()
    {
        haveFlag = false;
        flag = null;
    }

    public void Die()
    {
        if (haveFlag)
        {
            flag.SetAway(false);
        }
        GameController.instance.RespawnPlayer(this);
    }
    #endregion

    #region  WATER ---------------------------------------------
    [HideInInspector]
    public bool inWater = false;
    bool jumpedOutOfWater = true;

    public void EnterWater()
    {
        if (!inWater)
        {
            inWater = true;
            jumpedOutOfWater = false;
            maxTimePressingJump = 0f;
            myPlayerWeap.AttachWeaponToBack();
            if (haveFlag)
            {
                //GameController.instance.RespawnFlag(flag.GetComponent<Flag>());
                flag.SetAway(false);
            }
            //Desactivar al jugadro si se esta en la prorroga.
            if (GameController.instance.gameMode == GameController.GameMode.CaptureTheFlag)
            {
                ScoreManager.instance.PlayerEliminado();
            }
        }
    }

    void ProcessWater()
    {
        if (inWater)
        {
            controller.AroundCollisions();
            if (maxMoveSpeed2 > maxSpeedInWater)
            {
                maxMoveSpeed2 = maxSpeedInWater;
            }
        }
    }

    public void ExitWater()
    {
        if (inWater)
        {
            inWater = false;
            myPlayerWeap.AttachWeapon();
        }
    }
    #endregion

    #region  CHECK WIN ---------------------------------------------
    public void CheckScorePoint(FlagHome flagHome)
    {
        if (haveFlag && team == flagHome.team && this.moveSt != MoveState.Hooked)
        {
            GameController.instance.ScorePoint(team);
            if (flag != null)
            {
                flag.SetAway(true);
            }
        }
    }
    #endregion

    public void ResetPlayer()
    {
        ExitWater();
        jumpSt = JumpState.none;
    }

    #region  AUXILIAR FUNCTIONS ---------------------------------------------
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
        Debug.LogWarning("ANGLE = " + angle);
        float offsetAngleDir = Vector3.Angle(wallDirLeft, Vector3.forward) > 90 ? -1 : 1;
        float offsetAngle = Vector3.Angle(Vector3.right, wallDirLeft) * offsetAngleDir;
        angle += offsetAngle;
        //CALCULATE CIRCUMFERENCE POINT
        float px = circleCenter.x + (radius * Mathf.Cos(angle * Mathf.Deg2Rad));
        float pz = circleCenter.z + (radius * Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector3 circumfPoint = new Vector3(px, circleCenter.y, pz);

        Debug.LogWarning("; circleCenter= " + circleCenter + "; circumfPoint = " + circumfPoint + "; angle = " + angle + "; offsetAngle = " + offsetAngle + "; offsetAngleDir = " + offsetAngleDir
    + ";wallDirLeft = " + wallDirLeft);
        Debug.DrawLine(circleCenter, circumfPoint, Color.white, 20);

        return circumfPoint;
    }
    #endregion
    #endregion PlayerMovement

    #region PlayerCombat
    #endregion PlayerCombat

    #region PlayerMovement
    #endregion PlayerMovement

    #region PlayerMovement
    #endregion PlayerMovement
}

