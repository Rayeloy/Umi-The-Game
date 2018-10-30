using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampolin : MonoBehaviour
{
	public Vector3 targetPosition
    {
        get
        {
            return transform.localPosition + transform.up * altura;
        }
        set
        {
            altura = (value - transform.position).y;
        }
    }
    [SerializeField]
    private float altura = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
