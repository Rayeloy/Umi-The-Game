using InControl;


public class PlayerActions : PlayerActionSet
{
	public PlayerAction Boost;
	public PlayerAction Jump;
	public PlayerAction Aim;
	public PlayerAction UsePickup;

	public PlayerAction Attack1;
	public PlayerAction Attack2;
	public PlayerAction Attack3;

	public PlayerAction Left;
	public PlayerAction Right;
	public PlayerAction Up;
	public PlayerAction Down;
	public PlayerTwoAxisAction Movement;

	public PlayerAction CamLeft;
	public PlayerAction CamRight;
	public PlayerAction CamUp;
	public PlayerAction CamDown;
	public PlayerTwoAxisAction CamMovement;

	public PlayerAction R3;
	public PlayerAction L3;

	public PlayerAction Options;

	public PlayerActions()
	{
		Boost = CreatePlayerAction( "Boost" );
		Jump = CreatePlayerAction( "Jump" );
		Aim = CreatePlayerAction( "Aim" );
		UsePickup = CreatePlayerAction( "UsePickup" );

		Attack1 = CreatePlayerAction( "Attack1" );
		Attack2 = CreatePlayerAction( "Attack2" );
		Attack3 = CreatePlayerAction( "Attack3" );

		Left = CreatePlayerAction( "Left" );
		Right = CreatePlayerAction( "Right" );
		Up = CreatePlayerAction( "Up" );
		Down = CreatePlayerAction( "Down" );
		Movement = CreateTwoAxisPlayerAction( Left, Right, Down, Up );

		CamLeft = CreatePlayerAction( "CamLeft" );
		CamRight = CreatePlayerAction( "CamRight" );
		CamUp = CreatePlayerAction( "CamUp" );
		CamDown = CreatePlayerAction( "CamDown" );
		CamMovement = CreateTwoAxisPlayerAction( CamLeft, CamRight, CamDown, CamUp );

		R3 = CreatePlayerAction("R3");
		L3 = CreatePlayerAction("L3");	

		Options = CreatePlayerAction ( "Options" );
	}
	public static PlayerActions CreateWithKeyboardBindings()
	{
		var actions = new PlayerActions();

		actions.Boost.AddDefaultBinding( Key.Q );
		actions.Jump.AddDefaultBinding( Key.Space );
		actions.Aim.AddDefaultBinding( Key.E );
		actions.UsePickup.AddDefaultBinding( Mouse.LeftButton);

		actions.Attack1.AddDefaultBinding( Key.Key1 );
		actions.Attack2.AddDefaultBinding( Key.Key2 );
		actions.Attack3.AddDefaultBinding( Key.Key3 );

		actions.Up.AddDefaultBinding( Key.W );
		actions.Down.AddDefaultBinding( Key.S );
		actions.Left.AddDefaultBinding( Key.A );
		actions.Right.AddDefaultBinding( Key.D );

		actions.CamUp.AddDefaultBinding( Mouse.PositiveY );
		actions.CamDown.AddDefaultBinding( Mouse.NegativeY );
		actions.CamLeft.AddDefaultBinding( Mouse.NegativeX );
		actions.CamRight.AddDefaultBinding( Mouse.PositiveX );

		actions.L3.AddDefaultBinding( Key.Key4 );
		actions.R3.AddDefaultBinding( Key.Key5 );

		actions.Options.AddDefaultBinding ( Key.Escape );
		return actions;
	}
	public static PlayerActions CreateWithJoystickBindings()
	{
		var actions = new PlayerActions();
		actions.Boost.AddDefaultBinding( InputControlType.RightTrigger );
		actions.Jump.AddDefaultBinding( InputControlType.Action1 );
		actions.Aim.AddDefaultBinding( InputControlType.LeftTrigger );
		actions.UsePickup.AddDefaultBinding( InputControlType.LeftBumper);

		actions.Attack1.AddDefaultBinding( InputControlType.Action3 );
		actions.Attack2.AddDefaultBinding( InputControlType.Action4 );
		actions.Attack3.AddDefaultBinding( InputControlType.Action2 );

		actions.Up.AddDefaultBinding( InputControlType.LeftStickUp );
		actions.Down.AddDefaultBinding( InputControlType.LeftStickDown );
		actions.Left.AddDefaultBinding( InputControlType.LeftStickLeft );
		actions.Right.AddDefaultBinding( InputControlType.LeftStickRight );

		actions.CamUp.AddDefaultBinding( InputControlType.RightStickUp );
		actions.CamDown.AddDefaultBinding( InputControlType.RightStickDown );
		actions.CamLeft.AddDefaultBinding( InputControlType.RightStickLeft );
		actions.CamRight.AddDefaultBinding( InputControlType.RightStickRight );

		actions.R3.AddDefaultBinding ( InputControlType.RightStickButton );
		actions.L3.AddDefaultBinding ( InputControlType.LeftStickButton );

		actions.Options.AddDefaultBinding (InputControlType.Start);
		return actions;
	}
}