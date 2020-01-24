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
    public Collider myCollider;
    Rigidbody rb;

    public bool disableAllDebugs = true;
    public bool disableAllRays = true;
    public bool collideWithTriggers = false;
    QueryTriggerInteraction qTI;
    public LayerMask collisionMask;
    public LayerMask collisionMaskAround;
    public float maxSlopeAngle = 60;//in the future, take the value from "mover" script
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
    GameObject lastFloor;
    [HideInInspector]
    Vector3 groundContactPoint = Vector3.zero;
    bool justJumped = false;


    [Header(" -- Horizontal Collisions -- ")]
    public SphereCollider sphereCollTop;
    public SphereCollider sphereCollMiddle;
    public SphereCollider sphereCollBottom;

    public LayerMask wallMask;
    public float sphereRadius = 5;

    [HideInInspector]
    public Vector3 wallNormal;
    [HideInInspector]
    public GameObject wall;
    [HideInInspector]
    public float wallSlopeAngle, wallAngle;
    public float wallJumpWallMaxAngleWithMovement = 110f;

    List<CollisionHit> horizontalCollHits;
    List<CollisionHit> horizontalCollHitsTop;
    List<CollisionHit> horizontalCollHitsMiddle;
    List<CollisionHit> horizontalCollHitsBottom;
    List<CollisionHit> wallJumpCollHits;
    bool hit;
    public Collider[] hitColliders;
    public int maxHitColliders = 10;
    public Plane plane;
    public Collider colliderFinal;
    public Vector3 finalNormal;

    Collider Goodcol = new Collider();
    Collider oldcol = new Collider();

    public void KonoAwake(Collider _collider)
    {
        qTI = collideWithTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
        rb = GetComponent<Rigidbody>();
        hits = new RaycastHit[10];

        myCollider = _collider;
        floor = lastFloor = null;
        wallNormal = Vector3.zero;
        above = below = lastBelow = lastLastBelow = safeBelow = false;
        distanceToFloor = float.MaxValue;
        safeBelowStarted = false;
        safeBelowTime = 0;
        safeBelowMaxTime = 0;

        sphereCollTop.radius = sphereRadius;
        sphereCollMiddle.radius = sphereRadius;
        sphereCollBottom.radius = sphereRadius;
        hitColliders = new Collider[maxHitColliders];
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
        mover.CheckForGround(platformMovement.magnitude > 0.0001f);
        if (jumpSt != JumpState.Jumping && jumpSt != JumpState.Breaking)
            below = mover.IsGrounded();
        SetSlopeAngle(mover.GetGroundNormal());
        floor = mover.GetGroundCollider() != null ? mover.GetGroundCollider().gameObject : null;

        if (below)
        {
            groundContactPoint = mover.GetGroundPoint();
        }

        if(mover.sensor.castType == Sensor.CastType.RaycastArray)
        {
            if (!lastBelow && below)
            {
                //Debug.LogError("WE JUST LANDED");
                mover.sensor.RecalibrateRaycastArrayPositions();
            }
            else if (lastBelow && !below)
            {
                //Debug.LogError("WE JUST TOOK OFF");
                mover.sensor.RecalibrateRaycastArrayPositions(0.1f);
            }
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

        //colliderFinal = DetectWallCollision();

        HorizontalCollisions();
        WallJumpCollisions(vel);

        VerticalCollisionsDistanceCheck(ref vel);

        UpdateSafeBelow();

        //SavePlatformPoint();
    }

    public void ResetVariables()
    {
        lastSliping = sliping;

        lastLastBelow = lastBelow;

        lastBelow = justJumped?true:below;
        below = false;
        justJumped = false;

        lastFloor = floor;
        floor = null;
        wallNormal = groundContactPoint = Vector3.zero;
        above = below = safeBelow = safeBelowStarted = tooSteepSlope = false;
        distanceToFloor = float.MaxValue;
        safeBelowTime = 0;
        slopeAngle = -500;

        //Horizontal
        wall = null;
        wallAngle = wallSlopeAngle = 0;
        wallNormal = Vector3.zero;
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
    //public Collider DetectWallCollision()
    //{
    //    hitColliders = Physics.OverlapSphere(gameObject.transform.position + localStartPoint, sphereRadius, wallMask);
    //    if (hitColliders.Length > 0)
    //    {

    //        bool b = false;
    //        for (int i = 0; i < hitColliders.Length; i++)
    //        {
    //            if ((!b && plane.GetSide(hitColliders[i].transform.position)) || plane.GetSide(hitColliders[i].transform.position) &&
    //                (Vector3.Distance(gameObject.transform.position, hitColliders[i].transform.position) < Vector3.Distance(gameObject.transform.position, oldcol.transform.position)))
    //            {
    //                b = true;
    //                Goodcol = hitColliders[i];
    //                oldcol = hitColliders[i];
    //            }
    //            else
    //            {
    //                oldcol = hitColliders[i];
    //            }
    //        }
    //        Debug.Log(Goodcol != null);
    //        if (Goodcol != null)
    //        {
    //            hit = Physics.Raycast(gameObject.transform.position + localStartPoint, Goodcol.transform.position - gameObject.transform.position, out RaycastHit _hit, 5, wallMask, qTI);
    //            Debug.DrawRay(gameObject.transform.position + localStartPoint, Goodcol.transform.position - gameObject.transform.position);
    //            finalNormal = _hit.normal;
    //            if (Vector3.Distance(gameObject.transform.position + localStartPoint, Goodcol.transform.position) < 3 && hit)
    //            {
    //                return Goodcol;
    //            }
    //        }
    //    }
    //    return null;
    //}

    //Eloy's attemp:

    public void HorizontalCollisions()
    {
        Collider[] ourColliders= new Collider[]{ myCollider, sphereCollTop, sphereCollMiddle, sphereCollBottom};
        horizontalCollHits = new List<CollisionHit>();
        GetCollisionsHits(SpherePosition.Top, sphereCollTop, ourColliders);
        GetCollisionsHits(SpherePosition.Middle, sphereCollMiddle, ourColliders);
        GetCollisionsHits(SpherePosition.Bottom, sphereCollBottom, ourColliders);
    }

    public void WallJumpCollisions(Vector3 vel)
    {
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        if (horVel.magnitude < 0.001f) return;

        wallJumpCollHits = new List<CollisionHit>();
        for (int i = 0; i < horizontalCollHits.Count; i++)
        {
            Vector3 sphereCenter = horizontalCollHits[i].spherePos == SpherePosition.Top ? sphereCollTop.transform.position : horizontalCollHits[i].spherePos == SpherePosition.Bottom ?
sphereCollBottom.transform.position : sphereCollMiddle.transform.position;
            Vector3 collisionVector = horizontalCollHits[i].point - sphereCenter;
            collisionVector.y = 0;
            float angleWithMovementVector = Vector3.Angle(horVel, collisionVector);
            if (angleWithMovementVector <= wallJumpWallMaxAngleWithMovement)
            {
                wallJumpCollHits.Add(horizontalCollHits[i]);

                if (!disableAllRays) Debug.DrawLine(sphereCenter, horizontalCollHits[i].point, orange);
            }
        }

        //Find closest hit
        float minDist = float.MaxValue;
        CollisionHit wallCollHit = new CollisionHit(SpherePosition.None ,null, Vector3.zero, new RaycastHit());
        for (int i = 0; i < wallJumpCollHits.Count; i++)
        {
            Vector3 sphereCenter = wallJumpCollHits[i].spherePos == SpherePosition.Top ? sphereCollTop.transform.position : horizontalCollHits[i].spherePos == SpherePosition.Bottom ?
    sphereCollBottom.transform.position : sphereCollMiddle.transform.position;

            float dist = (sphereCenter - wallJumpCollHits[i].point).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                wallCollHit = wallJumpCollHits[i];
                //wallNormal =;
            }
        }

        //At least one walljumpable wall
        if (wallCollHit.collider != null)
        {
            wall = wallCollHit.collider.gameObject;
            wallSlopeAngle = wallCollHit.slopeAngle;
            wallNormal = wallCollHit.hit.normal;
            wallAngle = SignedRelativeAngle(Vector3.forward, wallNormal, Vector3.up);
        }
        
    }

    void GetCollisionsHits(SpherePosition spherePos, SphereCollider mySphereCollider, Collider[] ourColliders)
    {
        hitColliders = new Collider[maxHitColliders];
        hitColliders = Physics.OverlapSphere(mySphereCollider.transform.position, sphereRadius, wallMask);

        List<CollisionHit> collisionHits = new List<CollisionHit>();
        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider auxColl = hitColliders[i];

            //Skip if collided with ourselves
            bool collidedWithOurself = false;
            for (int f = 0; f < ourColliders.Length && !collidedWithOurself; f++)
            {
                if (auxColl == ourColliders[f]) collidedWithOurself = true;
            }
            if (collidedWithOurself) continue;

            //Skip if we already processed that collider
            bool collFound = false;
            for (int j = 0; j < horizontalCollHits.Count && !collFound; j++)
            {
                if (auxColl == horizontalCollHits[j].collider && horizontalCollHits[j].spherePos == spherePos)
                {
                    collFound = true;
                }
            }
            if (collFound) continue;

            //New collider found
            Vector3 hitDir = Vector3.zero;
            float hitDist = 0;
            if (Physics.ComputePenetration(mySphereCollider, mySphereCollider.transform.position, mySphereCollider.transform.rotation, auxColl, auxColl.transform.position, auxColl.transform.rotation,
                    out hitDir, out hitDist))
            {
                //Vector3 contactPoint = sphereColl.transform.position + (-hitDir * (sphereRadius - hitDist));

                RaycastHit hit;
                if (Physics.Raycast(mySphereCollider.transform.position, -hitDir, out hit, 2, wallMask, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider == auxColl)
                    {
                        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                        //too steep for a slope, so it's a wall
                        if (slopeAngle > maxSlopeAngle)
                        {

                            //Debug.LogWarning("HORIZONTAL COLLISION FOUND! myCollPos = " + sphereColl.transform.position + "; hitDir = " + hitDir + "; hitDist = " + hitDist.ToString("F8"));
                            //Debug.DrawRay(sphereColl.transform.position, hitDir * hitDist, orange);

                            CollisionHit collHit = new CollisionHit(spherePos, auxColl, hit.point, hit, slopeAngle);
                            if (!disableAllRays) Debug.DrawLine(mySphereCollider.transform.position, hit.point, darkBrown);
                            horizontalCollHits.Add(collHit);
                        }
                    }
                }
            }
        }

    }

    //void Update()
    //{
    //    colliderFinal = DetectWallCollision();
    //}

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
    Vector3 platformMovement;
    Vector3 platformOldWorldPoint;
    Vector3 platformOldLocalPoint;
    Vector3 platformNewWorldPoint;
    //Only for deciding if we calculate new platform movement, not used to decide if we need to save the current position and platform
    bool onMovingPlatform
    {
        get
        {
            if(!disableAllDebugs) Debug.Log("onMovingPlatform-> below = " + below + "; lastBelow = " + lastBelow + "; floor = " + floor+"; lastFloor = "+lastFloor);
            return below && lastBelow && !sliping && floor != null && lastFloor == floor; /*&& collisions.lastBelow && collisions.lastFloor != null && collisions.lastFloor == collisions.floor*/
        }
    }

    public void SavePlatformPoint()
    {
        if (below && !sliping && floor != null)
        {
            platformOldWorldPoint = groundContactPoint;
            platformOldLocalPoint = floor.transform.InverseTransformPoint(platformOldWorldPoint);
            /*if (!disableAllDebugs) */
            if (!disableAllDebugs) Debug.Log("OnMovingPlatform true && Save Platform Point: Local = " + platformOldLocalPoint.ToString("F4") + "; world = " + platformOldWorldPoint.ToString("F4"));
        }
        else
        {
            platformOldWorldPoint = Vector3.zero;
        }
    }

    void CalculatePlatformPointMovement()
    {
        if (onMovingPlatform && platformOldWorldPoint != Vector3.zero)
        {
            /*if (!disableAllDebugs)*/
            //Debug.Log("CALCULATE PLATFORM POINT MOVEMENT");
            platformNewWorldPoint = floor.transform.TransformPoint(platformOldLocalPoint);
            platformMovement = platformNewWorldPoint - platformOldWorldPoint;
            /*if (!disableAllDebugs)*/
            if (!disableAllDebugs) Debug.Log("platformOldWorldPoint = " + platformOldWorldPoint.ToString("F4") + "; New Platform Point = "
                + platformNewWorldPoint.ToString("F4") + "; platformMovement = " + platformMovement.ToString("F8"));
            /*if (!disableAllRays)*/
            Debug.DrawLine(platformOldWorldPoint, platformNewWorldPoint, Color.red, 1f);
        }
    }

    public Vector3 ChangePositionWithPlatform(bool instantMode)
    {
        platformMovement = Vector3.zero;
        CalculatePlatformPointMovement();

        //transform.Translate(platformMovement, Space.World);
        if (onMovingPlatform && platformMovement.magnitude > 0.0001f)
        {
            if (platformMovement.magnitude > 1) Debug.LogError("ERROR: MOVIMIENTO EXCESIVO");
            //GetComponent<Rigidbody>().velocity = myPlayerMov.currentVel + platformMovement * (1 / Time.fixedDeltaTime) * 100000;
            //transform.position += platformMovement;
            //rb.MovePosition(rb.position + platformMovement);
            if (instantMode)
            {
                Vector3 auxPlatformMovement = platformMovement;
                auxPlatformMovement.y = 0;
                rb.position += auxPlatformMovement;
            }
            //Debug.LogWarning("platformMovement = " + platformMovement.ToString("F8")+"; newPos = "+platformNewWorldPoint.ToString("F8") + "; current pos = "+rb.position.ToString("F8"));
        }
        return platformMovement;
    }
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

    #region --- JUMP ---
    public void StartJump()
    {
        below = false;
        justJumped = true;
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

    struct RaycastOrigins
    {
        public Vector3 BottomEnd;//TopEnd= center x, min y, max z
        public Vector3 BottomCentre;

        public Vector3 BottomLFCornerReal, BottomRFCornerReal, BottomLBCornerReal, BottomRBCornerReal;
        public Vector3 TopLFCornerReal, TopRFCornerReal, TopLBCornerReal, TopRBCornerReal;

        public Vector3 Center;
        public float AroundRadius;
    }

    public enum SpherePosition
    {
        None,
        Top,
        Middle,
        Bottom
    }

    struct CollisionHit
    {
        public SpherePosition spherePos;
        public Collider collider;
        public Vector3 point;
        public RaycastHit hit;
        public float slopeAngle;
        public CollisionHit(SpherePosition _spherePos, Collider _collider, Vector3 _point, RaycastHit _hit, float _slopeAngle = 0)
        {
            spherePos = _spherePos;
            collider = _collider;
            point = _point;
            hit = _hit;
            slopeAngle = _slopeAngle;
        }
    }
}
