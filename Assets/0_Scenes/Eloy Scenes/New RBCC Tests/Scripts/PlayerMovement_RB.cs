using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement_RB : MonoBehaviour
{
    Rigidbody myRB;
    public float movingAcc = 10;
    public float maxMovingSpeed = 10;
    float currentMaxMovingSpeed;

    private void Awake()
    {
        myRB = GetComponent<Rigidbody>();
        currentMaxMovingSpeed = maxMovingSpeed;
    }

    public void Move(Vector2 movingInput, float joystickSens)
    {
        Vector3 movingInputAux = new Vector3(movingInput.x, 0, movingInput.y).normalized;

        currentMaxMovingSpeed = maxMovingSpeed * joystickSens;
        if (myRB.velocity.magnitude < currentMaxMovingSpeed)
        {
            myRB.AddForce(movingInputAux * movingAcc * Time.deltaTime);
        }
        else
        {

        }
    }
}
