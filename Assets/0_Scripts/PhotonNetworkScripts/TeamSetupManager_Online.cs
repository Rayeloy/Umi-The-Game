using System.Collections.Generic;
using UnityEngine;
using InControl;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Juan:
/// Esta clase se va a encargar de hacer el equipo y esperar hasta que la "room" del servidor esté llena antes de cargar el juego real
/// a todos los efectos actúa como el launcher hasta que sepamos el número exacto de jugadores que van a participar en el juego
/// </summary>

public class TeamSetupManager_Online : MonoBehaviourPunCallbacks
{
    #region Variables

    //public static List<PlayerActions> playerActionsList = new List<PlayerActions>();
    public static TeamSetupManager_Online instance;
    
    [Tooltip("Prefab used to represent the player")]
    public GameObject playerPrefab;
    
    public static string SiguenteEscena;
    public static bool startFromMap = false;
    public string nextScene;
	private bool ready = false;
    

	public List<Transform> playerPositions = new List<Transform>();

	List<PlayerSelected> players = new List<PlayerSelected>(4);

	public PlayerSelecionUI[] pUI;

	PlayerActions keyboardListener;
	PlayerActions joystickListener;

	public float tiempoParaReady = 10.0f;

	[Header("Referencias")]
	public Animator animator;
	public GameObject ReadyButton;

    //Juan: mis variables
    [HideInInspector]
    bool allPlayersReady = false;
    bool myPlayerIsReady = false;

    //Juan: los siguientes valores guardan información al respecto de la cantidad máxima de jugadores posibles y la cantidad de jugadores conectados actualmente en la sala
    [HideInInspector]
    int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

