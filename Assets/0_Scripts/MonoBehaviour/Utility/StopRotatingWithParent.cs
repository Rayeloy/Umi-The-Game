using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopRotatingWithParent : MonoBehaviour
{
    private void LateUpdate()
    {
        if (transform.parent != null)
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.parent.rotation.z * -1.0f);
        }
    }
}
