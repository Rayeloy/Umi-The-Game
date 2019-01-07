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

public class TeamSetupManager_Online : MonoBehaviourPunCallbacks//, IPunObservable
{
    #region Variables

    //public static List<PlayerActions> playerActionsList = new List<PlayerActions>();
    public static TeamSetupManager_Online instance;
    
    [Tooltip("Prefab used to represent the player")]
    public GameObject playerPrefab;
    public List<Transform> playerPositions = new List<Transform>();
    public PlayerSelecionUI[] pUI;

	PlayerActions keyboardListener;
	PlayerActions joystickListener;

	public float tiempoParaReady = 10.0f;

	[Header("Referencias")]
	public Animator animator;
	public GameObject ReadyButton;

    //Juan: Los siguientes valores almacenan la información sobre si el jugador está o no listo para comenzar la partida
    [HideInInspector]
    bool myPlayerIsReady = false;
    [HideInInspector]
    bool areAllPlayersReady = false;
    [HideInInspector]
    bool myPlayerIsSet = false;
    

    //Juan: Lista de los objetos jugador en la escena para activarlos o desactivarlos.
    List<PlayerSelected> players = new List<PlayerSelected>(4);

    //Juan: los siguientes valores guardan información al respecto de la cantidad máxima de jugadores posibles y la cantidad de jugadores conectados actualmente en la sala
    int maxPlayers = 4;
    [SerializeField]
    private const int maxPlayersInspector = 4;

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

    int entro = 0;
	void Update()
	{
        if (PhotonNetwork.CurrentRoom.MaxPlayers != null)
        {
            ///Juan:
            ///A veces en el awake no se hace correctamente y no sé porqué así que lo haremos en el update,
            ///en el fondo que se compruebe que esta variable no solo no es un problema sino que es hasta recomendable
            maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        }
        if (areAllPlayersReady){
			if (contador < tiempoParaReady){
				contador += Time.deltaTime;
				return;			
			}
            else
            {
                LoadGame();
            }
		}
        if (Input.anyKeyDown && !myPlayerIsSet)
        {
            myPlayerIsSet = true;
            CreatePlayerInNetwork(null);
        }
	}
    
    #endregion

    #region Pun Callbacks
     
    //Juan: no sé porqué pero no se llama a ninguno de estos métodos

    public override void OnLeftRoom()
    {
        /// volvemos al menú principal, el cual es la escena menu_online
        Debug.Log("TeamSetupManager: Nos salimos de la sala enfadaos");
        SceneManager.LoadScene(menuSceneName);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("TeamSetupManager: {0} ha entrado en la sala", other.NickName);
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
        {
            //Juan: ya estamos todos, esperamos un rato e iniciamos la partida
            Debug.LogFormat("TeamSetupManager: Hemos llegado al número máximo de jugadores que es {0}, estamos casi listos para iniciar la carga del juego desde el master que es {1}", PhotonNetwork.CurrentRoom.PlayerCount, PhotonNetwork.IsMasterClient);
        }
        CreatePlayerInNetwork(null);               
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

    void CreatePlayerInNetwork(InputDevice inputDevice)
    {
        
        Vector3 playerPosition = playerPositions[0].position;
        playerPositions.RemoveAt(0);

        GameObject gameObject = Instantiate(playerPrefab, playerPosition, Quaternion.identity);
        ///Juan: No necesita ser un player instanciado en la red ya que la información que se comparte es pequeña y sólo referente a si está listo para empezar el juego o no.
        PlayerSelected player = gameObject.GetComponent<PlayerSelected>();

        if (inputDevice == null)
        {
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

        players.Add(player);
        player.playerSelecionUI = pUI[players.Count - 1];
        pUI[players.Count - 1].panel.SetActive(true);        
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
            Debug.LogError("TeamSetupManager: We're not the master client so we load nothing");
            return;
        }
        Debug.LogFormat("TeamSetupManager: Loading game since we're {0} players connected", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel(gameSceneName);
    }


    private bool arePlayersReady(PhotonStream stream)
    {
        for (int i = 0; i < 3; i++)
        {
            if ((bool)stream.ReceiveNext() && myPlayerIsReady && i==2)// haremos peticiones hasta recibir 3 true seguidos además del nuestro
            {
                Debug.Log("TeamSetupManager: All players are ready to begin the game");
                return true;              
            }
            else
            {
                Debug.Log("TeamSetupManager: not all players are ready to begin the game");
                return false;
            }
        }
        return false;
    }

    #endregion


    #region IPUNObservable imp

    /// <summary>
    /// Called by PUN several times per second, so that your script can write and read synchronization data for the PhotonView.
    /// </summary>
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            ///The isWriting property will be true if this client is the "owner" of the PhotonView (and thus the GameObject).
            ///Add data to the stream and it's sent via the server to the other players in a room.
            ///On the receiving side, isWriting is false and the data should be read.

            //Juan: En tal caso en este lado del If manejaremos los datos que deseamos enviar a otros jugadores
            if (myPlayerIsReady)
            {
                stream.SendNext(true);
                Debug.Log("TeamSetupManager: I'm sending that I'm ready to begin");
                // Juan: enviaremos la información sólo si estamos listos así no sobrecargamos la red
                areAllPlayersReady = arePlayersReady(stream);
            }
        }
        else
        {
            //Juan: Y en este lado daremos valor a las cosas que recibimos
            areAllPlayersReady = arePlayersReady(stream);
        }
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
