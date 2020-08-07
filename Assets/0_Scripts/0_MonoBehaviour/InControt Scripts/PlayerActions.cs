using UnityEngine;
using InControl;


public class PlayerActions : PlayerActionSet
{
    public bool isKeyboard
    {
        get
        {
            return Device == null;
        }
    }
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

    public PlayerAction DPadLeft;
    public PlayerAction DPadRight;
    public PlayerAction DPadDown;
    public PlayerAction DPadUp;
    public PlayerTwoAxisAction DPad;

    public PlayerAction RJLeft;
    public PlayerAction RJRight;
    public PlayerAction RJDown;
    public PlayerAction RJUp;
    public PlayerTwoAxisAction RightJoystick;

    public PlayerAction LJLeft;
    public PlayerAction LJRight;
    public PlayerAction LJDown;
    public PlayerAction LJUp;
    public PlayerTwoAxisAction LeftJoystick;
    public EAITwoAxisControls leftJoystickAsButtons;

    public PlayerAction R3;
    public PlayerAction L3;

    public PlayerAction Start;
    public PlayerAction Select;

    public PlayerAction MouseWheelUp;
    public PlayerAction MouseWheelDown;
    public PlayerOneAxisAction MouseWheel;
    public EAIMouseWheelControls EAIMouseWheel;

    public PlayerAction ThrowHook;

    //Housing Edit Mode
    public PlayerAction RotateCameraHEMLeft;
    public PlayerAction RotateCameraHEMRight;
    public PlayerAction RotateCameraHEMDown;
    public PlayerAction RotateCameraHEMUp;
    public PlayerTwoAxisAction HousingRotateCamera;
    public PlayerAction ZoomIn;
    public PlayerAction HousingMoveUp;
    public PlayerAction HousingMoveDown;
    public PlayerAction HousingRotateFurnitureClockwise;
    public PlayerAction HousingRotateFurnitureCounterClockwise;
    public PlayerAction HousingPickFurniture;
    public PlayerAction HousingSwitchFurnitureMenu;

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

        DPadLeft = CreatePlayerAction("DPadLeft");
        DPadRight = CreatePlayerAction("DPadRight");
        DPadDown = CreatePlayerAction("DPadDown");
        DPadUp = CreatePlayerAction("DPadUp");
        DPad = CreateTwoAxisPlayerAction(DPadLeft, DPadRight, DPadDown, DPadUp);

        RJLeft = CreatePlayerAction("RJLeft");
        RJRight = CreatePlayerAction("RJRight");
        RJDown = CreatePlayerAction("RJDown");
        RJUp = CreatePlayerAction("RJUp");
        RightJoystick = CreateTwoAxisPlayerAction(RJLeft, RJRight, RJDown, RJUp);

        LJLeft = CreatePlayerAction("LJLeft");
        LJRight = CreatePlayerAction("LJRight");
        LJDown = CreatePlayerAction("LJDown");
        LJUp = CreatePlayerAction("LJUp");
        LeftJoystick = CreateTwoAxisPlayerAction(LJLeft, LJRight, LJDown, LJUp);
        //LeftJoystick.LowerDeadZone = 

        R3 = CreatePlayerAction("R3");
        L3 = CreatePlayerAction("L3");

        Start = CreatePlayerAction("Start");
        Select = CreatePlayerAction("Select");

        MouseWheelUp = CreatePlayerAction("MouseScrollUp");
        MouseWheelDown = CreatePlayerAction("MouseScrollDown");
        MouseWheel = CreateOneAxisPlayerAction(MouseWheelDown, MouseWheelUp);

        ThrowHook = CreatePlayerAction("ThrowHook");

