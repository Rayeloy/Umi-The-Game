using UnityEngine;
using InControl;


public class PlayerActions : PlayerActionSet
{
    public InputDeviceClass controlsType;

    public PlayerAction R1;
    public PlayerAction R2;
    public PlayerAction L1;
    public PlayerAction L2;
    //public PlayerAction UsePickup;

    public PlayerAction A;
    public PlayerAction X;
    public PlayerAction Y;
    public PlayerAction B;

    public PlayerAction RJLeft;
    public PlayerAction RJRight;
    public PlayerAction RJUp;
    public PlayerAction RJDown;
    public PlayerTwoAxisAction RightJoystick;

    public PlayerAction LJLeft;
    public PlayerAction LJRight;
    public PlayerAction LJUp;
    public PlayerAction LJDown;
    public PlayerTwoAxisAction LeftJoystick;

    public PlayerAction R3;
    public PlayerAction L3;

    public PlayerAction Start;

    public PlayerActions()
    {
        R1 = CreatePlayerAction("R1");
        R2 = CreatePlayerAction("R2");
        L1 = CreatePlayerAction("L1");
        L2 = CreatePlayerAction("L2");

        A = CreatePlayerAction("A");
        X = CreatePlayerAction("X");
        Y = CreatePlayerAction("Y");
        B = CreatePlayerAction("B");

        RJLeft = CreatePlayerAction("RJLeft");
        RJRight = CreatePlayerAction("RJRight");
        RJUp = CreatePlayerAction("RJUp");
        RJDown = CreatePlayerAction("RJDown");
        RightJoystick = CreateTwoAxisPlayerAction(RJLeft, RJRight, RJDown, RJUp);

        LJLeft = CreatePlayerAction("LJLeft");
        LJRight = CreatePlayerAction("LJRight");
        LJUp = CreatePlayerAction("LJUp");
        LJDown = CreatePlayerAction("LJDown");
        LeftJoystick = CreateTwoAxisPlayerAction(LJLeft, LJRight, LJDown, LJUp);
        //LeftJoystick.LowerDeadZone = 

        R3 = CreatePlayerAction("R3");
        L3 = CreatePlayerAction("L3");

        Start = CreatePlayerAction("Start");
    }

    public static PlayerActions CreateDefaultBindings()
    {
        var actions = new PlayerActions();
        //KEYBOARD AND MOUSE
        actions.controlsType = InputDeviceClass.Keyboard;

        actions.R1.AddDefaultBinding(Key.T);
        actions.R2.AddDefaultBinding(Key.LeftShift);
        actions.L1.AddDefaultBinding(Key.G);
        actions.L2.AddDefaultBinding(Mouse.RightButton);

        actions.A.AddDefaultBinding(Key.Space);
        actions.X.AddDefaultBinding(Mouse.LeftButton);
        actions.Y.AddDefaultBinding(Key.Key2);
        actions.B.AddDefaultBinding(Key.Key3);

        //actions.RJLeft.LowerDeadZone = 0.2f;
        actions.RJUp.AddDefaultBinding(Mouse.PositiveY);
        actions.RJDown.AddDefaultBinding(Mouse.NegativeY);
        actions.RJLeft.AddDefaultBinding(Mouse.NegativeX);
        actions.RJRight.AddDefaultBinding(Mouse.PositiveX);

        actions.LJUp.AddDefaultBinding(Key.W);
        actions.LJDown.AddDefaultBinding(Key.S);
        actions.LJLeft.AddDefaultBinding(Key.A);
        actions.LJRight.AddDefaultBinding(Key.D);

        actions.L3.AddDefaultBinding(Key.Key4);
        actions.R3.AddDefaultBinding(Key.Key5);

        actions.Start.AddDefaultBinding(Key.Escape);

        // JOYSTICK
        actions.controlsType = InputDeviceClass.Controller;

        actions.R1.AddDefaultBinding(InputControlType.RightBumper);
        actions.R2.AddDefaultBinding(InputControlType.RightTrigger);
        actions.L1.AddDefaultBinding(InputControlType.LeftBumper);
        actions.L2.AddDefaultBinding(InputControlType.LeftTrigger);

        actions.A.AddDefaultBinding(InputControlType.Action1);
        actions.X.AddDefaultBinding(InputControlType.Action3);
        actions.Y.AddDefaultBinding(InputControlType.Action4);
        actions.B.AddDefaultBinding(InputControlType.Action2);

        actions.RJUp.AddDefaultBinding(InputControlType.RightStickUp);
        actions.RJDown.AddDefaultBinding(InputControlType.RightStickDown);
        actions.RJLeft.AddDefaultBinding(InputControlType.RightStickLeft);
        actions.RJRight.AddDefaultBinding(InputControlType.RightStickRight);
        

        actions.LJUp.AddDefaultBinding(InputControlType.LeftStickUp);
        actions.LJDown.AddDefaultBinding(InputControlType.LeftStickDown);
        actions.LJLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
        actions.LJRight.AddDefaultBinding(InputControlType.LeftStickRight);

        actions.R3.AddDefaultBinding(InputControlType.RightStickButton);
        actions.L3.AddDefaultBinding(InputControlType.LeftStickButton);

        actions.Start.AddDefaultBinding(InputControlType.Start);
        return actions;
    }

