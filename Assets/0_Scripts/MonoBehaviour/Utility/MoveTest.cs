using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour {

    public float minX = -5;
    public float maxX = 5;
    float originalX;
    public float speed = 2;
    Vector3 velocity = Vector3.zero;
    bool goingRight = true;

    private void Start()
    {
        originalX = transform.position.x;
    }
    // Update is called once per frame
    void Update ()
    {
        if (!GameInfo.instance.gameIsPaused)
        {
            if (goingRight)
            {
                if (transform.position.x >= originalX + maxX)
                {
                    goingRight = false;
                }
                else
                {
                    velocity = Vector3.right * speed * Time.deltaTime;
                    transform.Translate(velocity, Space.World);
                }
            }
            else
            {
                if (transform.position.x <= originalX + minX)
                {
                    goingRight = true;
                }
                else
                {
                    velocity = Vector3.left * speed * Time.deltaTime;
                    transform.Translate(velocity, Space.World);
                }
            }
        }
    }   
}