        RotateCameraHEMLeft = CreatePlayerAction("RotateCameraHEMLeft");
        RotateCameraHEMRight = CreatePlayerAction("RotateCameraHEMRight");
        RotateCameraHEMDown = CreatePlayerAction("RotateCameraHEMDown");
        RotateCameraHEMUp = CreatePlayerAction("RotateCameraHEMUp");
        HousingRotateCamera = CreateTwoAxisPlayerAction(RotateCameraHEMLeft, RotateCameraHEMRight, RotateCameraHEMDown, RotateCameraHEMUp);
        ZoomIn = CreatePlayerAction("ZoomIn");
        HousingMoveUp = CreatePlayerAction("HousingEditModeMoveUp");
        HousingMoveDown = CreatePlayerAction("HousingEditModeMoveDown");
        HousingRotateFurnitureClockwise = CreatePlayerAction("HousingRotateFurnitureClockwise");
        HousingRotateFurnitureCounterClockwise = CreatePlayerAction("HousingRotateFurnitureCounterClockwise");
        HousingPickFurniture = CreatePlayerAction("HousingPickFurniture");
        HousingSwitchFurnitureMenu = CreatePlayerAction("HousingSwitchFurnitureMenu");
    }

    static void SetUpXboxControllerActions(ref PlayerActions actions) 
    {
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

        actions.DPadLeft.AddDefaultBinding(InputControlType.DPadLeft);
        actions.DPadRight.AddDefaultBinding(InputControlType.DPadRight);
        actions.DPadDown.AddDefaultBinding(InputControlType.DPadDown);
        actions.DPadUp.AddDefaultBinding(InputControlType.DPadUp);

        actions.RJLeft.AddDefaultBinding(InputControlType.RightStickLeft);
        actions.RJRight.AddDefaultBinding(InputControlType.RightStickRight);
        actions.RJDown.AddDefaultBinding(InputControlType.RightStickDown);
        actions.RJUp.AddDefaultBinding(InputControlType.RightStickUp);

        actions.LJLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
        actions.LJRight.AddDefaultBinding(InputControlType.LeftStickRight);
        actions.LJDown.AddDefaultBinding(InputControlType.LeftStickDown);
        actions.LJUp.AddDefaultBinding(InputControlType.LeftStickUp);

        actions.R3.AddDefaultBinding(InputControlType.RightStickButton);
        actions.L3.AddDefaultBinding(InputControlType.LeftStickButton);

        actions.Start.AddDefaultBinding(InputControlType.Start);
        actions.Select.AddDefaultBinding(InputControlType.Back);

        //GAME IN GAME ACTIONS
        actions.ThrowHook.AddDefaultBinding(InputControlType.RightTrigger);
        actions.RotateCameraHEMLeft.AddDefaultBinding(InputControlType.RightStickLeft);
        actions.RotateCameraHEMRight.AddDefaultBinding(InputControlType.RightStickRight);
        actions.RotateCameraHEMDown.AddDefaultBinding(InputControlType.RightStickDown);
        actions.RotateCameraHEMUp.AddDefaultBinding(InputControlType.RightStickUp);
        actions.ZoomIn.AddDefaultBinding(InputControlType.RightStickButton);
        actions.HousingMoveUp.AddDefaultBinding(InputControlType.DPadUp);
        actions.HousingMoveDown.AddDefaultBinding(InputControlType.DPadDown);
        actions.HousingRotateFurnitureClockwise.AddDefaultBinding(InputControlType.RightBumper);
        actions.HousingRotateFurnitureCounterClockwise.AddDefaultBinding(InputControlType.LeftBumper);
        actions.HousingPickFurniture.AddDefaultBinding(InputControlType.Action1);
        actions.HousingSwitchFurnitureMenu.AddDefaultBinding(InputControlType.Action4);
    }

    static void SetUpKeyboardAndMouseActions(ref PlayerActions actions)
    {
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

        actions.DPadLeft.AddDefaultBinding(Key.LeftArrow);
        actions.DPadRight.AddDefaultBinding(Key.RightArrow);
        actions.DPadDown.AddDefaultBinding(Key.DownArrow);
        actions.DPadUp.AddDefaultBinding(Key.UpArrow);

        actions.RJLeft.AddDefaultBinding(Mouse.NegativeX);
        actions.RJRight.AddDefaultBinding(Mouse.PositiveX);
        actions.RJDown.AddDefaultBinding(Mouse.NegativeY);
        actions.RJUp.AddDefaultBinding(Mouse.PositiveY);

        actions.LJLeft.AddDefaultBinding(Key.A);
        actions.LJRight.AddDefaultBinding(Key.D);
        actions.LJDown.AddDefaultBinding(Key.S);
        actions.LJUp.AddDefaultBinding(Key.W);

        actions.L3.AddDefaultBinding(Key.Key4);
        actions.R3.AddDefaultBinding(Key.Key5);

        actions.Start.AddDefaultBinding(Key.Escape);
        actions.Select.AddDefaultBinding(Key.Tab);

        actions.MouseWheelUp.AddDefaultBinding(Mouse.PositiveScrollWheel);
        actions.MouseWheelDown.AddDefaultBinding(Mouse.NegativeScrollWheel);

        //GAME IN GAME ACTIONS
        actions.ThrowHook.AddDefaultBinding(Mouse.LeftButton);
        actions.RotateCameraHEMLeft.AddDefaultBinding(Key.LeftArrow);
        actions.RotateCameraHEMRight.AddDefaultBinding(Key.RightArrow);
        actions.RotateCameraHEMDown.AddDefaultBinding(Key.DownArrow);
        actions.RotateCameraHEMUp.AddDefaultBinding(Key.UpArrow);
        actions.ZoomIn.AddDefaultBinding(Key.Shift);
        actions.HousingMoveUp.AddDefaultBinding(Key.Space);
        actions.HousingMoveDown.AddDefaultBinding(Key.F);
        actions.HousingRotateFurnitureClockwise.AddDefaultBinding(Key.E);
        actions.HousingRotateFurnitureCounterClockwise.AddDefaultBinding(Key.Q);
        actions.HousingPickFurniture.AddDefaultBinding(Key.Space);
        actions.HousingSwitchFurnitureMenu.AddDefaultBinding(Key.T);
    }

    public static PlayerActions CreateDefaultBindings()
    {
        var actions = new PlayerActions();
        SetUpKeyboardAndMouseActions(ref actions);

        SetUpXboxControllerActions(ref actions);

        return actions;
    }

    public static PlayerActions CreateDefaultMenuBindings(float _deadzone, float _turboFrequency)
    {
        var actions = new PlayerActions();

        //KEYBOARD AND MOUSE
        actions.controlsType = InputDeviceClass.Keyboard;

        actions.A.AddDefaultBinding(Key.Space);
        actions.A.AddDefaultBinding(Key.Return);
        actions.B.AddDefaultBinding(Key.Escape);

        actions.LJUp.AddDefaultBinding(Key.W);
        actions.LJDown.AddDefaultBinding(Key.S);
        actions.LJLeft.AddDefaultBinding(Key.A);
        actions.LJRight.AddDefaultBinding(Key.D);
        actions.LJUp.AddDefaultBinding(Key.UpArrow);
        actions.LJDown.AddDefaultBinding(Key.DownArrow);
        actions.LJLeft.AddDefaultBinding(Key.LeftArrow);
        actions.LJRight.AddDefaultBinding(Key.RightArrow);

        actions.Start.AddDefaultBinding(Key.Escape);
        actions.Start.AddDefaultBinding(Key.Return);
        //actions.Start.AddDefaultBinding(Key.Space);
        actions.MouseWheelUp.AddDefaultBinding(Mouse.PositiveScrollWheel);
        actions.MouseWheelDown.AddDefaultBinding(Mouse.NegativeScrollWheel);


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
        actions.LJUp.AddDefaultBinding(InputControlType.DPadUp);
        actions.LJDown.AddDefaultBinding(InputControlType.DPadDown);
        actions.LJLeft.AddDefaultBinding(InputControlType.DPadLeft);
        actions.LJRight.AddDefaultBinding(InputControlType.DPadRight);

        actions.R3.AddDefaultBinding(InputControlType.RightStickButton);
        actions.L3.AddDefaultBinding(InputControlType.LeftStickButton);

        actions.Start.AddDefaultBinding(InputControlType.Start);


        actions.leftJoystickAsButtons = new EAITwoAxisControls(actions.LeftJoystick, _deadzone, 0.4f, _turboFrequency);
        actions.EAIMouseWheel = new EAIMouseWheelControls(actions.MouseWheel);

        return actions;
    }

    public static PlayerActions CreateWithKeyboardBindings()
    {
        var actions = new PlayerActions();

        SetUpKeyboardAndMouseActions(ref actions);

        return actions;
    }

    public static PlayerActions CreateWithJoystickBindings()
    {
        var actions = new PlayerActions();

        SetUpXboxControllerActions(ref actions);

        return actions;
    }

    public bool AnyButtonWasPressed()
    {
        //RJUp.WasPressed || RJLeft.WasPressed || RJRight.WasPressed || RJDown.WasPressed || LJUp.WasPressed || LJDown.WasPressed || LJLeft.WasPressed || LJRight.WasPressed
        if (R1.WasPressed || R2.WasPressed || L1.WasPressed || L2.WasPressed || A.WasPressed || B.WasPressed || X.WasPressed || Y.WasPressed ||
            (RightJoystick.X > 0.2f || RightJoystick.X < -0.2f) || (RightJoystick.Y > 0.2f || RightJoystick.Y < -0.2f) || (LeftJoystick.X > 0.2f || LeftJoystick.X < -0.2f) ||
            (LeftJoystick.Y > 0.2f || LeftJoystick.Y < -0.2f) || R3.WasPressed || L3.WasPressed || Start.WasPressed)
        {
            //if (InputManager.AnyKeyIsPressed && (InputManager.ActiveDevice == this.ActiveDevice) || (this.ActiveDevice == null && Input.anyKeyDown))
            //{
            if (LeftJoystick.X != 0)
            {
                Debug.Log("LeftJoystick.X = " + LeftJoystick.X);
            }
            return true;
            //}
        }
        return false;
    }        
}

