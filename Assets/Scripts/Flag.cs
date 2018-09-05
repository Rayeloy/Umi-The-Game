using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {
    [HideInInspector]
    public Vector3 respawnPos;
    [Tooltip("Not used yet. Can be used to differentiate flags")]
    public int flagNumber;
	void Start () {
        respawnPos = transform.position;
	}
}
