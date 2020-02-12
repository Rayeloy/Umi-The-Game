using UnityEngine;

public class BoltServerSpawner : Bolt.GlobalEventListener
{
    public GameControllerCMF Manage;

    public override void SceneLoadRemoteDone(BoltConnection connection)
    {
        if (BoltNetwork.IsServer)
        {
            Debug.Log("Scene finished loading !");
            if (connection != null)
            {
                BoltEntity client = BoltNetwork.Instantiate(BoltPrefabs.PlayerPrefCMF_actual_online);
                client.AssignControl(connection);
            }
        }
    }
}
