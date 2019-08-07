using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class FollowTransform : MonoBehaviour
{
    public Transform followTransform;

    private void Update()
    {
        transform.position = followTransform.position;
    }
}
