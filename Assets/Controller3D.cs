using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller3D : MonoBehaviour {

    public LayerMask collisionMask;

    const float skinWidth = .015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    public CapsuleCollider coll;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

    private void Awake()
    {
    }

    private void Start()
    {
        CalculateRaySpacing();
        //print("bounds.size.z = " + coll.bounds.size.z+"bounds.size.y = "+ coll.bounds.size.y);
    }

    public void Move(Vector3 vel)
    {
        UpdateRaycastOrigins();
        collisions.ResetVertical();
        collisions.ResetHorizontal();


        if (vel.x != 0)
        {
            HorizontalCollisions(ref vel);
        }

        if (vel.y != 0)
        {
            VerticalCollisions(ref vel);
        }


        transform.Translate(vel);
    }


    void HorizontalCollisions(ref Vector3 vel)
    {
        /*float directionX = Mathf.Sign(vel.x);
        float rayLength = Mathf.Abs(vel.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                vel.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
            }
        }*/
    }

    void VerticalCollisions(ref Vector3 vel)
    {
        /*
        float directionY = Mathf.Sign(vel.y);
        float rayLength = Mathf.Abs(vel.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + vel.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                vel.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }
        */
        // ---------------------- 3D CAPSULE -------------------
        float directionY = Mathf.Sign(vel.y);
        float rayLength = Mathf.Abs(vel.y) + skinWidth + 0.3f;
        float radius = 0f;
        //print("----------NEW SET OF RAYS------------");
        for(int i=0;i< verticalCircles; i++)
        {
            Vector3 circleOrigin = directionY == -1 ? raycastOrigins.BottomCenter : raycastOrigins.TopCenter;
            radius = i * verticalRadiusSpacing;
            //print("i= " + i + "; Radius = " + radius);
                //circleOrigin = new Vector3(circleOrigin.x, circleOrigin.y, circleOrigin.z + (i * verticalRadiusSpacing));

            for(int j=0; j < verticalRaysPerCircle; j++)
            {
                float angle = (j * verticalRayAngleSpacing) * Mathf.Deg2Rad;
                float px = circleOrigin.x + radius * Mathf.Cos(angle);
                float pz = circleOrigin.z + radius * Mathf.Sin(angle);
                Vector3 rayOrigin = new Vector3(px, circleOrigin.y, pz);

                RaycastHit hit;
                Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.red);
                //print("rayOrigin= " + rayOrigin + "; direction = " + directionY + "; rayLength = " + rayLength+"; BottomCenter.y= "+raycastOrigins.BottomCenter.y+"; min.y = "+coll.bounds.min.y);

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY,out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    //print("HIT AGAINST " + hit.collider.gameObject.name);
                    vel.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance;

                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;
                }
                //if (i == 0) { break; }
            }
        }
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = coll.bounds;
        //bounds.Expand(skinWidth * -2);

        /*raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);*/

        raycastOrigins.TopCenter = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        raycastOrigins.TopEnd = new Vector3(bounds.center.x, bounds.max.y, bounds.max.z);
        raycastOrigins.BottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        raycastOrigins.BottomEnd = new Vector3(bounds.center.x, bounds.min.y, bounds.max.z);
    }

    public int verticalCircles;
    public int verticalRaysPerCircle;
    float verticalRadiusSpacing;
    float verticalRayAngleSpacing;

    void CalculateRaySpacing()
    {
        Bounds bounds = coll.bounds;
        //bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);

        //-------------------

        verticalCircles = Mathf.Clamp(verticalCircles, 2, int.MaxValue);
        verticalRaysPerCircle = Mathf.Clamp(verticalRaysPerCircle, 3, int.MaxValue);

        verticalRadiusSpacing = (bounds.size.z/2) / (verticalCircles-1);
        verticalRayAngleSpacing = 360 / (verticalRaysPerCircle);
    }

    struct RaycastOrigins
    {
        /*public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;*/

        public Vector3 TopCenter, TopEnd;//TopEnd= center x, max y, max z
        public Vector3 BottomCenter, BottomEnd;//TopEnd= center x, min y, max z
    }


    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool foward, behind;

        public void ResetVertical()
        {
            above = below = false;
        }
        public void ResetHorizontal()
        {
            left = right = false;
            foward = behind = false;
        }
    }
}
