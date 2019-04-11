using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPoint : MonoBehaviour
{
    public Transform[] hookPoints;

    public Vector3 GetHookPoint(Vector3 collisionPoint)
    {
        if (collisionPoint.z >= transform.position.z)
        {
            if (collisionPoint.x >= transform.position.x)//Cuadrante 1
            {
                return hookPoints[0].position;
            }
            else//Cuadrante 4
            {
                return hookPoints[1].position;
            }
        }
        else
        {
            if (collisionPoint.x >= transform.position.x)//Cuadrante 2
            {
                return hookPoints[2].position;
            }
            else//Cuadrante 3
            {
                return hookPoints[3].position;
            }
        }
    }
}
