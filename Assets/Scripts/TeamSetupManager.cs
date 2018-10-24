using System.Collections.Generic;
using UnityEngine;
using InControl;
using UnityEngine.SceneManagement;

public class TeamSetupManager : MonoBehaviour
{
	//public static List<PlayerActions> playerActionsList = new List<PlayerActions>();
	public GameObject playerPrefab;

	public string SiguenteEscena;

	const int maxPlayers = 4;

	List<Vector3> playerPositions = new List<Vector3>() {
		new Vector3( -2, 2, 0 ),
		new Vector3( 2, 2, 0 ),
		new Vector3( -2, -2, 0 ),
		new Vector3( 2, -2, 0 ),
	};

	List<PlayerSelected> players = new List<PlayerSelected>( maxPlayers );

	PlayerActions keyboardListener;
	PlayerActions joystickListener;

	void OnEnable()
	{
		InputManager.OnDeviceDetached += OnDeviceDetached;
		keyboardListener = PlayerActions.CreateWithKeyboardBindings();
		joystickListener = PlayerActions.CreateWithJoystickBindings();
	}


	void OnDisable()
	{
		InputManager.OnDeviceDetached -= OnDeviceDetached;
		joystickListener.Destroy();
		keyboardListener.Destroy();
	}


	void Update()
	{
		if (JoinButtonWasPressedOnListener( joystickListener ))
		{
			InputDevice inputDevice = InputManager.ActiveDevice;

			if (ThereIsNoPlayerUsingJoystick( inputDevice ))
			{
				CreatePlayer( inputDevice );
			}
		}

		if (JoinButtonWasPressedOnListener( keyboardListener ))
		{
			if (ThereIsNoPlayerUsingKeyboard())
			{
				CreatePlayer( null );
			}
		}

		if (players.Count > 1){
			int contador = 0;
			foreach(PlayerSelected ps in players){
				if (ps.Ready){
					contador++;
				}
			}
			if (contador == players.Count){
				//GameInfo.playerActionsList = new PlayerActions[players.Count];
				//GameInfo.playerActionsList = new List<PlayerActions>();
				//for(int i = 0; i < players.Count; i++){
				foreach(PlayerSelected ps in players){
					//GameInfo.playerActionsList[i] = players[i].Actions;
					GameInfo.instance.playerActionsList.Add(ps.Actions);
					//Debug.Log(ps.Actions);
				}
                GameInfo.instance.nPlayers = players.Count;
				SceneManager.LoadScene(SiguenteEscena);
			}
		}
	}


	bool JoinButtonWasPressedOnListener( PlayerActions actions )
	{
		return actions.Attack1.WasPressed || actions.Attack2.WasPressed || actions.Attack3.WasPressed;
	}


	PlayerSelected FindPlayerUsingJoystick( InputDevice inputDevice )
	{
		int playerCount = players.Count;
		for (int i = 0; i < playerCount; i++)
		{
			PlayerSelected player = players[i];
			if (player.Actions.Device == inputDevice)
			{
				return player;
			}
		}

		return null;
	}


	bool ThereIsNoPlayerUsingJoystick( InputDevice inputDevice )
	{
		return FindPlayerUsingJoystick( inputDevice ) == null;
	}


	PlayerSelected FindPlayerUsingKeyboard()
	{
		int playerCount = players.Count;
		for (int i = 0; i < playerCount; i++)
		{
			PlayerSelected player = players[i];
			if (player.Actions == keyboardListener)
			{
				return player;
			}
		}

		return null;
	}


	bool ThereIsNoPlayerUsingKeyboard()
	{
		return FindPlayerUsingKeyboard() == null;
	}

 
	void OnDeviceDetached( InputDevice inputDevice )
	{
		PlayerSelected player = FindPlayerUsingJoystick( inputDevice );
		if (player != null)
		{
			RemovePlayer( player );
		}
	}


	PlayerSelected CreatePlayer( InputDevice inputDevice )
	{
		if (players.Count < maxPlayers)
		{
			// Pop a position off the list. We'll add it back if the player is removed.
			Vector3 playerPosition = playerPositions[0];
			playerPositions.RemoveAt( 0 );

			GameObject gameObject = Instantiate( playerPrefab, playerPosition, Quaternion.identity );
			PlayerSelected player = gameObject.GetComponent<PlayerSelected>();

			if (inputDevice == null)
			{
                // We could create a new instance, but might as well reuse the one we have
                // and it lets us easily find the keyboard player.

                //GameInfo.instance.playerActionsList.Add(keyboardListener);
                //GameInfo.instance.playerActionUno = keyboardListener;
                player.Actions = PlayerActions.CreateWithKeyboardBindings();
			}
			else
			{
				// Create a new instance and specifically set it to listen to the
				// given input device (joystick).
				PlayerActions actions = PlayerActions.CreateWithJoystickBindings();
				actions.Device = inputDevice;

                //GameInfo.instance.playerActionsList.Add(actions);
                player.Actions = actions;
			}

			players.Add( player );

			return player;
		}

		return null;
	}


	void RemovePlayer( PlayerSelected player )
	{
		playerPositions.Insert( 0, player.transform.position );
		players.Remove( player );
		player.Actions = null;
		Destroy( player.gameObject );
	}


	void OnGUI()
	{
		const float h = 22.0f;
		float y = 10.0f;

		GUI.Label( new Rect( 10, y, 300, y + h ), "Active players: " + players.Count + "/" + maxPlayers );
		y += h;

		if (players.Count < maxPlayers)
		{
			GUI.Label( new Rect( 10, y, 300, y + h ), "Press a button or a/s/d/f key to join!" );
			y += h;
		}
	}
}
