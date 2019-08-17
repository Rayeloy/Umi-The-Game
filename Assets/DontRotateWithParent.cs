using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DontRotateWithParent : MonoBehaviour
{
    public bool DoRotate = false;
    public bool dontRotateXAxis = false;
    public bool dontRotateYAxis = false;
    public bool dontRotateZAxis = true;
    public bool rotateChildren = false;

    public Vector3 localRotation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        //if (Application.isEditor)
        //{
        //    if (DoRotate)
        //    {
        //        DoRotate = false;
        //        CounterRotate();
        //    }
        //}
        //else
        //{
            CounterRotate();
        //}


    }

    void CounterRotate()
    {
        if (rotateChildren)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).rotation = Quaternion.Euler(dontRotateXAxis ? transform.rotation.x * -1.0f : 0, dontRotateYAxis ? transform.rotation.y * -1.0f : 0, dontRotateZAxis ? transform.rotation.z * -1.0f : 0);
            }
        }
        else
        {
            //Vector3 localRot = transform.localRotation.eulerAngles;
            //Vector3 finalRot = transform.parent.rotation.eulerAngles + localRotation;
            transform.rotation = Quaternion.Euler(dontRotateXAxis ? localRotation.x : transform.rotation.x,
                dontRotateYAxis ? localRotation.y: transform.rotation.y, dontRotateZAxis ? localRotation.z : transform.rotation.z);
            //transform.localRotation = Quaternion.Euler(transform.localRotation.x + localRotation.x, transform.localRotation.y + localRotation.y, transform.localRotation.z + localRotation.z);
        }
    }
}