    public static PlayerActions CreateWithKeyboardBindings()
    {
        var actions = new PlayerActions();

        actions.controlsType = InputDeviceClass.Keyboard;

        actions.R1.AddDefaultBinding(Key.T);
        actions.R2.AddDefaultBinding(Key.LeftShift);
        actions.L1.AddDefaultBinding(Key.G);
        actions.L2.AddDefaultBinding(Mouse.RightButton);

        actions.A.AddDefaultBinding(Key.Space);
        actions.X.AddDefaultBinding(Mouse.LeftButton);
        actions.Y.AddDefaultBinding(Key.Key2);
        actions.B.AddDefaultBinding(Key.Key3);

        actions.RJUp.AddDefaultBinding(Mouse.PositiveY);
        actions.RJDown.AddDefaultBinding(Mouse.NegativeY);
        actions.RJLeft.AddDefaultBinding(Mouse.NegativeX);
        actions.RJRight.AddDefaultBinding(Mouse.PositiveX);

        actions.LJUp.AddDefaultBinding(Key.W);
        actions.LJDown.AddDefaultBinding(Key.S);
        actions.LJLeft.AddDefaultBinding(Key.A);
        actions.LJRight.AddDefaultBinding(Key.D);

        actions.L3.AddDefaultBinding(Key.Key4);
        actions.R3.AddDefaultBinding(Key.Key5);

        actions.Start.AddDefaultBinding(Key.Escape);
        return actions;
    }

    public static PlayerActions CreateWithJoystickBindings()
    {
        var actions = new PlayerActions();

        actions.controlsType = InputDeviceClass.Controller;

        actions.R1.AddDefaultBinding(InputControlType.RightBumper);
        actions.R2.AddDefaultBinding(InputControlType.RightTrigger);
        actions.L1.AddDefaultBinding(InputControlType.LeftBumper);
        actions.L2.AddDefaultBinding(InputControlType.LeftTrigger);

        actions.A.AddDefaultBinding(InputControlType.Action1);
        actions.X.AddDefaultBinding(InputControlType.Action3);
        actions.Y.AddDefaultBinding(InputControlType.Action4);
        actions.B.AddDefaultBinding(InputControlType.Action2);

        actions.RJUp.AddDefaultBinding(InputControlType.RightStickUp);
        actions.RJDown.AddDefaultBinding(InputControlType.RightStickDown);
        actions.RJLeft.AddDefaultBinding(InputControlType.RightStickLeft);
        actions.RJRight.AddDefaultBinding(InputControlType.RightStickRight);

        actions.LJUp.AddDefaultBinding(InputControlType.LeftStickUp);
        actions.LJDown.AddDefaultBinding(InputControlType.LeftStickDown);
        actions.LJLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
        actions.LJRight.AddDefaultBinding(InputControlType.LeftStickRight);

        actions.R3.AddDefaultBinding(InputControlType.RightStickButton);
        actions.L3.AddDefaultBinding(InputControlType.LeftStickButton);

        actions.Start.AddDefaultBinding(InputControlType.Start);
        return actions;
    }

    public bool AnyButtonWasPressed()
    {
        //RJUp.WasPressed || RJLeft.WasPressed || RJRight.WasPressed || RJDown.WasPressed || LJUp.WasPressed || LJDown.WasPressed || LJLeft.WasPressed || LJRight.WasPressed
        if (R1.WasPressed || R2.WasPressed || L1.WasPressed || L2.WasPressed || A.WasPressed || B.WasPressed || X.WasPressed || Y.WasPressed ||
            (RightJoystick.X>0.2f || RightJoystick.X < -0.2f) || (RightJoystick.Y > 0.2f || RightJoystick.Y < -0.2f) || (LeftJoystick.X > 0.2f || LeftJoystick.X < -0.2f) ||
            (LeftJoystick.Y > 0.2f || LeftJoystick.Y < -0.2f) || R3.WasPressed || L3.WasPressed || Start.WasPressed)
        {
        //if (InputManager.AnyKeyIsPressed && (InputManager.ActiveDevice == this.ActiveDevice) || (this.ActiveDevice == null && Input.anyKeyDown))
        //{
        if(LeftJoystick.X !=0)
            {
                Debug.Log("LeftJoystick.X = " + LeftJoystick.X);
            }
        return true;
        //}
        }
        return false;
    }
}