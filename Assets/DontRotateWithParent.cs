using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontRotateWithParent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.z * -1.0f);
        }
    }
}