public class EAITwoAxisControls
{
    PlayerTwoAxisAction joyStick;
    float deadzone;
    float timeToTurbo = 0.4f;
    float turboFrequency = 0.1f;

    public bool leftIsPressed = false;
    public bool rightIsPressed = false;
    public bool downIsPressed = false;
    public bool upIsPressed = false;

    int lastPressedDir = -1; //0 -> up; 1 -> right; 2 -> down; 3 -> left

    public bool LeftWasPressed
    {
        get
        {
            bool result = joyStick.X < -deadzone && (!leftIsPressed || (leftIsPressed && turboStarted && turboPressedTime >= turboFrequency) && (!diagonal || (diagonal && lastPressedDir != 3)));
            if (result)
            {
                turboPressedTime = 0;
                leftIsPressed = true;
                lastPressedDir = 3;
            }
            return result;
        }
    }
    public bool RightWasPressed
    {
        get
        {
            bool result = joyStick.X > deadzone && (!rightIsPressed || (rightIsPressed && turboStarted && turboPressedTime >= turboFrequency) && (!diagonal || (diagonal && lastPressedDir != 1)));
            if (result)
            {
                turboPressedTime = 0;
                rightIsPressed = true;
                lastPressedDir = 1;
            }
            return result;
        }
    }
    public bool DownWasPressed
    {
        get
        {
            bool result = joyStick.Y < -deadzone && (!downIsPressed || (downIsPressed && turboStarted && turboPressedTime >= turboFrequency) && (!diagonal || (diagonal && lastPressedDir != 2)));
            if (result)
            {
                turboPressedTime = 0;
                downIsPressed = true;
                lastPressedDir = 2;
            }
            return result;
        }
    }
    public bool UpWasPressed
    {
        get
        {
            bool result = joyStick.Y > deadzone && (!upIsPressed || (upIsPressed && turboStarted && turboPressedTime >= turboFrequency) && (!diagonal || (diagonal && lastPressedDir != 0)));
            if (result)
            {
                turboPressedTime = 0;
                upIsPressed = true;
                lastPressedDir = 0;
            }
            //Debug.Log("result = " + result+"; turboPressedTime = " + turboPressedTime);
            return result;
        }
    }

