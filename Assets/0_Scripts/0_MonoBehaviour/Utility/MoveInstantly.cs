using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInstantly : MonoBehaviour
{
    public KeyCode keyCode;

    public float xDisplacement;

    Vector3 originalPos;
    Vector3 nextPos;
    int way = 1;

    private void Awake()
    {
        originalPos = transform.position;
        nextPos = originalPos + Vector3.right * xDisplacement * way;
    }

    private void Update()
    {
        if (Input.GetKeyUp(keyCode))
        {
            Move();
        }
    }

    void Move()
    {
        Debug.Log("MOVE PLATFORM INSTANTLY");
        transform.position = nextPos;
        way = -way;
        nextPos = transform.position + Vector3.right * xDisplacement * way;
    }
}
