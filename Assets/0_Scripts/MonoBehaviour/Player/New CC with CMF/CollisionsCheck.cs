//Property of Another Coffee Games S.L., Spain. Author: Carlos Eloy Jose Sanz
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CollisionsCheck : MonoBehaviour
{
    public PlayerMovementCMF myPlayerMov;
    [HideInInspector]
    public Collider collider;

    public bool disableAllDebugs = true;
    public bool disableAllRays = true;
    public bool collideWithTriggers = false;
    QueryTriggerInteraction qTI;
    public LayerMask collisionMask;
    public LayerMask collisionMaskAround;
    public float maxSlopeAngle = 60;
    public float FloorMaxDistanceCheck = 5;
    Color purple = new Color(0.749f, 0.380f, 1f);
    Color brown = new Color(0.615f, 0.329f, 0.047f);
    Color orange = new Color(0.945f, 0.501f, 0.117f);
    Color darkBrown = new Color(0.239f, 0.121f, 0f);
    Color darkYellow = new Color(0.815f, 0.780f, 0.043f);
    Color darkRed = new Color(0.533f, 0.031f, 0.027f);
    Color darkGreen = new Color(0.054f, 0.345f, 0.062f);
    RaycastHit[] hits;

    const float skinWidth = 0.1f;

    public CapsuleCollider coll;
    RaycastOrigins raycastOrigins;


    [Header(" -- Vertical Collisions -- ")]
    //public bool showVerticalRays;
    //public bool showVerticalLimits;
    public bool showDistanceCheckRays;
    //public bool showRoofRays;
    //public int verticalRows;
    //public int verticalRaysPerRow;
    //float verticalRowSpacing;
    //float verticalRaySpacing;
    [HideInInspector]
    public bool above, below, lastBelow, lastLastBelow, safeBelow, tooSteepSlope;
    [HideInInspector]
    public float distanceToFloor;
    [HideInInspector]
    public bool safeBelowStarted;
    float safeBelowTime, safeBelowMaxTime;
    [HideInInspector]
    public float slopeAngle;
    [HideInInspector]
    public bool lastSliping;
    public bool sliping { get { return below && tooSteepSlope; } }
    [HideInInspector]
    public GameObject floor;
    [HideInInspector]
    Vector3 groundContactPoint = Vector3.zero;


    [Header(" -- Horizontal Collisions -- ")]
    [HideInInspector]
    public Vector3 wallNormal;
    public Transform origin;
    public float radius = 5;
    bool hit;
    public Rigidbody rig;
    Collider[] hitColliders;
    Plane a;
    public Transform mid;
    public Collider colliderFinal;
    public Vector3 finalNormal;

    public void KonoAwake(Collider _collider)
    {
        qTI = collideWithTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        hits = new RaycastHit[10];

        collider = _collider;
        floor = null;
        wallNormal = Vector3.zero;
        above = below = lastBelow = lastLastBelow = safeBelow = false;
        distanceToFloor = float.MaxValue;
        safeBelowStarted = false;
        safeBelowTime = 0;
        safeBelowMaxTime = 0;
    }

    public void KonoStart()
    {
        //CalculateRaySpacing();
        //print("bounds.size.z = " + coll.bounds.size.z+"bounds.size.y = "+ coll.bounds.size.y);
    }

    #region FixedUpdate
    //RUN IN FIXED UPDATE
    public void UpdateCollisionVariables(Mover mover, JumpState jumpSt)
    {
        mover.CheckForGround();
        if (jumpSt != JumpState.Jumping && jumpSt != JumpState.Breaking)
            below = mover.IsGrounded();
        SetSlopeAngle(mover.GetGroundNormal());
        floor = mover.GetGroundCollider() != null ? mover.GetGroundCollider().gameObject : null;
        if (below)
        {
            groundContactPoint = mover.GetGroundPoint();
        }
    }

    /// <summary>
    /// MAIN FUNCTION OF CollisionsCheck
    /// </summary>
    /// <param name="vel"></param>
    public void UpdateCollisionChecks(Vector3 vel)
    {
        //ChangePositionWithPlatform();

        UpdateRaycastOrigins();

        VerticalCollisionsDistanceCheck(ref vel);

        UpdateSafeBelow();

        //SavePlatformPoint();
    }

    public void ResetVariables()
    {
        lastSliping = sliping;

        lastLastBelow = lastBelow;
        lastBelow = below;
        below = false;

        floor = null;
        wallNormal = groundContactPoint = Vector3.zero;
        above = below = safeBelow = safeBelowStarted = tooSteepSlope = false;
        distanceToFloor = float.MaxValue;
        safeBelowTime = 0;
        slopeAngle = -500;
    }
    #endregion

    #region Update
    //public void MoveWithPlatform()
    //{
    //    ChangePositionWithPlatform();
    //    SavePlatformPoint();
    //}
    #endregion

    public void SetSlopeAngle(Vector3 floorNormal)
    {
        slopeAngle = Vector3.Angle(floorNormal, Vector3.up);
        tooSteepSlope = slopeAngle > maxSlopeAngle;
    }

    #region --- COLLISIONS (RAYCASTS FUNCTIONS) --- 

    #region -- VERTICAL COLLISIONS --

    void VerticalCollisionsDistanceCheck(ref Vector3 vel)
    {
        if (vel.y < 0)
        {
            float rayLength = FloorMaxDistanceCheck;
            //Vector3 rowsOrigin = raycastOrigins.BottomLFCornerReal;
            //Vector3 rowOrigin = rowsOrigin;
            //print("----------NEW SET OF RAYS------------");
            //for (int i = 0; i < verticalRows; i++)
            //{
            //    //rowOrigin.z = rowsOrigin.z - (verticalRowSpacing * i);
            //    for (int j = 0; j < verticalRaysPerRow; j++)
            //    {
            //if (i % 2 == 0 && j % 2 == 0)// Every even number throw a ray. This is to reduce raycasts to half since not so many are needed.
            //{
            Vector3 rayOrigin = raycastOrigins.BottomCentre;
            //rayOrigin += Vector3.up * skinWidth;
            Vector3 rayDir = vel.normalized;
            RaycastHit hit;
            if (showDistanceCheckRays && !disableAllRays)
            {
                Debug.DrawRay(rayOrigin, rayDir * rayLength, Color.red);
            }


            if (ThrowRaycast(rayOrigin, rayDir, out hit, rayLength, collisionMask, qTI))
            {
                if (CanCollide(hit))
                {
                    //print("Vertical Hit");
                    //if (hit.distance < collisions.distanceToFloor)
                    //{
                    distanceToFloor = hit.distance;
                    //}
                }
            }
            //}
            //    }
            //}
            //collisions.distanceToFloor -= skinWidth;
        }
    }

    #endregion

    #region --- HORIZONTAL COLLISIONS ---
    public Collider DetectWallCollision()
    {
        hitColliders = Physics.OverlapSphere(gameObject.transform.position, radius, collisionMask);
        if (hitColliders.Length > 0)
        {
            Collider Goodcol = new Collider();
            Collider oldcol = new Collider();

            bool b = false;
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if ((!b && a.GetSide(hitColliders[i].transform.position)) || a.GetSide(hitColliders[i].transform.position) && (Vector3.Distance(gameObject.transform.position, hitColliders[i].transform.position) < Vector3.Distance(gameObject.transform.position, oldcol.transform.position)))
                {
                    b = true;
                    Goodcol = hitColliders[i];
                    oldcol = hitColliders[i];
                }
                else
                {
                    oldcol = hitColliders[i];
                }
            }
            hit = Physics.Raycast(mid.position, rig.velocity, out RaycastHit _hit, 5, collisionMask, qTI);
            Debug.DrawRay(mid.position, rig.velocity);
            finalNormal = _hit.normal;
            if (Goodcol != null && hit)
            {
                if (Vector3.Distance(origin.position, Goodcol.transform.position) < 3)
                {
                    return Goodcol;
                }
            }
        }
        return null;
    }

    public void CheckIfJump()
    {

    }
    //void Update()
    //{
    //    a = new Plane(rig.velocity, origin.position);
    //    colliderFinal = DetectWallCollision();
    //}
    #endregion

    #endregion

    #region --- MOVING PLATFORMS ---
    //Vector3 platformMovement;
    //Vector3 platformOldWorldPoint;
    //Vector3 platformOldLocalPoint;
    //Vector3 platformNewWorldPoint;
    //bool onMovingPlatform
    //{
    //    get
    //    {
    //        Debug.Log("onMovingPlatform-> below = "+ below + "; lastBelow = "+ lastBelow + "; floor = "+ floor);
    //        return below && !sliping && floor != null; /*&& collisions.lastBelow && collisions.lastFloor != null && collisions.lastFloor == collisions.floor*/
    //    }
    //}

    //void SavePlatformPoint()
    //{
    //    if (below && !sliping && floor != null)
    //    {
    //        platformOldWorldPoint = groundContactPoint;
    //        platformOldLocalPoint = floor.transform.InverseTransformPoint(platformOldWorldPoint);
    //        /*if (!disableAllDebugs) */Debug.Log("OnMovingPlatform true && Save Platform Point: Local = " + platformOldLocalPoint.ToString("F4") + "; world = " + platformOldWorldPoint.ToString("F4"));
    //    }
    //    else
    //    {
    //        platformOldWorldPoint = Vector3.zero;
    //    }
    //}

    //void CalculatePlatformPointMovement()
    //{
    //    if (onMovingPlatform && platformOldWorldPoint!=Vector3.zero)
    //    {
    //        /*if (!disableAllDebugs)*/ Debug.Log("CALCULATE PLATFORM POINT MOVEMENT");
    //        platformNewWorldPoint = floor.transform.TransformPoint(platformOldLocalPoint);
    //        platformMovement = platformNewWorldPoint - platformOldWorldPoint;
    //        /*if (!disableAllDebugs)*/ Debug.LogWarning("platformOldWorldPoint = " + platformOldWorldPoint.ToString("F4") + "; New Platform Point = " 
    //                                       + platformNewWorldPoint.ToString("F4") + "; platformMovement = " + platformMovement.ToString("F8"));
    //        /*if (!disableAllRays)*/ Debug.DrawLine(platformOldWorldPoint, platformNewWorldPoint, Color.red, 1f);
    //    }
    //}

    //void ChangePositionWithPlatform()
    //{
    //    platformMovement = Vector3.zero;
    //    CalculatePlatformPointMovement();

    //    //transform.Translate(platformMovement, Space.World);
    //    Debug.Log("platformMovement = " + platformMovement.ToString("F8"));
    //    if (onMovingPlatform && platformMovement != Vector3.zero)
    //        transform.position += platformMovement;
    //}
    #endregion

    #region --- AUXILIAR ---

    bool ThrowRaycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDist, int layerMask, QueryTriggerInteraction qTI)
    {
        Ray auxRay = new Ray(origin, direction);
        int length = Physics.RaycastNonAlloc(auxRay, hits, maxDist, layerMask, qTI);
        bool raycastHasHit = false;
        hit = new RaycastHit();
        for (int i = 0; i < length && !raycastHasHit; i++)
        {
            if (CanCollide(hits[i]))
            {
                raycastHasHit = true;
                hit = hits[i];
            }
        }
        return raycastHasHit;
    }

    /// <summary>
    /// //Funcion que calcula el angulo de un vector respecto a otro que se toma como referencia de "foward"
    /// </summary>
    /// <param name="referenceForward"></param>
    /// <param name="newDirection"></param>
    /// <returns></returns>
    float SignedRelativeAngle(Vector3 referenceForward, Vector3 newDirection, Vector3 referenceUp)
    {
        // the vector perpendicular to referenceForward (90 degrees clockwise)
        // (used to determine if angle is positive or negative)
        Vector3 referenceRight = Vector3.Cross(referenceUp, referenceForward);
        // Get the angle in degrees between 0 and 180
        float angle = Vector3.Angle(newDirection, referenceForward);
        // Determine if the degree value should be negative.  Here, a positive value
        // from the dot product means that our vector is on the right of the reference vector   
        // whereas a negative value means we're on the left.
        float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
        return (sign * angle);//final angle
    }

    /// <summary>
    /// Auxiliar function that draws a plane given it's normal, a point in it, and a color. The normal will always be drawn red.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normal"></param>
    /// <param name="planeColor"></param>
    void DrawPlane(Vector3 position, Vector3 normal, Color planeColor, float size = 1f)
    {
        //if (!disableAllDebugs) Debug.Log("DRAWING PLANE WITH NORMAL " + normal.ToString("F4") + "; planeColor = " + planeColor + "; size = " + size);
        if (normal == Vector3.zero) Debug.LogError("ERROR WHILE DRAWING PLANE: normal = (0,0,0) ");
        Vector3 v3;
        normal = (normal.normalized) * 2 * size;
        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;

        Vector3 corner0 = position + v3;
        Vector3 corner2 = position - v3;
        Quaternion q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        Vector3 corner1 = position + v3;
        Vector3 corner3 = position - v3;

        Debug.DrawLine(corner0, corner2, planeColor);
        Debug.DrawLine(corner1, corner3, planeColor);
        Debug.DrawLine(corner0, corner1, planeColor);
        Debug.DrawLine(corner1, corner2, planeColor);
        Debug.DrawLine(corner2, corner3, planeColor);
        Debug.DrawLine(corner3, corner0, planeColor);
        Debug.DrawRay(position, normal, Color.blue);
    }

    #endregion

    #region --- SAFE BELOW ---
    public void StartSafeBelow()
    {
        if (!safeBelowStarted)
        {
            safeBelowStarted = true;
            safeBelowMaxTime = 0.14f;
            safeBelowTime = 0;
        }
    }

    public void ProcessSafeBelow()
    {
        if (!lastBelow && below)
        {
            safeBelow = true;
            if (safeBelowStarted)
            {
                safeBelowStarted = false;
            }
        }

        if (safeBelowStarted)
        {
            safeBelowTime += Time.deltaTime;
            //print("safeBelowTime = " + safeBelowTime);
            if (safeBelowTime >= safeBelowMaxTime)
            {
                EndSafeBelow();
            }
        }
    }

    void EndSafeBelow()
    {
        safeBelowStarted = false;
        safeBelow = false;
        safeBelowTime = 0;
    }
    #endregion

    bool CanCollide(RaycastHit hit)
    {
        if (hit.collider.isTrigger)
        {
            return false;
        }
        else
        {
            if (hit.collider.tag == "PlayerCollider")
            {
                PlayerMovementCMF otherPlayer = hit.collider.GetComponentInParent<PlayerMovementCMF>();
                //Debug.LogWarning("otherPlayer = "+ otherPlayer);
                if (otherPlayer != null && otherPlayer != myPlayerMov)
                {
                    //Debug.LogWarning("COLLIDED WITH TRIGGER BUT IS ANOTHER PLAYER! otherPlayer = "+ otherPlayer + "; hit.transform = " + hit.collider);
                    return true;
                }
                else
                {
                    //Debug.LogWarning("COLLIDED WITH TRIGGER (MYSELF)");
                    return false;
                }
            }
            else
            {
                //Debug.LogWarning("COLLIDED WITH TRIGGER");
                return true;
            }

        }
    }

    void UpdateSafeBelow()
    {
        if (!below && lastBelow)
        {
            StartSafeBelow();
        }
        ProcessSafeBelow();
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = coll.bounds;

        raycastOrigins.BottomLFCornerReal = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomRFCornerReal = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomLBCornerReal = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
        raycastOrigins.BottomRBCornerReal = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);

        raycastOrigins.TopLFCornerReal = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        raycastOrigins.TopRFCornerReal = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
        raycastOrigins.TopLBCornerReal = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        raycastOrigins.TopRBCornerReal = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);

        raycastOrigins.BottomCentre = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        //--------------------------------- BOUNDS REDUCED BY SKINWIDTH ---------------------------------
        bounds.Expand(skinWidth * -2);

        raycastOrigins.BottomEnd = new Vector3(bounds.center.x, bounds.min.y, bounds.max.z);

        raycastOrigins.Center = bounds.center;
        raycastOrigins.AroundRadius = bounds.size.z / 2;

    }

    //void CalculateRaySpacing()
    //{
    //    Bounds bounds = coll.bounds;

    //    verticalRows = Mathf.Clamp(verticalRows, 2, int.MaxValue);
    //    verticalRaysPerRow = Mathf.Clamp(verticalRaysPerRow, 3, int.MaxValue);

    //    verticalRowSpacing = (bounds.size.z) / (verticalRows - 1);
    //    verticalRaySpacing = bounds.size.x / (verticalRaysPerRow - 1);

    //    bounds.Expand(skinWidth * -2);
    //}

    struct RaycastOrigins
    {
        public Vector3 BottomEnd;//TopEnd= center x, min y, max z
        public Vector3 BottomCentre;

        public Vector3 BottomLFCornerReal, BottomRFCornerReal, BottomLBCornerReal, BottomRBCornerReal;
        public Vector3 TopLFCornerReal, TopRFCornerReal, TopLBCornerReal, TopRBCornerReal;

        public Vector3 Center;
        public float AroundRadius;
    }

    //public struct CollisionInfo
    //{
    //    public bool above, below, lastBelow, lastLastBelow, safeBelow;
    //    public bool left, right;
    //    public bool foward, behind;
    //    public bool collisionHorizontal
    //    {
    //        get
    //        {
    //            return (left || right || foward || behind);
    //        }
    //        set { }
    //    }
    //    public bool around;

    //    //HORIZONTAL
    //    public CollisionState collSt;
    //    public CollisionState lastCollSt;
    //    public float slopeAngle, slopeAngleOld, realSlopeAngle, wallAngle, wallAngleOld, wallAngle2, wallAngleOld2, floorAngle;
    //    public Vector3 startVel;
    //    public Raycast closestHorRaycast;
    //    public Raycast[,] horRaycastsX;
    //    public Raycast[,] horRaycastsZ;
    //    //public Vector3 horCollisionsPoint;
    //    public Vector3 wallNormal;
    //    public Vector3 oldWallNormal;
    //    public GameObject horWall;
    //    public GameObject floor, lastFloor;
    //    public Vector3 lastClosestVertRayPoint;
    //    public SlideState slideSt;
    //    public SlideState oldSlideSt;
    //    public SlideState oldSlideSt2;
    //    public FirstCollisionWithWallType fcww;
    //    public FirstCollisionWithWallType oldFcww;
    //    public float fcwwSlopeAngle;
    //    public bool finishedClimbing, lastFinishedClimbing;
    //    public Vector3 lastHorVel;
    //    //WALLSLIDE COLLISIONS
    //    public CollisionState wallSlideCollSt;
    //    public float wallSlideSlopeAngle;
    //    public float wallEdgeSecondWallAngle;
    //    public float wallEdgeFirstWallAngle;
    //    public float wallEdgeNewWallAngle;

    //    //VERTICAL
    //    public Raycast closestVerRaycast;
    //    public Raycast[,] verRaycastsY;

    //    public float roofAngle, oldRoofAngle;
    //    public bool climbJump;

    //    public void ResetVertical()
    //    {
    //        lastLastBelow = lastBelow;
    //        lastBelow = below;
    //        above = below = false;
    //        lastClosestVertRayPoint = closestVerRaycast.ray.point;
    //        closestVerRaycast = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
    //        distanceToFloor = float.MaxValue;
    //        verRaycastsY = new Raycast[0, 0];
    //        lastFloor = floor;
    //        floor = null;
    //        oldRoofAngle = roofAngle;
    //        roofAngle = -600;
    //        climbJump = false;
    //    }

    //    public void ResetHorizontal()
    //    {
    //        left = right = false;
    //        foward = behind = false;
    //        wallAngleOld = wallAngle;
    //        wallAngle = -500;
    //        wallAngleOld2 = wallAngle2;
    //        wallAngle2 = 0;
    //        oldWallNormal = wallNormal;
    //        wallNormal = Vector3.zero;
    //        horWall = null;
    //        oldSlideSt2 = oldSlideSt;
    //        oldSlideSt = slideSt;
    //        slideSt = SlideState.none;
    //        Vector3 auxHorVel = new Vector3(startVel.x, 0, startVel.z);
    //        lastHorVel = auxHorVel != Vector3.zero ? auxHorVel : lastHorVel;
    //        startVel = Vector3.zero;
    //        closestHorRaycast = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
    //        horRaycastsX = new Raycast[0, 0];
    //        horRaycastsZ = new Raycast[0, 0];
    //        lastFinishedClimbing = finishedClimbing;
    //        finishedClimbing = false;

    //        oldFcww = fcww;
    //        fcww = FirstCollisionWithWallType.none;
    //        fcwwSlopeAngle = -400;

    //        //WallSlideCollisions
    //        wallSlideCollSt = CollisionState.none;
    //        wallSlideSlopeAngle = -504;
    //        wallEdgeFirstWallAngle = -501;
    //        wallEdgeSecondWallAngle = -502;
    //        wallEdgeNewWallAngle = -503;
    //    }

    //    public void ResetAround()
    //    {
    //        around = false;
    //    }

    //    public void ResetClimbingSlope()
    //    {
    //        lastCollSt = collSt;
    //        collSt = CollisionState.none;
    //        floorAngle = -1;
    //        slopeAngleOld = slopeAngle;
    //        slopeAngle = -700;
    //        realSlopeAngle = -701;
    //    }


    //    #region --- SAFE BELOW ---
    //    public void StartSafeBelow()
    //    {
    //        if (!safeBelowStarted)
    //        {
    //            safeBelowStarted = true;
    //            safeBelowMaxTime = 0.14f;
    //            safeBelowTime = 0;
    //        }
    //    }

    //    public void ProcessSafeBelow()
    //    {
    //        if (!lastBelow && below)
    //        {
    //            safeBelow = true;
    //            if (safeBelowStarted)
    //            {
    //                safeBelowStarted = false;
    //            }
    //        }

    //        if (safeBelowStarted)
    //        {
    //            safeBelowTime += Time.deltaTime;
    //            //print("safeBelowTime = " + safeBelowTime);
    //            if (safeBelowTime >= safeBelowMaxTime)
    //            {
    //                EndSafeBelow();
    //            }
    //        }
    //    }

    //    void EndSafeBelow()
    //    {
    //        safeBelowStarted = false;
    //        safeBelow = false;
    //        safeBelowTime = 0;
    //    }
    //    #endregion
    //}
}
