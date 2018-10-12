using InControl;


public class PlayerActions : PlayerActionSet
{
	public PlayerAction Boost;
	public PlayerAction Jump;
	public PlayerAction Aim;

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

	public PlayerActions()
	{
		Boost = CreatePlayerAction( "Boost" );
		Jump = CreatePlayerAction( "Jump" );
		Aim = CreatePlayerAction ( "Aim" );

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
	}
	public static PlayerActions CreateWithKeyboardBindings()
	{
		var actions = new PlayerActions();

		actions.Boost.AddDefaultBinding( Key.A );
		actions.Jump.AddDefaultBinding( Key.Space );
		actions.Aim.AddDefaultBinding( Key.S );

		actions.Attack1.AddDefaultBinding( Key.Q );
		actions.Attack2.AddDefaultBinding( Key.W );
		actions.Attack3.AddDefaultBinding( Key.E );

		actions.Up.AddDefaultBinding( Key.UpArrow );
		actions.Down.AddDefaultBinding( Key.DownArrow );
		actions.Left.AddDefaultBinding( Key.LeftArrow );
		actions.Right.AddDefaultBinding( Key.RightArrow );

		actions.CamUp.AddDefaultBinding( Mouse.PositiveY );
		actions.CamDown.AddDefaultBinding( Mouse.NegativeY );
		actions.CamLeft.AddDefaultBinding( Mouse.NegativeX );
		actions.CamRight.AddDefaultBinding( Mouse.PositiveX );
		return actions;
	}
	public static PlayerActions CreateWithJoystickBindings()
	{
		var actions = new PlayerActions();
		actions.Boost.AddDefaultBinding( InputControlType.RightTrigger );
		actions.Jump.AddDefaultBinding( InputControlType.Action1 );
		actions.Aim.AddDefaultBinding( InputControlType.LeftTrigger );

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
		return actions;
	}
}