    bool diagonal
    {
        get
        {
            int counter = 0;
            if (joyStick.X < -deadzone) counter++;
            if (joyStick.X > deadzone) counter++;
            if (joyStick.Y < -deadzone) counter++;
            if (joyStick.Y > deadzone) counter++;
            if (counter > 1) Debug.LogWarning("Diagonal! leftIsPressed = " + leftIsPressed + "; rightIsPressed = " + rightIsPressed + "; downIsPressed = " + downIsPressed + "; upIsPressed = " + upIsPressed);
            return counter > 1;
        }
    }

    bool turboStarted = false;

    float turboPressedTime = 0;

    public EAITwoAxisControls(PlayerTwoAxisAction _joyStick, float _deadzone = 0.2f, float _timeToTurbo = 0.4f, float _turboFrequency = 0.1f)
    {
        joyStick = _joyStick;
        deadzone = _deadzone;
        timeToTurbo = _timeToTurbo;
        turboFrequency = _turboFrequency;
    }

    /// <summary>
    /// IMPORTANT to do it ALWAYS, and at the END of the Update.
    /// </summary>
    public void ResetJoyStick()
    {
        if (joyStick.X > -deadzone && leftIsPressed)
        {
            leftIsPressed = false;
            //leftTurboStarted = false;
        }
        if (joyStick.X < deadzone && rightIsPressed)
        {
            rightIsPressed = false;
            //rightTurboStarted = false;
        }
        if (joyStick.Y > -deadzone && downIsPressed)
        {
            downIsPressed = false;
            //downTurboStarted = false;
        }
        if (joyStick.Y < deadzone && upIsPressed)
        {
            upIsPressed = false;
            //upTurboStarted = false;
        }

        //TURBO
        Vector2 input = new Vector2(joyStick.X, joyStick.Y);
        if (input.magnitude < deadzone)
        {
            turboStarted = false;
        }
        else if (leftIsPressed || rightIsPressed || downIsPressed || upIsPressed)
        {
            turboPressedTime += Time.deltaTime;
            if (!turboStarted && turboPressedTime >= timeToTurbo)
            {
                turboStarted = true;
                turboPressedTime = 0;
            }
        }
    }
}

