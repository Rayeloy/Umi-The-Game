using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveCollisions : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement myPlayerMovement;
    [SerializeField]
    private bool passiveCollisionsOn = true;
    [HideInInspector]
    public bool passiveCollisionActive = false;

    Transform lastCollidedStage;
    Vector3 lastContactPoint;
    Vector3 lastLocalContactPoint;
    Vector3 stageMovement;


    //private void OnTriggerStay(Collider other)
    //{
    //    if (passiveCollisionsOn)
    //    {
    //        Debug.LogWarning("TRIGGER COLLISION WITH " + other.transform.name);
    //    }
    //}

    private void OnCollisionStay(Collision collision)
    {
        if (passiveCollisionsOn)
        {
            Debug.LogWarning("COLLIDER COLLISION WITH " + collision.transform.name);
            if (lastCollidedStage == collision.collider.transform)
            {
                Vector3 contactPointNewPos = lastCollidedStage.TransformPoint(lastLocalContactPoint);
                stageMovement = contactPointNewPos - lastContactPoint;
                if (stageMovement != Vector3.zero) passiveCollisionActive = true;
                else passiveCollisionActive = false;
            }
        }
    }

    public void PassiveMove()
    {
        if (passiveCollisionActive)
        {
            if ((stageMovement.y < 0 && myPlayerMovement.controller.collisions.below) || (stageMovement.y > 0 && myPlayerMovement.controller.collisions.above)) stageMovement.y = 0;

            myPlayerMovement.transform.position += stageMovement;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (passiveCollisionsOn)
        {
            if (lastCollidedStage == collision.collider.transform)
                lastCollidedStage = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (passiveCollisionsOn)
        {
            Debug.LogWarning("COLLIDER COLLISION ENTER WITH " + collision.transform.name);
            if (lastCollidedStage == null)
                lastCollidedStage = collision.collider.transform;
            lastContactPoint = collision.GetContact(0).point;
            lastLocalContactPoint = lastCollidedStage.InverseTransformPoint(lastContactPoint);
        }
    }
}
