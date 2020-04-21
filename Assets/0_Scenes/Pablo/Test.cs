using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*

Worth noting that 2018.3b API docs are still missing quite a few things related to this, for example
LoadSceneParameters is missing overloaded entry for
public LoadSceneParameters(LoadSceneMode mode, LocalPhysicsMode physicsMode);
there's also no mention of:
PhysicsSceneExtensions.GetPhysicsScene





    */

public class Test : MonoBehaviour
{
    PhysicsScene localPhysicsScene;

    void Start()
    {
        var loadParams = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
        Scene localSimScene = SceneManager.LoadScene("PhysScene", loadParams);
        localPhysicsScene = localSimScene.GetPhysicsScene();
    }

    void FixedUpdate()
    {
        Physics.Simulate(Time.fixedDeltaTime);
        localPhysicsScene.Simulate(Time.fixedDeltaTime * 0.2f);
    }
}