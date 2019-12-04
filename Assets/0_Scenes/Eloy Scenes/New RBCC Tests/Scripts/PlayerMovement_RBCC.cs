using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement_RBCC : MonoBehaviour
{
    public Transform groundChecker;
    public LayerMask groundLayerMask;
    public float groundCheckSphereRadius;
    bool isGrounded = false;

    CharacterController myController;
    Vector3 currentVelocity;
    Vector3 currentMovingDir;
    public float movingAcc = 10;
    public float breakingAcc = 10;
    public float torqueAcc = 1;
    public float maxMovingSpeed = 10;
    float currentMovingSpeed = 0;
    float currentMaxMovingSpeed;

    private void Awake()
    {
        myController = GetComponent<CharacterController>();
        currentMaxMovingSpeed = maxMovingSpeed;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(groundChecker.position, groundCheckSphereRadius);
    }

        public void Move(Vector2 movingInput, float joystickSens)
    {
        //GROUND CHECK
        isGrounded = Physics.CheckSphere(groundChecker.position, groundCheckSphereRadius, groundLayerMask, QueryTriggerInteraction.Ignore);
        Debug.Log("Before: isGrounded -> "+isGrounded + " currentVelocity.y" + currentVelocity.y.ToString("F6"));
        if (isGrounded && currentVelocity.y < 0)
            currentVelocity.y = 0f;
        Debug.Log("After: isGrounded -> " + isGrounded + " currentVelocity.y" + currentVelocity.y.ToString("F6"));


        //INPUT
        Vector3 movingInputAux = new Vector3(movingInput.x, 0, movingInput.y).normalized;

        //Moving Dir
        currentMovingDir = (currentMovingDir + (movingInputAux * torqueAcc* Time.deltaTime)).normalized;

        //SPEED
        currentMaxMovingSpeed = maxMovingSpeed * joystickSens;
        if (movingInput.magnitude > 0)
        {
            if (currentMovingSpeed < maxMovingSpeed)
                currentMovingSpeed += (movingAcc * Time.deltaTime);
        }
        else
        {
            if (currentMovingSpeed > 0)
                currentMovingSpeed -= (breakingAcc * Time.deltaTime);
        }
 
        currentMovingSpeed = Mathf.Clamp(currentMovingSpeed, 0, maxMovingSpeed);
        if (currentMovingSpeed == 0) currentMovingDir = Vector3.zero;

        //Current Velocity
        Vector3 horVel = currentMovingDir * currentMovingSpeed;
        currentVelocity = new Vector3(horVel.x, currentVelocity.y, horVel.z);

        //Gravity
        currentVelocity.y += -25f * Time.deltaTime;

        myController.Move(currentVelocity * Time.deltaTime);
        Debug.Log("currentVelocity = "+ currentVelocity.ToString("F6")+ "CurrentMovingSpeed = " + currentMovingSpeed.ToString("F6"));
    }
}
