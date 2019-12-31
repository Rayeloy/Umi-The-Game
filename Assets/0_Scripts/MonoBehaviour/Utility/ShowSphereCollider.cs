using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSphereCollider : MonoBehaviour
{
    public Material sphereMaterial;
    SphereCollider sphereColl;
    GameObject sphereGO;

    private void Start()
    {
        sphereColl = GetComponent<SphereCollider>();
        if (sphereColl != null)
        {
            //create a capsule GameObject
            sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphereGO.GetComponent<SphereCollider>());
            if (sphereMaterial != null)
                sphereGO.GetComponent<MeshRenderer>().material = sphereMaterial;

            sphereGO.transform.SetParent(transform);
            sphereGO.transform.localPosition = sphereColl.center;
            sphereGO.transform.localScale = Vector3.one * sphereColl.radius * 2;// new Vector3(sphereColl.radius * 2, sphereColl.height * 0.5f, sphereColl.radius * 2);
        }
    }
}
