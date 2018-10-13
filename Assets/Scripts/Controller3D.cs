﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller3D : MonoBehaviour
{

    public LayerMask collisionMask;
    public LayerMask collisionMaskAround;

    const float skinWidth = 0.1f;
    const float skinWidthHeight = 0.001f;
    public float maxClimbAngle = 60f;
    public float minClimbAngle = 3f;
    public float maxDescendAngle = 60f;
    public float minDescendAngle = 3f;
    public float precisionHeight = 0.01f;
    //Vector3 finalVel;

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
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

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
        //print("SLOPE TYPE = " + collisions.climbSt+"; slopeAngle = "+collisions.slopeAngle+"; FinalVel = "+ vel.ToString("F5"));
        //print("FinalVel= " + vel.ToString("F5"));
        transform.Translate(vel, Space.World);
    }

    void VerticalCollisions(ref Vector3 vel)
    {
        // ---------------------- 3D "CAPSULE" -------------------
        float directionY = Mathf.Sign(vel.y);
        float rayLength = Mathf.Abs(vel.y) + skinWidth;
        Vector3 rowsOrigin = directionY == -1 ? raycastOrigins.BottomLFCorner : raycastOrigins.TopLFCorner;
        Vector3 rowOrigin = rowsOrigin;
        //print("----------NEW SET OF RAYS------------");
        for (int i = 0; i < verticalRows; i++)
        {
            rowOrigin.z = rowsOrigin.z - (verticalRowSpacing * i);
            //print("i= " + i + "; Radius = " + radius);

            for (int j = 0; j < verticalRaysPerRow; j++)
            {
                Vector3 rayOrigin = new Vector3(rowOrigin.x + (verticalRaySpacing * j + vel.x), rowOrigin.y, rowOrigin.z + vel.z);

                RaycastHit hit;
                //Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.red);
                //print("rayOrigin= " + rayOrigin + "; direction = " + directionY + "; rayLength = " + rayLength+"; BottomCenter.y= "+raycastOrigins.BottomCenter.y+"; min.y = "+coll.bounds.min.y);

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float slopeAngle = GetSlopeAngle(hit);
                    if (collisions.moveSt != MovingState.descending)
                    {
                        //print("VERTICAL COLLISIONS");
                        vel.y = (hit.distance - skinWidth) * directionY;
                        rayLength = hit.distance;

                        if (collisions.moveSt == MovingState.climbing)
                        {
                            Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                            horVel = horVel.normalized * (vel.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad));
                            vel = new Vector3(horVel.x, vel.y, horVel.z);
                        }
                        collisions.below = directionY == -1;
                        collisions.above = directionY == 1;
                    }
                }
            }
            if (collisions.moveSt == MovingState.climbing && collisions.slopeAngle != collisions.slopeAngleOld)//new slope, being on a slope already.This avoids going into the slope b4 adapting to new slope.
            {
                Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                rayLength = horVel.magnitude + skinWidth;
                Vector3 rayOrigin = collisions.closestHorRaycast.origin + Vector3.up * vel.y;//NEEDS TO BE FIXED, the real ray origin should be  = BottomCenter + horVel.normalized * radius - skinWidth;
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, horVel.normalized, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                    if (slopeAngle != collisions.slopeAngle)
                    {
                        Vector3 newHorVel = (hit.distance - skinWidth) * horVel.normalized;
                        collisions.slopeAngle = slopeAngle;
                    }
                }
            }
        }
    }//No se usa

    void XCollisions(ref Vector3 vel)
    {
        float directionX = Mathf.Sign(vel.x);
        float rayLength = Mathf.Abs(vel.x) + skinWidth;
        Vector3 rowsOrigin = directionX == 1 ? raycastOrigins.BottomRFCorner : raycastOrigins.BottomLFCorner;
        for (int i = 0; i < horizontalRows; i++)
        {
            Vector3 rowOrigin = rowsOrigin;
            rowOrigin.y = rowsOrigin.y + i * horizontalRowSpacing;
            for (int j = 0; j < horizontalRaysPerRow; j++)
            {
                Vector3 rayOrigin = rowOrigin + Vector3.back * (j * horizontalRaySpacing + vel.y);
                RaycastHit hit;
                Debug.DrawRay(rayOrigin, Vector3.right * directionX * rayLength, Color.red);
                //print("rayOrigin= " + rayOrigin + "; rayLength = " + rayLength+"; BottomCenter.y= "+raycastOrigins.BottomCenter.y+"; min.y = "+coll.bounds.min.y);

                if (Physics.Raycast(rayOrigin, Vector3.right * directionX, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float slopeAngle = GetSlopeAngle(hit);
                    if (collisions.moveSt == MovingState.none || slopeAngle > maxClimbAngle)
                    {
                        vel.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        if (collisions.moveSt == MovingState.climbing)
                        {
                            vel.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(vel.x);
                        }
                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;
                    }

                }
            }
        }
    }//No se usa

    void ZCollisions(ref Vector3 vel)
    {
        float directionZ = Mathf.Sign(vel.z);
        float rayLength = Mathf.Abs(vel.z) + skinWidth;
        Vector3 rowsOrigin = directionZ == 1 ? raycastOrigins.BottomLFCorner : raycastOrigins.BottomLBCorner;
        for (int i = 0; i < horizontalRows; i++)
        {
            Vector3 rowOrigin = rowsOrigin;
            rowOrigin.y = rowsOrigin.y + i * horizontalRowSpacing;
            for (int j = 0; j < horizontalRaysPerRow; j++)
            {
                Vector3 rayOrigin = rowOrigin + Vector3.right * (j * horizontalRaySpacing + vel.y);
                RaycastHit hit;
                Debug.DrawRay(rayOrigin, Vector3.forward * directionZ * rayLength, Color.red);
                //print("rayOrigin= " + rayOrigin + "; rayLength = " + rayLength+"; BottomCenter.y= "+raycastOrigins.BottomCenter.y+"; min.y = "+coll.bounds.min.y);

                if (Physics.Raycast(rayOrigin, Vector3.forward * directionZ, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float slopeAngle = GetSlopeAngle(hit);
                    if (collisions.moveSt == MovingState.none || slopeAngle > maxClimbAngle)
                    {
                        vel.z = (hit.distance - skinWidth) * directionZ;
                        rayLength = hit.distance;

                        if (collisions.moveSt == MovingState.climbing)
                        {
                            vel.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(vel.z);
                        }
                        collisions.behind = directionZ == -1;
                        collisions.foward = directionZ == 1;
                    }
                }
            }
        }
    }//No se usa

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
            //print("CLIMBING: Angle = " + rayCast.slopeAngle + "; old Vel= " + horVel.ToString("F5") + "; vel= " + vel.ToString("F5") +
            //    "; old magnitude=" + horVel.magnitude + "; new magnitude = " + vel.magnitude);
            Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 3);
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
            //for (int i = 0; i < 50; i++)
            //{
            //    print("////////////////////////////////////// DESCENDING");
            //}
            vel = climbVel;
            collisions.below = true;
            collisions.moveSt = MovingState.descending;
            collisions.slopeAngle = rayCast.slopeAngle;
            //print("CLIMBING: Angle = " + rayCast.slopeAngle + "; old Vel= " + horVel.ToString("F5") + "; vel= " + vel.ToString("F5") +
            //    "; old magnitude=" + horVel.magnitude + "; new magnitude = " + vel.magnitude);
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
                Debug.DrawRay(center, finalDir * rayLength, Color.red);
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

    void ClimbSlopeCollisions(ref Vector3 vel)
    {
        float directionX = Mathf.Sign(vel.x);
        float rayLengthX = skinWidth;
        Vector3 rowOriginX = directionX == 1 ? raycastOrigins.BottomRFCorner : raycastOrigins.BottomLFCorner;
        for (int j = 0; j < horizontalRaysPerRow; j++)
        {
            Vector3 rayOriginX = rowOriginX + Vector3.back * (j * horizontalRaySpacing);
            RaycastHit hit;
            Debug.DrawRay(rayOriginX, Vector3.right * directionX * rayLengthX, Color.yellow);
            //print("rayOrigin= " + rayOrigin + "; rayLength = " + rayLength+"; BottomCenter.y= "+raycastOrigins.BottomCenter.y+"; min.y = "+coll.bounds.min.y);

            if (Physics.Raycast(rayOriginX, Vector3.right * directionX, out hit, rayLengthX, collisionMask, QueryTriggerInteraction.Ignore))
            {
                float slopeAngle = GetSlopeAngle(hit);
                if (hit.distance < collisions.closestHorRaycast.distance)
                {
                    float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                    collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Raycast.Axis.X);
                }
            }
        }
        float directionZ = Mathf.Sign(vel.z);
        float rayLengthZ = skinWidth;
        Vector3 rowOriginZ = directionZ == 1 ? raycastOrigins.BottomLFCorner : raycastOrigins.BottomLBCorner;
        for (int j = 0; j < horizontalRaysPerRow; j++)
        {
            Vector3 rayOriginZ = rowOriginZ + Vector3.right * (j * horizontalRaySpacing);
            RaycastHit hit;
            Debug.DrawRay(rayOriginZ, Vector3.forward * directionZ * rayLengthZ, Color.yellow);
            //print("rayOrigin= " + rayOrigin + "; rayLength = " + rayLength+"; BottomCenter.y= "+raycastOrigins.BottomCenter.y+"; min.y = "+coll.bounds.min.y);

            if (Physics.Raycast(rayOriginZ, Vector3.forward * directionZ, out hit, rayLengthZ, collisionMask, QueryTriggerInteraction.Ignore))
            {
                float slopeAngle = GetSlopeAngle(hit);
                if (hit.distance < collisions.closestHorRaycast.distance)
                {
                    float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                    collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Raycast.Axis.Z);
                }
            }
        }
        if (collisions.closestHorRaycast.axis != Raycast.Axis.none)// En otras palabras, si se ha movido y rellenado el valor de "closestHorRaycast"(hit)
        {
            if (collisions.closestHorRaycast.slopeAngle <= maxClimbAngle)
            {
                Vector3 horVel;
                float distanceToSlopeStart = 0;
                if (collisions.closestHorRaycast.slopeAngle != collisions.slopeAngleOld)//if new slope
                {//Substract the distance to slope from the velocity
                    distanceToSlopeStart = collisions.closestHorRaycast.distance - skinWidth;
                    horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                }
                ClimbSlope(ref vel, collisions.closestHorRaycast);
                horVel = new Vector3(vel.x, 0, vel.z);
                horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
                vel = new Vector3(horVel.x, vel.y, horVel.z);
            }
        }
    }//No se usa

    void DescendSlopeCollisions(ref Vector3 vel)
    {
        float directionY = Mathf.Sign(vel.y);
        float rayLength = 1 + skinWidth;
        Vector3 rowsOrigin = raycastOrigins.BottomLFCorner;
        Vector3 rowOrigin = rowsOrigin;
        //print("----------NEW SET OF RAYS------------");
        for (int i = 0; i < verticalRows; i++)
        {
            rowOrigin.z = rowsOrigin.z - (verticalRowSpacing * i);
            //print("i= " + i + "; Radius = " + radius);

            for (int j = 0; j < verticalRaysPerRow; j++)
            {
                Vector3 rayOrigin = new Vector3(rowOrigin.x + (verticalRaySpacing * j), rowOrigin.y, rowOrigin.z);

                RaycastHit hit;
                Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.yellow);
                //print("rayOrigin= " + rayOrigin + "; direction = " + directionY + "; rayLength = " + rayLength+"; BottomCenter.y= "+raycastOrigins.BottomCenter.y+"; min.y = "+coll.bounds.min.y);

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float slopeAngle = GetSlopeAngle(hit);
                    if (hit.distance < collisions.closestVerRaycast.distance)
                    {
                        //print("NEW CLOSEST VERTRAY, slope angle= " + slopeAngle);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        collisions.closestVerRaycast = new Raycast(hit, hit.distance, vel, rayOrigin, slopeAngle, wallAngle, Raycast.Axis.Y);
                    }
                }
            }
        }

        if (collisions.closestVerRaycast.axis == Raycast.Axis.Y)// We are touching floor
        {
            if (CheckSlopeType(ref vel, collisions.closestVerRaycast) == MovingState.descending)
            {
                if (collisions.closestVerRaycast.slopeAngle <= maxDescendAngle)
                {
                    float distanceToSlopeStart = 0;
                    if (collisions.closestVerRaycast.slopeAngle != collisions.slopeAngleOld)//if new slope
                    {
                        print("NEW SLOPE DESCENDING slope angle= " + collisions.closestVerRaycast.slopeAngle + "; hit point =" + collisions.closestVerRaycast.ray.point.ToString("F4"));
                        distanceToSlopeStart = collisions.closestVerRaycast.distance - skinWidth;
                        vel.y -= distanceToSlopeStart * -1;
                    }
                    //print("DESCEND SLOPE");
                    DescendSlope(ref vel, collisions.closestVerRaycast);
                    vel.y += distanceToSlopeStart * -1;
                    //print("vel= " + vel.ToString("F5"));
                }
            }


            /*Vector3 horVel;
            float distanceToSlopeStart = 0;
            if (collisions.closestHorRaycast.slopeAngle != collisions.slopeAngleOld)//if new slope
            {//Substract the distance to slope from the velocity
                distanceToSlopeStart = collisions.closestHorRaycast.distance - skinWidth;
                horVel = new Vector3(vel.x, 0, vel.z);
                horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                vel = new Vector3(horVel.x, vel.y, horVel.z);
            }
            ClimbSlope(ref vel, collisions.closestHorRaycast);
            horVel = new Vector3(vel.x, 0, vel.z);
            horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
            vel = new Vector3(horVel.x, vel.y, horVel.z);*/
        }
    }//No se usa

    void SlideWall(ref Vector3 vel, Raycast rayCast)
    {
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        float wallAngle = Vector3.Angle(rayCast.ray.normal, Vector3.forward);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        Vector3 normal = -new Vector3(rayCast.ray.normal.x, 0, rayCast.ray.normal.z).normalized;
        float angle = Vector3.Angle(normal, horVel);
        float a = Mathf.Sin(angle * Mathf.Deg2Rad) * horVel.magnitude;
        Vector3 movementNormal = Vector3.up;
        Vector3 slideVel = Vector3.Cross(normal, movementNormal).normalized;
        //LEFT OR RIGHT ORIENTATION?
        float ang = Vector3.Angle(slideVel, horVel);
        slideVel = ang > 90 ? -slideVel : slideVel;
        print("SLIDE ANGLE= " + angle + "; slideVel = " + slideVel.ToString("F4") + "; a = " + a + "; wallAngle = " + wallAngle + "; distanceToWall = " + rayCast.distance);
        slideVel *= a;
        vel = new Vector3(slideVel.x, vel.y, slideVel.z);
        collisions.moveSt = MovingState.wall;
        collisions.wallAngle = wallAngle;
        Debug.DrawRay(raycastOrigins.Center, slideVel.normalized * 2, Color.green, 3);
    }

    void CylinderHorizontalCollisions(ref Vector3 vel)
    {
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float rayLength = horVel.magnitude + skinWidth;
        Vector3 rowOrigin = raycastOrigins.BottomCenter + (horVel.normalized * raycastOrigins.AroundRadius);
        for (int i = 0; i < horizontalRows; i++)
        {
            rowOrigin.y = raycastOrigins.BottomCenter.y + (i * horizontalRowSpacing);
            //for(int j=0; j<)
        }
    }//No se usa

    void NewHorizontalCollisions2(ref Vector3 vel)
    {
        Raycast wallRay = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
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
                    Debug.DrawLine(lastOriginX, rayOriginX, Color.blue);
                    lastOriginX = rayOriginX;
                    //rayOriginX += (Vector3.right * directionX * skinWidth);
                    rayOriginX += (-horVel * skinWidth);
                    Debug.DrawLine(rayOriginX, rayOriginX, Color.blue);
                    RaycastHit hit;
                    //Debug.DrawRay(rayOriginX, horVel * rayLength, Color.red);

                    if (Physics.Raycast(rayOriginX, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Raycast.Axis.X, i);
                        }
                        if (slopeAngle > maxClimbAngle && hit.distance < wallRay.distance)
                        {
                            wallRay = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Raycast.Axis.X, i);
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
                    Debug.DrawLine(lastOriginZ, rayOriginZ, Color.blue);
                    lastOriginZ = rayOriginZ;
                    //rayOriginZ += (Vector3.forward * directionZ * skinWidth);
                    rayOriginZ += (-horVel * skinWidth);

                    RaycastHit hit;
                    //Debug.DrawRay(rayOriginZ, horVel * rayLength, Color.red);

                    if (Physics.Raycast(rayOriginZ, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Raycast.Axis.Z, i);
                        }
                        if (slopeAngle > maxClimbAngle && hit.distance < wallRay.distance)
                        {
                            wallRay = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Raycast.Axis.Z, i);
                        }
                    }
                }
            }
        }

        if (collisions.closestHorRaycast.axis != Raycast.Axis.none)//si ha habido una collision horizontal
        {
            MovingState value = collisions.closestHorRaycast.row == 0 ? CheckSlopeType(ref vel, collisions.closestHorRaycast) : MovingState.wall;
            print("COLLISION HOR: " + value + "; slopeAngle=" + collisions.closestHorRaycast.slopeAngle);
            switch (value)//con que tipo de objeto collisionamos? pared/cuesta arriba/cuesta abajo
            {
                case MovingState.wall:
                    //horVel = horVel * (collisions.closestHorRaycast.distance - skinWidth);
                    //vel = new Vector3(horVel.x, vel.y, horVel.z);

                    //float distanceToWall = 0;
                    if (collisions.wallAngleOld != collisions.closestHorRaycast.wallAngle)
                    {
                        print("APPROACHING WALL: " + "distance = " + collisions.closestHorRaycast.distance);
                        horVel = horVel * (collisions.closestHorRaycast.distance - skinWidth);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                        collisions.wallAngle = collisions.closestHorRaycast.wallAngle;
                        /*distanceToWall = collisions.closestHorRaycast.distance - skinWidth;
                        horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (horVel.magnitude - distanceToWall);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);*/
                    }
                    else
                    {
                        SlideWall(ref vel, collisions.closestHorRaycast);
                    }
                    /*horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude + distanceToWall);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);*/

                    /*if (collisions.lastMoveSt == MovingState.climbing)
                    {
                        collisions.moveSt = MovingState.climbing;
                        vel.y = Mathf.Tan(collisions.slopeAngleOld * Mathf.Deg2Rad) * horVel.magnitude;
                    }*/
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
                    break;
                case MovingState.climbing:
                        print("AUXILIAR RAYS FOR DISTANCE CALCULATION");
                        Debug.DrawRay(collisions.closestHorRaycast.origin, horVel * rayLength, Color.cyan, 4);
                        float distanceToSlopeStart = 0;
                        if (collisions.slopeAngleOld != collisions.closestHorRaycast.slopeAngle)
                        {
                            //RaycastHit hit;
                            //Debug.DrawRay(collisions.closestHorRaycast.origin, horVel * rayLength, Color.white);
                            distanceToSlopeStart = collisions.closestHorRaycast.distance - skinWidth;
                            horVel = new Vector3(vel.x, 0, vel.z);
                            horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                            vel = new Vector3(horVel.x, vel.y, horVel.z);
                        }
                        ClimbSlope(ref vel, collisions.closestHorRaycast);
                        horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                    if (wallRay.axis != Raycast.Axis.none)
                    {

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
        //print("----------NEW SET OF RAYS------------");
        for (int i = 0; i < verticalRows; i++)
        {
            rowOrigin.z = rowsOrigin.z - (verticalRowSpacing * i);
            Vector3 lastOrigin = rowOrigin;
            for (int j = 0; j < verticalRaysPerRow; j++)
            {
                Vector3 rayOrigin = new Vector3(rowOrigin.x + (verticalRaySpacing * j), rowOrigin.y, rowOrigin.z);
                Debug.DrawLine(lastOrigin, rayOrigin, Color.blue);
                lastOrigin = rayOrigin;
                rayOrigin += (Vector3.up * -directionY) * skinWidth;

                RaycastHit hit;
                Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.red);

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    //print("Vertical Hit");
                    if (hit.distance < collisions.closestVerRaycast.distance)
                    {
                        float slopeAngle = GetSlopeAngle(hit);
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
            //print("COLLISION " + value + "; slopeAngle=" + collisions.closestVerRaycast.slopeAngle);
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
                case MovingState.climbing:
                    break;
                case MovingState.descending:
                    if (collisions.moveSt != MovingState.climbing)
                    {
                        float distanceToSlopeStart = 0;
                        //print("NEW SLOPE DESCENDING slope angle= " + collisions.closestVerRaycast.slopeAngle + "; hit point =" + collisions.closestVerRaycast.ray.point.ToString("F4"));
                        distanceToSlopeStart = collisions.closestVerRaycast.distance - skinWidth;
                        vel.y -= distanceToSlopeStart * -1;
                        print("DESCEND SLOPE");
                        DescendSlope(ref vel, collisions.closestVerRaycast);
                        vel.y += distanceToSlopeStart * -1;
                    }
                    //--------------------- CHECK FOR NEXT SLOPE/FLOOR -------------------------------------
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (Mathf.Abs(vel.y) + skinWidth);
                    Vector3 rayOrigin = collisions.closestVerRaycast.origin + horVelAux;
                    RaycastHit hit;
                    Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.yellow, 4);
                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        //+print("HIT  with angle = " + slopeAngle);
                        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.magenta, 4);
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

    void NewHorizontalCollisions(ref Vector3 vel)
    {
        Raycast wallRay = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float rayLength = horVel.magnitude + skinWidth;
        horVel = horVel.normalized;
        float directionX = 0, directionZ = 0; ;

        if (vel.x != 0)
        {
            directionX = Mathf.Sign(vel.x);
            Vector3 rowsOriginX = directionX == 1 ? raycastOrigins.BottomRFCorner : raycastOrigins.BottomLFCorner;
            for (int i = 0; i < horizontalRows; i++)
            {
                Vector3 rowOriginX = rowsOriginX;
                rowOriginX.y = (rowsOriginX.y) + i * horizontalRowSpacing;
                for (int j = 0; j < horizontalRaysPerRow; j++)
                {
                    Vector3 rayOriginX = rowOriginX + Vector3.back * (j * horizontalRaySpacing);
                    RaycastHit hit;
                    Debug.DrawRay(rayOriginX, Vector3.right * directionX * rayLength, Color.red);

                    if (Physics.Raycast(rayOriginX, Vector3.right * directionX, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Raycast.Axis.X, i);
                        }
                        if (slopeAngle > maxClimbAngle && hit.distance<wallRay.distance)
                        {
                            wallRay = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Raycast.Axis.X, i);
                        }

                    }
                }
            }
        }

        if (vel.z != 0)
        {
            directionZ = Mathf.Sign(vel.z);
            Vector3 rowsOriginZ = directionZ == 1 ? raycastOrigins.BottomLFCorner : raycastOrigins.BottomLBCorner;
            for (int i = 0; i < horizontalRows; i++)
            {
                Vector3 rowOriginZ = rowsOriginZ;
                rowOriginZ.y = (rowsOriginZ.y) + i * horizontalRowSpacing;
                for (int j = 0; j < horizontalRaysPerRow; j++)
                {
                    Vector3 rayOriginZ = rowOriginZ + Vector3.right * (j * horizontalRaySpacing);
                    RaycastHit hit;
                    Debug.DrawRay(rayOriginZ, Vector3.forward * directionZ * rayLength, Color.red);

                    if (Physics.Raycast(rayOriginZ, Vector3.forward * directionZ, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Raycast.Axis.Z, i);
                        }
                        if (slopeAngle > maxClimbAngle && hit.distance < wallRay.distance)
                        {
                            wallRay = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Raycast.Axis.Z, i);
                        }
                    }
                }
            }
        }

        if (collisions.closestHorRaycast.axis != Raycast.Axis.none)//si ha habido una collision horizontal
        {
            MovingState value = collisions.closestHorRaycast.row == 0 ? CheckSlopeType(ref vel, collisions.closestHorRaycast) : MovingState.wall;
            print("COLLISION HOR: " + value + "; slopeAngle=" + collisions.closestHorRaycast.slopeAngle);
            switch (value)//con que tipo de objeto collisionamos? pared/cuesta arriba/cuesta abajo
            {
                case MovingState.wall:
                    //horVel = horVel * (collisions.closestHorRaycast.distance - skinWidth);
                    //vel = new Vector3(horVel.x, vel.y, horVel.z);

                    //float distanceToWall = 0;
                    if (collisions.wallAngleOld != collisions.closestHorRaycast.wallAngle)
                    {
                        print("APPROACHING WALL: " + "distance = " + collisions.closestHorRaycast.distance);
                        horVel = horVel * (collisions.closestHorRaycast.distance - skinWidth);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                        collisions.wallAngle = collisions.closestHorRaycast.wallAngle;
                        /*distanceToWall = collisions.closestHorRaycast.distance - skinWidth;
                        horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (horVel.magnitude - distanceToWall);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);*/
                    }
                    else
                    {
                        SlideWall(ref vel, collisions.closestHorRaycast);
                    }
                    /*horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude + distanceToWall);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);*/

                    /*if (collisions.lastMoveSt == MovingState.climbing)
                    {
                        collisions.moveSt = MovingState.climbing;
                        vel.y = Mathf.Tan(collisions.slopeAngleOld * Mathf.Deg2Rad) * horVel.magnitude;
                    }*/
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
                    break;
                case MovingState.climbing:
                    Raycast hit = MovingDirHorizontalCollisions(ref vel);
                    if (hit.axis != Raycast.Axis.none)//Physics.Raycast(collisions.closestHorRaycast.origin, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        print("AUXILIAR RAYS FOR DISTANCE CALCULATION");
                        Debug.DrawRay(hit.origin, horVel * rayLength, Color.cyan, 4);
                        float distanceToSlopeStart = 0;
                        if (collisions.slopeAngleOld != collisions.closestHorRaycast.slopeAngle)
                        {
                            //RaycastHit hit;
                            //Debug.DrawRay(collisions.closestHorRaycast.origin, horVel * rayLength, Color.white);
                            distanceToSlopeStart = hit.distance - skinWidth;
                            horVel = new Vector3(vel.x, 0, vel.z);
                            horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                            vel = new Vector3(horVel.x, vel.y, horVel.z);
                        }
                        ClimbSlope(ref vel, collisions.closestHorRaycast);
                        horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                        if (wallRay.axis != Raycast.Axis.none)
                        {

                        }
                    }
                    else
                    {
                        Debug.LogWarning("COULDNT HIT SLOPE to check for distance");
                    }
                    break;
                case MovingState.descending:
                    break;
                case MovingState.none:
                    break;
            }
        }
    }

    void NewVerticalCollisions(ref Vector3 vel)
    {
        // ---------------------- 3D "Cube" -------------------
        float directionY = Mathf.Sign(vel.y);
        float rayLength = Mathf.Abs(vel.y) + skinWidth;
        Vector3 rowsOrigin = directionY == -1 ? raycastOrigins.BottomLFCorner : raycastOrigins.TopLFCorner;
        Vector3 rowOrigin = rowsOrigin;
        //print("----------NEW SET OF RAYS------------");
        for (int i = 0; i < verticalRows; i++)
        {
            rowOrigin.z = rowsOrigin.z - (verticalRowSpacing * i);

            for (int j = 0; j < verticalRaysPerRow; j++)
            {
                Vector3 rayOrigin = new Vector3(rowOrigin.x + (verticalRaySpacing * j), rowOrigin.y, rowOrigin.z);

                RaycastHit hit;
                Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.red);

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    //print("Vertical Hit");
                    if (hit.distance < collisions.closestVerRaycast.distance)
                    {
                        float slopeAngle = GetSlopeAngle(hit);
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
            //print("COLLISION " + value + "; slopeAngle=" + collisions.closestVerRaycast.slopeAngle);
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
                case MovingState.climbing:
                    break;
                case MovingState.descending:
                    if (collisions.moveSt != MovingState.climbing)
                    {
                        float distanceToSlopeStart = 0;
                        //print("NEW SLOPE DESCENDING slope angle= " + collisions.closestVerRaycast.slopeAngle + "; hit point =" + collisions.closestVerRaycast.ray.point.ToString("F4"));
                        distanceToSlopeStart = collisions.closestVerRaycast.distance - skinWidth;
                        vel.y -= distanceToSlopeStart * -1;
                        print("DESCEND SLOPE");
                        DescendSlope(ref vel, collisions.closestVerRaycast);
                        vel.y += distanceToSlopeStart * -1;
                    }
                    //--------------------- CHECK FOR NEXT SLOPE/FLOOR -------------------------------------
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (Mathf.Abs(vel.y) + skinWidth);
                    Vector3 rayOrigin = collisions.closestVerRaycast.origin + horVelAux;
                    RaycastHit hit;
                    Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.yellow, 4);
                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        //+print("HIT  with angle = " + slopeAngle);
                        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.magenta, 4);
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

    Raycast MovingDirHorizontalCollisions(ref Vector3 vel)
    {
        Raycast result = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float rayLength = horVel.magnitude + skinWidth;
        horVel = horVel.normalized;
        float directionX = 0, directionZ = 0; ;

        if (vel.x != 0)
        {
            directionX = Mathf.Sign(vel.x);
            Vector3 rowOriginX = directionX == 1 ? raycastOrigins.BottomRFCorner : raycastOrigins.BottomLFCorner;
            for (int j = 0; j < horizontalRaysPerRow; j++)
            {
                Vector3 rayOriginX = rowOriginX + Vector3.back * (j * horizontalRaySpacing);
                rayOriginX += (Vector3.right * directionX *skinWidth);
                rayOriginX += (-horVel * skinWidth);
                RaycastHit hit;
                Debug.DrawRay(rayOriginX, horVel * rayLength, Color.yellow, 0.1f);

                if (Physics.Raycast(rayOriginX, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    if (hit.distance < result.distance)
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        result = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, wallAngle, Raycast.Axis.X, 0);
                    }


                }
            }
        }

        if (vel.z != 0)
        {
            directionZ = Mathf.Sign(vel.z);
            Vector3 rowOriginZ = directionZ == 1 ? raycastOrigins.BottomLFCorner : raycastOrigins.BottomLBCorner;
            for (int j = 0; j < horizontalRaysPerRow; j++)
            {
                Vector3 rayOriginZ = rowOriginZ + Vector3.right * (j * horizontalRaySpacing);
                rayOriginZ += (Vector3.forward * directionZ * skinWidth);
                rayOriginZ += (-horVel * skinWidth);
                RaycastHit hit;
                Debug.DrawRay(rayOriginZ, horVel * rayLength, Color.yellow, 0.1f);

                if (Physics.Raycast(rayOriginZ, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    if (hit.distance < result.distance)
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        result = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, wallAngle, Raycast.Axis.Z, 0);
                    }

                }
            }
        }
        MovingState val = CheckSlopeType(ref vel, result);
        if (val == MovingState.climbing)
        {
            return result;
        }
        else
        {
            return new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
        }

    }

    void HorizontalCollisions(ref Vector3 vel)
    {
        // ---------------------- 3D CAPSULE -------------------
        float directionX = Mathf.Sign(vel.x);
        float directionZ = Mathf.Sign(vel.z);

        Vector3 dirXZ = new Vector3(vel.x, 0, vel.z);
        //PERPENDICULAR VECTOR TO MOVING DIR
        float perpX = 1;
        float perpZ = (-dirXZ.x / dirXZ.z) * perpX;
        Vector3 perpVector = new Vector3(perpX, 0, perpZ).normalized;
        Vector3 rowDir = -perpVector;

        float rayLength = Mathf.Abs(dirXZ.magnitude) + skinWidth;
        //CIRCUMFERENCE POINT THAT INTERSECTS WITH dirXZ THAT CROSSES CENTER
        Vector3 rowsOriginCenter = dirXZ.normalized * (horizontalRadius - skinWidth);
        float rx = rowsOriginCenter.x + raycastOrigins.BottomCenterH.x;
        float rz = rowsOriginCenter.z + raycastOrigins.BottomCenterH.z;
        rowsOriginCenter = new Vector3(rx, raycastOrigins.BottomCenterH.y, rz);
        //CIRCUMFERENCE POINT THAT INTERSECTS WITH PERPENDICULAR VECTOR THAT CROSSES CENTER
        /*float angle = Mathf.Acos(((1 * perpVector.x) + (0 * perpVector.z)) / (1 * perpVector.magnitude)) * Mathf.Rad2Deg;
        float px = raycastOrigins.BottomCenter.x + horizontalRadius * Mathf.Cos(angle);
        float pz = raycastOrigins.BottomCenter.z + horizontalRadius * Mathf.Sin(angle);
        Vector3 rowsOrigin = new Vector3(px, raycastOrigins.BottomCenter.y, pz);*/
        Vector3 rowsOrigin = perpVector * (horizontalRadius);
        rx = rowsOrigin.x + rowsOriginCenter.x;
        rz = rowsOrigin.z + rowsOriginCenter.z;
        rowsOrigin = new Vector3(rx, rowsOriginCenter.y, rz);
        //print("dirXZ=" + dirXZ.ToString("F4") + "; perpVector=" + perpVector + ";rowsOrigin="+rowsOrigin);
        //print("----------NEW SET OF RAYS------------");
        for (int i = 0; i < horizontalRows; i++)
        {
            Vector3 rowOrigin = new Vector3(rowsOrigin.x, rowsOrigin.y + (i * horizontalRowSpacing), rowsOrigin.z);
            //print("horizontalRowSpacing= " + horizontalRowSpacing + "; rowOrigin = " + rowOrigin.ToString("F4"));
            for (int j = 0; j < horizontalRaysPerRow; j++)
            {
                //o=(rowOrigin.x,rowOrigin.z), f=(fx,fz), r=(perpVector2.x,perpVector2.z)
                //(perpVector2.x,perpVector2.z)=(fx-rowOrigin.x,fz-rowOrigin.z)
                Vector3 finalRowDir = rowDir * (j * horizontalRaySpacing);
                float fx = finalRowDir.x + rowOrigin.x;
                float fz = finalRowDir.z + rowOrigin.z;
                Vector3 rayOrigin = new Vector3(fx, rowOrigin.y, fz);

                RaycastHit hit;
                Debug.DrawRay(rayOrigin, dirXZ * rayLength, Color.red);
                //print("rayOrigin= " + rayOrigin + "; rayLength = " + rayLength+"; BottomCenter.y= "+raycastOrigins.BottomCenter.y+"; min.y = "+coll.bounds.min.y);

                if (Physics.Raycast(rayOrigin, dirXZ, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    //print("HIT AGAINST " + hit.collider.gameObject.name);
                    //print("DISTANCE HIT = " + hit.distance);
                    if (hit.distance > (skinWidth))
                    {
                        print("HIT DISTANCE= " + hit.distance);
                        Vector3 aux = dirXZ.normalized * (hit.distance - skinWidth);
                        vel = new Vector3(aux.x, vel.y, aux.z);
                        rayLength = hit.distance;
                    }
                    else
                    {
                        //calculate parallel direction to plane hit and percentage of magnitude, based on incidence angle (90º->0)
                        Vector3 normal = new Vector3(hit.normal.x, 0, hit.normal.z);
                        Vector3 parallel = -normal;
                        //angle
                        float cos = Vector3.Dot(dirXZ, normal);
                        float cosDeg = Mathf.Acos(cos) * Mathf.Rad2Deg;
                        float prop = ((cosDeg - 90) / 90);
                        prop = Mathf.Clamp(prop, 0, 1);
                        float finalSpeed = vel.magnitude * prop;
                        vel = new Vector3(parallel.x * finalSpeed, vel.y, parallel.z * finalSpeed);
                        print("HIT ANGLE = " + cosDeg + "; PARALLEL DIR= " + parallel.ToString("F4") + "; FINAL SPEED= " + finalSpeed);
                    }

                    collisions.behind = directionZ == -1;
                    collisions.foward = directionZ == 1;
                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
                //if (i == 0) { break; }
            }
        }
    }//No se usa

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

    public int verticalRows;
    public int verticalRaysPerRow;
    float verticalRowSpacing;
    float verticalRaySpacing;

    public int horizontalRows;
    public int horizontalRaysPerRow;
    float horizontalRowSpacing;
    float horizontalRaySpacing;
    float horizontalRadius;

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

        horizontalRadius = Mathf.Abs(bounds.size.x / 2);

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
        descending
    }
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool foward, behind;
        public bool around;

        public MovingState moveSt;
        public MovingState lastMoveSt;
        public float slopeAngle, slopeAngleOld, wallAngle, wallAngleOld;
        public Vector3 startVel;
        public Raycast closestHorRaycast;
        public Raycast closestVerRaycast;


        public void ResetVertical()
        {
            above = below = false;
        }
        public void ResetHorizontal()
        {
            left = right = false;
            foward = behind = false;
            wallAngleOld = wallAngle;
            wallAngle = 0;
        }
        public void ResetAround()
        {
            around = false;
        }
        public void ResetClimbingSlope()
        {
            lastMoveSt = moveSt;
            moveSt = MovingState.none;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
            startVel = Vector3.zero;
            closestHorRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            closestVerRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
        }
    }
}
