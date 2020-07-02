using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerCallback : MonoBehaviour
{
    public GameControllerCMF Manage;

    //public override void ControlOfEntityGained(BoltEntity entit)
    //{
    //    if (BoltNetwork.IsClient)
    //    {
    //        Debug.Log("control of entity gained : " + entit + ", Manager : " + Manage);
    //        Manage.ControlOfEntityGained(entit);
    //    }
    //}

    //public override void SceneLoadLocalDone(string scene)
    //{
    //    Manage.SceneLoadLocalDone(scene);
    //}

    //public override void EntityReceived(BoltEntity entit)
    //{
    //    if (BoltNetwork.IsClient)
    //    {
    //        Manage.EntityReceivedOrCreated(entit);
    //    }
    //}

}