    //Juan: los siguientes valores contienen información sobre las escenas del juego y los menús
    [Tooltip("Nombre de la escena del juego, default = 'FINAL_Flag_Online'")]
    public string gameSceneName = "FINAL_Flag_Online";
    [Tooltip("Nombre de la escena del menú, default = 'Menus_Online'")]
    public string menuSceneName = "Menus_Online";

    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        instance = this;
        maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
    }


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

	private float contador = 0;
	void Update()
	{
		if (ready){
			if (contador < tiempoParaReady){
				contador += Time.deltaTime;
				return;			
			}

			ReadyButton.SetActive(true);
			foreach(PlayerSelected ps in players){
				if (ps.Actions.Jump.WasPressed){
					if (startFromMap)
					{
						SceneManager.LoadScene(SiguenteEscena);
					}
					else
					{
						SceneManager.LoadScene(nextScene);
					}
				}
			}
			return;
		}

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

		if (players.Count >=4){ //minNumPlayers){
			int contador = 0;
			foreach(PlayerSelected ps in players){
				if (ps.Ready){
					contador++;
				}
			}
			if (contador == players.Count){
				ready = true;
				//GameInfo.playerActionsList = new PlayerActions[players.Count];
				//GameInfo.playerActionsList = new List<PlayerActions>();
				//for(int i = 0; i < players.Count; i++){
				foreach(PlayerSelected ps in players){
					//GameInfo.playerActionsList[i] = players[i].Actions;
					GameInfo.instance.playerActionsList.Add(ps.Actions);
					GameInfo.instance.playerTeamList.Add(ps.team);
					//Debug.Log(ps.Actions);
				}
                GameInfo.instance.nPlayers = players.Count;
				if (SiguenteEscena != "Tutorial")
					animator.SetBool("Ready", true);
				else{
					if (startFromMap)
					{
						SceneManager.LoadScene(SiguenteEscena);
					}
					else
					{
						SceneManager.LoadScene(nextScene);
					}
				}
			}
		}
	}


    //Juan: esto queda comentado por ahora pero aquí se debe producir de forma ordenada la instanciación de los jugadores que ya están en la sala
    /*
    public void Start()
    {
        if (playerPrefab == null)
        {
            Debug.Log("<Color=Red><a>Missing playerprefab reference</a></color>: Script " + this + " is missing playerprefab property player can't be loaded in the room");
        }
        else
        {
            if (PlayerCombat_Online.LocalPlayerInstance == null)
            {
                Debug.LogFormat("Localplayer is being Instantiated in the scene {0}", SceneManagerHelper.ActiveSceneName);
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0); // Juan: cargamos el jugador por ahora en un spawn fijo no determinado
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }*/

    #endregion

    #region Pun Callbacks
        
    public override void OnLeftRoom()
    {
        /// volvemos al menú principal, el cual es la escena menu_online
        SceneManager.LoadScene(menuSceneName);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("{0} ha entrado en la sala", other.NickName);
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
        {
            //Juan: ya estamos todos, esperamos un rato e iniciamos la partida
            Debug.LogFormat("TeamSetupManager: Hemos llegado al número máximo de jugadores que es {0}, así que vamos a iniciar la carga del juego desde el master que es {1}", PhotonNetwork.CurrentRoom.PlayerCount, PhotonNetwork.IsMasterClient);
            LoadGame();
        }
        else
        {
            //Juan: aun no estamos todos, así que vamos a cargar al jugador que acaba de entrar para que salga en la selección de personaje :)
            
            SetPlayer();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        //esto deberá ser representado por la interfáz
        Debug.LogFormat("TeamSetupManager: {0} ha salido de la sala", other.NickName);        
    }

    #endregion

    #region Private Functions

    bool JoinButtonWasPressedOnListener( PlayerActions actions )
	{
		return actions.Attack1.WasPressed || actions.Attack2.WasPressed || actions.Attack3.WasPressed || (actions != keyboardListener && actions.Jump.WasPressed);
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

    #endregion

    #region Player Functions

    PlayerSelected CreatePlayer( InputDevice inputDevice )
	{
		if (PhotonNetwork.CurrentRoom.PlayerCount <= maxPlayers)
		{
			// Pop a position off the list. We'll add it back if the player is removed.
			Vector3 playerPosition = playerPositions[0].position;
			playerPositions.RemoveAt( 0 );

			GameObject gameObject = Instantiate( playerPrefab, playerPosition, Quaternion.identity );
            ///Juan: No necesita ser un player instanciado en la red ya que la información que se comparte es pequeña y sólo referente a si está listo para empezar el juego o no.
            
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
                player.isAReleased = false;
            }

			players.Add( player );
			player.playerSelecionUI = pUI[players.Count - 1];
			pUI[players.Count - 1].panel.SetActive(true);

			return player;
        }
        else
        {
            //Juan: qué haces en esta sala supuestamente llena?, fuera
            Debug.Log("TeamSetupManager: <color=Red><a>CRITICAL ERROR</a></color> you're trying to create a new player even if the room is full, you'll be kicked from the room");
            LeaveRoom();
        }

		return null;
	}

    void SetPlayer()
    {
        
    }


	void RemovePlayer( PlayerSelected player )
	{
		playerPositions.Insert( 0, player.transform );
		players.Remove( player );
		player.Actions = null;
		Destroy( player.gameObject );
	}

    #endregion

    #region Network Functions

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        //SceneManager.LoadScene(menuSceneName); Juan: Innecesario ya que se utiliza el evento "OnLeftRoom" que es llamado siempre que se sale de la sala y por ello es más limpio de usar que esta construcción
    }


    ///JUAN: cosas a tener en cuenta cuando usamos "loadgame" que implica cambiar a otra escena:
    ///
    ///Switching Scenes
    ///When you load a scene, Unity usually destroys all GameObjects currently in the hierarchy.This includes networked objects, which can be confusing at times.
    ///Example: In a menu scene, you join a room and load another.You might actually arrive in the room a bit too early and get the initial messages of the room. 
    ///PUN begins to instantiate networked objects but your logic loads another scene and they are gone.
    ///To avoid issues with loading scenes, you can set PhotonNetwork.automaticallySyncScene to true and use PhotonNetwork.LoadLevel() to switch scenes.
    ///
    ///·PhotonNetwork.LoadLevel() should only be called if we are the master. So we check first that we are the master using PhotonNetwork.isMasterClient.
    ///It will be the responsibility of the caller to also check for this, we'll cover that in the next part of this section.
    ///
    ///·We use PhotonNetwork.LoadLevel() to load the level we want, we don't use Unity directly, because we want to rely on Photon to load this level on
    ///all connected clients in the room, since we've enabled PhotonNetwork.automaticallySyncScene for this Game.

    public void LoadGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("TeamSetupManager: No somos el dueño de la sala");
        }
        Debug.LogFormat("TeamSetupManager: Cargando el juego ya que hemos alcanzado el máximo de jugadores por sala que es: {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel(gameSceneName);
    }
       
    #endregion
}


[System.Serializable]
public struct PlayerSelecionUI_Online
{
	public GameObject panel;
	public Image TeamSelect;
	public Image FlechaDerecha;
	public Image FlechaIzquierda;
	public TextMeshProUGUI AcctionsText;
}
