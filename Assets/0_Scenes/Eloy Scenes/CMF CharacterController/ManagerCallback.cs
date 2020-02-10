using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCallback : Bolt.GlobalEventListener
{
    public GameControllerCMF Manage;

    /*public override void ControlOfEntityGained(BoltEntity entity)
    {
        Manage.ControlOfEntityGained(entity);
    }*/

    public override void SceneLoadLocalDone(string scene)
    {
        Manage.SceneLoadLocalDone(scene);
    }

    public override void EntityReceived(BoltEntity entity)
    {
        Manage.EntityReceived(entity);
    }
}
