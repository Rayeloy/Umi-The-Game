using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    public bool destroyIfDuplicated = true;
    GameObject[] goList;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (destroyIfDuplicated)
        {
            goList = GameObject.FindGameObjectsWithTag(tag);
            if (goList.Length > 1) Destroy(gameObject);
        }
    }
}
