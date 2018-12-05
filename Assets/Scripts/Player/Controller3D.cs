using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller3D : MonoBehaviour
{
    public bool disableAllRays;
    public LayerMask collisionMask;
    public LayerMask collisionMaskAround;
    public float FloorMaxDistanceCheck = 5;

    const float skinWidth = 0.1f;
    [Tooltip("DO NOT CHANGE. Space the first horizontal raycast (feet) is lifted up to avoid colliding horizontally (like it was a wall) when on the edge of floor.")]
    [Header("Slopes")]
    public float maxClimbAngle = 60f;
    public float minClimbAngle = 0f;
    public float maxDescendAngle = 60f;
    public float minDescendAngle = 0f;
    public float precisClimbSlopeInsideWall = 0.000f;
    [Header("Precision distances")]
    public float skinWidthHeight = 0.0001f;
    public float precisionHeight = 0.01f;
    public float precisionSpaceFromSlideWall = 0.001f;

    public struct Raycast
    {
        public RaycastHit ray;
        public Vector3 origin;
        public float distance;
        public Vector3 vel;
        public float slopeAngle;
        public float wallAngle;
        public Axis axis;
        public int row;

        public Raycast(RaycastHit _ray, float _dist, Vector3 _vel, Vector3 _origin, float _slopeAngle = 0, float _wallAngle = 0, Axis _axis = Axis.none, int _row = 0)
        {
            ray = _ray;
            distance = _dist;
            vel = _vel;
            origin = _origin;
            axis = _axis;
            slopeAngle = _slopeAngle;
            wallAngle = _wallAngle;
            row = _row;
        }
        public enum Axis
        {
            none,
            X,
            Y,
            Z
        }
    }

    public CapsuleCollider coll;
    public float bigCollRadius;
    public float smallCollRadius;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

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
        //print("Start Vel = " + vel.ToString("F4"));
        //Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.blue, 3);
        if (vel.x != 0 || vel.z != 0)
        {
            NewHorizontalCollisions2(ref vel);
        }


        if (vel.y != 0 || vel.x != 0 || vel.z != 0)
        {
            NewVerticalCollisions2(ref vel);
        }
        VerticalCollisionsDistanceCheck(ref vel);
        //print("SLOPE TYPE = " + collisions.climbSt+"; slopeAngle = "+collisions.slopeAngle+"; FinalVel = "+ vel.ToString("F5"));
        //print("FinalVel= " + vel.ToString("F5"));
        transform.Translate(vel, Space.World);
    }

    bool colliderChanged = false;
    void AdjustColliderSize(Vector3 vel)
    {
        if(vel.x !=0 || vel.z != 0)
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
            if (!disableAllRays)
            {
                Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 3);
            }

        }
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
            if (!disableAllRays)
            {
                Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 3);
            }
        }
    }

    void SlipSlope(ref Vector3 vel, Raycast rayCast)
    {
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        Vector3 wallHorNormal = new Vector3(rayCast.ray.normal.x, 0, rayCast.ray.normal.z).normalized;
        Vector3 movementNormal = new Vector3(wallHorNormal.z, 0, -wallHorNormal.x).normalized;
        Vector3 slipDir = Vector3.Cross(rayCast.ray.normal, movementNormal).normalized;
        Vector3 slipVel = (slipDir * vel.y) + horVel;
        //slipVel.y = vel.y;
        //float angWithWall = Vector3.Angle(wallHorNormal, horVel);

        vel = slipVel;
        collisions.below = false;
        collisions.moveSt = MovingState.sliping;
        collisions.slopeAngle = rayCast.slopeAngle;
        if (!disableAllRays)
        {
            Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 3);
        }
    }

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

    MovingState CheckSlopeType(ref Vector3 vel, Raycast ray)
    {
        RaycastHit hit = ray.ray;
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        Vector3 climbVel = Vector3.Cross(hit.normal, movementNormal).normalized;
        //print("movementNormal= " + movementNormal.ToString("F5")+"; ClimbVel= "+climbVel.ToString("F5"));
        climbVel *= moveDistance;
        //print("CHECK SLOPE TYPE: climbVel=" + climbVel.ToString("F4") + ";horVel= " + horVel.ToString("F4"));
        if (climbVel.y > 0 && ray.slopeAngle <= maxClimbAngle && ray.slopeAngle > minClimbAngle)
        {
            return MovingState.climbing;
        }
        else if (climbVel.y < 0 && ray.slopeAngle <= maxDescendAngle && ray.slopeAngle > minDescendAngle)
        {
            return MovingState.descending;
        }
        else if (ray.axis == Raycast.Axis.Y && ((vel.y<=0 && ray.slopeAngle > maxDescendAngle)||(vel.y>0 && ray.slopeAngle!=0)))
        {
            return MovingState.sliping;
        }
        else if ((ray.axis == Raycast.Axis.X || ray.axis == Raycast.Axis.Z))
        {
            return MovingState.wall;
        }
        else if (ray.axis == Raycast.Axis.Y)
        {
            return MovingState.none;//FLOOR
        }
        else
        {
            return MovingState.none;
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
        if (!disableAllRays)
        {
            Debug.DrawRay(raycastOrigins.Center, slideVel.normalized * 2, Color.green, 3);
        }
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
        print("SLIDE ANGLE= " + angle + "; vel = " + vel + "; slideVel = " + slideVel.ToString("F4") + "; a = " + a + "; wallAngle = " + wallAngle + "; distanceToWall = " + rayCast.distance);
        slideVel *= a;
        SlideState slideSt = ang > 90 ? SlideState.right : SlideState.left;
        print("------------SLIDE STATE ------------ = " + slideSt);
        if (slideSt == collisions.slideSt)
        {
            collisions.slideSt = slideSt;
            vel = new Vector3(slideVel.x, vel.y, slideVel.z);
            collisions.moveSt = MovingState.wall;
            collisions.wallAngle = wallAngle;
            if (!disableAllRays)
            {
                Debug.DrawRay(raycastOrigins.Center, slideVel.normalized * 2, Color.green, 3);
            }
            return true;
        }
        else
        {
            Debug.LogWarning("-----------SECOND WALL SLIDE HAS WRONG DIRECTION---------- = " + slideSt);
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
                        if (hit.distance < collisions.closestHorRaycastSlide.distance && hit.transform.gameObject != collisions.wall)
                        {
                            collisions.closestHorRaycastSlide = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Raycast.Axis.X, i);
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
                        if (hit.distance < collisions.closestHorRaycastSlide.distance && hit.transform.gameObject != collisions.wall)
                        {
                            collisions.closestHorRaycastSlide = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Raycast.Axis.Z, i);
                        }
                    }
                }
            }
        }

        if (collisions.closestHorRaycastSlide.axis != Raycast.Axis.none)//si ha habido una collision horizontal
        {
            MovingState value = collisions.closestHorRaycastSlide.row == 0 ? CheckSlopeType(ref vel, collisions.closestHorRaycastSlide) : MovingState.wall;
            print("---------- SECOND COLLISION HOR: " + value + "; slopeAngle=" + collisions.closestHorRaycastSlide.slopeAngle);
            switch (value)//con que tipo de objeto collisionamos? pared/cuesta arriba/cuesta abajo
            {
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
                            print("APPROACHING WALL: " + "distance = " + collisions.closestHorRaycastSlide.distance);
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
                                collisions.wall = collisions.closestHorRaycast.ray.transform.gameObject;
                                switch (collisions.closestHorRaycast.axis)
                                {
                                    case Raycast.Axis.X:
                                        collisions.left = directionX == -1;
                                        collisions.right = directionX == 1;
                                        break;
                                    case Raycast.Axis.Z:
                                        collisions.behind = directionZ == -1;
                                        collisions.foward = directionZ == 1;
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case MovingState.climbing:
                    //print("AUXILIAR RAYS FOR DISTANCE CALCULATION");
                    if (!disableAllRays)
                    {
                        Debug.DrawRay(collisions.closestHorRaycastSlide.origin, horVel * rayLength, Color.cyan, 4);
                    }
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
                    if (!disableAllRays)
                    {
                        Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.yellow, 4);
                    }
                    if (Physics.Raycast(rayOrigin, horVelAux, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        //+print("HIT  with angle = " + slopeAngle);
                        if (!disableAllRays)
                        {
                            Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.magenta, 4);
                        }
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
                case MovingState.descending:
                    break;
                case MovingState.none:
                    break;
            }
        }
    }

    void NewHorizontalCollisions2(ref Vector3 vel)
    {
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float rayLength = horVel.magnitude + skinWidth;
        horVel = horVel.normalized;
        float directionX = 0, directionZ = 0; ;

        if (vel.x != 0)
        {
            directionX = Mathf.Sign(vel.x);
            Vector3 rowsOriginX = directionX == 1 ? raycastOrigins.BottomRFCornerReal : raycastOrigins.BottomLFCornerReal;
            for (int i = 0; i < horizontalRows; i++)
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
                for (int j = 0; j < horizontalRaysPerRow; j++)
                {
                    Vector3 rayOriginX = rowOriginX + Vector3.back * (j * horizontalRaySpacing);
                    if (showHorizontalLimits && !disableAllRays)
                    {
                        Debug.DrawLine(lastOriginX, rayOriginX, Color.blue);
                    }
                    lastOriginX = rayOriginX;
                    rayOriginX += (-horVel * skinWidth);
                    RaycastHit hit;
                    if (showHorizontalRays && !disableAllRays)
                    {
                        Debug.DrawRay(rayOriginX, horVel * rayLength, Color.red);
                    }

                    if (Physics.Raycast(rayOriginX, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Raycast.Axis.X, i);
                        }
                    }
                }
            }
        }

        if (vel.z != 0)
        {
            directionZ = Mathf.Sign(vel.z);
            Vector3 rowsOriginZ = directionZ == 1 ? raycastOrigins.BottomLFCornerReal : raycastOrigins.BottomLBCornerReal;
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
                    if (showHorizontalLimits && !disableAllRays)
                    {
                        Debug.DrawLine(lastOriginZ, rayOriginZ, Color.blue);
                    }
                    lastOriginZ = rayOriginZ;
                    rayOriginZ += (-horVel * skinWidth);

                    RaycastHit hit;
                    if (showHorizontalRays && !disableAllRays)
                    {
                        Debug.DrawRay(rayOriginZ, horVel * rayLength, Color.red);
                    }

                        if (Physics.Raycast(rayOriginZ, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Raycast.Axis.Z, i);
                        }
                    }
                }
            }
        }

        if (collisions.closestHorRaycast.axis != Raycast.Axis.none)//si ha habido una collision horizontal
        {
            MovingState value = collisions.closestHorRaycast.row == 0 ? CheckSlopeType(ref vel, collisions.closestHorRaycast) : MovingState.wall;
            //print("COLLISION HOR: " + value + "; slopeAngle=" + collisions.closestHorRaycast.slopeAngle);
            switch (value)//con que tipo de objeto collisionamos? pared/cuesta arriba/cuesta abajo
            {
                case MovingState.wall:
                    if (!disableAllRays)
                    {
                        Debug.DrawRay(collisions.closestHorRaycast.origin, horVel * 0.5f, Color.white);
                    }
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
                        collisions.wall = collisions.closestHorRaycast.ray.transform.gameObject;
                        switch (collisions.closestHorRaycast.axis)
                        {
                            case Raycast.Axis.X:
                                collisions.left = directionX == -1;
                                collisions.right = directionX == 1;
                                break;
                            case Raycast.Axis.Z:
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
                case MovingState.climbing:
                    //print("AUXILIAR RAYS FOR DISTANCE CALCULATION");
                    if (!disableAllRays)
                    {
                        Debug.DrawRay(collisions.closestHorRaycast.origin, horVel * rayLength, Color.cyan, 4);
                    }
                    float distanceToSlopeStart = 0;
                    if (collisions.slopeAngleOld != collisions.closestHorRaycast.slopeAngle)
                    {
                        distanceToSlopeStart = collisions.closestHorRaycast.distance - skinWidth;
                        horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                    }
                    ClimbSlope(ref vel, collisions.closestHorRaycast);
                    horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                    //--------------------- CHECK FOR NEXT SLOPE/WALL -------------------------------------
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (horVelAux.magnitude + skinWidth);
                    Vector3 rayOrigin = collisions.closestHorRaycast.origin + Vector3.up * vel.y;
                    RaycastHit hit;
                    if (!disableAllRays)
                    {
                        Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.yellow, 4);
                    }
                    if (Physics.Raycast(rayOrigin, horVelAux, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        //+print("HIT  with angle = " + slopeAngle);
                        if (!disableAllRays)
                        {
                            Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.magenta, 4);
                        }
                        if (slopeAngle != collisions.slopeAngle)
                        {
                            horVelAux = horVelAux.normalized * (hit.distance - skinWidth + precisClimbSlopeInsideWall);
                            //tan(realAngle)=y/xz;
                            float y = vel.y;
                            if (slopeAngle > maxClimbAngle)//IF IT'S A WALL
                            {
                                y = Mathf.Tan(collisions.realSlopeAngle * Mathf.Deg2Rad) * horVelAux.magnitude;
                            }
                            vel = new Vector3(horVelAux.x, y, horVelAux.z);
                            //print("HIT NEW SLOPE/Wall with angle = " + slopeAngle);
                            //vel = new Vector3(horVelAux.x, vel.y, horVelAux.z);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    break;
                case MovingState.descending:
                    break;
                case MovingState.none:
                    break;
            }
        }
    }

    void NewVerticalCollisions2(ref Vector3 vel)
    {
        // ---------------------- 3D "Cube" -------------------
        float directionY = Mathf.Sign(vel.y);
        float rayLength = Mathf.Abs(vel.y) + skinWidth;
        Vector3 rowsOrigin = directionY == -1 ? raycastOrigins.BottomLFCornerReal : raycastOrigins.TopLFCornerReal;
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

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    //print("Vertical Hit");
                    if (hit.distance < collisions.closestVerRaycast.distance)
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        if (directionY == 1)
                        {
                            slopeAngle = slopeAngle == 180 ? slopeAngle = 0 : slopeAngle - 90;
                        }
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        collisions.closestVerRaycast = new Raycast(hit, hit.distance, vel, rayOrigin, slopeAngle, wallAngle, Raycast.Axis.Y, i);
                    }
                }
            }
        }
        if (collisions.closestVerRaycast.axis != Raycast.Axis.none)//si ha habido una collision horizontal
        {
            MovingState value = CheckSlopeType(ref vel, collisions.closestVerRaycast);
            //print("COLLISION VER: " + value + "; slopeAngle=" + collisions.closestVerRaycast.slopeAngle);
            if (value == MovingState.climbing)
            {
                value = MovingState.none;
            }
            switch (value)//con que tipo de objeto collisionamos? suelo/cuesta arriba/cuesta abajo
            {
                case MovingState.none:
                    vel.y = (collisions.closestVerRaycast.distance - skinWidth) * directionY;
                    rayLength = collisions.closestVerRaycast.distance;
                    if (collisions.moveSt == MovingState.climbing)//Subiendo chocamos con un techo
                    {
                        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (vel.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad));
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                    }
                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;
                    break;
                case MovingState.sliping:

                    SlipSlope(ref vel, collisions.closestVerRaycast);
                    break;
                case MovingState.descending:
                    if (collisions.moveSt != MovingState.climbing)
                    {
                        float distanceToSlopeStart = 0;
                        //print("NEW SLOPE DESCENDING slope angle= " + collisions.closestVerRaycast.slopeAngle + "; hit point =" + collisions.closestVerRaycast.ray.point.ToString("F4"));
                        distanceToSlopeStart = collisions.closestVerRaycast.distance - skinWidth;
                        vel.y -= distanceToSlopeStart * -1;
                        //print("DESCEND SLOPE");
                        DescendSlope(ref vel, collisions.closestVerRaycast);
                        vel.y += distanceToSlopeStart * -1;
                    }
                    //--------------------- CHECK FOR NEXT SLOPE/FLOOR -------------------------------------
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (Mathf.Abs(vel.y) + skinWidth);
                    Vector3 rayOrigin = collisions.closestVerRaycast.origin + horVelAux;
                    RaycastHit hit;
                    if (!disableAllRays)
                    {
                        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.yellow, 4);
                    }
                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        //+print("HIT  with angle = " + slopeAngle);
                        if (!disableAllRays)
                        {
                            Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.magenta, 4);
                        }
                        if (slopeAngle != collisions.slopeAngle)
                        {
                            //print("HIT NEW SLOPE/FLOOR with angle = " + slopeAngle);
                            vel.y = -(hit.distance - skinWidth);
                            //vel = new Vector3(horVelAux.x, vel.y, horVelAux.z);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    break;
            }
        }
    }

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
                    if (i % 2 == 0 && j % 2 == 0)
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

    void UpdateRaycastOrigins()
    {
        Bounds bounds = coll.bounds;

        raycastOrigins.BottomLFCornerReal = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomRFCornerReal = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomLBCornerReal = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);

        raycastOrigins.TopLFCornerReal = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        //--------------------------------- BOUNDS REDUCED BY SKINWIDTH ---------------------------------
        bounds.Expand(skinWidth * -2);

        raycastOrigins.TopCenter = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        raycastOrigins.TopEnd = new Vector3(bounds.center.x, bounds.max.y, bounds.max.z);
        raycastOrigins.TopLFCorner = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        raycastOrigins.BottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        raycastOrigins.BottomEnd = new Vector3(bounds.center.x, bounds.min.y, bounds.max.z);

        raycastOrigins.BottomCenterH = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        //raycastOrigins.BottomLeft = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z);
        raycastOrigins.BottomLFCorner = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomRFCorner = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomLBCorner = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);

        raycastOrigins.Center = bounds.center;
        raycastOrigins.AroundRadius = bounds.size.z / 2;

    }

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

    [Header("In Water Collisions")]
    public bool showWaterRays;
    public int aroundRaysPerCircle;
    public int aroundCircles;
    public float aroundRaycastsLength = 3f;
    float aroundCirclesSpacing;
    float aroundAngleSpacing;

    void CalculateRaySpacing()
    {
        Bounds bounds = coll.bounds;

        horizontalRows = Mathf.Clamp(horizontalRows, 2, int.MaxValue);
        horizontalRaysPerRow = Mathf.Clamp(horizontalRaysPerRow, 2, int.MaxValue);

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
        public Vector3 TopCenter, TopEnd;//TopEnd= center x, max y, max z
        public Vector3 TopLFCorner, TopLFCornerReal;
        public Vector3 BottomCenter, BottomEnd;//TopEnd= center x, min y, max z
        public Vector3 BottomCenterH;
        //public Vector3 BottomLeft;//min x, miny, center z 
        public Vector3 BottomLFCorner, BottomRFCorner;
        public Vector3 BottomLBCorner;

        public Vector3 BottomLFCornerReal, BottomRFCornerReal;
        public Vector3 BottomLBCornerReal;

        public Vector3 Center;
        public float AroundRadius;

    }

    public enum MovingState
    {
        none,
        wall,
        climbing,
        descending,
        sliping

    }

    public enum SlideState
    {
        none,
        left,
        right
    }

    public struct CollisionInfo
    {
        public bool above, below, lastBelow;
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

        public MovingState moveSt;
        public MovingState lastMoveSt;
        public float slopeAngle, slopeAngleOld, realSlopeAngle, wallAngle, wallAngleOld, wallAngle2, wallAngleOld2, floorAngle;
        public Vector3 startVel;
        public Raycast closestHorRaycast;
        public Raycast closestHorRaycastSlide;
        public Raycast closestVerRaycast;
        public float distanceToFloor;
        public Vector3 horCollisionsPoint;
        public Vector3 wallNormal;
        public GameObject wall;
        public SlideState slideSt;


        public void ResetVertical()
        {
            lastBelow = below;
            above = below = false;
            closestVerRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            distanceToFloor = float.MaxValue;
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
            wall = null;
            slideSt = SlideState.none;
            startVel = Vector3.zero;
            closestHorRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            closestHorRaycastSlide = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
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
    }
}
