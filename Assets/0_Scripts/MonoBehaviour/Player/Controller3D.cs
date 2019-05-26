using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller3D : MonoBehaviour
{
    public bool disableAllRays;
    public LayerMask collisionMask;
    public LayerMask collisionMaskAround;
    public float FloorMaxDistanceCheck = 5;
    public bool corsbOn= true;

    const float skinWidth = 0.1f;
    [Header("Slopes")]
    public float maxClimbAngle = 60f;
    public float minClimbAngle = 0f;
    public float maxDescendAngle = 60f;
    public float minDescendAngle = 0f;
    public float precisClimbSlopeInsideWall = 0.000f;
    [Header("Precision distances")]
    public float skinWidthHeight = 0.0001f;
    [Tooltip("DO NOT CHANGE. Space the first horizontal raycast (feet) is lifted up to avoid colliding horizontally (like it was a wall) when on the edge of floor.")]
    public float precisionHeight = 0.01f;
    public float precisionSpaceFromSlideWall = 0.001f;
    public float rayExtraLengthPeak = 0.1f;

    public CapsuleCollider coll;
    public float bigCollRadius;
    public float smallCollRadius;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

    [Header("Vertical Collisions")]
    public bool showVerticalRays;
    public bool showVerticalLimits;
    public bool showDistanceCheckRays;
    public int verticalRows;
    public int verticalRaysPerRow;
    float verticalRowSpacing;
    float verticalRaySpacing;

    [Header("Horizontal Collisions")]
    public bool showHorizontalRays;
    public bool showHorizontalLimits;
    public bool showWallRays;
    public bool showWallLimits;
    public int horizontalRows;
    public int horizontalRaysPerRow;
    float horizontalRowSpacing;
    float horizontalRaySpacing;
    public float maxHeightToClimbStep=0.3f;
    // --- CORSB ---
    //"Cutting Out Raycast Skinwidth Borders". this is the 20% of horizontalRaysPerRow. 
    //It's used to only do the CORSB system to the first 20% and the last 20% (border rays)
    int corsbBorderHorRaysPerRow;
    Color corsbColor = new Color(0, 0, 0);
    [Range(0, 49)]
    public int corsbBorderPercent = 20;

    [Header("In Water Collisions")]
    public bool showWaterRays;
    public int aroundRaysPerCircle;
    public int aroundCircles;
    public float aroundRaycastsLength = 3f;
    float aroundCirclesSpacing;
    float aroundAngleSpacing;

    public enum MovingState
    {
        none,
        wall,
        climbing,
        descending,
        sliping,
        crossingPeak,
        climbStep
    }

    public enum SlideState // for sliding against WALLS
    {
        none,
        left,
        right
    }

    private void Start()
    {
        CalculateRaySpacing();
        //print("bounds.size.z = " + coll.bounds.size.z+"bounds.size.y = "+ coll.bounds.size.y);
    }

    public void Move(Vector3 vel)
    {
        //AdjustColliderSize(vel);
        UpdateRaycastOrigins();
        collisions.ResetVertical();
        collisions.ResetHorizontal();
        collisions.ResetClimbingSlope();
        collisions.startVel = vel;
        print("Start Vel = " + vel.ToString("F4"));
        Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.blue);
        if (vel.x != 0 || vel.z != 0)
        {
            NewHorizontalCollisions2(ref vel);
        }
        //Debug.Log("Middle Vel = " + vel.ToString("F4") + "; MovingState = " + collisions.moveSt + "; below = " + collisions.below);
        if (vel.y != 0 || vel.x != 0 || vel.z != 0)
        {
            NewVerticalCollisions2(ref vel);
        }
        Debug.Log("End Vel = " + vel.ToString("F4")+ "; MovingState = "+ collisions.moveSt + "; below = " +collisions.below);
        if (collisions.lastMoveSt == MovingState.crossingPeak && collisions.moveSt != MovingState.crossingPeak)
        {
            Debug.LogError("We stopped crossing peak");
        }
        if (collisions.lastMoveSt == MovingState.none && collisions.moveSt == MovingState.climbing)
        {
            Debug.LogError("We started climbing");
        }
        VerticalCollisionsDistanceCheck(ref vel);

        UpdateSafeBelow();

        transform.Translate(vel, Space.World);
    }

    bool colliderChanged = false;
    void AdjustColliderSize(Vector3 vel)
    {
        if (vel.x != 0 || vel.z != 0)
        {
            if (!colliderChanged)
            {
                coll.radius = bigCollRadius;
                CalculateRaySpacing();
                colliderChanged = true;
            }

        }
        else
        {
            if (colliderChanged)
            {
                coll.radius = smallCollRadius;
                CalculateRaySpacing();
                colliderChanged = false;
            }
        }
    }

    float GetSlopeAngle(RaycastHit hit)
    {
        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        return slopeAngle;
    }

    void ClimbSlope(ref Vector3 vel, Raycast rayCast)
    {
        //Debug.Log("Start ClimbSlope = " + vel.ToString("F4") + "; MovingState = " + collisions.moveSt + "; below = " + collisions.below);
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        //Plane slopePlane = new Plane(rayCast.ray.normal.normalized,rayCast.ray.point);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        //Plane movementPlane = new Plane(movementNormal, raycastOrigins.Center);
        Vector3 climbVel = Vector3.Cross(rayCast.ray.normal, movementNormal).normalized;
        //print("movementNormal= " + movementNormal.ToString("F5")+"; ClimbVel= "+climbVel.ToString("F5"));
        climbVel *= moveDistance;
        if (rayCast.vel.y <= climbVel.y)
        {
            //print("CLIMBING");
            vel = climbVel;
            collisions.below = true;
            collisions.moveSt = MovingState.climbing;
            collisions.slopeAngle = rayCast.slopeAngle;
            collisions.realSlopeAngle = Mathf.Asin(climbVel.y / climbVel.magnitude) * Mathf.Rad2Deg;
            //print("REAL SLOPE ANGLE = " + collisions.realSlopeAngle);
            //print("CLIMBING: Angle = " + rayCast.slopeAngle + "; old Vel= " + horVel.ToString("F5") + "; vel= " + vel.ToString("F5") +
            //    "; old magnitude=" + horVel.magnitude + "; new magnitude = " + vel.magnitude);
            if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 3);

        }
        //Debug.Log("Finish ClimbSlope = " + vel.ToString("F4") + "; MovingState = " + collisions.moveSt + "; below = " + collisions.below);
    }

    void DescendSlope(ref Vector3 vel, Raycast rayCast)
    {
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        //Plane slopePlane = new Plane(rayCast.ray.normal.normalized,rayCast.ray.point);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        //Plane movementPlane = new Plane(movementNormal, raycastOrigins.Center);
        Vector3 climbVel = Vector3.Cross(rayCast.ray.normal, movementNormal).normalized;
        //print("movementNormal= " + movementNormal.ToString("F5")+"; ClimbVel= "+climbVel.ToString("F5"));
        climbVel *= moveDistance;
        if (rayCast.vel.y <= 0 && climbVel.y < 0)//NO SE CON SEGURIDAD SI ESTA BIEN ESTA COMPROBACION
        {
            vel = climbVel;
            collisions.below = true;
            collisions.moveSt = MovingState.descending;
            collisions.slopeAngle = rayCast.slopeAngle;
            if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 3);

        }
    }

    void SlipSlope(ref Vector3 vel, Raycast rayCast)
    {
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        Vector3 wallHorNormal = new Vector3(rayCast.ray.normal.x, 0, rayCast.ray.normal.z).normalized;
        if (!disableAllRays) Debug.DrawRay(rayCast.ray.point, wallHorNormal * 2, Color.red, 3);
        Vector3 movementNormal = new Vector3(wallHorNormal.z, 0, -wallHorNormal.x).normalized;
        if (!disableAllRays) Debug.DrawRay(rayCast.origin, movementNormal * 2, Color.yellow, 3);
        Vector3 slipDir = Vector3.Cross(rayCast.ray.normal, movementNormal).normalized;
        if (!disableAllRays) Debug.DrawRay(rayCast.origin, slipDir * 2, Color.green, 3);
        Vector3 slipVel = (slipDir * vel.y) + horVel;
        //slipVel.y = vel.y;
        //float angWithWall = Vector3.Angle(wallHorNormal, horVel);

        vel = slipVel;
        collisions.below = false;
        collisions.moveSt = MovingState.sliping;
        collisions.slopeAngle = rayCast.slopeAngle;
        collisions.verWall = collisions.closestVerRaycast.ray.transform.gameObject;
        if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 3);
    }

    //public void StartWallJump(GameObject wall)
    //{
    //    wallJumping = true;
    //    wallJumpWall = wall;
    //}

    //public void StopWallJump()
    //{
    //    wallJumping = false;
    //    wallJumpWall = null;
    //}

    MovingState CheckSlopeType(ref Vector3 vel, Raycast ray)
    {
        RaycastHit hit = ray.ray;
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        Vector3 climbVel = Vector3.Cross(hit.normal, movementNormal).normalized;
        climbVel *= moveDistance;

        // --- AXIS X & Z ---
        if(ray.axis == Axis.X || ray.axis == Axis.Z)
        {
            if (climbVel.y > 0 && ray.slopeAngle <= maxClimbAngle && ray.slopeAngle > minClimbAngle)
            {
                return MovingState.climbing;
            }
            else if (climbVel.y < 0 && ray.slopeAngle <= maxDescendAngle && ray.slopeAngle > minDescendAngle)
            {
                return MovingState.descending;
            }
            else
            {
                return MovingState.wall;
            }
        }
        else // --- AXIS Y ---
        {
            if (climbVel.y > 0 && ray.slopeAngle <= maxClimbAngle && ray.slopeAngle > minClimbAngle)
            {
                return MovingState.climbing;
            }
            else if (climbVel.y < 0 && ray.slopeAngle <= maxDescendAngle && ray.slopeAngle > minDescendAngle)
            {
                return MovingState.descending;
            }
            else if (ray.axis == Axis.Y && ((vel.y <= 0 && ray.slopeAngle > maxDescendAngle) || (vel.y > 0 && ray.slopeAngle != 0)))
            {
                return MovingState.sliping;
            }
            else
            {
                return MovingState.none;//FLOOR
            }
        }

    }

    #region --- COLLISIONS CHECKING (RAYCASTS FUNCTIONS) --- 

    public void AroundCollisions()
    {
        float rayLength = aroundRaycastsLength + skinWidth;
        Vector3 center = raycastOrigins.Center;
        float radius = raycastOrigins.AroundRadius;
        Vector3 circlesOrigin = raycastOrigins.BottomEnd;
        Vector3 circleOrigin = circlesOrigin;
        //print("----------NEW SET OF RAYS------------");
        for (int i = 0; i < aroundCircles; i++)
        {
            circleOrigin.y = circlesOrigin.y + (i * aroundCirclesSpacing);
            //print("Circle Origin= " + circleOrigin.ToString("F4"));
            for (int j = 0; j < aroundRaysPerCircle; j++)
            {
                float angle = (j * aroundAngleSpacing) * Mathf.Deg2Rad;
                float px = center.x + radius * Mathf.Cos(angle);
                float pz = center.z + radius * Mathf.Sin(angle);
                Vector3 rayCrossPoint = new Vector3(px, circleOrigin.y, pz);
                Vector3 finalDir = (rayCrossPoint - center).normalized;

                RaycastHit hit;
                if (showWaterRays && !disableAllRays)
                {
                    Debug.DrawRay(center, finalDir * rayLength, Color.red);
                }
                if (Physics.Raycast(center, finalDir, out hit, rayLength, collisionMaskAround, QueryTriggerInteraction.Ignore))
                {
                    collisions.around = true;
                }
            }
        }
    }

    void WallSlide(ref Vector3 vel, Raycast rayCast)
    {
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        float wallAngle = Vector3.Angle(rayCast.ray.normal, Vector3.forward);
        Vector3 normal = -new Vector3(rayCast.ray.normal.x, 0, rayCast.ray.normal.z).normalized;
        float angle = Vector3.Angle(normal, horVel);
        float a = Mathf.Sin(angle * Mathf.Deg2Rad) * horVel.magnitude;
        Vector3 movementNormal = Vector3.up;
        Vector3 slideVel = Vector3.Cross(normal, movementNormal).normalized;
        //LEFT OR RIGHT ORIENTATION?
        float ang = Vector3.Angle(slideVel, horVel);
        slideVel = ang > 90 ? -slideVel : slideVel;
        //print("SLIDE ANGLE= " + angle + "; vel = " + vel + "; slideVel = " + slideVel.ToString("F4") + "; a = " + a + "; wallAngle = " + wallAngle + "; distanceToWall = " + rayCast.distance);
        slideVel *= a;
        SlideState slideSt = ang > 90 ? SlideState.right : SlideState.left;
        //print("------------SLIDE STATE ------------ = " + slideSt);

        collisions.slideSt = slideSt;
        vel = new Vector3(slideVel.x, vel.y, slideVel.z);
        collisions.moveSt = MovingState.wall;
        collisions.wallAngle = wallAngle;
        if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, slideVel.normalized * 2, Color.green, 3);
    }

    bool SecondWallSlide(ref Vector3 vel, Raycast rayCast, Vector3 originalVel)
    {
        Vector3 horVel = new Vector3(originalVel.x, 0, originalVel.z);
        float wallAngle = Vector3.Angle(rayCast.ray.normal, Vector3.forward);
        Vector3 normal = -new Vector3(rayCast.ray.normal.x, 0, rayCast.ray.normal.z).normalized;
        float angle = Vector3.Angle(normal, horVel);
        float a = Mathf.Sin(angle * Mathf.Deg2Rad) * horVel.magnitude;
        Vector3 movementNormal = Vector3.up;
        Vector3 slideVel = Vector3.Cross(normal, movementNormal).normalized;
        //LEFT OR RIGHT ORIENTATION?
        float ang = Vector3.Angle(slideVel, horVel);
        slideVel = ang > 90 ? -slideVel : slideVel;
        //print("SLIDE ANGLE= " + angle + "; vel = " + vel + "; slideVel = " + slideVel.ToString("F4") + "; a = " + a + "; wallAngle = " + wallAngle + "; distanceToWall = " + rayCast.distance);
        slideVel *= a;
        SlideState slideSt = ang > 90 ? SlideState.right : SlideState.left;
        //print("------------SLIDE STATE ------------ = " + slideSt);
        if (slideSt == collisions.slideSt)
        {
            collisions.slideSt = slideSt;
            vel = new Vector3(slideVel.x, vel.y, slideVel.z);
            collisions.moveSt = MovingState.wall;
            collisions.wallAngle = wallAngle;
            if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, slideVel.normalized * 2, Color.green, 3);
            return true;
        }
        else
        {
            //Debug.LogWarning("-----------SECOND WALL SLIDE HAS WRONG DIRECTION---------- = " + slideSt);
            horVel = horVel * (rayCast.distance - skinWidth);
            vel = new Vector3(horVel.x, vel.y, horVel.z);
            collisions.wallAngle2 = rayCast.wallAngle;
            return false;
        }
    }

    void WallSlideCollisions(ref Vector3 vel)
    {
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float rayLength = horVel.magnitude + skinWidth;
        horVel = horVel.normalized;
        float directionX = 0, directionZ = 0; ;
        Vector3 wallNormal = new Vector3(collisions.wallNormal.x, 0, collisions.wallNormal.z).normalized;

        if (vel.x != 0)
        {
            directionX = Mathf.Sign(vel.x);
            Vector3 rowsOriginX = directionX == 1 ? raycastOrigins.BottomRFCornerReal : raycastOrigins.BottomLFCornerReal;
            //LEAVE SAFE SPACE FROM WALL 
            rowsOriginX += wallNormal * precisionSpaceFromSlideWall;
            for (int i = 0; i < horizontalRows; i++)
            {
                Vector3 rowOriginX = rowsOriginX;
                rowOriginX.y = (rowsOriginX.y) + i * horizontalRowSpacing;
                //For drawing the character collider limits only
                Vector3 lastOriginX = rowOriginX;
                for (int j = 0; j < horizontalRaysPerRow; j++)
                {
                    Vector3 rayOriginX = rowOriginX + Vector3.back * (j * horizontalRaySpacing);
                    if (showWallLimits && !disableAllRays)
                    {
                        Debug.DrawLine(lastOriginX, rayOriginX, Color.blue);
                    }
                    lastOriginX = rayOriginX;
                    rayOriginX += (-horVel * skinWidth);
                    RaycastHit hit;
                    if (showWallRays && !disableAllRays)
                    {
                        Debug.DrawRay(rayOriginX, horVel * rayLength, Color.yellow);
                    }

                    if (Physics.Raycast(rayOriginX, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycastSlide.distance && hit.transform.gameObject != collisions.horWall)
                        {
                            collisions.closestHorRaycastSlide = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Axis.X, i, 0);
                        }
                    }
                }
            }
        }

        if (vel.z != 0)
        {
            directionZ = Mathf.Sign(vel.z);
            Vector3 rowsOriginZ = directionZ == 1 ? raycastOrigins.BottomLFCornerReal : raycastOrigins.BottomLBCornerReal;
            //LEAVE SAFE SPACE FROM WALL 
            rowsOriginZ += wallNormal * precisionSpaceFromSlideWall;
            for (int i = 0; i < horizontalRows; i++)
            {
                Vector3 rowOriginZ = rowsOriginZ;
                rowOriginZ.y = (rowsOriginZ.y) + i * horizontalRowSpacing;
                //For drawing the character collider limits only
                Vector3 lastOriginZ = rowOriginZ;
                for (int j = 0; j < horizontalRaysPerRow; j++)
                {
                    Vector3 rayOriginZ = rowOriginZ + Vector3.right * (j * horizontalRaySpacing);
                    if (showWallLimits && !disableAllRays)
                    {
                        Debug.DrawLine(lastOriginZ, rayOriginZ, Color.blue);
                    }
                    lastOriginZ = rayOriginZ;
                    rayOriginZ += (-horVel * skinWidth);

                    RaycastHit hit;
                    //Debug.DrawRay(rayOriginZ, horVel * rayLength, Color.yellow);

                    if (Physics.Raycast(rayOriginZ, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycastSlide.distance && hit.transform.gameObject != collisions.horWall)
                        {
                            collisions.closestHorRaycastSlide = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Axis.Z, i, 0);
                        }
                    }
                }
            }
        }

        if (collisions.closestHorRaycastSlide.axis != Axis.none)//si ha habido una collision horizontal
        {
            MovingState value = collisions.closestHorRaycastSlide.row == 0 ? CheckSlopeType(ref vel, collisions.closestHorRaycastSlide) : MovingState.wall;
            //print("---------- SECOND COLLISION HOR: " + value + "; slopeAngle=" + collisions.closestHorRaycastSlide.slopeAngle);
            switch (value)//con que tipo de objeto collisionamos? pared/cuesta arriba/cuesta abajo
            {
                #region --- Wall --- 
                case MovingState.wall:
                    //check if the "wall" is not just the floor/really small ridge
                    bool validWall = true;
                    float heightToPrecisionHeight = precisionHeight - (collisions.closestHorRaycast.origin.y - raycastOrigins.BottomLBCornerReal.y);
                    if (heightToPrecisionHeight <= 0)
                    {
                        validWall = true;
                    }
                    else
                    {
                        Vector3 rayOriginAux = collisions.closestHorRaycast.origin + Vector3.up * heightToPrecisionHeight;
                        RaycastHit hitAux;
                        if (Physics.Raycast(rayOriginAux, horVel, out hitAux, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                        {
                            float slopeAngle = GetSlopeAngle(hitAux);
                            if (slopeAngle == collisions.closestHorRaycast.slopeAngle)
                            {
                                validWall = true;
                            }
                        }
                    }
                    if (validWall)
                    {
                        if (collisions.wallAngleOld2 != collisions.closestHorRaycastSlide.wallAngle)
                        {
                            //print("APPROACHING WALL: " + "distance = " + collisions.closestHorRaycastSlide.distance);
                            horVel = horVel * (collisions.closestHorRaycastSlide.distance - skinWidth);
                            vel = new Vector3(horVel.x, vel.y, horVel.z);
                            collisions.wallAngle2 = collisions.closestHorRaycastSlide.wallAngle;
                        }
                        else
                        {

                            if (SecondWallSlide(ref vel, collisions.closestHorRaycastSlide, collisions.closestHorRaycast.vel))
                            {
                                collisions.horCollisionsPoint = collisions.closestHorRaycast.ray.point;
                                collisions.wallNormal = collisions.closestHorRaycast.ray.normal;
                                collisions.horWall = collisions.closestHorRaycast.ray.transform.gameObject;
                                switch (collisions.closestHorRaycast.axis)
                                {
                                    case Axis.X:
                                        collisions.left = directionX == -1;
                                        collisions.right = directionX == 1;
                                        break;
                                    case Axis.Z:
                                        collisions.behind = directionZ == -1;
                                        collisions.foward = directionZ == 1;
                                        break;
                                }
                            }
                        }
                    }
                    break;
                #endregion
                #region --- Climbing ---
                case MovingState.climbing:
                    //print("AUXILIAR RAYS FOR DISTANCE CALCULATION");
                    if (!disableAllRays) Debug.DrawRay(collisions.closestHorRaycastSlide.origin, horVel * rayLength, Color.cyan, 4);

                    float distanceToSlopeStart = 0;
                    if (collisions.slopeAngleOld != collisions.closestHorRaycastSlide.slopeAngle)
                    {
                        distanceToSlopeStart = collisions.closestHorRaycastSlide.distance - skinWidth;
                        horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                    }
                    ClimbSlope(ref vel, collisions.closestHorRaycastSlide);
                    horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                    //--------------------- CHECK FOR NEXT SLOPE/WALL -------------------------------------
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (horVelAux.magnitude + skinWidth);
                    Vector3 rayOrigin = collisions.closestHorRaycastSlide.origin + Vector3.up * vel.y;
                    RaycastHit hit;
                    if (!disableAllRays) Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.yellow, 4);

                    if (Physics.Raycast(rayOrigin, horVelAux, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        //+print("HIT  with angle = " + slopeAngle);
                        if (!disableAllRays) Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.magenta, 4);

                        if (slopeAngle != collisions.slopeAngle)
                        {
                            horVelAux = horVelAux.normalized * (hit.distance - skinWidth);
                            //tan(realAngle)=y/xz;
                            float y = vel.y;
                            if (slopeAngle > maxClimbAngle)//IF IT'S A WALL
                            {
                                y = Mathf.Tan(collisions.realSlopeAngle * Mathf.Deg2Rad) * horVelAux.magnitude;
                            }
                            vel = new Vector3(horVelAux.x, y, horVelAux.z);
                            print("HIT NEW SLOPE/Wall with angle = " + slopeAngle);
                            //vel = new Vector3(horVelAux.x, vel.y, horVelAux.z);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    break;
                    #endregion
            }
        }
    }

    void NewHorizontalCollisions2(ref Vector3 vel)
    {
        #region Raycasts
        collisions.horRaycastsX = new Raycast[horizontalRows, horizontalRaysPerRow];
        collisions.horRaycastsZ = new Raycast[horizontalRows, horizontalRaysPerRow];
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float rayLength = horVel.magnitude + skinWidth;
        horVel = horVel.normalized;
        float directionX = 0, directionZ = 0;

        //VARIABLES FOR "Cutting Out Raycast's Skinwidth in Borders"
        float corsbAngle = 0;

        #region Raycasts X
        if (vel.x != 0)
        {
            directionX = Mathf.Sign(vel.x);
            Vector3 rowsOriginX = directionX == 1 ? raycastOrigins.BottomRFCornerReal : raycastOrigins.BottomLFCornerReal;
            corsbAngle = Vector3.Angle(Vector3.forward, horVel);

            //print("CONTROLLER 3D: " + directionX + "*X corsbAngle = " + corsbAngle + "; corsbBorderHorRaysPerRow = " + corsbBorderHorRaysPerRow +
            //    "; (horizontalRaysPerRow - corsbBorderHorRaysPerRow) = " + (horizontalRaysPerRow - corsbBorderHorRaysPerRow));
            for (int i = 0; i < horizontalRows; i++)//ROWS
            {
                Vector3 rowOriginX = rowsOriginX;
                rowOriginX.y = (rowsOriginX.y) + i * horizontalRowSpacing;
                if (i == 0)
                {
                    rowOriginX += Vector3.up * skinWidthHeight;
                }
                else if (i == horizontalRows - 1)
                {
                    rowOriginX += Vector3.down * skinWidthHeight;
                }
                Vector3 lastOriginX = rowOriginX;
                for (int j = 0; j < horizontalRaysPerRow; j++)//COLUMNS
                {
                    Vector3 rayOriginX = rowOriginX + Vector3.back * (j * horizontalRaySpacing);

                    #region --- CORSB SYSTEM ---
                    // --- CORSB SYSTEM ---    
                    //VARIABLES FOR CORSB
                    float corsbDistanceToBorder = 0;
                    float corsbSkinWidth = skinWidth;
                    float corsbRayLength = rayLength;

                        if (((j < corsbBorderHorRaysPerRow && corsbAngle > 90) || (j >= (horizontalRaysPerRow - corsbBorderHorRaysPerRow) && corsbAngle < 90)) && corsbOn)
                        {
                            float auxCorsbSkinWidth = float.MaxValue;
                            float auxAngle;
                            if (j < corsbBorderHorRaysPerRow && corsbAngle > 90)//primer 20% de rayos
                            {
                                auxAngle = 180 - corsbAngle;
                                corsbDistanceToBorder = (float)j * (float)horizontalRaySpacing;
                                //print("CONTROLLER 3D: CORSB system checking started (Part1)-> j = " + j + "; corsbDistanceToBorder = " + corsbDistanceToBorder);
                            }
                            else //último 20%
                            {
                                auxAngle = corsbAngle;
                                corsbDistanceToBorder = (float)(horizontalRaysPerRow - (j + 1)) * (float)horizontalRaySpacing;
                                //print("CONTROLLER 3D: CORSB system checking started (Part2)-> j = " + j + "; corsbDistanceToBorder = " + corsbDistanceToBorder);
                            }
                            auxCorsbSkinWidth = corsbDistanceToBorder / Mathf.Cos(auxAngle * Mathf.Deg2Rad);
                            //print("CONTROLLER 3D: CORSB system checking: i = " + i + "; j = " + j + "; corsbAngle = " + corsbAngle +
                            //    "; skinWidth = " + skinWidth + "; auxCorsbSkinWidth = " + auxCorsbSkinWidth + "; corsbDistanceToBorder = " +
                            //    corsbDistanceToBorder + "; Mathf.Cos(auxAngle * Mathf.Deg2Rad) = " + Mathf.Cos(auxAngle * Mathf.Deg2Rad));
                            if (auxCorsbSkinWidth < skinWidth)
                            {
                                corsbSkinWidth = auxCorsbSkinWidth;
                                corsbRayLength = rayLength - (skinWidth - auxCorsbSkinWidth);
                                //print("CONTROLLER 3D: CORSB system activated!");
                            }
                        }
                    // --- CORSB SYSTEM END ---
                    #endregion

                    if (showHorizontalLimits && !disableAllRays)
                    {
                        Debug.DrawLine(lastOriginX, rayOriginX, Color.blue);
                    }
                    lastOriginX = rayOriginX;
                    rayOriginX += (-horVel * corsbSkinWidth);
                    RaycastHit hit;
                    if (showHorizontalRays && !disableAllRays)
                    {
                        Debug.DrawRay(rayOriginX, horVel * corsbRayLength, (corsbSkinWidth < skinWidth ? corsbColor : Color.red));
                    }

                    if (Physics.Raycast(rayOriginX, horVel, out hit, corsbRayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Axis.X, i, horizontalRows, corsbSkinWidth);
                        }
                        //WE STORE ALL THE RAYCASTS INFO
                        collisions.horRaycastsX[i, j] = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Axis.X, i, horizontalRows,corsbSkinWidth);
                    }
                }
            }
        }
        #endregion
        #region Raycasts Z
        if (vel.z != 0)
        {
            directionZ = Mathf.Sign(vel.z);
            Vector3 rowsOriginZ = directionZ == 1 ? raycastOrigins.BottomLFCornerReal : raycastOrigins.BottomLBCornerReal;
            corsbAngle = Vector3.Angle(Vector3.left, horVel);
            //print("CONTROLLER 3D: " + directionZ + "*Z corsbAngle = " + corsbAngle + "; corsbBorderHorRaysPerRow = " + corsbBorderHorRaysPerRow +
            //    "; (horizontalRaysPerRow - corsbBorderHorRaysPerRow) = " + (horizontalRaysPerRow - corsbBorderHorRaysPerRow)+ "; horizontalRaySpacing = " + horizontalRaySpacing);
            for (int i = 0; i < horizontalRows; i++)
            {
                Vector3 rowOriginZ = rowsOriginZ;
                rowOriginZ.y = (rowsOriginZ.y) + i * horizontalRowSpacing;
                if (i == 0)
                {
                    rowOriginZ += Vector3.up * skinWidthHeight;
                }
                else if (i == horizontalRows - 1)
                {
                    rowOriginZ += Vector3.down * skinWidthHeight;
                }
                Vector3 lastOriginZ = rowOriginZ;
                for (int j = 0; j < horizontalRaysPerRow; j++)
                {
                    Vector3 rayOriginZ = rowOriginZ + Vector3.right * (j * horizontalRaySpacing);

                    #region --- CORSB SYSTEM ---
                    // --- CORSB SYSTEM ---    
                    //VARIABLES FOR CORSB
                    float corsbDistanceToBorder = 0;
                    float corsbSkinWidth = skinWidth;
                    float corsbRayLength = rayLength;

                    if (((j < corsbBorderHorRaysPerRow && corsbAngle > 90) || (j >= (horizontalRaysPerRow - corsbBorderHorRaysPerRow) && corsbAngle < 90)) && corsbOn)
                    {
                        float auxCorsbSkinWidth = float.MaxValue;
                        float auxAngle;
                        if (j < corsbBorderHorRaysPerRow && corsbAngle > 90)//primer 20% de rayos
                        {
                            auxAngle = 180 - corsbAngle;
                            corsbDistanceToBorder = (float)j * (float)horizontalRaySpacing;
                            //print("CONTROLLER 3D: CORSB system checking started (Part1)-> j = " + j + "; corsbDistanceToBorder = " + corsbDistanceToBorder);
                        }
                        else //último 20%
                        {
                            auxAngle = corsbAngle;
                            corsbDistanceToBorder = (float)(horizontalRaysPerRow - (j + 1)) * (float)horizontalRaySpacing;
                            //print("CONTROLLER 3D: CORSB system checking started (Part2)-> j = " + j + "; corsbDistanceToBorder = " + corsbDistanceToBorder);
                        }
                        auxCorsbSkinWidth = corsbDistanceToBorder / Mathf.Cos(auxAngle * Mathf.Deg2Rad);
                        //print("CONTROLLER 3D: CORSB system checking: i = " + i + "; j = " + j + "; corsbAngle = " + corsbAngle +
                        //    "; skinWidth = " + skinWidth + "; auxCorsbSkinWidth = " + auxCorsbSkinWidth + "; corsbDistanceToBorder = " +
                        //    corsbDistanceToBorder + "; Mathf.Cos(auxAngle * Mathf.Deg2Rad) = " + Mathf.Cos(auxAngle * Mathf.Deg2Rad));
                        if (auxCorsbSkinWidth < skinWidth)
                        {
                            corsbSkinWidth = auxCorsbSkinWidth;
                            corsbRayLength = rayLength - (skinWidth - auxCorsbSkinWidth);
                            //print("CONTROLLER 3D: CORSB system activated!");
                        }
                    }
                    // --- CORSB SYSTEM END ---
                    #endregion

                    if (showHorizontalLimits && !disableAllRays)
                    {
                        Debug.DrawLine(lastOriginZ, rayOriginZ, Color.blue);
                    }
                    lastOriginZ = rayOriginZ;
                    rayOriginZ += (-horVel * corsbSkinWidth);
                    RaycastHit hit;
                    if (showHorizontalRays && !disableAllRays)
                    {
                        Debug.DrawRay(rayOriginZ, horVel * corsbRayLength, (corsbSkinWidth < skinWidth ? corsbColor : Color.red));
                    }

                    if (Physics.Raycast(rayOriginZ, horVel, out hit, corsbRayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Axis.Z, i, horizontalRows, corsbSkinWidth);
                        }
                        //WE STORE ALL THE RAYCASTS INFO
                        collisions.horRaycastsZ[i, j] = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Axis.Z, i, horizontalRows, corsbSkinWidth);
                    }
                }
            }
        }
        #endregion
        #endregion

        if (collisions.closestHorRaycast.axis != Axis.none)//si ha habido una collision horizontal
        {
            MovingState value = collisions.closestHorRaycast.row == 0 ? CheckSlopeType(ref vel, collisions.closestHorRaycast) : MovingState.wall;
            #region --- CHECK FOR CLIMBSTEP ---
            if (value == MovingState.wall)//check for climbStep
            {
                float rayHeight = collisions.closestHorRaycast.row * horizontalRowSpacing;
                if (rayHeight <= maxHeightToClimbStep)
                {
                    //CHECK IF THE WALL IS SHORT
                    bool success = true;
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (horVelAux.magnitude + collisions.closestHorRaycast.skinWidth);
                    Vector3 rayOrigin = collisions.closestHorRaycast.origin + Vector3.up * vel.y;
                    RaycastHit hit;
                    if (!disableAllRays) Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.yellow, 4);

                    if (Physics.Raycast(rayOrigin, horVelAux, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {

                        //for(int i = collisions.closestHorRaycast.row+1; i< collisions.horRaycastsX.GetLength(0); i++)
                        //{
                        //    //check if there was a hit 
                        //}
                        if (success)
                        {
                            value = MovingState.climbStep;
                        }
                    }
                }
            }
            #endregion
            //print("COLLISION HOR: " + value + "; slopeAngle=" + collisions.closestHorRaycast.slopeAngle);
            switch (value)//con que tipo de objeto collisionamos? pared/cuesta arriba/cuesta abajo
            {
                #region Wall
                case MovingState.wall:
                    if (!disableAllRays) Debug.DrawRay(collisions.closestHorRaycast.origin, horVel * 0.5f, Color.white);
                    //check if the "wall" is not just the floor/really small ridge
                    bool validWall = false;
                    if (collisions.lastMoveSt == MovingState.descending)
                    {
                        float heightToPrecisionHeight = precisionHeight - (collisions.closestHorRaycast.origin.y - raycastOrigins.BottomLBCornerReal.y);
                        if (heightToPrecisionHeight <= 0)
                        {
                            validWall = true;
                        }
                        else
                        {
                            Vector3 rayOriginAux = collisions.closestHorRaycast.origin + Vector3.up * heightToPrecisionHeight;
                            RaycastHit hitAux;
                            if (Physics.Raycast(rayOriginAux, horVel, out hitAux, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                            {
                                float slopeAngle = GetSlopeAngle(hitAux);
                                if (slopeAngle == collisions.closestHorRaycast.slopeAngle)
                                {
                                    validWall = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        validWall = true;
                    }
                    if (validWall)
                    {
                        if (collisions.wallAngleOld != collisions.closestHorRaycast.wallAngle)
                        {
                            //print("APPROACHING WALL: " + "distance = " + collisions.closestHorRaycast.distance);
                            horVel = horVel * (collisions.closestHorRaycast.distance - skinWidth);
                            vel = new Vector3(horVel.x, vel.y, horVel.z);
                            collisions.wallAngle = collisions.closestHorRaycast.wallAngle;
                        }
                        else
                        {
                            WallSlide(ref vel, collisions.closestHorRaycast);
                        }
                        collisions.horCollisionsPoint = collisions.closestHorRaycast.ray.point;
                        collisions.wallNormal = collisions.closestHorRaycast.ray.normal;
                        collisions.horWall = collisions.closestHorRaycast.ray.transform.gameObject;
                        switch (collisions.closestHorRaycast.axis)
                        {
                            case Axis.X:
                                collisions.left = directionX == -1;
                                collisions.right = directionX == 1;
                                break;
                            case Axis.Z:
                                collisions.behind = directionZ == -1;
                                collisions.foward = directionZ == 1;
                                break;
                        }
                        RaycastHit hitAux;
                        if (Physics.Raycast(collisions.closestHorRaycast.origin, Vector3.down, out hitAux, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                        {
                            collisions.floorAngle = GetSlopeAngle(hitAux);
                        }
                        WallSlideCollisions(ref vel);
                    }
                    break;
                #endregion
                #region Climbing
                case MovingState.climbing:
                    //Debug.Log("Start climbing = " + vel.ToString("F4") + "; MovingState = " + collisions.moveSt + "; below = " + collisions.below);
                    //print("AUXILIAR RAYS FOR DISTANCE CALCULATION");
                    if (!disableAllRays) Debug.DrawRay(collisions.closestHorRaycast.origin, horVel * rayLength, Color.cyan, 4);

                    float distanceToSlopeStart = 0;
                    if (collisions.slopeAngleOld != collisions.closestHorRaycast.slopeAngle)
                    {
                        distanceToSlopeStart = collisions.closestHorRaycast.distance - collisions.closestHorRaycast.skinWidth;
                        horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                    }
                    Debug.Log("Start ClimbSlope = " + vel.ToString("F4") + "; MovingState = " + collisions.moveSt + "; below = " + collisions.below);
                    ClimbSlope(ref vel, collisions.closestHorRaycast);
                    Debug.Log("Finish ClimbSlope = " + vel.ToString("F4") + "; MovingState = " + collisions.moveSt + "; below = " + collisions.below);
                    horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                    Debug.Log("After ClimbSlope = " + vel.ToString("F4") + "; MovingState = " + collisions.moveSt + "; below = " + collisions.below);
                    //--------------------- CHECK FOR NEXT SLOPE/WALL -------------------------------------
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (horVelAux.magnitude + collisions.closestHorRaycast.skinWidth);
                    Vector3 rayOrigin = collisions.closestHorRaycast.origin + Vector3.up * vel.y;
                    RaycastHit hit;
                    if (!disableAllRays) Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.yellow, 4);

                    if (Physics.Raycast(rayOrigin, horVelAux, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        //+print("HIT  with angle = " + slopeAngle);
                        if (!disableAllRays) Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.magenta, 4);

                        if (slopeAngle != collisions.slopeAngle)
                        {
                            horVelAux = horVelAux.normalized * (hit.distance - collisions.closestHorRaycast.skinWidth);
                            //tan(realAngle)=y/xz;
                            float y = vel.y;
                            if (slopeAngle > maxClimbAngle)//IF IT'S A WALL
                            {
                                Debug.Log("Found a wall while climbing. realSlopeAngle = " + collisions.realSlopeAngle + "; distance to wall = " + horVelAux.magnitude);
                                y = Mathf.Tan(collisions.realSlopeAngle * Mathf.Deg2Rad) * horVelAux.magnitude;
                            }
                            vel = new Vector3(horVelAux.x, y, horVelAux.z);
                            //print("HIT NEW SLOPE/Wall with angle = " + slopeAngle);
                            //vel = new Vector3(horVelAux.x, vel.y, horVelAux.z);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    else
                    {
                        //Debug.LogWarning("Climbing slope finished!");
                        collisions.finishedClimbing = true;
                    }
                    //Debug.Log("END climbing = " + vel.ToString("F4") + "; MovingState = " + collisions.moveSt + "; below = " + collisions.below);
                    break;
                #endregion
                #region ClimbStep
                case MovingState.climbStep:
                    Debug.LogWarning("CLIMB STEP STARTED");
                    break;
                    #endregion
            }
        }
    }

    void NewVerticalCollisions2(ref Vector3 vel)
    {
        #region Raycasts
        collisions.verRaycastsY = new Raycast[verticalRows, verticalRaysPerRow];
        // ---------------------- 3D "Ortoedro" -------------------
        float directionY = Mathf.Sign(vel.y);
        float rayLength = Mathf.Abs(vel.y) + skinWidth;
        if (collisions.lastFinishedClimbing || collisions.lastMoveSt == MovingState.crossingPeak)
        {
            rayLength += rayExtraLengthPeak;
        }
        Vector3 rowsOrigin = directionY == -1 ? raycastOrigins.BottomLFCornerReal : raycastOrigins.TopLFCornerReal;
        Vector3 rowOrigin = rowsOrigin;
        Vector3 wallNormal = new Vector3(collisions.wallNormal.x, 0, collisions.wallNormal.z).normalized;
        //SEPARATION FROM WALL IN CASE WE ARE STANDING ON FLOOR AND COLLIDING WITH WALL
        if (collisions.floorAngle >= 0 && collisions.floorAngle < 0.2f)
        {
            rowOrigin += wallNormal * 0.01f;
        }

        //Check for peak under
        bool peak = false;
        MovingState lastSlopeType = MovingState.none;

        //print("----------NEW SET OF RAYS------------");
        for (int i = 0; i < verticalRows; i++)
        {
            rowOrigin.z = rowsOrigin.z - (verticalRowSpacing * i);
            Vector3 lastOrigin = rowOrigin;
            for (int j = 0; j < verticalRaysPerRow; j++)
            {
                Vector3 rayOrigin = new Vector3(rowOrigin.x + (verticalRaySpacing * j), rowOrigin.y, rowOrigin.z);
                if (showVerticalLimits && !disableAllRays)
                {
                    Debug.DrawLine(lastOrigin, rayOrigin, Color.blue);
                }
                lastOrigin = rayOrigin;
                rayOrigin += (Vector3.up * -directionY) * skinWidth;

                RaycastHit hit;
                if (showVerticalRays && !disableAllRays)
                {
                    Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.red);
                }

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore)) // throw raycast here
                {
                    float slopeAngle = GetSlopeAngle(hit);
                    //print("Vertical Hit");
                    if (hit.distance < collisions.closestVerRaycast.distance)
                    {
                        if (directionY == 1)
                        {
                            slopeAngle = slopeAngle == 180 ? slopeAngle = 0 : slopeAngle - 90;
                        }
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        collisions.closestVerRaycast = new Raycast(hit, hit.distance, vel, rayOrigin, slopeAngle, wallAngle, Axis.Y, i, 0);
                    }
                    MovingState slopeType = CheckSlopeType(ref vel, new Raycast(hit, hit.distance, vel, rayOrigin, slopeAngle, 0, Axis.Y, i, 0));
                    //if(collisions.lastMoveSt == MovingState.crossingPeak)Debug.Log("Checking for peak : Vertical collisions slopeType = " + slopeType);
                    if ((slopeType == MovingState.climbing || slopeType == MovingState.descending) && !peak)
                    {
                        if (lastSlopeType == MovingState.none)
                        {
                            lastSlopeType = slopeType;//solo puede ser climbing o descending
                        }
                        else if (lastSlopeType != slopeType)//solo puede ser que uno sea climbing y otro descending, lo cual interpretamos como estar en un peak
                        {
                            peak = true;
                            Debug.LogWarning("I'm on a peak");
                        }
                    }
                    //STORE ALL THE RAYCASTS
                    collisions.verRaycastsY[i, j] = new Raycast(hit, hit.distance, vel, rayOrigin, slopeAngle, 0, Axis.Y, i, 0);
                }
            }
        }
        #endregion

        if (collisions.closestVerRaycast.axis != Axis.none)//si ha habido una collision vertical
        {
            MovingState value;
            //print("COLLISION VER: " + value + "; slopeAngle=" + collisions.closestVerRaycast.slopeAngle);
            if (!peak)
            {
                value = CheckSlopeType(ref vel, collisions.closestVerRaycast);
                value = value == MovingState.climbing ? MovingState.none : value;
                print("Vertical Raycasts: value = "+value+ "; collisions.lastMoveSt = " + collisions.lastMoveSt + "; vel.y = " + vel.y);
                if (value == MovingState.none && collisions.lastMoveSt == MovingState.crossingPeak && vel.y <= 0)
                {
                    value = MovingState.crossingPeak;
                }
            }
            else
            {
                value = MovingState.crossingPeak;
            }

            switch (value)//con que tipo de objeto collisionamos? suelo/cuesta arriba/cuesta abajo
            {
                #region None
                case MovingState.none:
                    vel.y = (collisions.closestVerRaycast.distance - skinWidth) * directionY;
                    //rayLength = collisions.closestVerRaycast.distance;
                    if (collisions.moveSt == MovingState.climbing)//Subiendo chocamos con un techo
                    {
                        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (vel.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad));
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                    }
                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;
                    break;
                #endregion
                #region Sliping
                case MovingState.sliping:
                    SlipSlope(ref vel, collisions.closestVerRaycast);
                    //--------------------- CHECK FOR NEXT SLOPE/FLOOR -------------------------------------
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (Mathf.Abs(vel.y) + skinWidth);
                    Vector3 rayOrigin = collisions.closestVerRaycast.origin + horVelAux;
                    RaycastHit hit;
                    if (!disableAllRays) Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.yellow, 4);

                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        if (!disableAllRays)
                        {
                            Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.magenta, 4);
                        }

                        print("Slope Angle: " + collisions.slopeAngle + "; new slope angle: " + slopeAngle);
                        if (slopeAngle != collisions.slopeAngle)
                        {
                            Debug.LogWarning("SlipSlope: Clipping through floor avoided by stoping vel.y");
                            vel.y = -(hit.distance - skinWidth);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    break;
                #endregion
                #region Descending
                case MovingState.descending:
                    if (collisions.moveSt != MovingState.climbing)
                    {
                        float distanceToSlopeStart = 0;
                        distanceToSlopeStart = collisions.closestVerRaycast.distance - skinWidth;
                        vel.y -= distanceToSlopeStart * -1;
                        DescendSlope(ref vel, collisions.closestVerRaycast);
                        vel.y += distanceToSlopeStart * -1;
                    }
                    else
                    {
                        Debug.LogError("Esto no es un error, solo quería saber si esta condición ocurría en algún momento.");
                    }
                    //--------------------- CHECK FOR NEXT SLOPE/FLOOR -------------------------------------
                    horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (Mathf.Abs(vel.y) + skinWidth);
                    rayOrigin = collisions.closestVerRaycast.origin + horVelAux;
                    if (!disableAllRays) Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.yellow, 4);

                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        if (!disableAllRays) Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.magenta, 4);

                        if (slopeAngle != collisions.slopeAngle)
                        {
                            vel.y = -(hit.distance - skinWidth);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    break;
                #endregion
                #region Arista Vertical
                case MovingState.crossingPeak:
                    if (collisions.moveSt == MovingState.climbing)
                    {
                    }
                    else
                    {
                        vel.y = 0;
                    }
                    collisions.moveSt = MovingState.crossingPeak;
                    collisions.below = true;
                    break;
                    #endregion
            }
        }
    }

    //void ProcessAllCollisions(ref Vector3 vel)
    //{
    //    if (vel.x != 0 || vel.z != 0)
    //    {
    //        NewHorizontalCollisions2(ref vel);
    //    }

    //    if (vel.y != 0 || vel.x != 0 || vel.z != 0)
    //    {
    //        NewVerticalCollisions2(ref vel);
    //    }
    //}

    void VerticalCollisionsDistanceCheck(ref Vector3 vel)
    {
        if (vel.y != 0)
        {
            float rayLength = FloorMaxDistanceCheck;
            Vector3 rowsOrigin = raycastOrigins.BottomLFCornerReal;
            Vector3 rowOrigin = rowsOrigin;
            Vector3 wallNormal = new Vector3(collisions.wallNormal.x, 0, collisions.wallNormal.z).normalized;
            if (collisions.floorAngle >= 0 && collisions.floorAngle < 0.2f)//SEPARATION FROM WALL IN CASE WE ARE STANDING ON FLOOR AN COLLIDING WITH WALL
            {
                rowOrigin += wallNormal * 0.01f;
            }
            //print("----------NEW SET OF RAYS------------");
            for (int i = 0; i < verticalRows; i++)
            {
                rowOrigin.z = rowsOrigin.z - (verticalRowSpacing * i);
                Vector3 lastOrigin = rowOrigin;
                for (int j = 0; j < verticalRaysPerRow; j++)
                {
                    if (i % 2 == 0 && j % 2 == 0)// Every even number throw a ray. This is to reduce raycasts to half since not so many are needed.
                    {
                        Vector3 rayOrigin = new Vector3(rowOrigin.x + (verticalRaySpacing * j), rowOrigin.y, rowOrigin.z);
                        lastOrigin = rayOrigin;
                        rayOrigin += Vector3.up * skinWidth;

                        RaycastHit hit;
                        if (showDistanceCheckRays && !disableAllRays)
                        {
                            Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red);
                        }


                        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                        {
                            //print("Vertical Hit");
                            if (hit.distance < collisions.distanceToFloor)
                            {
                                collisions.distanceToFloor = hit.distance;
                            }
                        }
                    }
                }
            }
            collisions.distanceToFloor -= skinWidth;
        }
    }

    //bool wallJumping = false;
    //GameObject wallJumpWall;
    //void WallJumping()
    //{
    //    if (wallJumping)
    //    {

    //    }
    //}

    #endregion

    void UpdateSafeBelow()
    {
        if (!collisions.below && collisions.lastBelow)
        {
            collisions.StartSafeBelow();
        }
        collisions.ProcessSafeBelow();
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

        //--------------------------------- BOUNDS REDUCED BY SKINWIDTH ---------------------------------
        bounds.Expand(skinWidth * -2);

        raycastOrigins.BottomEnd = new Vector3(bounds.center.x, bounds.min.y, bounds.max.z);

        raycastOrigins.Center = bounds.center;
        raycastOrigins.AroundRadius = bounds.size.z / 2;

    }

    void CalculateRaySpacing()
    {
        Bounds bounds = coll.bounds;

        horizontalRows = Mathf.Clamp(horizontalRows, 2, int.MaxValue);
        horizontalRaysPerRow = Mathf.Clamp(horizontalRaysPerRow, 2, int.MaxValue);
        corsbBorderHorRaysPerRow = (horizontalRaysPerRow * corsbBorderPercent) / 100;

        horizontalRowSpacing = bounds.size.y / (horizontalRows - 1);
        horizontalRaySpacing = bounds.size.x / (horizontalRaysPerRow - 1);

        verticalRows = Mathf.Clamp(verticalRows, 2, int.MaxValue);
        verticalRaysPerRow = Mathf.Clamp(verticalRaysPerRow, 3, int.MaxValue);

        verticalRowSpacing = (bounds.size.z) / (verticalRows - 1);
        verticalRaySpacing = bounds.size.x / (verticalRaysPerRow - 1);


        bounds.Expand(skinWidth * -2);
        //-------------------

        aroundCircles = Mathf.Clamp(aroundCircles, 3, int.MaxValue);
        aroundRaysPerCircle = Mathf.Clamp(aroundRaysPerCircle, 3, int.MaxValue);

        aroundCirclesSpacing = bounds.size.y / (aroundCircles - 1);
        aroundAngleSpacing = 360 / (aroundRaysPerCircle);
    }

    struct RaycastOrigins
    {
        public Vector3 BottomCenter, BottomEnd;//TopEnd= center x, min y, max z

        public Vector3 BottomLFCornerReal, BottomRFCornerReal, BottomLBCornerReal, BottomRBCornerReal;
        public Vector3 TopLFCornerReal, TopRFCornerReal, TopLBCornerReal, TopRBCornerReal;

        public Vector3 Center;
        public float AroundRadius;
    }

    public struct CollisionInfo
    {
        public bool above, below, lastBelow, safeBelow;
        public bool left, right;
        public bool foward, behind;
        public bool collisionHorizontal
        {
            get
            {
                return (left || right || foward || behind);
            }
            set { }
        }
        public bool around;
        public bool finishedClimbing, lastFinishedClimbing;
        public bool safeBelowStarted;
        float safeBelowTime, safeBelowMaxTime;

        public MovingState moveSt;
        public MovingState lastMoveSt;
        public float slopeAngle, slopeAngleOld, realSlopeAngle, wallAngle, wallAngleOld, wallAngle2, wallAngleOld2, floorAngle;
        public Vector3 startVel;
        public Raycast closestHorRaycast;
        public Raycast[,] horRaycastsX;
        public Raycast[,] horRaycastsZ;
        public Raycast[,] verRaycastsY;
        public Raycast closestHorRaycastSlide;
        public Raycast closestVerRaycast;
        public float distanceToFloor;
        public Vector3 horCollisionsPoint;
        public Vector3 wallNormal;
        public GameObject horWall;
        public GameObject verWall;
        public SlideState slideSt;


        public void ResetVertical()
        {
            lastBelow = below;
            above = below = false;
            closestVerRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            distanceToFloor = float.MaxValue;
            verRaycastsY = new Raycast[0, 0];
            verWall = null;
        }

        public void ResetHorizontal()
        {
            left = right = false;
            foward = behind = false;
            wallAngleOld = wallAngle;
            wallAngle = 0;
            wallAngleOld2 = wallAngle2;
            wallAngle2 = 0;
            horCollisionsPoint = Vector3.zero;
            wallNormal = Vector3.zero;
            horWall = null;
            slideSt = SlideState.none;
            startVel = Vector3.zero;
            closestHorRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            horRaycastsX = new Raycast[0, 0];
            horRaycastsZ = new Raycast[0, 0];
            closestHorRaycastSlide = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            lastFinishedClimbing = finishedClimbing;
            finishedClimbing = false;
        }

        public void ResetAround()
        {
            around = false;
        }

        public void ResetClimbingSlope()
        {
            lastMoveSt = moveSt;
            moveSt = MovingState.none;
            floorAngle = -1;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
            realSlopeAngle = 0;
        }

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
    }
}
public struct Raycast
{
    public RaycastHit ray;
    public Vector3 origin;
    public float distance;
    public Vector3 vel;
    public float slopeAngle;
    public float wallAngle;
    public Axis axis;
    public int row;//row in which the ray was thrown from
    public float rayHeightPercentage;//from 0 (feet) to 100(head)
    public float skinWidth;

    public Raycast(RaycastHit _ray, float _dist, Vector3 _vel, Vector3 _origin, float _slopeAngle = 0, float _wallAngle = 0, 
        Axis _axis = Axis.none, int _row = 0, int horizontalRows = 0, float _skinWidth = 0.1f)
    {
        ray = _ray;
        distance = _dist;
        vel = _vel;
        origin = _origin;
        axis = _axis;
        slopeAngle = _slopeAngle;
        wallAngle = _wallAngle;
        row = _row;
        if (horizontalRows == 0)
        {
            rayHeightPercentage = 0;
        }
        else
        {
            rayHeightPercentage = Mathf.Clamp((row / horizontalRows), 0, 100);
        }
        skinWidth = Mathf.Clamp(_skinWidth, 0, 0.1f);
    }
}
public enum Axis
{
    none,
    X,
    Y,
    Z
}
