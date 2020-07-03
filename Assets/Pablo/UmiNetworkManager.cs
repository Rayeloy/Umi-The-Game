using Mirror;
using UnityEngine;

public class UmiNetworkManager : NetworkManager
{
    public GameControllerCMF gameController;

    #region Server
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Transform startPos = GetStartPosition();
        GameObject entity = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        NetworkServer.AddPlayerForConnection(conn, entity);
    }
    #endregion

  
}