public class EAIMouseWheelControls
{
    PlayerOneAxisAction joyStick;

    float value
    {
        get
        {
            return joyStick.Value * 10;
        }
    }

    public bool NegWasPressed
    {
        get
        {
            return value < 0;
        }
    }
    public bool PosWasPressed
    {
        get
        {
            return value > 0;
        }
    }
    public bool NegWasPressedGreatly
    {
        get
        {
            return value ==-1;
        }
    }
    public bool PosWasPressedGreatly
    {
        get
        {
            return value == 1;
        }
    }


    public EAIMouseWheelControls(PlayerOneAxisAction _joyStick)
    {
        joyStick = _joyStick;
    }
}

public class EAIButtonControls
{
    PlayerAction button;

    bool isPressed = false;

    bool turboMode = false;
    float timeToTurbo = 0.4f;
    float turboFrequency = 0.1f;
    float turboPressedTime = 0;

    public bool WasPressed
    {
        get
        {
            bool result = false;

            if (button.IsPressed && (!isPressed || (isPressed && turboMode && turboPressedTime >= turboFrequency)))
            {
                turboPressedTime = 0;
                isPressed = true;
                result = true;
            }

            return result;
        }
    }

    public EAIButtonControls(PlayerAction _button, float _timeToTurbo = 0.4f, float _turboFrequency = 0.1f)
    {
        button = _button;
        timeToTurbo = _timeToTurbo;
        turboFrequency = _turboFrequency;
    }

    /// <summary>
    /// IMPORTANT to do it ALWAYS, and at the END of the Update.
    /// </summary>
    public void ResetButton()
    {
        if (!button.IsPressed && isPressed)
        {
            turboPressedTime = 0;
            turboMode = false;
            isPressed = false;
        }

        if (isPressed)
        {
            turboPressedTime += Time.deltaTime;
            if (turboPressedTime >= timeToTurbo && !turboMode)
            {

                turboMode = true;
            }
        }
    }
}

//public class TwoAxisButtons
//{
//    public PlayerTwoAxisAction twoAxis;
//    public float deadzone = 0.5f;
//    public ButtonForTwoAxisButtons left;
//    public ButtonForTwoAxisButtons right;
//    public ButtonForTwoAxisButtons up;
//    public ButtonForTwoAxisButtons down;
//    public TwoAxisButtons(PlayerTwoAxisAction _twoAxis, float _deadzone, float _turboFrequency)
//    {
//        twoAxis = _twoAxis;
//        deadzone= _deadzone;

