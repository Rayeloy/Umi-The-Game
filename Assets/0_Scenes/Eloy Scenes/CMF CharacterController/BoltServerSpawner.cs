using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class BoltServerSpawner : Bolt.GlobalEventListener
{
    public override void SceneLoadRemoteDone(BoltConnection connection)
    {
        if (connection != null)
        {
            BoltEntity client = BoltNetwork.Instantiate(BoltPrefabs.PlayerPrefCMF_actual_online);
            client.AssignControl(connection);
        }
    }
}
