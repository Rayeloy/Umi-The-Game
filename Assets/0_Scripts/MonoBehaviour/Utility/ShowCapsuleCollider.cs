using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCapsuleCollider : MonoBehaviour
{
    public Material capsuleMaterial;
    CapsuleCollider capColl;
    GameObject capsuleGO;

    private void Start()
    {
        capColl = GetComponent<CapsuleCollider>();
        if (capColl != null)
        {
            //create a capsule GameObject
            capsuleGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Destroy(capsuleGO.GetComponent<CapsuleCollider>());
            if(capsuleMaterial!=null)
            capsuleGO.GetComponent<MeshRenderer>().material = capsuleMaterial;

            capsuleGO.transform.SetParent(transform);
            capsuleGO.transform.localPosition = capColl.center;
            capsuleGO.transform.localScale = new Vector3(capColl.radius*2, capColl.height * 0.5f, capColl.radius*2);
        }
    }
}
