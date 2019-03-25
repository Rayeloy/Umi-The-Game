using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#region ----[ PUBLIC ENUMS ]----
#endregion

[System.Serializable]
public class MyIntEvent : UnityEvent<int>
{
}

public class Dummy : MonoBehaviour
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    public Controller3D controller;

    public MyIntEvent phaseProgressFunction;
    [Tooltip("Max number of hits the dummy can take before deflating")]
    public int maxHP;
    int currentHP;

    //VARIABLES DE MOVIMIENTO
    Vector3 objectiveVel;
    [HideInInspector]
    public float maxMoveSpeed = 10;
    float maxMoveSpeed2; // is the max speed from which we aply the joystick sensitivity value
    float currentMaxMoveSpeed = 10.0f; // its the final max speed, after the joyjoystick sensitivity value

    [Header("Body Mass")]
    [Tooltip("Body Mass Index. 1 is for normal body mass.")]
    public float bodyMass;

    [Header("SPEED")]
    [Tooltip("Maximum speed that you can travel at horizontally when hit by someone")]
    public float maxKnockbackSpeed = 300f;
    public float maxSpeedInWater = 5f;
    public float maxVerticalSpeedInWater = 3f;

    [Header("ACCELERATIONS")]
    public float breakAcc = -30;
    [Tooltip("Breaking negative acceleration that is used under the effects of a knockback (stunned). Value is clamped to not be higher than breakAcc.")]
    public float knockbackBreakAcc = -30f;

    [HideInInspector]
    public float gravity;
    [Header("JUMP")]
    public float jumpHeight = 4f;
    public float jumpApexTime = 0.4f;
    #endregion

    #region ----[ PROPERTIES ]----
    [HideInInspector]
    public MoveState moveSt = MoveState.NotMoving;

    [HideInInspector]
    public Vector3 currentVel;
    [HideInInspector]
    public float currentSpeed = 0;

    [HideInInspector]
    public bool inWater = false;

    [HideInInspector]
    public Vector3 spawnPosition;
    [HideInInspector]
    public Quaternion spawnRotation;

    //KNOCKBACK AND STUN
    bool knockBackDone;
    Vector3 knockback;

    //HOOK
    bool hooked;
    #endregion

    #region ----[ VARIABLES ]----
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    private void Awake()
    {
        maxMoveSpeed = 10;
        currentSpeed = 0;
        currentHP = maxHP;
    }
    #endregion

    #region Start
    private void Start()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);
        print("Dummy Gravity = " + gravity);
        knockbackBreakAcc = Mathf.Clamp(knockbackBreakAcc, -float.MaxValue, breakAcc);//menos de break Acc lo haría ver raro
    }
    #endregion

    #region Update
    private void Update()
    {
        if ((controller.collisions.above || controller.collisions.below) && !hooked)
        {
            currentVel.y = 0;
        }

        HorizontalMovement();
        VerticalMovement();

        //Debug.Log("currentVel = " + currentVel + "; Time.deltaTime = " + Time.deltaTime + "; currentVel * Time.deltaTime = " + (currentVel * Time.deltaTime) + "; Time.fixedDeltaTime = " + Time.fixedDeltaTime);
        controller.Move(currentVel * Time.deltaTime);
        controller.collisions.ResetAround();
        Debug.LogWarning("DUMMY: vel = " + currentVel.ToString("F4"));
    }

    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    #region MOVEMENT -----------------------------------------------
    void SetVelocity(Vector3 vel)
    {
        objectiveVel = vel;
        currentVel = objectiveVel;
    }

    void HorizontalMovement()
    {
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        #region//------------------------------------------------ DECIDO TIPO DE MOVIMIENTO --------------------------------------------
        #region//----------------------------------------------------- Efecto externo --------------------------------------------
        if (hooked)
        {
            ProcessHooked();
        }
        #endregion
        #region //----------------------------------------------------- Efecto interno --------------------------------------------
        if (!hooked)
        {
            //------------------------------------------------ aceleracion, maxima velocidad y velocidad ---------------------------------
            //------------------------------- Max Move Speed -------------------------------
            maxMoveSpeed2 = maxMoveSpeed;
            ProcessWater();
            currentMaxMoveSpeed = maxMoveSpeed2;
            //------------------------------- Acceleration -------------------------------
            float actAccel;
            switch (moveSt)
            {
                case MoveState.Knockback:
                    actAccel = knockbackBreakAcc;
                    break;
                default:
                    actAccel = breakAcc;
                    break;
            }
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
            case MoveState.NotMoving:
                Vector3 aux = currentVel.normalized * currentSpeed;
                currentVel = new Vector3(aux.x, currentVel.y, aux.z);
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
            case MoveState.Hooked:
                horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                currentSpeed = horizontalVel.magnitude;
                break;
        }
        #endregion
    }

    void VerticalMovement()
    {
        currentVel.y += gravity * Time.deltaTime;

        if (inWater)
        {
            currentVel.y = Mathf.Clamp(currentVel.y, -maxVerticalSpeedInWater, float.MaxValue);
        }

    }
    #endregion

    #region HOOKED
    public void StartHooked()
    {
        if (!hooked)
        {
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
            hooked = false;
            currentVel = Vector3.zero;
            currentSpeed = 0;
        }
    }
    #endregion

    #region WATER
    void EnterWater()
    {
        if (!inWater)
        {
            inWater = true;
            phaseProgressFunction.Invoke(0);
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

    void ExitWater()
    {
        if (inWater)
        {
            inWater = false;
        }
    }
    #endregion

    #region COLLISIONS
    private void OnTriggerStay(Collider col)
    {
        switch (col.tag)
        {
            case "Water":
                float waterSurface = col.GetComponent<Collider>().bounds.max.y;
                if (transform.position.y <= waterSurface)
                {
                    EnterWater();
                }
                else
                {
                    ExitWater();
                }
                break;
        }
    }
    #endregion


    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----
    public void StartRecieveHit(Vector3 _knockback)
    {
        print("Dummy: Recieve hit");
        moveSt = MoveState.Knockback;
        knockBackDone = false;
        _knockback = _knockback / bodyMass;
        knockback = _knockback;
        currentHP--;
        if (currentHP == 0)
        {
            DeflateDummy();
        }
    }

    public void InflateDummy()
    {
        GetComponent<CapsuleCollider>().enabled = true;
        //animación hinchado
    }

    public void DeflateDummy()
    {
        gameObject.layer = LayerMask.NameToLayer("Atrezzo");
        //animación desinchado
    }
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

#region ----[ STRUCTS ]----
#endregion

