using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UmiCannon : MonoBehaviour
{
    public float timeToReach;
    [Tooltip("How much percentage of the parabole's X (length) is the character blocked from inputing anything. 1 -> blocked all the parabole.")]
    [Range(0, 1)]
    public float noInputPercentage = 1f;
    public Transform targetPosition;


    private void OnDrawGizmos()
    {
        
    }

    /// <summary>
    /// Given an origin, a target and a gravity value, returns a vector3 with the direction and speed to do a parabole to go from origin to target.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="gravity"> of the player</param>
    public Vector3 CalculateVelocity(Vector3 origin, float gravity)
    {
        Vector3 dir = targetPosition.position - origin;
        Vector3 dirXZ = dir; dirXZ.y = 0;
        float distY = dir.y;
        float distXZ = dirXZ.magnitude;
        float speedXZ = distXZ / timeToReach;
        float speedY = distY / timeToReach + 0.5f * Mathf.Abs(gravity) * timeToReach;

        Vector3 result = dir.normalized;
        result *= speedXZ;
        Debug.Log("SpeedY = " + speedY);
        result.y = speedY;
        return result;
    }
}
