using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

namespace UMI.Multiplayer
{
    public class GameManager_Test : MonoBehaviourPunCallbacks
    {
        #region Photon Callbacks

        /// llamaremos a esta función cuando el usuario se desconecte de la sala
        public override void OnLeftRoom()
        {
            /// volvemos al menú principal, el cual es la escena menu_online
            SceneManager.LoadScene("Menus_Online");
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("{0} ha entrado en la sala", other.NickName);

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadArena();
            }
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("{0} ha salido de la sala", other.NickName);

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadArena();
            }
        }

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        #endregion

        #region Private Methods

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("GameManager: Intentando cargar el nivel pero no somos el dueño de la sala");
            }
            Debug.LogFormat("GameManager: Cargando Nivel: {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
        }

        /// COSAS IMPORTANTES A SABER:
        /// PhotonNetwork.LoadLevel() sólo debe ser utilizado si el cliente es el dueño de la sala (masterclient)
        /// así que siempre se debe comprobar si el que está ejecutando el código es el dueño de la sala usando IsMasterClient

        /// usamos LoadLevel() para cargar el nivel que queremos específico por eso las escenas se llaman "Room for [número]"
        #endregion
    }
}
