using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoTools : MonoBehaviour
{
    public static void DrawConeGizmo(Vector3 baseCenter, Vector3 vertex, Color color, int rays = 5, float radius = 0.5f, float coneHeight=0)
    {
        Color originalColor = Gizmos.color;
        Gizmos.color = color;

        Vector3 coneDir = vertex - baseCenter;
        if (coneHeight != 0) baseCenter = vertex - (coneDir.normalized * coneHeight);

        Vector3 coneDirPerp = Vector3.Cross(coneDir, Vector3.up).normalized;
        float anglePartition = 360 / rays;
        for (int i = 0; i < rays; i++)
        {
            float angle = anglePartition * i;
            Vector3 radiusDir = Quaternion.AngleAxis(angle, coneDir.normalized) * coneDirPerp;
            Vector3 basePoint = baseCenter + radiusDir.normalized * radius;

            Gizmos.DrawLine(baseCenter, basePoint);
            Gizmos.DrawLine(basePoint, vertex);
        }

        Gizmos.color = originalColor;
    }


    public static List<Vector3> DrawCurve(Vector3 startPoint, Vector3 endPoint, Color color, int subdivisions = 1)
    {
        Color originalColor = Gizmos.color;
        Gizmos.color = color;

        subdivisions = Mathf.Clamp(subdivisions, 1, int.MaxValue);
        List<Vector3> points = new List<Vector3>();
        points.Add(startPoint);
        points.Add(endPoint);
        float distVal = 0.1f;
        for (int i = 0; i < subdivisions; i++)
        {
            List<Vector3> oldPoints = new List<Vector3>();
            for (int j = 0; j < points.Count; j++)
            {
                oldPoints.Add(points[j]);
            }
            int extraCounter = 0;
            for (int j = 0; j < oldPoints.Count-1; j++)
            {
                Vector3 start = oldPoints[j];
                Vector3 end = oldPoints[j + 1];
                Vector3 origin = VectorMath.MiddlePoint(start, end);
                Vector3 dir = (start - end);
                Vector3 perpVector = Vector3.Cross(dir.normalized, Vector3.forward).normalized;
                Vector3 newPoint = origin + (perpVector * dir.magnitude * distVal);
                //Debug.Log("origin = " + origin + "; perpVector = " + perpVector + "; dist = " + (dir.magnitude * distVal));

                points.Insert(j + 1 + extraCounter, newPoint);
                //Debug.LogWarning("Insterting new point("+ newPoint+") at " + (j + 1));
                extraCounter++;
            }
            distVal = distVal / 2.5f;
        }
        //Debug.Log(points.Count);
        for (int i = 0; i < points.Count-1; i++)
        {
            //Debug.Log("Point " + i + " pos = " + points[i] + "; Point " + (i + 1) + " pos = " + points[i + 1]);
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
        Gizmos.color = originalColor;

        return points;
    }

    public static void DrawCurveArrow(Vector3 startPoint, Vector3 endPoint, Color curveColor, Color arrowColor, int subdivisions = 1, int rays = 5, float radius = 0.5f, float arrowHeight=0)
    {
        List<Vector3> points = DrawCurve(startPoint, endPoint, curveColor, subdivisions);
        DrawConeGizmo(points[points.Count - 2], points[points.Count - 1], arrowColor, rays, radius, arrowHeight);
    }
}
