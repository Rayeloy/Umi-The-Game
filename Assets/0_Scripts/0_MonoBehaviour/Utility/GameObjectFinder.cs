using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectFinder : MonoBehaviour
{
    [SerializeField]
    private string gameObjectName;
    [SerializeField]
    private string gameObjectTag;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameObject go = GameObject.Find(gameObjectName);
            Debug.Log("GameObject found by name is = " + go.name);
            go = GameObject.FindGameObjectWithTag(gameObjectTag);
            Debug.Log("GameObject found by tag is = " + go.name);
        }
    }


}
