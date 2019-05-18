// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UMILauncher.cs" company="AnotherCoffeeGames">
//   Made with: Photon Unity Networking Plugin
// </copyright>
// <summary>
//  Used in "UMI" to connect, and join/create room automatically
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class UMILauncher: MonoBehaviourPunCallbacks
{
    #region ----[ VARIABLES FOR DESIGNERS ]---- 
    [Tooltip("Panel que controla el nombre de usuario y el botón de login")]
    [SerializeField]
    private GameObject controlPanel;
    [Tooltip("Informa al usuario del proceso de conexión con este shittypanel")]
    [SerializeField]
    private GameObject LoadingPanel;

    [Tooltip("The maximum number of players per room default 4")]
    [SerializeField]
    private byte maxPlayersPerRoom = 4;


    [Tooltip("Nombre de la escena que debe cargar después de la conexión, esto en la iteración final debe ser el HUB. Default: Capture The Whale")]
    [SerializeField]
    private string hub_Name= "Capture The Whale";


    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    [Tooltip("NO TOCAR SI NO SABEH HULIO: Esto sirve para aislar builds en la misma cloud")]
    [SerializeField]
    string gameVersion = "1";
    #endregion

    #region ----[ PROPERTIES ]----
    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    [HideInInspector]
    bool isConnecting;
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    private void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        Debug.Log("UMILAUNCHER: Vamos a probar a sincronizar la escena.");
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect()
    {
        // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
        isConnecting = true;

        // hide the Play button for visual consistency
        controlPanel.SetActive(false);
        LoadingPanel.SetActive(true);
        

        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = this.gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    #endregion

    #region ----[ PUN CALLBACKS ]----
    // below, we implement some callbacks of PUN
    // you can find PUN's callbacks in the class MonoBehaviourPunCallbacks


    /// <summary>
    /// Called after the connection to the master is established and authenticated
    /// </summary>
    public override void OnConnectedToMaster()
    {
        // we don't want to do anything if we are not attempting to join a room. 
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting)
        {
            Debug.Log("UMILauncher: OnConnectedToMaster(). El cliente está conectado al servidor Master.\n Realizando llamada a: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");

            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
        }
    }

    /// <summary>
    /// Called when a JoinRandom() call failed. The parameter provides ErrorCode and message.
    /// </summary>
    /// <remarks>
    /// Most likely all rooms are full or no rooms are available. <br/>
    /// </remarks>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("UMILauncher:OnJoinRandomFailed(). No hemos encontrado ninguna room vacía o no existía ninguna en el servidor previamente por ello crearemos una.\nLlamando a: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom });
    }


    /// <summary>
    /// Called after disconnecting from the Photon server.
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("UMILauncher: Desconectando");

        // #Critical: we failed to connect or got disconnected. There is not much we can do. Typically, a UI system should be in place to let the user attemp to connect again.
        isConnecting = false;
        controlPanel.SetActive(true);

    }

    /// <summary>
    /// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
    /// </summary>
    /// <remarks>
    /// This method is commonly used to instantiate player characters.
    /// If a match has to be started "actively", you can call an [PunRPC](@ref PhotonView.RPC) triggered by a user's button-press or a timer.
    ///
    /// When this is called, you can usually already access the existing players in the room via PhotonNetwork.PlayerList.
    /// Also, all custom properties should be already available as Room.customProperties. Check Room..PlayerCount to find out if
    /// enough players are in the room to start playing.
    /// </remarks>
    public override void OnJoinedRoom()
    {
        Debug.Log("UMILauncher: OnJoinedRoom(). Ahora el cliente está en la nueva sala creada.\nDe aquí en adelante el juego se ejecutará.");

        // #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.AutomaticallySyncScene to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("UMILAUNCHER: Cargaremos el hub");

            //desactivamos el panel de connecting
            LoadingPanel.SetActive(false);

            // #Critical
            // Load the Room Level. 
            PhotonNetwork.LoadLevel(hub_Name);

        }
    }

    #endregion
}