//        left = new ButtonForTwoAxisButtons(AxisDir.Left, twoAxis, deadzone, _turboFrequency);
//        right = new ButtonForTwoAxisButtons(AxisDir.Right, twoAxis, deadzone, _turboFrequency);
//        up = new ButtonForTwoAxisButtons(AxisDir.Up, twoAxis, deadzone, _turboFrequency);
//        down = new ButtonForTwoAxisButtons(AxisDir.Down, twoAxis, deadzone, _turboFrequency);
//    }

//}

//public enum AxisDir
//{
//    none,
//    Right,
//    Left,
//    Up,
//    Down
//}

//public class ButtonForTwoAxisButtons
//{
//    public AxisDir axisDir;
//    public PlayerTwoAxisAction twoAxis;
//    public float deadzone = 0.5f;

//    public bool isPressed
//    {
//        get
//        {
//            //bool result = false;
//            bool result = false;
//            switch (axisDir)
//            {
//                case AxisDir.Right:
//                    if (twoAxis.X >= deadzone)
//                    {
//                        lastDirPressed = axisDir;
//                        result = true;
//                    }
//                    return result;
//                case AxisDir.Left:
//                    if (twoAxis.X <= -deadzone)
//                    {
//                        lastDirPressed = axisDir;
//                        result = true;
//                    }
//                    return result;
//                case AxisDir.Up:
//                    if (twoAxis.Y >= deadzone)
//                    {
//                        lastDirPressed = axisDir;
//                        result = true;
//                    }
//                    return result;
//                case AxisDir.Down:
//                    if (twoAxis.Y <= -deadzone)
//                    {
//                        lastDirPressed = axisDir;
//                        result = true;
//                    }
//                    return result;
//            }
//            return false;
//        }
//    }
//    float turboModeFreq = 0.1f;
//    float timeToStartTurboMode = 0.6f;
//    float currentTurboModeFreq = 0.6f;

//    AxisDir lastDirPressed = AxisDir.none;

//    public bool hasBeenPressed = false;
//    public bool wasPressed
//    {
//        get
//        {
//            bool result = false;
//            if(!hasBeenPressed && isPressed)
//            {
//                hasBeenPressed = true;
//                hasBeenReleased = false;
//                result = true;
//            }else if(hasBeenPressed && !isPressed)
//            {
//                hasBeenPressed = false;
//            }
//            return result;
//        }
//    }
//    float wasPressedTime = 0;

//    public bool hasBeenReleased = false;
//    public bool wasReleased
//    {
//        get
//        {
//            bool result = false;
//            if (!hasBeenReleased && !isPressed)
//            {
//                hasBeenPressed = false;
//                hasBeenReleased = true;
//                result = true;
//            }
//            else if (hasBeenReleased && isPressed)
//            {
//                hasBeenReleased = false;
//            }
//            return result;
//        }
//    }

//    public bool wasPressedLong
//    {
//        get
//        {
//            //Debug.Log("was Pressed Long: Start");
//            if (isPressed)
//            {
//                //Debug.Log("was Pressed Long "+ axisDir + ": is pressed. wasPressedTime = "+ wasPressedTime.ToString("F4"));
//                wasPressedTime += Time.fixedDeltaTime;
//            }
//            else
//            {
//                wasPressedTime = 0;
//                if (currentTurboModeFreq != timeToStartTurboMode)
//                {
//                    //Debug.Log("was Pressed Long "+axisDir+": stop being pressed");
//                    currentTurboModeFreq = timeToStartTurboMode;
//                }
//            }

//            if (wasPressedTime >= currentTurboModeFreq)
//            {
//                //Debug.Log("was Pressed Long "+ axisDir+": is pressed and input true");
//                wasPressedTime = 0;
//                if(currentTurboModeFreq != turboModeFreq)
//                    currentTurboModeFreq = turboModeFreq;
//                return true;
//            }
//            return false;
//        }
//    }


//    public ButtonForTwoAxisButtons(AxisDir _axisDir, PlayerTwoAxisAction _twoAxis, float _deadzone, float _turboFrequency)
//    {
//        axisDir = _axisDir;
//        twoAxis = _twoAxis;
//        deadzone = _deadzone;
//        currentTurboModeFreq = timeToStartTurboMode;
//        wasPressedTime = 0;
//    }
//}