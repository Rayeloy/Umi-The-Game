using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPoint : MonoBehaviour
{
    public Transform[] hookPoints;
    public GameObject smallTrigger;
    public GameObject bigTrigger;


    public Transform GetHookPoint(Vector3 collisionPoint)
    {
        //print("collision point world pos = " + collisionPoint.ToString("F4"));
        collisionPoint = transform.InverseTransformPoint(collisionPoint);
        //print("collision point local pos = "+collisionPoint.ToString("F4"));
        if (collisionPoint.z >= 0)
        {
            if (collisionPoint.x >= 0)//Cuadrante 1
            {
                //print("Cuadrante 1");
                return hookPoints[0];
            }
            else//Cuadrante 4
            {
                //print("Cuadrante 4");
                return hookPoints[3];
            }
        }
        else
        {
            if (collisionPoint.x >= 0)//Cuadrante 2
            {
                //print("Cuadrante 2");
                return hookPoints[1];
            }
            else//Cuadrante 3
            {
                //print("Cuadrante 3");
                return hookPoints[2];
            }
        }
    }
}
