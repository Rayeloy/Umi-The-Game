using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

namespace UMI.Multiplayer
{
    // Input para permitir elegir al jugador su nombre de usuario, se empleará este método hasta que se haga el sistema de cuentas
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {
        #region Private Constants

        const string playerNamePerfkey = "UmiBoy";

        #endregion

        #region funciones MonoBehaviour
        void Start()
        {
            string defaultName = string.Empty;
            InputField _inputField = this.GetComponent<InputField>();
            if (_inputField != null)
            {
                if (PlayerPrefs.HasKey(playerNamePerfkey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePerfkey);
                    _inputField.text = defaultName;
                }
            }

            PhotonNetwork.NickName = defaultName;
        }

        #endregion

        #region Métodos Públicos

        /// dato al canto deberíamos poner un filtro por aquí para que no se llamen "mamonazo93" o cosas del rollo
        /// "el_gran_comeconejos"
        /// "destructor_de_vaginas"
        /// "follacabras"
        /// "Shurmanito_92"
        /// entendéis lo que digo :)
       
        public void SetPlayerName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("El nombre del jugador es inválido o está vacío");
                return;
            }
            PhotonNetwork.NickName = value();

            PlayerPrefs.SetString(playerNamePerfkey, value);
        }

        /// "B00B_SUCK3R"
        /// "El_ninio_polla"
        /// "Botarate"
        /// "PU55Y_D35TR0Y3R_4514"
        /// "nalgasduras"
        /// "ludoputas"
        /// "pedobear"
        /// "TuPutaMadre88"
        /// "factorpolla92"
        /// "pedo_silencioso", este lleva una skin de ninja

        #endregion
    }
}
