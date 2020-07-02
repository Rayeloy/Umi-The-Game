using Mirror;
using System.Collections;
using System.Collections.Generic;
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
        PlayerMovementCMF player = entity.GetComponent<PlayerMovementCMF>();
        if (player != null)
        {
            Debug.Log("entity \"" + entity + "\" is player and starting !");
            Debug.Log("entity awake");
            player.KonoAwake(false);
            Debug.Log("entity start");
            player.KonoStart();
            Debug.Log("entity add to list");
            gameController.allPlayers.Add(player);
            player.gC = gameController;
        }

        NetworkServer.AddPlayerForConnection(conn, entity);
    }
    #endregion

    #region Client
    #endregion
}
