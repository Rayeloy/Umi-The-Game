using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_RB : MonoBehaviour
{
    public PlayerMovement_RB myPlayerMov;

    public PlayerMovement_RBCC myPlayerMovRBCC;

    PlayerActions myControls;
    [Range(0, 1)]
    public float deadzone = 0.2f;
    Vector2 movingInput;//left joystick
    float joystickSens = 0;

    private void Awake()
    {
        myControls = PlayerActions.CreateDefaultBindings();
    }

    private void Update()
    {
        movingInput = new Vector2(myControls.LeftJoystick.X, myControls.LeftJoystick.Y);
        joystickSens = movingInput.magnitude;
        if (movingInput.magnitude >= deadzone)
        {
            joystickSens = joystickSens >= 0.88f ? 1 : joystickSens;//Eloy: esto evita un "bug" por el que al apretar el joystick 
                                                                    //contra las esquinas no da un valor total de 1, sino de 0.9 o así
            movingInput.Normalize();
        }
        else
        {
            movingInput = Vector2.zero;
        }
        if (myPlayerMov != null)
            myPlayerMov.Move(movingInput, joystickSens);
        else
            myPlayerMovRBCC.Move(movingInput, joystickSens);
    }

    void RaycastFloor()
    {

    }
}

class Inputs
{
    public Vector2 LeftJoystick;
    public bool A;
}
