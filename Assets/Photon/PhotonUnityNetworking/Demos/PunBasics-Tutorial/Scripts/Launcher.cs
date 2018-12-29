// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Launcher.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in "PUN Basic tutorial" to connect, and join/create room automatically
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;


namespace UMI.Multiplayer
{
    //setup for UMI launcher

    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields

        [Tooltip("Botón de la UI para conectarse y jugar")]
        [SerializeField]
        private GameObject controlPanel;

        [Tooltip("Texto que informa al usuario sobre el progreso de la conexión")]
        [SerializeField]
        private Text feedbackText;

        [Tooltip("El número máximo de jugadores por sala")]
        [SerializeField]
        private byte MaxPlayersPerRoom = 5;

        #endregion


        #region Private Fields
        /// booleano para determinar si está en proceso de conexión, se usa habitualmente con la función OnConnectedToMaster()
        bool isConnecting;

        /// Versión actual del juego, se recomienda según el tutorial dejarlo en 1 a no ser que se hagan grandes cambios en el juego
        string gameVersion = "1";

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
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
            isConnecting = true; // y decimos que nos estamos conectando
            //controlPanel.SetActive(false); //ocultamos el botón de jugar/logear

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
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

            // #Crítico: si se falla en la conexión al unirse a una sala aleatoria significa que o no existe o hace falta crear una
            // en tal caso crearemos una sala más abajo en la función OnJounRandomFailed()
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("UMI Launcher: OnJoinRandomFailed() la conexión con una sala aleatoria ha fallado, crearemos una sala nueva pues no existe alguna actualmente en el servidor");
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("UMI Launcher: OnJoinedRoom(), ahora el cliente se encuentra en una sala");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("UMI Launcher: OnDisconnected() nos hemos desconectado del servidor, razón {0}", cause);
        }

        #endregion
    }
}