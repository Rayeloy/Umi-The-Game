using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Manager/GameSettings")]
public class GameSettings : ScriptableObject
{

    [SerializeField]
    private string _gameVersion = "0.0.0";

    public string GameVersion
    {
        get
        {
            return _gameVersion;
        }
    }

    [SerializeField] private string _nickName = "Punfish";

    public string NickName
    {
        get
        {
            int value = Random.Range(0, 9999);
            return _nickName + value.ToString();
        }
    }

    [Tooltip("Nombre de la escena que debe cargar después de la conexión, esto en la iteración final debe ser el HUB. Default: Capture The Whale")]
    [SerializeField]
    private string _hubName = "Capture The Whale";

    public string HubName
    {
        get
        {
            return _hubName;
        }
    }

    [Tooltip("The maximum number of players per room default 4")]
    [SerializeField]
    private byte _maxPlayersPerGameRoom = 4;

    public byte MaxPlayersPerGameRoom
    {
        get
        {
            return _maxPlayersPerGameRoom;
        }
    }

}

