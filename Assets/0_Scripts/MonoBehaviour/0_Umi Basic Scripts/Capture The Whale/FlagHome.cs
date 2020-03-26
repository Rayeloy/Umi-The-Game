using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagHome : MonoBehaviour {

    public Team team;

    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
