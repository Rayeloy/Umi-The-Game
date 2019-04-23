using InControl;


public class PlayerActions : PlayerActionSet
{
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
        R2 = CreatePlayerAction( "R2" );
        L1 = CreatePlayerAction("L1");
        L2 = CreatePlayerAction( "L2" );

        A = CreatePlayerAction("A");
        X = CreatePlayerAction( "X" );
		Y = CreatePlayerAction( "Y" );
		B = CreatePlayerAction( "B" );

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

		R3 = CreatePlayerAction("R3");
		L3 = CreatePlayerAction("L3");

        Start = CreatePlayerAction ("Start");
	}
	public static PlayerActions CreateWithKeyboardBindings()
	{
		var actions = new PlayerActions();

        actions.R1.AddDefaultBinding(Key.T);
        actions.R2.AddDefaultBinding( Key.LeftShift );
        actions.L1.AddDefaultBinding(Key.G);
        actions.L2.AddDefaultBinding( Mouse.RightButton);

        actions.A.AddDefaultBinding(Key.Space);
        actions.X.AddDefaultBinding( Key.Key1 );
		actions.Y.AddDefaultBinding( Key.Key2 );
		actions.B.AddDefaultBinding( Key.Key3 );

		actions.RJUp.AddDefaultBinding( Key.W );
		actions.RJDown.AddDefaultBinding( Key.S );
		actions.RJLeft.AddDefaultBinding( Key.A );
		actions.RJRight.AddDefaultBinding( Key.D );

		actions.LJUp.AddDefaultBinding( Mouse.PositiveY );
		actions.LJDown.AddDefaultBinding( Mouse.NegativeY );
		actions.LJLeft.AddDefaultBinding( Mouse.NegativeX );
		actions.LJRight.AddDefaultBinding( Mouse.PositiveX );

		actions.L3.AddDefaultBinding( Key.Key4 );
		actions.R3.AddDefaultBinding( Key.Key5 );

		actions.Start.AddDefaultBinding ( Key.Escape );
		return actions;
	}
	public static PlayerActions CreateWithJoystickBindings()
	{
		var actions = new PlayerActions();

        actions.R1.AddDefaultBinding(InputControlType.RightBumper);
        actions.R2.AddDefaultBinding( InputControlType.RightTrigger );
        actions.L1.AddDefaultBinding(InputControlType.LeftBumper);
        actions.L2.AddDefaultBinding( InputControlType.LeftTrigger );

        actions.A.AddDefaultBinding(InputControlType.Action1);
        actions.X.AddDefaultBinding( InputControlType.Action3 );
		actions.Y.AddDefaultBinding( InputControlType.Action4 );
		actions.B.AddDefaultBinding( InputControlType.Action2 );

		actions.RJUp.AddDefaultBinding( InputControlType.LeftStickUp );
		actions.RJDown.AddDefaultBinding( InputControlType.LeftStickDown );
		actions.RJLeft.AddDefaultBinding( InputControlType.LeftStickLeft );
		actions.RJRight.AddDefaultBinding( InputControlType.LeftStickRight );

		actions.LJUp.AddDefaultBinding( InputControlType.RightStickUp );
		actions.LJDown.AddDefaultBinding( InputControlType.RightStickDown );
		actions.LJLeft.AddDefaultBinding( InputControlType.RightStickLeft );
		actions.LJRight.AddDefaultBinding( InputControlType.RightStickRight );

		actions.R3.AddDefaultBinding ( InputControlType.RightStickButton );
		actions.L3.AddDefaultBinding ( InputControlType.LeftStickButton );

		actions.Start.AddDefaultBinding (InputControlType.Start);
		return actions;
	}
}