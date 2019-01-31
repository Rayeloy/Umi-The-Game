using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAnimation : MonoBehaviour {
    public float speed = 3;
    private void Update()
    {
        transform.Rotate(Vector3.down, speed* Time.deltaTime);
    }
}
