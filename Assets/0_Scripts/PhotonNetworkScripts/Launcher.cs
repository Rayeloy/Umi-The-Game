using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;


namespace UMI.Multiplayer
{
    //setup for UMI launcher

    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields
        [Tooltip("Panel que se oculta en el momento de la conexión, configurable para todo el menú o lo que sea, todo lo que se oculta debe colgar de él")]
        [SerializeField]
        private GameObject controlPanel;
        [Tooltip("Objeto que informa al usuario que la conexión está en progreso")]
        [SerializeField]
        private GameObject progressLabel;


        [Tooltip("El número máximo de jugadores por sala")]
        [SerializeField]
        private byte MaxPlayersPerRoom = 4;

        #endregion

        #region Private Fields
        /// booleano para determinar si está en proceso de conexión, se usa habitualmente con la función OnConnectedToMaster()
        bool isConnecting;

        /// Versión actual del juego, se recomienda según el tutorial dejarlo en 1 a no ser que se hagan grandes cambios en el juego
        string gameVersion = "1";

        #endregion

        #region MonoBehaviour CallBacks

        private void Start()
        {
            // Mostramos el menú que se encuentra en "controlPanel" y ocultamos el texto de "conectando"
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);

            /// hecho para conocer el número de la escena de menu_online el index de dicha escena es 4
            /// es principalmente relevante para el GameManager y poder volver al menú principal en caso de desconexión
            //Scene thisScene = SceneManager.GetActiveScene();
            //Debug.Log("El número de escena es: "+thisScene.buildIndex);
        }

        void Awake()
        {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        #endregion

        #region Public Methods

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
                PhotonNetwork.JoinRandomRoom();
            }
            else // sino nos conectamos al servidor
            {
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        #endregion

        #region MonoBehaviourPunCallbacks Overrides

        public override void OnConnectedToMaster()
        {
            Debug.Log("UMI Launcher: OnConnectedToMaster() se ha conectado de forma correcta al servidor");

            if (isConnecting)
            {
                // #Crítico: si se falla en la conexión al unirse a una sala aleatoria significa que o no existe o hace falta crear una
                // en tal caso crearemos una sala más abajo en la función OnJounRandomFailed()
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("UMI Launcher: OnJoinRandomFailed() la conexión con una sala aleatoria ha fallado, crearemos una sala nueva pues no existe alguna actualmente en el servidor");
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("UMI Launcher: OnJoinedRoom(), ahora el cliente se encuentra en una sala");

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("Vamos a cargar la sala para 1");

                ///#Critical
                ///Cargamos el nivel
                PhotonNetwork.LoadLevel("FINAL_Flag");
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            /// por ahora lo dejamos así pero en el momento de la desconexión debería volver a la escena y no solo reestablecer la UI a los parámetros base
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);

            Debug.LogWarningFormat("UMI Launcher: OnDisconnected() nos hemos desconectado del servidor, razón {0}", cause);
        }

        #endregion
    }
}