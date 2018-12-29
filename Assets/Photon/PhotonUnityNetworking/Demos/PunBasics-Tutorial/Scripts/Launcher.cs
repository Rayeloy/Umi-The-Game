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
using Photon.Pun;


namespace UMI.Multiplayer
{
    //setup for UMI launcher

    public class Launcher : MonoBehaviour
    {
        #region Private Serializable Fields

        [Tooltip("Botón de la UI para conectarse y jugar")]
        [SerializeField]
        private GameObject controlPanel;

        [Tooltip("Texto que informa al usuario sobre el progreso de la conexión")]
        [SerializeField]
        private Text feedbackText;

       /// [Tooltip("El número máximo de jugadores por sala")]
       /// [SerializeField]
       /// private int maxPlayersPerRoom = 5;

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

        void Start()
        {
            Connect();   
        }

        #endregion

        #region Public Methods

        public void Connect()
        {
            isConnecting = true; // y decimos que nos estamos conectando
            controlPanel.SetActive(false); //ocultamos el botón de jugar/logear

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
    }
}