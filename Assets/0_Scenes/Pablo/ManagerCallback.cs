using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerCallback : Bolt.GlobalEventListener
{
    public GameControllerCMF Manage;

    void Start()
    {

        var loadParams = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
        StaticTest.localSimScene = SceneManager.LoadScene("CMF_Online_TestMirror", loadParams);
        StaticTest.localPhysicsScene = StaticTest.localSimScene.GetPhysicsScene();

    }


    public override void SceneLoadRemoteDone(BoltConnection connection)
    {
        if (BoltNetwork.IsServer)
        {
            if (connection != null)
            {
                Debug.Log("Scene finished loading !");
                BoltEntity entit= BoltNetwork.Instantiate(BoltPrefabs.PlayerPrefCMF_actual_online);
                entit.AssignControl(connection);
                Manage.EntityReceivedOrCreated(entit);
            }
        }
    }

    public override void ControlOfEntityGained(BoltEntity entit)
    {
        if (BoltNetwork.IsClient)
        {
            Debug.Log("control of entity gained : " + entit + ", Manager : " + Manage);
            Manage.ControlOfEntityGained(entit);
        }
    }

    //public override void SceneLoadLocalDone(string scene)
    //{
    //    Manage.SceneLoadLocalDone(scene);
    //}

    public override void EntityReceived(BoltEntity entit)
    {
        if (BoltNetwork.IsClient)
        {
            Manage.EntityReceivedOrCreated(entit);
        }
    }

}
