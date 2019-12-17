using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class FollowTransform : MonoBehaviour
{
    public Transform followTransform;
    public Vector3 offset;

    private void Update()
    {
        if(followTransform != null)
        {
            transform.position = followTransform.position;
            transform.localPosition = new Vector3(transform.localPosition.x + offset.x, transform.localPosition.y + offset.y, transform.localPosition.z + offset.z);
        }
    }
}
