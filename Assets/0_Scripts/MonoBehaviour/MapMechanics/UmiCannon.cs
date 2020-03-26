using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UmiCannon : MonoBehaviour
{
    [Header(" --- GIZMOS --- ")]
    bool showGizmos = true;
    [Tooltip("The more rays, the more defined the trajectory is drawn. Does not affect the definition of the arrow point, that is on code only. Ask Eloy.")]
    public int gizmoRays = 20;
    public Color normalTrajectoryColor = Color.blue;
    public Color playerTrajectoryColor = Color.red;

    [Header(" --- CANNON PARAMETERS ---")]
    public Transform targetPosition;
    [Tooltip("The smaller the time, the fastest the shot, and the flatter the parabole.")]
    [Range(0.2f, 15)]
    public float timeToReach;
    [Tooltip("How much percentage of the parabole's X (length) is the character blocked from inputing anything. 1 -> blocked all the parabole.")]
    [Range(0, 1)]
    public float noInputPercentage = 1f;
    [Tooltip("Enables or disables the bounce mechanic during the 'cannon shot'. If enabled, character may bounce after using the cannon due to falling from the highest point of the parabole.")]
    public bool bounceEnabled = false;
    //public Transform arrowIndicator;

    float exampleGravity = -31.25f;

    bool showLastUse = false;
    float lastGravity = 0;
    Vector3 lastOrigin = Vector3.zero;

    void ShowParabole(Vector3 _origin,  float _gravity, Color color)
    {
        Vector3[] parabolePoints = new Vector3[gizmoRays + 1];
        float timePartition = timeToReach / gizmoRays;

        Vector3 dir = targetPosition.position - _origin;
        Vector3 dirXZ = dir; dirXZ.y = 0;
        float distY = dir.y;
        float distXZ = dirXZ.magnitude;
        float speedXZ = distXZ / timeToReach;
        float speedY = distY / timeToReach + 0.5f * Mathf.Abs(_gravity) * timeToReach;


        Gizmos.color = color;
        for (int i = 0; i< parabolePoints.Length; i++)
        {
            float time = timePartition * i;
            //float y = result.y + (gravity * time);
            Vector3 pos = _origin + (dirXZ.normalized * speedXZ * time);
            pos.y = ((_gravity / 2) * (time * time)) + (Mathf.Abs(speedY)* time) + _origin.y;
            parabolePoints[i] = pos;
            if (i > 0)
                Gizmos.DrawLine(parabolePoints[i-1], parabolePoints[i]);
            //arrow indicator
            if(i+1 == parabolePoints.Length)
            {
                Vector3 coneDir = (parabolePoints[i - 1] - parabolePoints[i]).normalized;
                Vector3 coneBase = parabolePoints[i] + coneDir * 3;
                DrawConeGizmo(coneBase, parabolePoints[i], color, 8, 0.3f);
                //arrowIndicator.localPosition = Vector3.zero;
                //Vector3 arrowDir = (parabolePoints[i] - parabolePoints[i - 1]).normalized;
                //arrowIndicator.position += -arrowDir * 0.12f;
                //arrowIndicator.transform.LookAt(parabolePoints[i]);

            }
        }
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            ShowParabole(transform.position, exampleGravity, normalTrajectoryColor);
            if (showLastUse)
                ShowParabole(lastOrigin, lastGravity, playerTrajectoryColor);
        }
    }

    /// <summary>
    /// Given an origin, a target and a gravity value, returns a vector3 with the direction and speed to do a parabole to go from origin to target.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="gravity"> of the player</param>
    public Vector3 CalculateVelocity(Vector3 origin, float _gravity)
    {
        Vector3 dir = targetPosition.position - origin;
        Vector3 dirXZ = dir; dirXZ.y = 0;
        float distY = dir.y;
        float distXZ = dirXZ.magnitude;
        float speedXZ = distXZ / timeToReach;
        float speedY = distY / timeToReach + 0.5f * Mathf.Abs(_gravity) * timeToReach;

        Vector3 result = dirXZ.normalized;
        result *= speedXZ;
        //Debug.Log("SpeedY = " + speedY);
        result.y = speedY;

        showLastUse = true;
        lastOrigin = origin;
        lastGravity = _gravity;
        return result;
    }

    void DrawConeGizmo(Vector3 baseCenter, Vector3 vertex, Color color, int rays=5, float radius=0.5f)
    {
        Vector3 coneDir = vertex - baseCenter;
        Vector3 coneDirPerp = Vector3.Cross(coneDir, Vector3.up).normalized;
        float anglePartition = 360 / rays;
        for(int i = 0; i<rays; i++)
        {
            float angle = anglePartition * i;
            Vector3 radiusDir = Quaternion.AngleAxis(angle, coneDir.normalized) * coneDirPerp;
            Vector3 basePoint = baseCenter + radiusDir.normalized * radius;

            Gizmos.DrawLine(baseCenter, basePoint);
            Gizmos.DrawLine(basePoint, vertex);
        }
    }
}
