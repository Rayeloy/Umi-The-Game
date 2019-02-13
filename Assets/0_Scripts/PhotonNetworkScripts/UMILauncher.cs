using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;


public class UMILauncher : MonoBehaviourPunCallbacks
{

    #region ----[ HEADER REFERENCES ]----
    [Header(" --- POPIEDADES DEL LAUNCHER --- ")]
    [Tooltip("Panel que se oculta en el momento de la conexión, configurable para todo el menú o lo que sea, todo lo que se quiera ocultar debe colgar de él")]
    [SerializeField]
    private GameObject controlPanel;
    [Tooltip("Objeto que informa al usuario que la conexión está en progreso")]
    [SerializeField]
    private GameObject progressLabel;
    [Tooltip("Nombre de la escena que carga el menú, default = 'FINAL_Flag'")]
    [SerializeField]
    private string loadLevelName = "FINAL_Flag";

    [Tooltip("El número máximo de jugadores por sala")]
    [SerializeField]
    private byte MaxPlayersPerRoom = 4;
    #endregion

    #region ----[ PROPERTIES ]----

    /// booleano para determinar si está en proceso de conexión, se usa habitualmente con la función OnConnectedToMaster()
    bool isConnecting;

    /// Versión actual del juego, se recomienda según el tutorial dejarlo en 1 a no ser que se hagan grandes cambios en el juego
    string gameVersion = "1";

    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    private void Start()
    {
        // Mostramos el menú que se encuentra en "controlPanel" y ocultamos el texto de "conectando"
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);

    }

    void Awake()
    {
        // #Critical
        // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
        //PhotonNetwork.

        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #endregion

    #region ----[ PUN CALLBACKS ]----

    public override void OnConnectedToMaster()
    {
        Debug.Log("UMI Launcher: OnConnectedToMaster() se ha conectado de forma correcta al servidor");

        if (isConnecting)
        {
            // #Crítico: si se falla en la conexión al unirse a una sala aleatoria significa que o no existe o hace falta crear una
            // en tal caso crearemos una sala más abajo en la función OnJounRandomFailed()
            if (PhotonNetwork.JoinRandomRoom())
            {
                Debug.Log("UMI Launcher: OnJoinedRoom(), ahora te encuentras en la sala como cliente");
                Debug.Log("UMI Launcher: Vamos a cargar el lobby al que te vas a unir");
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("UMI Launcher: OnJoinRandomFailed() la conexión con una sala aleatoria ha fallado, crearemos una sala nueva pues no existe alguna actualmente en el servidor");
        string roomName = null;
        if (PhotonNetwork.NickName != null)
        {
            roomName = "Room of " + PhotonNetwork.NickName;
            // Juan: si el usuario tiene un nickname la sala se llamará "Room of nickname" ya que la idea es que cada nick sea único, sino el server generará un nombre random al dejarlo como null
        }
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("UMI Launcher: OnJoinedRoom(), ahora el cliente se encuentra en la sala " + PhotonNetwork.CurrentRoom.Name);


        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("UMI Launcher: Vamos a cargar el lobby al que te vas a unir");
            ///#Critical
            ///Cargamos el nivel
            PhotonNetwork.LoadLevel(loadLevelName);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);

        Debug.LogWarningFormat("UMI Launcher: OnDisconnected() nos hemos desconectado del servidor, razón {0}, así que volvemos al menú principal.", cause);
    }
    #endregion

    #region ----[ NETWORK FUNCTIONS ]----

    public void Connect()
    {
        //establecemos la UI para que diga "conectando" y desaparezca el menú
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);

        /// y hacemos true isConnecting para que el programa sepa que está en proceso de conexión
        /// y no haya errores de intentos de unirse a la sala previos a la conexión con el servidor maestro
        isConnecting = true;


        if (PhotonNetwork.IsConnected) // si estamos conectados intentamos unirnos a la sala
        {
            //Si queremos construir un sistema de elo en el futuro debería cambiarse este "joinRandomRoom" en otra cosa
            PhotonNetwork.JoinRandomRoom();
        }
        else // sino nos conectamos al servidor
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    #endregion
}