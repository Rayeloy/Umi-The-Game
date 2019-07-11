//Property of Another Coffee Games S.L., Spain. Author: Carlos Eloy Jose Sanz
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller3D : MonoBehaviour
{
    public bool disableAllDebugs;
    public bool disableAllRays;
    public LayerMask collisionMask;
    public LayerMask collisionMaskAround;
    public float FloorMaxDistanceCheck = 5;
    Color purple = new Color(0.749f, 0.380f, 1f);
    Color brown = new Color(0.615f, 0.329f, 0.047f);
    Color orange = new Color(0.945f, 0.501f, 0.117f);
    Color darkBrown = new Color(0.239f, 0.121f, 0f);
    Color darkYellow = new Color(0.815f, 0.780f, 0.043f);
    Color darkRed = new Color(0.533f, 0.031f, 0.027f);
    Color darkGreen = new Color(0.054f, 0.345f, 0.062f);

    const float skinWidth = 0.1f;
    [Header(" -- Slopes -- ")]
    public float maxClimbAngle = 60f;
    public float minClimbAngle = 0f;
    public float maxDescendAngle = 60f;
    public float minDescendAngle = 0f;
    public float precisClimbSlopeInsideWall = 0.000f;
    [Header(" -- Precision distances -- ")]
    public float skinWidthHeight = 0.0001f;
    [Tooltip("DO NOT CHANGE. Space the first horizontal row of raycasts (feet) is lifted up to avoid colliding horizontally " +
        "(like it was a wall) when on the edge of floor.")]
    public float precisionHeight = 0.01f;
    public float precisionSpaceFromSlideWall = 0.001f;
    public float rayExtraLengthPeak = 0.1f;

    public CapsuleCollider coll;
    public float bigCollRadius;
    public float smallCollRadius;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

    [Header(" -- Vertical Collisions -- ")]
    public bool showVerticalRays;
    public bool showVerticalLimits;
    public bool showDistanceCheckRays;
    public bool showRoofRays;
    public int verticalRows;
    public int verticalRaysPerRow;
    float verticalRowSpacing;
    float verticalRaySpacing;

    [Header(" -- Horizontal Collisions -- ")]
    public bool showHorizontalRays;
    public bool showHorizontalLimits;
    public bool showWallRays;
    public bool showWallLimits;
    public bool showWallEdgeRays;
    public int horizontalRows;
    public int horizontalRaysPerRow;
    float horizontalRowSpacing;
    float horizontalRaySpacing;
    public float maxHeightToClimbStep = 0.3f;
    [Header(" - Wall Slide - ")]
    public int wallSlideRows;
    public int wallSlideRaysPerRow;
    float wallSlideRowSpacing;
    float wallSlideRaySpacing;
    [Header(" - Wall Edge - ")]
    public int wallEdgeHighPrecissionRays = 20;
    public float wallEdgeHighCheckForPerfectEdgeRaySpacing;
    float wallEdgeHighPrecisionHorRaySpacing;
    float wallEdgeHighPrecisionVerRaySpacing;
    // --- CORSB ---
    //"Cutting Out Raycast Skinwidth Borders". this is the 20% of horizontalRaysPerRow. 
    //It's used to only do the CORSB system to the first 20% and the last 20% (border rays)
    [Header(" - CORSB - ")]
    public bool corsbOn = true;
    int corsbBorderHorRaysPerRow;
    Color corsbColor = new Color(0, 0, 0);
    [Range(0, 49)]
    public int corsbBorderPercent = 20;
    public float corsbMinSkinWidth = 0.0001f;

    [Header(" -- In Water Collisions -- ")]
    public bool showWaterRays;
    public int aroundRaysPerCircle;
    public int aroundCircles;
    public float aroundRaycastsLength = 3f;
    float aroundCirclesSpacing;
    float aroundAngleSpacing;

    public enum CollisionState
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

    public enum FirstCollisionWithWallType
    {
        climbingAndFowardWall,
        climbingAndStraightWall,
        climbingAndBackwardsWall,
        slidingAndWallContinue,
        normalWall,
        none
    }

    private void Awake()
    {
        if (minClimbAngle != minDescendAngle) Debug.LogError("Warning: minClimbAngle and minDescendAngle values are different, " +
             "are you sure you want this? It will generate extrange behaviours.");
        if (maxClimbAngle != maxDescendAngle) Debug.LogError("Warning: maxClimbAngle and maxDescendAngle values are different, " +
            "are you sure you want this? It will generate extrange behaviours.");
    }

    private void Start()
    {
        CalculateRaySpacing();
        //print("bounds.size.z = " + coll.bounds.size.z+"bounds.size.y = "+ coll.bounds.size.y);
    }

    /// <summary>
    /// MAIN FUNCTION OF CONTROLLER3D
    /// </summary>
    /// <param name="vel"></param>
    public void Move(Vector3 vel)
    {
        //AdjustColliderSize(vel);
        UpdateRaycastOrigins();
        collisions.ResetVertical();
        collisions.ResetHorizontal();
        collisions.ResetClimbingSlope();
        collisions.startVel = vel;
        if (!disableAllDebugs) Debug.LogWarning("Start Vel = " + vel.ToString("F4"));
        if (!disableAllRays)
        {
            Debug.DrawRay(raycastOrigins.Center, vel * 5, Color.blue);
            Vector3 horVel = new Vector3(vel.x, 0, vel.z);
            Debug.DrawRay(raycastOrigins.Center, horVel.normalized * 2, Color.yellow);
        }
        if (vel.x != 0 || vel.z != 0)
        {
            HorizontalCollisions(ref vel, false);
        }
        if (!disableAllDebugs) Debug.Log("Middle Vel = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
        if (vel.y != 0 || vel.x != 0 || vel.z != 0)
        {
            VerticalCollisions(ref vel);
        }

        //if (collisions.lastcollSt == CollisionState.crossingPeak && collisions.collSt != CollisionState.crossingPeak)
        //{
        //    Debug.LogError("We stopped crossing peak");
        //}
        //if (collisions.lastcollSt == CollisionState.none && collisions.collSt == CollisionState.climbing)
        //{
        //    Debug.LogWarning("We started climbing");
        //}
        VerticalCollisionsDistanceCheck(ref vel);

        UpdateSafeBelow();
        if (vel.magnitude > 2.5f)
        {
            vel = Vector3.zero;
            if (!disableAllDebugs) Debug.LogError("ERROR: WE ARE GOING TOO FAST! SETTING VEL TO 0");
        }
        if (!disableAllDebugs) Debug.LogWarning("End Vel = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
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

    #region --- SLOPES CALCULATIONS ---
    float GetSlopeAngle(Vector3 floorNormal)
    {
        float slopeAngle = Vector3.Angle(floorNormal, Vector3.up);
        return slopeAngle;
    }

    public float GetSlopeAngle(RaycastHit hit)
    {
        return GetSlopeAngle(hit.normal);
    }

    public float GetWallAngle(Vector3 planeNormal)
    {
        return SignedRelativeAngle(Vector3.forward, planeNormal, Vector3.up);
    }

    bool IsSlopeAngleAWall(float slopeAngle)
    {
        return !(slopeAngle > minClimbAngle && slopeAngle <= maxClimbAngle);
    }

    void ClimbSlope(ref Vector3 vel, Raycast rayCast)
    {
        //Debug.Log("Start ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        //Plane slopePlane = new Plane(rayCast.normal.normalized,rayCast.ray.point);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        //Plane movementPlane = new Plane(movementNormal, raycastOrigins.Center);
        Vector3 climbVel = Vector3.Cross(rayCast.normal, movementNormal).normalized;
        //print("movementNormal= " + movementNormal.ToString("F5")+"; ClimbVel= "+climbVel.ToString("F5"));
        climbVel *= moveDistance;
        if (rayCast.vel.y <= climbVel.y)
        {
            //print("CLIMBING");
            vel = climbVel;
            collisions.below = true;
            collisions.collSt = CollisionState.climbing;
            collisions.slopeAngle = rayCast.slopeAngle;
            collisions.realSlopeAngle = Mathf.Asin(climbVel.y / climbVel.magnitude) * Mathf.Rad2Deg;
            //print("REAL SLOPE ANGLE = " + collisions.realSlopeAngle);
            //print("CLIMBING: Angle = " + rayCast.slopeAngle + "; old Vel= " + horVel.ToString("F5") + "; vel= " + vel.ToString("F5") +
            //    "; old magnitude=" + horVel.magnitude + "; new magnitude = " + vel.magnitude);
            if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green);

        }
        //Debug.Log("Finish ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
    }

    void DescendSlope(ref Vector3 vel, Raycast rayCast)
    {
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        //Plane slopePlane = new Plane(rayCast.normal.normalized,rayCast.ray.point);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        //Plane movementPlane = new Plane(movementNormal, raycastOrigins.Center);
        Vector3 climbVel = Vector3.Cross(rayCast.normal, movementNormal).normalized;
        //print("movementNormal= " + movementNormal.ToString("F5")+"; ClimbVel= "+climbVel.ToString("F5"));
        climbVel *= moveDistance;
        if (rayCast.vel.y <= 0 && climbVel.y < 0)//NO SE CON SEGURIDAD SI ESTA BIEN ESTA COMPROBACION
        {
            vel = climbVel;
            collisions.below = true;
            collisions.collSt = CollisionState.descending;
            collisions.slopeAngle = rayCast.slopeAngle;
            if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green);
        }
    }

    void SlipSlope(ref Vector3 vel, Raycast rayCast)
    {
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        Vector3 wallHorNormal = new Vector3(rayCast.normal.x, 0, rayCast.normal.z).normalized;
        if (!disableAllRays) Debug.DrawRay(rayCast.ray.point, wallHorNormal * 2, Color.red);
        Vector3 movementNormal = new Vector3(wallHorNormal.z, 0, -wallHorNormal.x).normalized;
        if (!disableAllRays) Debug.DrawRay(rayCast.origin, movementNormal * 2, Color.yellow);
        Vector3 slipDir = Vector3.Cross(rayCast.normal, movementNormal).normalized;
        if (!disableAllRays) Debug.DrawRay(rayCast.origin, slipDir * 2, Color.green);
        Vector3 slipVel = (slipDir * vel.y) + horVel;
        //slipVel.y = vel.y;
        //float angWithWall = Vector3.Angle(wallHorNormal, horVel);

        vel = slipVel;
        collisions.below = false;
        collisions.collSt = CollisionState.sliping;
        collisions.slopeAngle = rayCast.slopeAngle;
        collisions.verWall = collisions.closestVerRaycast.ray.transform.gameObject;
        if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green);
    }

    CollisionState CheckSlopeType(Vector3 vel, Raycast ray)
    {
        RaycastHit hit = ray.ray;
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float moveDistance = horVel.magnitude;
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        Vector3 climbVel = Vector3.Cross(hit.normal, movementNormal).normalized;
        climbVel *= moveDistance;

        // --- AXIS X & Z ---
        if (ray.axis == Axis.X || ray.axis == Axis.Z)
        {
            if (ray.row == 0)
            {
                if (climbVel.y > 0 && !IsSlopeAngleAWall(ray.slopeAngle))
                {
                    return CollisionState.climbing;
                }
                else
                {
                    return CollisionState.wall;
                }
            }
            else
            {
                return CollisionState.wall;
            }
        }
        else // --- AXIS Y ---
        {
            if (climbVel.y > 0 && !IsSlopeAngleAWall(ray.slopeAngle))
            {
                return CollisionState.climbing;
            }
            else if (climbVel.y < 0 && ray.slopeAngle <= maxDescendAngle && ray.slopeAngle > minDescendAngle)
            {
                return CollisionState.descending;
            }
            else if (ray.axis == Axis.Y && ((vel.y < 0 && ray.slopeAngle > maxDescendAngle) || (vel.y > 0 && ray.slopeAngle != 0)))
            {
                return CollisionState.sliping;
            }
            else
            {
                return CollisionState.none;//FLOOR
            }
        }

    }

    /// <summary>
    /// the difference with CheckSlopeType is that this is only for wall edge function (horizontal raycasts) and any row's ray can return a result of climbing, instead of only the row 0
    /// </summary>
    /// <param name="vel"></param>
    /// <param name="ray"></param>
    /// <returns></returns>
    CollisionState CheckSlopeTypeWallEdges(Vector3 vel, Raycast ray)
    {
        RaycastHit hit = ray.ray;
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        Vector3 climbVel = Vector3.Cross(hit.normal, movementNormal).normalized;
        climbVel *= moveDistance;

        // --- AXIS X & Z ---
        if (ray.axis == Axis.X || ray.axis == Axis.Z)
        {
            if (ray.row != 0)
            {
                return CollisionState.wall;
            }
            else
            {
                if (climbVel.y > 0 && !IsSlopeAngleAWall(ray.slopeAngle))
                {
                    return CollisionState.climbing;
                }
                else
                {
                    return CollisionState.wall;
                }
            }
        }
        else // --- AXIS Y ---
        {
            Debug.Log("Error: should not be sending raycasts from axis Y to this function");
            return CollisionState.none;
        }

    }
    #endregion

    #region --- COLLISIONS (RAYCASTS FUNCTIONS) --- 

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

    #region -- HORIZONTAL COLLISIONS --
    void HorizontalCollisions(ref Vector3 vel, bool wallSlide)
    {
        #region Raycasts
        Raycast closestRaycast = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
        Raycast closestWallRaycast = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
        Raycast closestClimbRaycast = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
        corsbColor = wallSlide ? darkYellow : Color.black;
        Color rayColorHit = wallSlide ? Color.yellow : Color.red;
        Color rayColorNoHit = wallSlide ? darkYellow : darkRed;
        int rows, raysPerRow;
        float rowSpacing, raySpacing;
        rows = wallSlide ? wallSlideRows : horizontalRows;
        raysPerRow = wallSlide ? wallSlideRaysPerRow : horizontalRaysPerRow;
        rowSpacing = wallSlide ? wallSlideRowSpacing : horizontalRowSpacing;
        raySpacing = wallSlide ? wallSlideRaySpacing : horizontalRaySpacing;

        collisions.horRaycastsX = new Raycast[rows, raysPerRow];
        collisions.horRaycastsZ = new Raycast[rows, raysPerRow];

        Vector3 horVel = new Vector3(vel.x, 0, vel.z);//DO NOT CHANGE ORDER
        float rayLength = horVel.magnitude + skinWidth;//DO NOT CHANGE ORDER
        horVel = horVel.normalized;//DO NOT CHANGE ORDER
        float directionX = 0, directionZ = 0;

        //VARIABLES FOR "Cutting Out Raycast's Skinwidth in Borders"
        float corsbAngle = 0;

        //VARAIBLES FOR WALLSLIDE
        Vector3 lastWallNormal = new Vector3(collisions.wallNormal.x, 0, collisions.wallNormal.z).normalized;

        #region Raycasts X
        //if (!(wallSlide && collisions.closestHorRaycast.axis == Axis.X && Mathf.Sign(collisions.closestHorRaycast.vel.x) == Mathf.Sign(vel.x)))
        //{
        if (vel.x != 0)
        {
            directionX = Mathf.Sign(vel.x);
            Vector3 rowsOriginX = directionX == 1 ? raycastOrigins.BottomRFCornerReal : raycastOrigins.BottomLFCornerReal;
            corsbAngle = Vector3.Angle(Vector3.forward, horVel);

            //print("CONTROLLER 3D: " + directionX + "*X corsbAngle = " + corsbAngle + "; corsbBorderHorRaysPerRow = " + corsbBorderHorRaysPerRow +
            //    "; (raysPerRow - corsbBorderHorRaysPerRow) = " + (raysPerRow - corsbBorderHorRaysPerRow));
            if (wallSlide) rowsOriginX += collisions.wallNormal.normalized * precisionSpaceFromSlideWall;//LEAVE SAFE SPACE FROM WALL 
            for (int i = 0; i < rows; i++)//ROWS
            {
                Vector3 rowOriginX = rowsOriginX;
                rowOriginX.y = (rowsOriginX.y) + i * rowSpacing;
                if (i == 0)
                {
                    rowOriginX += Vector3.up * skinWidthHeight;
                }
                else if (i == rows - 1)
                {
                    rowOriginX += Vector3.down * skinWidthHeight;
                }
                Vector3 lastOriginX = rowOriginX;
                for (int j = 0; j < raysPerRow; j++)//COLUMNS
                {
                    Vector3 rayOriginX = rowOriginX + Vector3.back * (j * raySpacing);
                    if (showHorizontalLimits && !disableAllRays)
                    {
                        Debug.DrawLine(lastOriginX, rayOriginX, wallSlide ? Color.cyan : Color.blue);
                    }
                    lastOriginX = rayOriginX;

                    #region --- CORSB SYSTEM ---
                    // --- CORSB SYSTEM ---    
                    //VARIABLES FOR CORSB
                    float corsbDistanceToBorder = 0;
                    float corsbSkinWidth = skinWidth;
                    float corsbRayLength = rayLength;

                    if (((j < corsbBorderHorRaysPerRow && corsbAngle > 90) || (j >= (raysPerRow - corsbBorderHorRaysPerRow) && corsbAngle < 90)) && corsbOn)
                    {
                        float auxCorsbSkinWidth = float.MaxValue;
                        float auxAngle;
                        if (j < corsbBorderHorRaysPerRow && corsbAngle > 90)//primer 20% de rayos
                        {
                            auxAngle = 180 - corsbAngle;
                            corsbDistanceToBorder = (float)j * (float)raySpacing;
                            //print("CONTROLLER 3D: CORSB system checking started (Part1)-> j = " + j + "; corsbDistanceToBorder = " + corsbDistanceToBorder);
                        }
                        else //último 20%
                        {
                            auxAngle = corsbAngle;
                            corsbDistanceToBorder = (float)(raysPerRow - (j + 1)) * (float)raySpacing;
                            //print("CONTROLLER 3D: CORSB system checking started (Part2)-> j = " + j + "; corsbDistanceToBorder = " + corsbDistanceToBorder);
                        }
                        auxCorsbSkinWidth = corsbDistanceToBorder / Mathf.Cos(auxAngle * Mathf.Deg2Rad);
                        auxCorsbSkinWidth = Mathf.Clamp(auxCorsbSkinWidth, corsbMinSkinWidth, skinWidth);
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

                    rayOriginX += (-horVel * corsbSkinWidth);
                    RaycastHit hit;
                    Raycast auxRay = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);

                    if (Physics.Raycast(rayOriginX, horVel, out hit, corsbRayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = GetWallAngle(hit.normal); //SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up);// Vector3.Angle(hit.normal, Vector3.forward);
                        auxRay = new Raycast(hit, hit.normal, (hit.distance - corsbSkinWidth), vel, rayOriginX, true, slopeAngle, wallAngle, Axis.X,
                            i, j, rows, corsbSkinWidth);
                        //WE STORE ALL THE RAYCASTS INFO
                        collisions.horRaycastsX[i, j] = auxRay;

                        if (IsCloserHorizontalRay(closestRaycast, auxRay))
                        {
                            closestRaycast = auxRay;
                        }
                        //Closest wall ray
                        SaveClosestHorizontalWallRay(ref closestWallRaycast, auxRay, closestClimbRaycast);
                        //Closest climbing ray
                        SaveClosestHorizontalClimbingRay(ref closestClimbRaycast, auxRay);

                    }
                    else
                    {
                        //WE STORE ALL THE RAYCASTS INFO
                        auxRay = new Raycast(hit, hit.normal, (hit.distance - corsbSkinWidth), vel, rayOriginX, false, 0, 0, Axis.X, i, j, rows, corsbSkinWidth);
                        collisions.horRaycastsX[i, j] = auxRay;
                    }
                    if (showHorizontalRays && !disableAllRays) Debug.DrawRay(rayOriginX, horVel * corsbRayLength, (corsbSkinWidth < skinWidth ? corsbColor : auxRay.hit ? rayColorHit : rayColorNoHit));
                }
            }
        }
        //}
        #endregion

        #region Raycasts Z
        //if (!(wallSlide && collisions.closestHorRaycast.axis == Axis.Z && Mathf.Sign(collisions.closestHorRaycast.vel.z) == Mathf.Sign(vel.z)))
        //{
        if (vel.z != 0)
        {
            directionZ = Mathf.Sign(vel.z);
            Vector3 rowsOriginZ = directionZ == 1 ? raycastOrigins.BottomLFCornerReal : raycastOrigins.BottomLBCornerReal;
            corsbAngle = Vector3.Angle(Vector3.left, horVel);
            //print("CONTROLLER 3D: " + directionZ + "*Z corsbAngle = " + corsbAngle + "; corsbBorderHorRaysPerRow = " + corsbBorderHorRaysPerRow +
            //    "; (raysPerRow - corsbBorderHorRaysPerRow) = " + (raysPerRow - corsbBorderHorRaysPerRow)+ "; horizontalRaySpacing = " + horizontalRaySpacing);
            if (wallSlide) rowsOriginZ += (collisions.wallNormal.normalized * precisionSpaceFromSlideWall);//LEAVE SAFE SPACE FROM WALL 
            for (int i = 0; i < rows; i++)
            {
                Vector3 rowOriginZ = rowsOriginZ;
                rowOriginZ.y = (rowsOriginZ.y) + i * rowSpacing;
                if (i == 0)
                {
                    rowOriginZ += Vector3.up * skinWidthHeight;
                }
                else if (i == rows - 1)
                {
                    rowOriginZ += Vector3.down * skinWidthHeight;
                }
                Vector3 lastOriginZ = rowOriginZ;
                for (int j = 0; j < raysPerRow; j++)
                {
                    Vector3 rayOriginZ = rowOriginZ + Vector3.right * (j * raySpacing);
                    if (showHorizontalLimits && !disableAllRays)
                    {
                        Debug.DrawLine(lastOriginZ, rayOriginZ, wallSlide ? Color.cyan : Color.blue);
                    }
                    lastOriginZ = rayOriginZ;
                    if (!DontThrowRaycastTwice(vel, j, i, rows, raysPerRow))
                    {

                        #region --- CORSB SYSTEM ---
                        // --- CORSB SYSTEM ---    
                        //VARIABLES FOR CORSB
                        float corsbDistanceToBorder = 0;
                        float corsbSkinWidth = skinWidth;
                        float corsbRayLength = rayLength;

                        if (((j < corsbBorderHorRaysPerRow && corsbAngle > 90) || (j >= (raysPerRow - corsbBorderHorRaysPerRow) && corsbAngle < 90)) && corsbOn)
                        {
                            float auxCorsbSkinWidth = float.MaxValue;
                            float auxAngle;
                            if (j < corsbBorderHorRaysPerRow && corsbAngle > 90)//primer 20% de rayos
                            {
                                auxAngle = 180 - corsbAngle;
                                corsbDistanceToBorder = (float)j * (float)raySpacing;
                                //print("CONTROLLER 3D: CORSB system checking started (Part1)-> j = " + j + "; corsbDistanceToBorder = " + corsbDistanceToBorder);
                            }
                            else //último 20%
                            {
                                auxAngle = corsbAngle;
                                corsbDistanceToBorder = (float)(raysPerRow - (j + 1)) * (float)raySpacing;
                                //print("CONTROLLER 3D: CORSB system checking started (Part2)-> j = " + j + "; corsbDistanceToBorder = " + corsbDistanceToBorder);
                            }
                            auxCorsbSkinWidth = corsbDistanceToBorder / Mathf.Cos(auxAngle * Mathf.Deg2Rad);
                            auxCorsbSkinWidth = Mathf.Clamp(auxCorsbSkinWidth, corsbMinSkinWidth, skinWidth);
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

                        rayOriginZ += (-horVel * corsbSkinWidth);
                        RaycastHit hit;
                        Raycast auxRay = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
                        if (Physics.Raycast(rayOriginZ, horVel, out hit, corsbRayLength, collisionMask, QueryTriggerInteraction.Ignore))
                        {
                            float slopeAngle = GetSlopeAngle(hit);
                            float wallAngle = GetWallAngle(hit.normal); // SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up);// Vector3.Angle(hit.normal, Vector3.forward);
                            auxRay = new Raycast(hit, hit.normal, (hit.distance - corsbSkinWidth), vel, rayOriginZ, true, slopeAngle, wallAngle, Axis.Z,
                                i, j, rows, corsbSkinWidth);
                            //WE STORE ALL THE RAYCASTS INFO
                            collisions.horRaycastsZ[i, j] = auxRay;//when wall Slide this is overwritten by the second set of rays (wallSlideCollisions)

                            //bool newClosestRay = !(i == 0 && j == 0) ? IsCloserHorizontalRay(closestRaycast, auxRay) : true;
                            if (IsCloserHorizontalRay(closestRaycast, auxRay))
                            {
                                closestRaycast = auxRay;
                            }
                            //Closes wall ray
                            SaveClosestHorizontalWallRay(ref closestWallRaycast, auxRay, closestClimbRaycast);
                            //Closest climbing ray
                            SaveClosestHorizontalClimbingRay(ref closestClimbRaycast, auxRay);
                        }
                        else
                        {
                            //WE STORE ALL THE RAYCASTS INFO
                            auxRay = new Raycast(hit, hit.normal, (hit.distance - corsbSkinWidth), vel, rayOriginZ, false, 0, 0, Axis.Z, i, j, rows, corsbSkinWidth);
                            collisions.horRaycastsZ[i, j] = auxRay;
                        }
                        if (showHorizontalRays && !disableAllRays) Debug.DrawRay(rayOriginZ, horVel * corsbRayLength, (corsbSkinWidth < skinWidth ? corsbColor : auxRay.hit ? rayColorHit : rayColorNoHit));
                    }
                }
            }
        }
        //}
        #endregion

        #endregion

        if (closestRaycast.hit)//si ha habido una collision horizontal
        {
            if (!disableAllDebugs) if (wallSlide) Debug.LogWarning("Wall Slide collisions had a hit");
            CollisionState value = (closestClimbRaycast.hit && closestWallRaycast.hit) || (!closestClimbRaycast.hit && !closestWallRaycast.hit) ? CheckSlopeType(vel, closestRaycast):
                closestClimbRaycast.hit? CheckSlopeType(vel, closestClimbRaycast) : CheckSlopeType(vel, closestWallRaycast);

            #region --- SLOPE'S EDGE WALL PROBLEMATIC ---
            if (!disableAllDebugs) Debug.Log("SLOPE EDGE PROBLEMATIC: START -> closestClimbRaycast.hit = " + closestClimbRaycast.hit + "; closestWallRaycast.hit = " + closestWallRaycast.hit);
            if (closestClimbRaycast.hit && closestWallRaycast.hit)
            {
                if (!disableAllDebugs) Debug.Log("SLOPE EDGE PROBLEMATIC: closestWallRaycast.wallAngle = " + closestWallRaycast.wallAngle + "; closestClimbRaycast.wallAngle = " + closestClimbRaycast.wallAngle
                    + "; closestClimbRaycast.distance = " + closestClimbRaycast.distance + "; closestWallRaycast.distance = " + closestWallRaycast.distance +
                    "; closestWallRaycast.row = " + closestWallRaycast.row + "; closestWallRaycast.column = " + closestWallRaycast.column);
                if (closestWallRaycast.wallAngle == closestClimbRaycast.wallAngle && closestClimbRaycast.distance < closestWallRaycast.distance)
                {
                    //value = CollisionState.climbing;
                    if (value != CollisionState.climbing) Debug.LogError("SLOPE EDGE PROBLEMATIC: Decided to climb but we are not climbing. Just checking if this happens");
                }
                else
                {
                    if (!disableAllDebugs) Debug.Log("SLOPE EDGE PROBLEMATIC: collisions.lastCollSt = " + collisions.lastCollSt);
                    if (collisions.lastCollSt == CollisionState.climbing)
                    {
                        if (!disableAllDebugs) Debug.Log("SLOPE EDGE PROBLEMATIC: collisions.oldFcww = " + collisions.oldFcww);

                        if (collisions.oldFcww == FirstCollisionWithWallType.none)//CLIMBING AND APPROACHING WALL
                        {
                            collisions.fcww = closestWallRaycast.normal.y < 0 ? FirstCollisionWithWallType.climbingAndFowardWall : closestWallRaycast.normal.y > 0 ?
    FirstCollisionWithWallType.climbingAndBackwardsWall : FirstCollisionWithWallType.climbingAndStraightWall;
                            if (!disableAllDebugs) Debug.LogWarning("SLOPE EDGE PROBLEMATIC: CLIMBING AND COLLIDING WITH WALL for the first time; collisions.fcww = " + collisions.fcww);

                        }
                        else
                        {
                            //Debug.Log("SLOPE EDGE PROBLEMATIC: closestWallRaycast.row = " + closestWallRaycast.row);
                            //if (closestWallRaycast.row != 0 && closestClimbRaycast.slopeAngle != closestWallRaycast.slopeAngle)
                            //{
                            value = CollisionState.wall;
                            closestRaycast = closestWallRaycast;
                            if (!disableAllDebugs) Debug.LogWarning("SLOPE EDGE PROBLEMATIC: CLIMBING AND COLLIDING WITH WALL but not for the first time");


                            //}
                        }
                    }
                }
            }
            #endregion

            #region --- CHECK FOR CLIMBSTEP ---
            if (value == CollisionState.wall)
            {
                ////check for climbStep
                //float rayHeight = closestRaycast.row * rowSpacing;
                //if (rayHeight <= maxHeightToClimbStep)
                //{
                //    //CHECK IF THE WALL IS SHORT
                //    bool success = true;
                //    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                //    rayLength = (horVelAux.magnitude + closestRaycast.skinWidth);
                //    Vector3 rayOrigin = closestRaycast.origin + Vector3.up * vel.y;
                //    RaycastHit hit;
                //    if (!disableAllRays) Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.yellow, 4);

                //    if (Physics.Raycast(rayOrigin, horVelAux, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                //    {

                //        //for(int i = closestRaycast.row+1; i< collisions.horRaycastsX.GetLength(0); i++)
                //        //{
                //        //    //check if there was a hit 
                //        //}
                //        if (success)
                //        {
                //            //value = CollisionState.climbStep;
                //        }
                //    }
                //}
            }
            #endregion

            //print("COLLISION HOR: " + value + "; slopeAngle=" + closestRaycast.slopeAngle);
            switch (value)//con que tipo de objeto colisionamos? pared/cuesta arriba/cuesta abajo
            {
                #region Wall
                case CollisionState.wall:
                    //Debug.LogWarning("WALL: START");
                    float auxRayLength = new Vector3(closestRaycast.vel.x, 0, closestRaycast.vel.z).magnitude;
                    if (!disableAllRays && showHorizontalRays) Debug.DrawRay(closestRaycast.origin, horVel * (auxRayLength), Color.white);
                    #region -- Wall Edges --
                    WallEdgeAll(vel, ref closestRaycast, rows, raysPerRow, wallSlide);
                    #endregion

                    #region -- VALID WALL --
                    //check if the "wall" is not just the floor/really small ridge
                    bool validWall = false;
                    if (collisions.lastCollSt == CollisionState.descending)
                    {
                        float heightToPrecisionHeight = precisionHeight - (closestRaycast.origin.y - raycastOrigins.BottomLBCornerReal.y);
                        if (heightToPrecisionHeight <= 0)
                        {
                            validWall = true;
                        }
                        else
                        {
                            Vector3 rayOriginAux = closestRaycast.origin + Vector3.up * heightToPrecisionHeight;
                            RaycastHit hitAux;
                            if (Physics.Raycast(rayOriginAux, horVel, out hitAux, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                            {
                                float slopeAngle = GetSlopeAngle(hitAux);
                                if (slopeAngle == closestRaycast.slopeAngle)
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
                    if (wallSlide && (closestRaycast.wallAngle == collisions.wallEdgeFirstWallAngle || closestRaycast.wallAngle == collisions.wallEdgeSecondWallAngle
      || closestRaycast.wallAngle == collisions.wallEdgeNewWallAngle))
                    {
                        validWall = false;
                    }
                    else
                    {
                        validWall = true;
                    }

                    #endregion
                    if (validWall)
                    {
                        SlideState slideSt = GetSlideState(collisions.startVel, closestRaycast);
                        SlideState oldSlideSt = wallSlide ? collisions.slideSt : collisions.oldSlideSt;
                        if (!disableAllDebugs) Debug.Log("Vel Pre First Time Wall= " + vel.ToString("F4"));
                        if (!disableAllDebugs) Debug.LogWarning("old wall angle = " + collisions.wallAngleOld + "; new wall angle = " + closestRaycast.wallAngle +
                        "; SlideSt = " + slideSt + "; oldSlideSt = " + oldSlideSt);
                        #region -- FIRST TIME COLLIDING WITH WALL --
                        if (collisions.wallAngleOld == -500 || (AreAnglesDifferent(collisions.wallAngleOld, closestRaycast.wallAngle) && oldSlideSt != slideSt))//NEW WALL //(auxSlideSt == SlideState.right && clockWise == 1) || (auxSlideSt == SlideState.left && clockWise == -1) 
                        {
                            if ((collisions.slideSt != SlideState.none && wallSlide) || (collisions.slideSt == SlideState.none && !wallSlide))
                            {
                                //if (!disableAllDebugs) if (wallSlide) Debug.Log("closestRaycast.wallAngle = " + closestRaycast.wallAngle + "; collisions.wallEdgeFirstWallNormal = " + collisions.wallEdgeFirstWallNormal +
                                //    "; collisions.wallEdgeSecondWallNormal = " + collisions.wallEdgeSecondWallNormal);
  
                                    if (!disableAllDebugs) Debug.LogError("WALL: APPROACHING NEW WALL: " + "distance = " + closestRaycast.distance);
                                    collisions.fcww = FirstCollisionWithWallType.normalWall;
                                    horVel = horVel * (closestRaycast.distance);
                                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                                    collisions.wallAngle = closestRaycast.wallAngle;
                                    collisions.wallNormal = closestRaycast.normal;
                                    if (collisions.wallAngleOld2 == closestRaycast.wallAngle && collisions.oldSlideSt2 == slideSt)
                                    {
                                        if (!disableAllDebugs) Debug.LogWarning("NEW WALL BUT WE ARE STUCK IN CORNER SO WE DONT MOVE");
                                    }
                                    if (collisions.wallAngleOld != -500 && AreAnglesDifferent(collisions.wallAngleOld, closestRaycast.wallAngle) && oldSlideSt != slideSt)
                                    {
                                        collisions.slideSt = slideSt;
                                    }
                                    if (!disableAllDebugs) Debug.Log("Vel POST First Time Wall= " + vel.ToString("F4"));
                                }
                                else if (!disableAllDebugs) Debug.LogWarning("Wallslide hit but we do nothing");
                        }
                        #endregion
                        #region -- NOT FIRST TIME COLLIDING WITH WALL --
                        else//COLLIDING WITH WALL and not the first frame (stop frame, the frame we use to stop in time before going through the wall)
                        {
                            if (collisions.wallAngleOld != -500 && AreAnglesDifferent(collisions.wallAngleOld, closestRaycast.wallAngle) && oldSlideSt == slideSt)
                            {
                                collisions.fcww = FirstCollisionWithWallType.slidingAndWallContinue;
                            }
                            if (closestRaycast.distance < 0)//SI ESTAMOS METIDOS DENTRO DEL MURO POR ALGUN MOTIVO
                            {
                                if (!disableAllDebugs) Debug.LogWarning("WE GOT INSIDE THE WALL! -> closestRaycast.distance = " + closestRaycast.distance);
                                //horVel = horVel * (closestRaycast.distance);
                                //vel = new Vector3(horVel.x, vel.y, horVel.z);
                                if (Mathf.Abs(closestRaycast.distance) > 1)
                                {
                                    if (!disableAllDebugs) Debug.LogError("WE ARE MOVING TOO FAR AWAY! not moving.");
                                }
                                else
                                {
                                    Vector3 moveOutVel = horVel.normalized * closestRaycast.distance;
                                    transform.Translate(moveOutVel, Space.World);
                                    UpdateRaycastOrigins();
                                }
                            }
                            //else if (closestRaycast.distance > 0.0001f && collisions.fcww != FirstCollisionWithWallType.slidingAndWallContinue)
                            //{
                            //    Debug.LogWarning("WE ARE AWAY FROM THE WALL! -> closestRaycast.distance = " + closestRaycast.distance);
                            //    //horVel = horVel * (closestRaycast.distance);
                            //    //vel = new Vector3(horVel.x, vel.y, horVel.z);
                            //    Vector3 moveOutVel = horVel.normalized * closestRaycast.distance;
                            //    transform.Translate(moveOutVel, Space.World);
                            //    UpdateRaycastOrigins();
                            //}
                            if (!wallSlide || (wallSlide && collisions.fcww == FirstCollisionWithWallType.slidingAndWallContinue))
                            {
                                Vector3 inputVel = collisions.startVel;
                                WallSlide(ref inputVel, closestRaycast);
                                vel = inputVel;
                                if (wallSlide && collisions.fcww == FirstCollisionWithWallType.slidingAndWallContinue)//snap to wall and slide
                                {
                                    horVel = new Vector3(inputVel.x, 0, inputVel.z);
                                    Vector3 finalPos = closestRaycast.ray.point;
                                    if (!disableAllDebugs) Debug.LogWarning("finalPos1 = " + finalPos);
                                    finalPos += horVel;
                                    if (!disableAllDebugs) Debug.LogWarning("finalPos2 = " + finalPos + "; horVel = " + horVel.ToString("F4"));
                                    Vector3 oldHorVel = new Vector3(closestRaycast.vel.x, 0, closestRaycast.vel.z);
                                    Vector3 originalPos = closestRaycast.origin + (oldHorVel.normalized * closestRaycast.skinWidth);
                                    Vector3 newHorVel = finalPos - originalPos;
                                    if (!disableAllDebugs) Debug.LogWarning("Vel Pre Snap To WallnSlide= " + vel.ToString("F4") + "; originalPos = " + originalPos + "; finalPos = " + finalPos +
                                        "; newHorVel = " + newHorVel);
                                    vel = new Vector3(newHorVel.x, inputVel.y, newHorVel.z);
                                    if (newHorVel.magnitude > 10) Debug.LogError("Error: LA VELOCIDAD ES DEMASIADO ALTA!!!");
                                    if (!disableAllDebugs) Debug.LogWarning("Vel Post Snap To WallnSlide= " + vel.ToString("F4"));
                                    if (!disableAllDebugs) Debug.LogWarning("Wall Slide found new wall in the same wall slide direction");
                                    if (!disableAllRays && showWallRays) Debug.DrawLine(originalPos, finalPos, Color.yellow);
                                }
                            }
                            //GUARDAMOS PARÁMETROS DE COLLISIONS
                            collisions.horWall = closestRaycast.ray.transform.gameObject;
                            collisions.wallNormal = closestRaycast.normal;
                            switch (closestRaycast.axis)
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
                            //TO TEST: Esto debería ser en un rayo a la altura de los pies(done)
                            RaycastHit hitAux;
                            Vector3 origin = closestRaycast.origin;
                            origin.y = collisions.horRaycastsX[0, 0].origin.y;
                            if (Physics.Raycast(origin, Vector3.down, out hitAux, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                            {
                                collisions.floorAngle = GetSlopeAngle(hitAux);
                            }
                            if (!wallSlide)
                            {
                                HorizontalCollisions(ref vel, true);
                            }
                        }
                        #endregion
                    }
                    break;
                #endregion
                #region Climbing
                case CollisionState.climbing:
                    if (!disableAllDebugs) Debug.Log("Start climbing = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
                    //print("AUXILIAR RAYS FOR DISTANCE CALCULATION");
                    #region error check
                    if (closestRaycast.row > 0)//THIS IS FOR ERROR CHECKING
                    {
                        Debug.LogError("collided with a ray that is row " + closestRaycast.row + "; Axis = " + closestRaycast.axis);
                        for (int i = 0; i <= closestRaycast.row; i++)
                        {
                            string message = "Line " + i + " has distances:[";
                            for (int j = 0; j < collisions.horRaycastsZ.GetLength(1); j++)
                            {
                                message += collisions.horRaycastsZ[i, j].distance.ToString("F6") + ",";
                            }
                            message += "]";
                            print(message);
                        }
                    }
                    #endregion
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (horVelAux.magnitude + closestRaycast.skinWidth);
                    if (!disableAllRays) Debug.DrawRay(closestRaycast.origin, horVelAux.normalized * rayLength, Color.cyan);

                    float distanceToSlopeStart = 0;
                    if (collisions.slopeAngleOld != closestRaycast.slopeAngle)
                    {
                        distanceToSlopeStart = closestRaycast.distance;
                        horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                    }
                    //Debug.Log("Start ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
                    ClimbSlope(ref vel, closestRaycast);
                    //Debug.Log("Finish ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
                    horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                    //Debug.Log("After ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);

                    #region -- CHECK FOR NEXT SLOPE / WALL --
                    //--------------------- CHECK FOR NEXT SLOPE OR WALL -------------------------------------
                    //TO DO: NEW SLOPE CHECK MUST BE DONE BY THROWING every ray possible in line i=0, not by throwing 1 ray only. 
                    ClimbingAndFoundWall(ref vel, closestRaycast, closestWallRaycast);
                    #region Check for Next Slope / Wall System
                    bool hasHit = false;
                    for (int k = 0; k < raysPerRow && !hasHit; k++)
                    {
                        horVel = new Vector3(vel.x, 0, vel.z);
                        Raycast originalRaycast = closestClimbRaycast.axis == Axis.X ? collisions.horRaycastsX[0, k] : collisions.horRaycastsZ[0, k];
                        Vector3 rayOrigin = originalRaycast.origin + Vector3.up * vel.y;//closestRaycast.origin + Vector3.up * vel.y;
                        rayLength = horVel.magnitude + originalRaycast.skinWidth;
                        Vector3 rayDir = horVel.normalized;
                        RaycastHit hit;
                        if (!disableAllRays) Debug.DrawRay(rayOrigin, rayDir * rayLength, Color.yellow);

                        if (Physics.Raycast(rayOrigin, rayDir, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                        {
                            float slopeAngle = GetSlopeAngle(hit);
                            if (!disableAllRays) Debug.DrawRay(rayOrigin, rayDir * rayLength, Color.magenta);
                            if (!disableAllDebugs) Debug.LogWarning("CHECK IF NEW SLOPE: newSlopeAngle = " + slopeAngle + "; collisions.slopeAngle = " + collisions.slopeAngle);
                            if (slopeAngle != collisions.slopeAngle)//NEW SLOPE FOUND
                            {
                                if (IsSlopeAngleAWall(slopeAngle))
                                {
                                    hasHit = true;
                                    if (collisions.fcww == FirstCollisionWithWallType.none)
                                    {
                                        if (!disableAllDebugs) Debug.Log("FCWW NOT TRIGGERED so we do it");
                                        collisions.fcww = hit.normal.y < 0 ? FirstCollisionWithWallType.climbingAndFowardWall : hit.normal.y > 0 ?
        FirstCollisionWithWallType.climbingAndBackwardsWall : FirstCollisionWithWallType.climbingAndStraightWall;
                                        float wallAngle = GetWallAngle(hit.normal);// SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up);// Vector3.Angle(hit.normal, Vector3.forward);
                                        Raycast wallRaycast = new Raycast(hit, hit.normal, hit.distance - originalRaycast.skinWidth, vel, rayOrigin, true, slopeAngle, wallAngle, originalRaycast.axis,
                                            originalRaycast.row, originalRaycast.column, rows, originalRaycast.skinWidth);
                                        ClimbingAndFoundWall(ref vel, closestRaycast, wallRaycast);
                                    }
                                }
                                else
                                {
                                    Vector3 ClimbSlopeBackWall_rayOrigin = closestRaycast.origin + (horVel.normalized * closestRaycast.skinWidth);//hit.point;
                                    Vector3 ClimbSlopeBackWall_rayDir = vel.normalized;//Vector3.down;
                                    float ClimbSlopeBackWall_rayLength = 1;
                                    if (!disableAllRays && showHorizontalRays) Debug.DrawRay(ClimbSlopeBackWall_rayOrigin, ClimbSlopeBackWall_rayDir * ClimbSlopeBackWall_rayLength, purple);
                                    RaycastHit ClimbSlopeBackWall_hit;

                                    if (Physics.Raycast(ClimbSlopeBackWall_rayOrigin, ClimbSlopeBackWall_rayDir, out ClimbSlopeBackWall_hit, ClimbSlopeBackWall_rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                                    {
                                        if (ClimbSlopeBackWall_hit.normal == closestWallRaycast.normal)
                                        {
                                            //hasHit = true;
                                            vel = vel.normalized * ClimbSlopeBackWall_hit.distance;
                                        }
                                    }
                                    else if (!disableAllDebugs) Debug.LogError("NEW SLOPE FOUND but could not hit it with the climbSLopeBackWall_ray");
                                }
                            }
                        }
                    }
                    if (!hasHit)
                    {
                        //Debug.LogWarning("Climbing slope finished!");
                        collisions.finishedClimbing = true;
                    }
                    #endregion
                    #endregion
                    //Debug.Log("END climbing = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
                    break;
                #endregion
                #region ClimbStep
                case CollisionState.climbStep:
                    Debug.LogWarning("CLIMB STEP STARTED");
                    break;
                    #endregion
            }
            collisions.closestHorRaycast = closestRaycast;
        }
    }

    bool DontThrowRaycastTwice(Vector3 vel, int column, int row, int rows, int raysPerRow)
    {
        bool success = false;
        if (vel.x != 0 && vel.z != 0 && (column == 0 || column == raysPerRow - 1))
        {
            if (vel.x > 0)
            {
                if (column == raysPerRow - 1)
                {
                    success = true;
                    Raycast x;
                    if (vel.z > 0)
                    {
                        x = collisions.horRaycastsX[row, 0];
                    }
                    else
                    {
                        x = collisions.horRaycastsX[row, raysPerRow - 1];
                    }
                    Raycast auxRay = new Raycast(x.ray, x.normal, x.distance, x.vel, x.origin, x.hit, x.slopeAngle, x.wallAngle, Axis.Z, x.row, column, rows, x.skinWidth);
                    collisions.horRaycastsZ[row, column] = auxRay;
                }
            }
            else
            {
                if (column == 0)
                {
                    success = true;
                    Raycast x;
                    if (vel.z > 0)
                    {
                        x = collisions.horRaycastsX[row, 0];
                    }
                    else
                    {
                        x = collisions.horRaycastsX[row, raysPerRow - 1];
                    }
                    Raycast auxRay = new Raycast(x.ray, x.normal, x.distance, x.vel, x.origin, x.hit, x.slopeAngle, x.wallAngle, Axis.Z, x.row, column, rows, x.skinWidth);
                    collisions.horRaycastsZ[row, column] = auxRay;
                }
            }
        }
        //if (success) Debug.LogError("NO LANZAMOS RAYO EN ROW = "+row+" Y COLUMN = " +column);
        return success;
    }

    void ClimbingAndFoundWall(ref Vector3 vel, Raycast closestRaycast, Raycast closestWallRaycast)
    {
        if (collisions.fcww == FirstCollisionWithWallType.climbingAndBackwardsWall || collisions.fcww == FirstCollisionWithWallType.climbingAndFowardWall
    || collisions.fcww == FirstCollisionWithWallType.climbingAndStraightWall)
        {
            if (!disableAllDebugs) Debug.Log("CLIMBING AND FOUND A WALL: collisions.fcww = " + collisions.fcww);
            Vector3 horVel = new Vector3(vel.x, 0, vel.z);
            switch (collisions.fcww)
            {
                case FirstCollisionWithWallType.climbingAndStraightWall:
                    ////tan alpha = y/xz;
                    //horVelAux = horVelAux.normalized * (closestWallRaycast.distance - closestWallRaycast.skinWidth);
                    //float y = Mathf.Tan(collisions.realSlopeAngle * Mathf.Deg2Rad) * horVelAux.magnitude;
                    //vel = new Vector3(horVelAux.x, y, horVelAux.z);
                    Vector3 ClimbSlopeStraightWall_rayOrigin = closestRaycast.origin + (horVel.normalized * closestRaycast.skinWidth);//hit.point;
                    Vector3 ClimbSlopeStraightWall_rayDir = vel.normalized;//Vector3.down;
                    float ClimbSlopeStraightWall_rayLength = 1;
                    if (!disableAllRays && showHorizontalRays) Debug.DrawRay(ClimbSlopeStraightWall_rayOrigin, ClimbSlopeStraightWall_rayDir * ClimbSlopeStraightWall_rayLength, purple);
                    RaycastHit ClimbSlopeStraightWall_hit;

                    if (Physics.Raycast(ClimbSlopeStraightWall_rayOrigin, ClimbSlopeStraightWall_rayDir, out ClimbSlopeStraightWall_hit, ClimbSlopeStraightWall_rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        if (ClimbSlopeStraightWall_hit.normal == closestWallRaycast.normal)
                        {
                            //hasHit = true;
                            vel = vel.normalized * ClimbSlopeStraightWall_hit.distance;
                        }
                    }
                    else Debug.LogError("NEW SLOPE FOUND but could not hit it with the climbSLopeBackWall_ray");
                    break;
                case FirstCollisionWithWallType.climbingAndBackwardsWall:
                    Vector3 ClimbSlopeBackWall_rayOrigin = closestRaycast.origin + (horVel.normalized * closestRaycast.skinWidth);//hit.point;
                    Vector3 ClimbSlopeBackWall_rayDir = vel.normalized;//Vector3.down;
                    float ClimbSlopeBackWall_rayLength = 1;
                    if (!disableAllRays && showHorizontalRays) Debug.DrawRay(ClimbSlopeBackWall_rayOrigin, ClimbSlopeBackWall_rayDir * ClimbSlopeBackWall_rayLength, purple);
                    RaycastHit ClimbSlopeBackWall_hit;

                    if (Physics.Raycast(ClimbSlopeBackWall_rayOrigin, ClimbSlopeBackWall_rayDir, out ClimbSlopeBackWall_hit, ClimbSlopeBackWall_rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        if (ClimbSlopeBackWall_hit.normal == closestWallRaycast.normal)
                        {
                            //hasHit = true;
                            vel = vel.normalized * ClimbSlopeBackWall_hit.distance;
                        }
                    }
                    else if (!disableAllDebugs) Debug.LogError("NEW SLOPE FOUND but could not hit it with the climbSLopeBackWall_ray");
                    break;
                case FirstCollisionWithWallType.climbingAndFowardWall:
                    Vector3 ClimbingAndFowardWall_rayOrigin = closestWallRaycast.origin + (horVel.normalized * closestWallRaycast.skinWidth);//hit.point;
                    Vector3 ClimbingAndFowardWall_rayDir = vel.normalized;//Vector3.down;
                    float ClimbingAndFowardWall_rayLength = 1;
                    if (!disableAllRays && showHorizontalRays) Debug.DrawRay(ClimbingAndFowardWall_rayOrigin, ClimbingAndFowardWall_rayDir * ClimbingAndFowardWall_rayLength, purple);

                    RaycastHit ClimbingAndFowardWall_hit;
                    if (Physics.Raycast(ClimbingAndFowardWall_rayOrigin, ClimbingAndFowardWall_rayDir, out ClimbingAndFowardWall_hit, ClimbingAndFowardWall_rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        if (ClimbingAndFowardWall_hit.normal == closestWallRaycast.normal)
                        {
                            vel = vel.normalized * ClimbingAndFowardWall_hit.distance;
                        }
                    }
                    else if (!disableAllDebugs) Debug.LogError("NEW WALL FOUND but could not hit it with the ray");
                    break;
            }
            collisions.wallAngle = closestWallRaycast.wallAngle;
            collisions.wallNormal = closestWallRaycast.normal;
        }
    }

    SlideState GetSlideState(Vector3 vel, Raycast raycast)
    {
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        Vector3 normal = -new Vector3(raycast.normal.x, 0, raycast.normal.z).normalized;

        Vector3 movementNormal = Vector3.up;
        Vector3 slideVel = Vector3.Cross(normal, movementNormal).normalized;
        //LEFT OR RIGHT ORIENTATION?
        float ang = Vector3.Angle(slideVel, horVel);
        //Debug.Log("GetSlideState -> slideVel = " + slideVel + "; horVel = " + horVel + "; ang = " + ang);
        return ang > 90 ? SlideState.right : SlideState.left;
    }

    void WallSlide(ref Vector3 vel, Raycast raycast)
    {
        if (!disableAllDebugs) Debug.Log("WALL SLIDE");
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float wallAngle = raycast.wallAngle;
        Vector3 normal = -new Vector3(raycast.normal.x, 0, raycast.normal.z).normalized;
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
        collisions.collSt = CollisionState.wall;
        collisions.wallAngle = wallAngle;
        if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, slideVel.normalized * 2, Color.green);
    }

    bool IsCloserHorizontalRay(Raycast lastClosestHorRay, Raycast newRay)
    {
        bool success = false;
        if (newRay.distance < lastClosestHorRay.distance)
        {
            //Debug.LogError(" I COLLIDED WITH A WALL FOR THE FIRST TIME; there was already another raycast saved as wall collision");
            success = true;
        }
        return success;
    }

    void SaveClosestHorizontalClimbingRay(ref Raycast closestClimbRaycast, Raycast newRay)
    {
        if (newRay.row == 0 && !IsSlopeAngleAWall(newRay.slopeAngle))
        {
            if (newRay.distance < closestClimbRaycast.distance)
            {
                closestClimbRaycast = newRay;
            }
        }
    }

    void SaveClosestHorizontalWallRay(ref Raycast closestWallRaycast, Raycast newRay, Raycast closestClimbRaycast)
    {
        if ((newRay.row != 0 && closestClimbRaycast.slopeAngle != newRay.slopeAngle) || IsSlopeAngleAWall(newRay.slopeAngle))
        {
            if (newRay.distance < closestWallRaycast.distance)
            {
                closestWallRaycast = newRay;
            }
        }
    }

    /// <summary>
    /// This function returns true when the angle difference between 2 angles is big enough, and avoids small changes.
    /// </summary>
    /// <param name="angle1"></param>
    /// <param name="angle2"></param>
    /// <returns></returns>
    bool AreAnglesDifferent(float angle1, float angle2)//angles in degrees
    {
        float angleDiff = Mathf.Abs(angle1 - angle2);
        return (angleDiff > 0.01f);
    }

    /// <summary>
    /// This function checks if you are colliding with a vertical wall edge and creates an invisible plane to collide with it 
    /// as if you were colliding with a "perpendicular wall to the corner.                
    /// Like in this drawing:          \  B /   
    ///                         Wall 1->\  /<-Wall 2
    ///                               A (\/) A    <- Wall edge
    ///                       --------------------- Plane
    /// </summary>
    /// <param name="vel"></param>
    /// <param name="closestHorRaycast"></param>
    /// <returns></returns>
    bool WallEdgeAll(Vector3 vel, ref Raycast closestHorRaycast, int rows, int columns, bool wallSlide)
    {
        if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: START ");
        bool success = false;
        if (!WallEdgeMultiple(vel, ref closestHorRaycast, rows))
        {
            int found = 0;
            Raycast auxClosestRay = closestHorRaycast;
            Raycast auxOtherRay = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
            Raycast differentWallRay = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
            bool closestRayIsRight = false;
            float checkForPerfectWallRayDir = 0;

            #region --- Horizontalmente ---
            //Segunda comprobacion / HIGH PRECISION (lanzando nuevos raycasts) ---
            if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: HORIZONTAL");
            //bool seikai = false;//seikai = correcto en japonés. es que quería usar "success" pero ya estaba usado
            int column = closestHorRaycast.column;
            int row = closestHorRaycast.row;
            Axis originalAxis = closestHorRaycast.axis;
            for (int sentido = -1; sentido < 2 && found == 0; sentido = sentido == -1 ? sentido = 1 : sentido = 2)//1=+;-1=- (en el orden de creación de rayos: BACK Y RIGHT ) 
            {
                #region Parámetros
                Raycast lastPrecisionRaycast = closestHorRaycast;
                float auxSkinWidth, originSkinWidth, endSkinWidth, raylength;
                Vector3 horVel, rayOrigin;
                Axis finalAxis = originalAxis;
                int finalSentido = sentido;
                int finalColumn = column;
                bool throwRays = false;
                #region Parámetros sin cambio de eje
                if (!((column == 0 && sentido == -1) || (column == (columns - 1) && sentido == 1)))
                {
                    throwRays = true;
                    originSkinWidth = closestHorRaycast.skinWidth;
                    endSkinWidth = originalAxis == Axis.X ?
                        collisions.horRaycastsX[row, column + sentido].skinWidth : collisions.horRaycastsZ[row, column + sentido].skinWidth;
                    horVel = new Vector3(vel.x, 0, vel.z);// DO NOT CHANGE ORDER
                    rayOrigin = closestHorRaycast.origin + (horVel.normalized * closestHorRaycast.skinWidth);
                }
                #endregion
                #region Parámetros con cambio de eje
                else//CAMBIO DE EJE
                {
                    if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: HORIZONTAL -> cambio de eje -> original axis = " + originalAxis + "; column = " + column + "; sentido = " + sentido);
                    originSkinWidth = closestHorRaycast.skinWidth;
                    horVel = new Vector3(vel.x, 0, vel.z);// DO NOT CHANGE ORDER
                    rayOrigin = closestHorRaycast.origin + (horVel.normalized * closestHorRaycast.skinWidth);
                    if (originalAxis == Axis.X)
                    {
                        finalAxis = Axis.Z;
                        if (vel.x > 0)
                        {
                            if (column == 0 && vel.z > 0)//AL PRINCIPIO, en este caso es ARRIBA
                            {
                                throwRays = true;
                                finalColumn = columns - 1;
                            }
                            else if (column == columns - 1 && vel.z < 0)//AL FINAL, en este caso es ABAJO
                            {
                                throwRays = true;
                                finalSentido = -1;
                            }
                        }
                        else if (vel.x < 0)
                        {
                            if (column == 0 && vel.z > 0)//AL PRINCIPIO, en este caso es ARRIBA
                            {
                                throwRays = true;
                                finalSentido = 1;
                            }
                            else if (column == columns - 1 && vel.z < 0)//AL FINAL, en este caso es ABAJO
                            {
                                throwRays = true;
                                finalColumn = 0;
                            }
                        }
                    }
                    else //EJE Z
                    {
                        finalAxis = Axis.X;
                        if (vel.z > 0)
                        {
                            if (column == 0 && vel.x < 0) //AL PRINCIPIO, en este caso es a la IZDA
                            {
                                throwRays = true;
                                finalSentido = 1;
                            }
                            else if (column == columns - 1 && vel.x > 0)//AL FINAL, en este caso es a la DCHA
                            {
                                throwRays = true;
                                finalColumn = 0;
                            }
                        }
                        else if (vel.z < 0)
                        {
                            if (column == 0 && vel.x < 0) //AL PRINCIPIO, en este caso es a la IZDA
                            {
                                throwRays = true;
                                finalColumn = columns - 1;
                            }
                            else if (column == columns - 1 && vel.x > 0)//AL FINAL, en este caso es a la DCHA
                            {
                                throwRays = true;
                                finalSentido = -1;
                            }
                        }

                    }
                    endSkinWidth = finalAxis == Axis.X ?
                        collisions.horRaycastsX[row, finalColumn].skinWidth : collisions.horRaycastsZ[row, finalColumn].skinWidth;
                }
                #endregion
                #endregion
                #region lanzar rayos
                for (int j = 1; j < wallEdgeHighPrecissionRays - 1 && found == 0 && throwRays; j++)
                {
                    float skinWidthDiff = -(originSkinWidth - endSkinWidth);
                    float progress = (float)j / (float)wallEdgeHighPrecissionRays;
                    auxSkinWidth = originSkinWidth + skinWidthDiff * progress;
                    //if (!disableAllDebugs) Debug.Log("skinWidthDiff = "+ skinWidthDiff.ToString("F6") + "; progress = "+ progress.ToString("F4") +"; j = "+j+ "; wallEdgeHighPrecissionRays = "
                    //    + wallEdgeHighPrecissionRays +"; j / wallEdgeHighPrecissionRays = " + "; auxSkinWidth = " + auxSkinWidth.ToString("F6") +"; originSkinWidth = "+ originSkinWidth.ToString("F6") + 
                    //    "; endSkinWidth = " + endSkinWidth.ToString("F6"));
                    raylength = horVel.magnitude + auxSkinWidth;// DO NOT CHANGE ORDER
                    Vector3 localRayOrigin = rayOrigin;
                    float localRayOriginOffset = finalSentido * wallEdgeHighPrecisionHorRaySpacing * j;
                    localRayOrigin += finalAxis == Axis.X ?
                        (Vector3.back * localRayOriginOffset) : (Vector3.right * localRayOriginOffset);
                    //Debug.Log("WALL EDGE: High Precision -> auxSkinWidth = " + auxSkinWidth.ToString("F6")+"; storedRaySkinWidth = "+ 
                    //    (collisions.horRaycastsZ[row, column + sentido].skinWidth).ToString("F6"));
                    localRayOrigin += (-horVel.normalized * auxSkinWidth);
                    //lanzar raycast
                    RaycastHit hit;
                    if (showWallEdgeRays && !disableAllRays)
                    {
                        Debug.DrawRay(localRayOrigin, horVel.normalized * raylength, purple);
                    }
                    if (Physics.Raycast(localRayOrigin, horVel.normalized, out hit, raylength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = GetWallAngle(hit.normal);// SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up); //Vector3.Angle(hit.normal, Vector3.forward);
                        Raycast auxRay = new Raycast(hit, hit.normal, (hit.distance - auxSkinWidth), vel, localRayOrigin, true, slopeAngle, wallAngle,
                            originalAxis, row, column, horizontalRows, auxSkinWidth);
                        CollisionState slopeType = CheckSlopeTypeWallEdges(vel, auxRay);
                        if (slopeType == CollisionState.wall && AreAnglesDifferent(auxRay.wallAngle, closestHorRaycast.wallAngle))
                        {
                            //Debug.LogWarning("WALL EDGE ALL: SEGUNDA COMPROBACION -> found!");
                            differentWallRay = auxRay;
                            closestHorRaycast = lastPrecisionRaycast;
                            found = 1;
                            if (finalAxis == Axis.X)
                            {
                                if (vel.x > 0)
                                {
                                    if (finalSentido == 1)
                                    {
                                        closestRayIsRight = false;
                                    }
                                    else
                                    {
                                        closestRayIsRight = true;
                                    }
                                }
                                else
                                {
                                    if (finalSentido == 1)
                                    {
                                        closestRayIsRight = true;
                                    }
                                    else
                                    {
                                        closestRayIsRight = false;
                                    }
                                }
                            }
                            else
                            {
                                if (vel.z > 0)
                                {
                                    if (finalSentido == 1)
                                    {
                                        closestRayIsRight = false;
                                    }
                                    else
                                    {
                                        closestRayIsRight = true;
                                    }
                                }
                                else
                                {
                                    if (finalSentido == 1)
                                    {
                                        closestRayIsRight = true;
                                    }
                                    else
                                    {
                                        closestRayIsRight = false;
                                    }
                                }
                            }
                            if (!disableAllDebugs) Debug.Log(" finalAxis = " + finalAxis + "; vel.x = " + vel.x + "; vel.z = " + vel.z + "; finalSentido = " + finalSentido +
                                "; closestRayIsRight = " + closestRayIsRight);
                            checkForPerfectWallRayDir = localRayOriginOffset;
                            //Debug.Log("WALL EDGE ALL: Horizontal ->  differentWallRay.distance = "+ differentWallRay.distance + "; auxClosestRay.distance = " + auxClosestRay.distance);
                            if (differentWallRay.distance < auxClosestRay.distance)
                            {
                                auxClosestRay = differentWallRay;
                                auxOtherRay = lastPrecisionRaycast;
                            }
                            else
                            {
                                auxOtherRay = differentWallRay;
                                auxClosestRay = lastPrecisionRaycast;

                            }
                        }
                        else
                        {
                            lastPrecisionRaycast = auxRay;
                        }
                    }
                    #region HARD CORNER
                    else // IF DIDN'T HIT
                    {
                        /*Vector3 hardCornerRayOrigin = lastRaycast.ray.point;
                        float hardCornerOriginOffset = finalSentido * wallEdgeHighPrecisionHorRaySpacing;
                        hardCornerRayOrigin += finalAxis == Axis.X ?
                            (Vector3.back * localRayOriginOffset) : (Vector3.right * localRayOriginOffset);
                        float hardCornerRayLength = 2;
                        Vector3 hardCornerRayDir = finalAxis == Axis.X ?
                            (Vector3.right * finalSentido) : (Vector3.back * finalSentido);
                        for (int k = 1; found==0; k++)
                        {
                            Vector3 hardCornerLocalRayOrigin = hardCornerRayOrigin + (horVel * k * wallEdgeHighPrecisionHorRaySpacing);
                            if(!disableAllRays && showWallEdgeRays)
                            {
                                Debug.DrawRay(hardCornerLocalRayOrigin, hardCornerRayDir * hardCornerRayLength, brown);
                            }
                            RaycastHit hardCornerHit;
                            if (Physics.Raycast(hardCornerLocalRayOrigin, hardCornerRayDir, out hardCornerHit, hardCornerRayLength, collisionMask, QueryTriggerInteraction.Ignore))
                            {
                                float slopeAngle = GetSlopeAngle(hardCornerHit);
                                float wallAngle = SignedRelativeAngle(Vector3.forward, hardCornerHit.normal, Vector3.up); //Vector3.Angle(hit.normal, Vector3.forward);
                                Raycast auxRay = new Raycast(hardCornerHit, (hit.distance - auxSkinWidth), vel, localRayOrigin, true, slopeAngle, wallAngle,
                                    originalAxis, row, column, horizontalRows, auxSkinWidth);
                                lastRaycast = auxRay;
                                CollisionState slopeType = CheckSlopeTypeWallEdges(vel, auxRay);
                                if (slopeType == CollisionState.wall && AreAnglesDifferent(auxRay.wallAngle, closestHorRaycast.wallAngle))
                                {
                                }
                        }
                        */
                    }
                    #endregion
                }
                #endregion
            }

            #endregion

            #region --- Verticalmente ---
            if (found == 0)//solo buscamos este si no hemos encontrado nada en el horizontal
            {
                if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: VERTICAL");
                for (int sentido = -1; sentido < 2 && found == 0; sentido = sentido == -1 ? sentido = 1 : sentido = 2)//1=+;-1=- (en el orden de creación de rayos: BACK Y RIGHT ) 
                {
                    Raycast lastPrecisionRaycast = closestHorRaycast;
                    float auxSkinWidth, raylength;
                    Vector3 horVel, rayOrigin;
                    Axis finalAxis = originalAxis;

                    if (!((row == 0 && sentido == -1) || (row == (horizontalRows - 1) && sentido == 1)))
                    {
                        #region Parámetros
                        auxSkinWidth = skinWidth;//originalAxis == Axis.X ? collisions.horRaycastsX[row + sentido, column].skinWidth : collisions.horRaycastsZ[row + sentido, column].skinWidth;
                        horVel = new Vector3(vel.x, 0, vel.z);// DO NOT CHANGE ORDER
                        raylength = horVel.magnitude + auxSkinWidth;// DO NOT CHANGE ORDER
                        horVel.Normalize();// DO NOT CHANGE ORDER
                        rayOrigin = closestHorRaycast.origin + (horVel * closestHorRaycast.skinWidth);
                        #endregion
                        #region lanzar rayos
                        for (int j = 1; j < wallEdgeHighPrecissionRays - 1 && found == 0; j++)
                        {
                            Vector3 localRayOrigin = rayOrigin;
                            float localRayOriginOffset = sentido * wallEdgeHighPrecisionVerRaySpacing * j;
                            localRayOrigin += Vector3.up * localRayOriginOffset;
                            //Debug.Log("WALL EDGE: High Precision -> auxSkinWidth = " + auxSkinWidth.ToString("F6")+"; storedRaySkinWidth = "+ 
                            //    (collisions.horRaycastsZ[row, column + sentido].skinWidth).ToString("F6"));
                            localRayOrigin += (-horVel * auxSkinWidth);
                            //lanzar raycast
                            RaycastHit hit;
                            if (showWallEdgeRays && !disableAllRays)
                            {
                                Debug.DrawRay(localRayOrigin, horVel * raylength, purple);
                            }
                            if (Physics.Raycast(localRayOrigin, horVel, out hit, raylength, collisionMask, QueryTriggerInteraction.Ignore))
                            {
                                float slopeAngle = GetSlopeAngle(hit);
                                float wallAngle = GetWallAngle(hit.normal);// SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up); //Vector3.Angle(hit.normal, Vector3.forward);
                                Raycast auxRay = new Raycast(hit, hit.normal, (hit.distance - auxSkinWidth), vel, localRayOrigin, true, slopeAngle, wallAngle,
                                    originalAxis, row, column, horizontalRows, auxSkinWidth);
                                CollisionState slopeType = CheckSlopeTypeWallEdges(vel, auxRay);
                                if (slopeType == CollisionState.wall && AreAnglesDifferent(auxRay.wallAngle, closestHorRaycast.wallAngle))
                                {
                                    if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: VERTICAL ->  found! slopeType = " + slopeType + "; auxRay.wallAngle = " + auxRay.wallAngle +
                                    "; closestHorRaycast.wallAngle = " + closestHorRaycast.wallAngle);
                                    differentWallRay = auxRay;
                                    closestHorRaycast = lastPrecisionRaycast;
                                    found = 2;
                                    if (sentido == 1)
                                    {
                                        closestRayIsRight = true;
                                    }
                                    else
                                    {
                                        closestRayIsRight = false;
                                    }
                                    checkForPerfectWallRayDir = localRayOriginOffset;
                                    if (differentWallRay.distance < auxClosestRay.distance)
                                    {
                                        auxClosestRay = differentWallRay;
                                        auxOtherRay = lastPrecisionRaycast;
                                    }
                                    else
                                    {
                                        auxOtherRay = differentWallRay;
                                        auxClosestRay = lastPrecisionRaycast;
                                    }
                                }
                                else
                                {
                                    lastPrecisionRaycast = auxRay;
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion

            if (found > 0)//DOS TIPOS DE MURO ENCONTRADOS
            {
                #region --- Comprobar si convexo o cóncavo ---
                if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: COMPROBACION CONCAVO/CONVEXO START");
                Vector3 edgeVector = closestRayIsRight ? Vector3.Cross(closestHorRaycast.normal, differentWallRay.normal).normalized :
                    Vector3.Cross(differentWallRay.normal, closestHorRaycast.normal).normalized;
                //Debug.Log("WALL EDGE ALL: closestRayIsRight = " + closestRayIsRight +
                //    "; edgeVector = " + edgeVector.ToString("F4"));
                #region Calculate if edge vector is facing to our right
                Vector3 vect1 = auxClosestRay.ray.point - coll.bounds.center; vect1.y = 0;
                Vector3 vect2 = (auxClosestRay.ray.point + edgeVector * 2) - coll.bounds.center; vect2.y = 0;
                float edgeVectorHorAngle = SignedRelativeAngle(vect1, vect2, Vector3.up);
                if (edgeVectorHorAngle < 0) edgeVector = -edgeVector;
                #endregion
                if (!disableAllRays && showWallEdgeRays)
                {
                    Debug.DrawLine(coll.bounds.center, auxClosestRay.ray.point, brown);
                    Debug.DrawLine(coll.bounds.center, auxClosestRay.ray.point + edgeVector * 2, darkBrown);

                    Debug.DrawRay(auxClosestRay.ray.point, edgeVector * 2, Color.magenta);
                }

                float cornerAngle = SignedRelativeAngle(closestHorRaycast.normal, differentWallRay.normal, found == 1 ? (edgeVector.y > 0 ? edgeVector : -edgeVector) : (edgeVector));
                //Debug.LogWarning("WALL EDGE: COMPROBACION CONCAVO/CONVEXO -> cornerAngle = "+ cornerAngle);
                #endregion
                //Debug.Log("WALL EDGE ALL: cornerAngle = " + cornerAngle + "; closestRayIsRight = " + closestRayIsRight);

                if (((closestRayIsRight && cornerAngle >= 0) || (!closestRayIsRight && cornerAngle < 0)) && Mathf.Abs(cornerAngle) != 180)// -- ES UNA ESQUINA CONVEXA -- 
                {
                    #region --- Crear el plano ---
                    if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: Estamos en una esquina convexa!" + "; closestRayIsRight = " + closestRayIsRight);
                    success = true;

                    Vector3 edgeVectorUpwards = edgeVector.y < 0 ? -edgeVector : edgeVector;
                    float edgeAngle = Vector3.Angle(Vector3.up, edgeVectorUpwards);
                    Vector3 edgeVectorProyected;
                    #region CHECK IF VALID EDGEVECTOR ANGLE
                    if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: CHECK IF VALID EDGEVECTOR ANGLE->" + "; closestHorRaycast.axis = " + closestHorRaycast.axis+ "; vel.z = "+ vel.z + "; vel.x = "+
                        vel.x + "; closestHorRaycast.row = " + closestHorRaycast.row);
                    if (closestHorRaycast.axis == Axis.X && !(Mathf.Abs(vel.z) > Mathf.Abs(vel.x) && ((closestHorRaycast.column == 0 && vel.z>0) || (closestHorRaycast.column == columns - 1 && vel.z < 0))))
                    {
                        edgeVectorProyected = new Vector3(0, edgeVectorUpwards.y, edgeVectorUpwards.z);
                    }
                    else
                    {
                        edgeVectorProyected =new Vector3(edgeVectorUpwards.x, edgeVectorUpwards.y, 0);
                    }
                    float edgeVectorProyectedAngle = Vector3.Angle(edgeVectorProyected, Vector3.up);
                    if (!disableAllDebugs) Debug.LogWarning("WALL EDGE ALL: EDGE ANGLE = " + edgeAngle+ "; edgeVectorProyectedAngle = " + edgeVectorProyectedAngle);
                    if (!disableAllRays && showWallEdgeRays)
                    {
                        Debug.DrawRay(raycastOrigins.BottomCentre, edgeVectorProyected.normalized * 1, Color.white);
                        Debug.DrawRay(raycastOrigins.BottomCentre, Vector3.up * 1, Color.gray);

                    }
                    #endregion

                    #region -- Calculate Plane Normal --
                    Vector3 planeNormal;
                    if ((edgeVector != Vector3.up && edgeVector != Vector3.down) && edgeVectorProyectedAngle > 30)
                    {
                        if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: Plane Normal Algorithm 2");
                        Vector3 firstPlaneVector, secondPlaneVector;
                        Vector3 horEdgeVector = new Vector3(edgeVector.x, 0, edgeVector.z);
                        if (edgeVector.y > 0.0001f)
                        {
                            firstPlaneVector = edgeVector;
                            secondPlaneVector = horEdgeVector;
                        }
                        else if (edgeVector.y < -0.0001f)
                        {
                            firstPlaneVector = horEdgeVector;
                            secondPlaneVector = edgeVector;
                        }
                        else
                        {
                            firstPlaneVector = edgeVector;
                            secondPlaneVector = Vector3.down;
                        }
                        planeNormal = Vector3.Cross(firstPlaneVector, secondPlaneVector);//found==1? Vector3.Cross(edgeVector, Vector3.down) : Vector3.Cross(edgeVector, Vector3.down);
                        if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: planeNormal = " + planeNormal.ToString("F4") + "; edgeVector = " + edgeVector.ToString("F6")
                            + "; firstPlaneVector = " + firstPlaneVector.ToString("F4") + "; secondPlaneVector = " + secondPlaneVector.ToString("F4"));
                    }
                    else
                    {
                        if (!disableAllDebugs) Debug.Log("Plane Normal Algorithm 1");
                        //crear un plano teniendo en cuenta que ángulo A + B + A = 180º
                        //como en este dibujo:           \  B /   
                        //                        Muro 1->\  /<-Muro 2
                        //                              A (\/) A    <- Pico de un muro
                        //                        --------------------- Plano
                        //float sideAngle = (180 - cornerAngle) / 2;
                        Vector3 wallVectorDcha;
                        Vector3 wallVectorIzda;
                        if (closestRayIsRight)
                        {
                            wallVectorDcha = new Vector3(-closestHorRaycast.normal.z, 0, closestHorRaycast.normal.x).normalized;
                            wallVectorIzda = new Vector3(differentWallRay.normal.z, 0, -differentWallRay.normal.x).normalized;
                        }
                        else
                        {
                            wallVectorIzda = new Vector3(closestHorRaycast.normal.z, 0, -closestHorRaycast.normal.x).normalized;
                            wallVectorDcha = new Vector3(-differentWallRay.normal.z, 0, differentWallRay.normal.x).normalized;
                        }
                        if (!disableAllRays && showWallEdgeRays)
                        {
                            Debug.DrawRay(differentWallRay.ray.point, wallVectorIzda * 2, closestRayIsRight ? Color.magenta : Color.black);
                            Debug.DrawRay(closestHorRaycast.ray.point, wallVectorDcha * 2, closestRayIsRight ? Color.black : Color.magenta);
                        }
                        planeNormal = -(wallVectorIzda + wallVectorDcha);
                    }

                    planeNormal.y = 0;
                    if (!wallSlide)
                    {
                        float wallAngle1 = GetWallAngle(differentWallRay.normal); // SignedRelativeAngle(Vector3.forward, differentWallRay.normal, Vector3.up);
                        float wallAngle2 = GetWallAngle(closestHorRaycast.normal); //SignedRelativeAngle(Vector3.forward, closestHorRaycast.normal, Vector3.up);
                        float wallAngleNew = GetWallAngle(planeNormal); //SignedRelativeAngle(Vector3.forward, planeNormal, Vector3.up);
                        if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: SAVING WALL EDGE NORMALS: wallAngle1 = " + wallAngle1 + "; wallAngle2 = " + wallAngle2 + "; wallAngleNew = " + wallAngleNew);
                        collisions.wallEdgeSecondWallAngle = wallAngle1;
                        collisions.wallEdgeFirstWallAngle = wallAngle2;
                        collisions.wallEdgeNewWallAngle = wallAngleNew;
                    }
                    #endregion

                    #region --- CHECK FOR PERFECT EDGE COLLISION POINT ---
                    Vector3 imaginaryWallPoint = Vector3.zero;
                    Plane perfectEdgeOtherWallPlane = new Plane(-differentWallRay.normal, differentWallRay.ray.point);
                    Vector3 perfectEdgeOrigin = closestHorRaycast.ray.point;
                    Vector3 perfectEdgeRayDir;
                    if (found == 1)
                    {
                        perfectEdgeRayDir = closestRayIsRight ? -(Vector3.Cross(edgeVector, closestHorRaycast.ray.normal)).normalized :
                           (Vector3.Cross(edgeVector, closestHorRaycast.ray.normal)).normalized;
                        if (edgeVector.y > 0) perfectEdgeRayDir = -perfectEdgeRayDir;
                    }
                    else
                    {
                        perfectEdgeRayDir = closestRayIsRight ? (Vector3.Cross(edgeVector, closestHorRaycast.ray.normal)).normalized :
                            -(Vector3.Cross(edgeVector, closestHorRaycast.ray.normal)).normalized;
                    }

                    Ray perfectEdgeRay = new Ray(perfectEdgeOrigin, perfectEdgeRayDir);
                    if (!disableAllRays && showWallEdgeRays)
                    {
                        DrawPlane(differentWallRay.ray.point, -differentWallRay.normal, Color.red, 0.05f);
                        Debug.DrawRay(perfectEdgeOrigin, perfectEdgeRayDir * 0.1f, Color.cyan);
                    }
                    float perfectEdgeEnter = 0;
                    if (perfectEdgeOtherWallPlane.Raycast(perfectEdgeRay, out perfectEdgeEnter))
                    {
                        imaginaryWallPoint = perfectEdgeRay.GetPoint(perfectEdgeEnter);
                    }
                    else
                    {
                        if (!disableAllDebugs) Debug.LogWarning("ERROR: When checking for perfect edge, couldn't hit the other wall plane; enter = " +
                        perfectEdgeEnter.ToString("F6") + "; perfectEdgeOrigin = " + perfectEdgeOrigin);
                        imaginaryWallPoint = auxClosestRay.ray.point;
                    }

                    #endregion

                    Plane imaginaryWall = new Plane(planeNormal, imaginaryWallPoint);
                    if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: ImaginayWallPoint = " + imaginaryWallPoint.ToString("F4"));

                    if (showWallEdgeRays && !disableAllRays)
                    {
                        DrawPlane(imaginaryWallPoint, planeNormal, Color.green, 0.9f);
                    }
                    #endregion

                    #region --- Raycast colisionando con ese plano ---
                    // --- RAYCAST COLISIONANDO CON ESE PLANO --- 
                    float enter = 0.0f;
                    Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                    float auxSkinWidth = auxClosestRay.skinWidth;// auxClosestRay.skinWidth;
                    float raylength = 1;//horVel.magnitude + auxSkinWidth;
                    Vector3 origin = auxClosestRay.origin;//Eloy: for Eloy: y esta resta por qué?xd (skinWidth - auxClosestRay.skinWidth
                    Ray ray = new Ray(origin, horVel.normalized);
                    if (!disableAllRays && showWallEdgeRays) Debug.DrawRay(origin, horVel.normalized * raylength, Color.magenta);
                    imaginaryWall.Raycast(ray, out enter);
                    if (enter != 0)
                    {
                        if (enter < 0 && !disableAllDebugs) Debug.LogWarning("WALL EDGE ALL: the plane is behind the beggining of the ray;");
                        float slopeAngle = GetSlopeAngle(planeNormal);
                        float wallAngle = GetWallAngle(planeNormal); // SignedRelativeAngle(Vector3.forward, planeNormal, Vector3.up);// Vector3.Angle(planeNormal, Vector3.forward);
                        if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: imaginary plane wallAngle = " + wallAngle + "; plane Normal = " + planeNormal.ToString("F4"));
                        float distance = ((ray.GetPoint(enter) - origin).magnitude * Mathf.Sign(enter)) - (auxSkinWidth * Mathf.Sign(enter));
                        auxClosestRay.normal = planeNormal;
                        auxClosestRay.ray.point = ray.GetPoint(enter);
                        closestHorRaycast = new Raycast(auxClosestRay.ray, planeNormal, distance, vel, origin, true, slopeAngle, wallAngle,
                            auxClosestRay.axis, auxClosestRay.row, auxClosestRay.column, horizontalRows, auxSkinWidth);

                        if (!disableAllDebugs) Debug.Log("WALL EDGE ALL: We hit the imaginary wall: enter = " + enter + "; ray dir = " + horVel + "; raylength = " + raylength +"; origin = " + origin +
                            "; slopeAngle = " + slopeAngle + "; oldSlopeAngle = " + auxClosestRay.slopeAngle + "; wallAngle = " + wallAngle + "; oldWallAngle = " + auxClosestRay.wallAngle + "; distance = " +
                            distance + "; oldDistance = " + auxClosestRay.distance);
                    }
                    else
                    {
                        if (!disableAllDebugs) Debug.LogWarning("Error: Wall edge is trying to collide with the plane we just calculated, but there was no collision.");
                        closestHorRaycast = PlaneBetween2Points(vel, auxClosestRay, auxOtherRay);
                    }
                    #endregion
                }
            }
        }

        return success;
    }

    bool WallEdgeMultiple(Vector3 vel, ref Raycast closestHorRaycast, int rows)
    {
        if (!disableAllDebugs) Debug.Log("WALL EDGE MULTIPLE: START");
        bool success = false;
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        Raycast closestRaycast = closestHorRaycast;
        Raycast closestRaycast2 = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
        float[] diffWallAngles = new float[3];
        diffWallAngles[0] = closestRaycast.wallAngle;


        for (Axis checkAxis = vel.x != 0 ? Axis.X : Axis.Z; checkAxis != Axis.none; checkAxis = checkAxis == Axis.X && vel.z != 0 ? Axis.Z : Axis.none)
        {
            int i = closestRaycast.row;
            for (int j = 0; j < collisions.horRaycastsX.GetLength(1); j++)
            {
                Raycast checkingRaycast = checkAxis == Axis.X ? collisions.horRaycastsX[i, j] : collisions.horRaycastsZ[i, j];
                if (checkingRaycast.hit)
                {
                    //Debug.Log("WALL EDGE MULTIPLE: checkAxis = " + checkAxis + "; checkingRaycast.normal = " + checkingRaycast.normal + "; closestRaycast.normal = " + closestRaycast.normal);
                    if (IsSlopeAngleAWall(checkingRaycast.slopeAngle) && checkingRaycast.wallAngle != closestRaycast.wallAngle)
                    {
                        if (checkingRaycast.distance < closestRaycast2.distance)
                        {
                            closestRaycast2 = checkingRaycast;
                        }
                    }
                }
            }
        }
        #region CHECK IF MORE THAN 2 TYPES OF WALL
        if (closestRaycast2.hit)
        {
            if (!disableAllDebugs) Debug.LogWarning("WALL EDGE MULTIPLE: second type of wall found");
            diffWallAngles[1] = closestRaycast2.wallAngle;
            for (Axis checkAxis = vel.x != 0 ? Axis.X : Axis.Z; checkAxis != Axis.none; checkAxis = checkAxis == Axis.X && vel.z != 0 ? Axis.Z : Axis.none)
            {
                int i = closestRaycast.row;
                for (int j = 0; j < collisions.horRaycastsX.GetLength(1); j++)
                {
                    Raycast checkingRaycast = checkAxis == Axis.X ? collisions.horRaycastsX[i, j] : collisions.horRaycastsZ[i, j];
                    if (checkingRaycast.hit)
                    {
                        //Debug.Log("WALL EDGE MULTIPLE: checkAxis = " + checkAxis + "; checkingRaycast.normal = " + checkingRaycast.normal + "; closestRaycast.normal = " + closestRaycast.normal);
                        if (IsSlopeAngleAWall(checkingRaycast.slopeAngle) && checkingRaycast.wallAngle != closestRaycast.wallAngle)
                        {
                            if (!success)
                            {
                                        if (checkingRaycast.wallAngle != diffWallAngles[1])
                                        {
                                            if (!disableAllDebugs) Debug.LogWarning("WALL EDGE MULTIPLE: third type of wall found");
                                            diffWallAngles[2] = checkingRaycast.wallAngle;
                                            success = true;
                                        }
                                        else
                                        {
                                            Vector3 vect1 = closestRaycast.ray.point - coll.bounds.center; vect1.y = 0;
                                            Vector3 vect2 = closestRaycast2.ray.point - coll.bounds.center; vect2.y = 0;
                                            Vector3 vect3 = checkingRaycast.ray.point - coll.bounds.center; vect3.y = 0;
                                            float angle1to2 = SignedRelativeAngle(vect1, vect2, Vector3.up);
                                            float angle1to3 = SignedRelativeAngle(vect1, vect3, Vector3.up);
                                            if (Mathf.Sign(angle1to2) != Mathf.Sign(angle1to3))
                                            {
                                                if (!disableAllDebugs) Debug.LogWarning("WALL EDGE MULTIPLE: only 2 types of wall but unevenly distributed (2)-> closestRaycast.wallAngle = "
                                                    + closestRaycast.wallAngle+ "; closestRaycast2.wallAngle = " + closestRaycast2.wallAngle+ "; checkingRaycast.wallAngle = " + checkingRaycast.wallAngle);
                                                success = true;
                                                if (!disableAllRays && showWallEdgeRays)
                                                {
                                                    Debug.DrawRay(coll.bounds.center, vect1, Color.white);
                                                    Debug.DrawRay(coll.bounds.center, vect2, Color.gray);
                                                    Debug.DrawRay(coll.bounds.center, vect3, orange);
                                                }
                                            }
                                        }
                            }
                        }
                        else if (checkingRaycast.wallAngle == closestRaycast.wallAngle && !success)
                        {
                            Vector3 vect1 = closestRaycast.ray.point - coll.bounds.center; vect1.y = 0;
                            Vector3 vect2 = closestRaycast2.ray.point - coll.bounds.center; vect2.y = 0;
                            Vector3 vect3 = checkingRaycast.ray.point - coll.bounds.center; vect3.y = 0;
                            float angle2to1 = SignedRelativeAngle(vect2, vect1, Vector3.up);
                            float angle2to3 = SignedRelativeAngle(vect2, vect3, Vector3.up);
                            if (Mathf.Sign(angle2to1) != Mathf.Sign(angle2to3))
                            {
                                if (!disableAllDebugs) Debug.LogWarning("WALL EDGE MULTIPLE: only 2 types of wall but unevenly distributed (1)-> closestRaycast.wallAngle = "
                                    + closestRaycast.wallAngle + "; closestRaycast2.wallAngle = " + closestRaycast2.wallAngle + "; checkingRaycast.wallAngle = " + checkingRaycast.wallAngle);
                                success = true;
                                if (!disableAllRays && showWallEdgeRays)
                                {
                                    Debug.DrawRay(coll.bounds.center, vect2, Color.white);
                                    Debug.DrawRay(coll.bounds.center, vect1, Color.gray);
                                    Debug.DrawRay(coll.bounds.center, vect3, orange);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        if (!disableAllDebugs)
        {
            closestRaycast.Print("ClosestRaycast");
            closestRaycast2.Print("ClosestRaycast2");
        }
        if (!disableAllRays && showWallEdgeRays) Debug.DrawRay(closestRaycast2.origin, horVel.normalized * (horVel.magnitude + closestRaycast2.skinWidth), darkGreen);

        if (success)
        {
            closestHorRaycast = PlaneBetween2Points(vel, closestRaycast, closestRaycast2);
        }

        return success;
    }

    Raycast PlaneBetween2Points(Vector3 vel, Raycast closestRaycast, Raycast closestRaycast2)
    {
        #region CREATE NEW PLANE
        if (!disableAllDebugs) Debug.LogWarning(" FOUND AN UNEVEN SURFACE!!");
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        Vector3 vect1 = closestRaycast.ray.point - coll.bounds.center; vect1.y = 0;
        Vector3 vect2 = closestRaycast2.ray.point - coll.bounds.center; vect2.y = 0;
        float angle1to2 = SignedRelativeAngle(vect1, vect2, Vector3.up);
        Vector3 planeNormal;
        if (Mathf.Sign(angle1to2) < 0)
        {
            planeNormal = closestRaycast2.ray.point - closestRaycast.ray.point;
        }
        else
        {
            planeNormal = closestRaycast.ray.point - closestRaycast2.ray.point;
        }
        planeNormal = new Vector3(-planeNormal.z, 0, planeNormal.x);

        Plane newPlane = new Plane(planeNormal, closestRaycast.ray.point);
        DrawPlane(closestRaycast.ray.point, planeNormal, orange, 0.8f);
        #endregion
        #region Create Ray to collide with plane
        Ray newRay = new Ray(closestRaycast.origin, horVel.normalized * (closestRaycast.skinWidth + 1));
        float enter = 0;
        if (newPlane.Raycast(newRay, out enter))
        {
            float distance = (newRay.GetPoint(enter) - closestRaycast.origin).magnitude - closestRaycast.skinWidth;
            float slopeAngle = GetSlopeAngle(planeNormal);
            float wallAngle = SignedRelativeAngle(Vector3.forward, planeNormal, Vector3.up);
            Raycast auxRay = new Raycast(closestRaycast.ray, planeNormal, distance, vel, closestRaycast.origin, true, slopeAngle, wallAngle, closestRaycast.axis,
                closestRaycast.row, closestRaycast.column, horizontalRows);
            return auxRay;
        }
        else
        {
            if (!disableAllDebugs) Debug.LogError("WALL EDGE MULTIPLE: ERROR: tried to collide with new plane created but did not hit.");
            Raycast auxRay = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
            return auxRay;
        }
        #endregion
    }

    bool IsAngle1Right(float angle1, float angle2)
    {
        bool angle1IsRight = false;
        if (angle1 >= 0 && angle2 <= 0)
        {
            angle1IsRight = true;
        }
        else if (angle1 <= 0 && angle2 >= 0)
        {
            angle1IsRight = false;
        }
        else
        {
            angle1IsRight = angle1 >= angle2 ? true : false;
        }
        return angle1IsRight;
    }//funcion auxiliar de WallEdges

    #endregion

    #region -- VERTICAL COLLISIONS --
    void VerticalCollisions(ref Vector3 vel)
    {
        #region Raycasts
        Color rayColorHit = Color.red;
        Color rayColorNoHit = darkRed;
        collisions.verRaycastsY = new Raycast[verticalRows, verticalRaysPerRow];
        // ---------------------- 3D "Ortoedro" -------------------
        float directionY = Mathf.Sign(vel.y);
        float rayLength = Mathf.Abs(vel.y) + skinWidth;
        if (collisions.lastFinishedClimbing || collisions.lastCollSt == CollisionState.crossingPeak)
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
        CollisionState lastSlopeType = CollisionState.none;

        //print("----------NEW SET OF RAYS------------");
        if (vel.y != 0)
        {
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
                    Raycast auxRay = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
                    if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore)) // throw raycast here
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        //print("Vertical Hit");
                        if (directionY == 1)
                        {
                            slopeAngle = slopeAngle == 180 ? slopeAngle = 0 : slopeAngle - 90;
                        }
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        auxRay = new Raycast(hit, hit.normal, (hit.distance - skinWidth), vel, rayOrigin, true, slopeAngle, 0, Axis.Y, i, j, 0);
                        if (auxRay.distance < collisions.closestVerRaycast.distance)
                        {
                            collisions.closestVerRaycast = auxRay;
                        }
                        CollisionState slopeType = CheckSlopeType(vel, auxRay);
                        //if(collisions.lastCollSt == CollisionState.crossingPeak)Debug.Log("Checking for peak : Vertical collisions slopeType = " + slopeType);
                        if ((slopeType == CollisionState.climbing || slopeType == CollisionState.descending) && !peak)
                        {
                            if (lastSlopeType == CollisionState.none)
                            {
                                lastSlopeType = slopeType;//solo puede ser climbing o descending
                            }
                            else if (lastSlopeType != slopeType)//solo puede ser que uno sea climbing y otro descending, lo cual interpretamos como estar en un peak
                            {
                                peak = true;
                                //Debug.LogWarning("I'm on a peak");
                            }
                        }
                        //STORE ALL THE RAYCASTS
                        collisions.verRaycastsY[i, j] = auxRay;
                    }
                    else
                    {
                        auxRay = new Raycast(hit, hit.normal, (hit.distance - skinWidth), vel, rayOrigin, false, 0, 0, Axis.Y, i, j, 0);
                        collisions.verRaycastsY[i, j] = auxRay;
                    }
                    if (showVerticalRays && !disableAllRays) Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, auxRay.hit ? rayColorHit : rayColorNoHit);
                }
            }
        }
        #endregion

        if (collisions.closestVerRaycast.hit)//si ha habido una collision vertical
        {
            CollisionState value = CheckSlopeType(vel, collisions.closestVerRaycast);
            //print("COLLISION VER: " + value + "; slopeAngle=" + collisions.closestVerRaycast.slopeAngle);
            if (!peak)
            {
                if (value == CollisionState.climbing)
                {
                    value = CollisionState.none;
                }
                else
                {
                    //if (!disableAllDebugs) Debug.Log("VERTICAL SlopeAngle = " + collisions.closestVerRaycast.slopeAngle + ";  vel = " + vel.ToString("F4") + "; collisions.slopeAngle = " + collisions.slopeAngle);
                    //print("Vertical Raycasts: value = "+value+ "; collisions.lastCollSt = " + collisions.lastCollSt + "; vel.y = " + vel.y);
                    #region -- Sliping Second Check --
                    if (value == CollisionState.sliping)
                    {
                        Vector3 dirToChar = coll.bounds.center - collisions.closestVerRaycast.origin; dirToChar.y = 0; dirToChar.Normalize();
                        Vector3 slipingSecondCheck_rayOrigin = collisions.closestVerRaycast.origin + (dirToChar.normalized * 0.001f);
                        Vector3 slipingSecondCheck_rayDir = Vector3.down;
                        float slipingSecondCheck_rayLength = 1;
                        if (!disableAllRays && showVerticalRays) Debug.DrawRay(slipingSecondCheck_rayOrigin, slipingSecondCheck_rayDir * slipingSecondCheck_rayLength, Color.cyan);

                        RaycastHit slipingSecondCheck_Hit;
                        if (Physics.Raycast(slipingSecondCheck_rayOrigin, slipingSecondCheck_rayDir, out slipingSecondCheck_Hit,
                            slipingSecondCheck_rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                        {
                            if (slipingSecondCheck_Hit.normal != collisions.closestVerRaycast.normal)
                            {
                                value = CollisionState.none;
                            }
                        }
                    }
                    #endregion
                    if (value == CollisionState.none && collisions.lastCollSt == CollisionState.crossingPeak && vel.y <= 0)
                    {
                        value = CollisionState.crossingPeak;
                    }
                }
            }
            else
            {
                value = CollisionState.crossingPeak;
            }
            //if (!disableAllDebugs) print("Vertical collisions value = " + value);
            switch (value)//con que tipo de objeto collisionamos? suelo/cuesta arriba/cuesta abajo
            {
                #region None (FLOOR/ROOF)
                case CollisionState.none:

                    //rayLength = collisions.closestVerRaycast.distance;
                    if (!disableAllRays && showVerticalRays) Debug.DrawRay(collisions.closestVerRaycast.origin, Vector3.up * directionY * (collisions.closestVerRaycast.distance + skinWidth), Color.white);
                    if (collisions.collSt == CollisionState.climbing && vel.y>0)//Subiendo chocamos con un techo
                    {
                        if (!disableAllDebugs) Debug.Log("While climbing we have collided with a roof: vel = " + vel.ToString("F6"));
                        collisions.roofAngle = collisions.closestVerRaycast.slopeAngle;
                        collisions.below = true;
                        if (collisions.oldRoofAngle==-600 || AreAnglesDifferent(collisions.oldRoofAngle, collisions.closestVerRaycast.slopeAngle))//first time
                        {
                            if (!disableAllDebugs) Debug.LogError("Climb vs Roof first time");
                            Vector3 newRoofRayOrigin = collisions.closestVerRaycast.origin + Vector3.up * skinWidth;
                            newRoofRayOrigin -= vel.normalized * skinWidth;
                            Vector3 newRoofRayDir = vel.normalized;
                            float newRoofRayLength = 0.5f;
                            if (!disableAllRays && showRoofRays) Debug.DrawRay(newRoofRayOrigin, newRoofRayDir * newRoofRayLength, purple);
                            RaycastHit newRoofHit;
                            if (Physics.Raycast(newRoofRayOrigin, newRoofRayDir, out newRoofHit, newRoofRayLength, collisionMask, QueryTriggerInteraction.Ignore))
                            {
                                vel = vel.normalized * (newRoofHit.distance - skinWidth);
                            }
                            else if (!disableAllDebugs)
                            {
                                Debug.LogError("Climbing and roof but could not hit roof with the raycast check");
                                vel = Vector3.zero;
                            }

                        }
                        else
                        {
                            Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                            if (!disableAllDebugs) Debug.Log("Climb vs Roof NOT first time");
                            Vector3 slopeParallel = collisions.closestHorRaycast.normal.normalized;
                            slopeParallel = new Vector3(-slopeParallel.z, 0, slopeParallel.x);
                            Vector3 roofWallNormal = Vector3.Cross(slopeParallel, Vector3.down);
                            Vector3 roofWallPoint = collisions.closestHorRaycast.ray.point;
                            Plane roofWall = new Plane(roofWallNormal, roofWallPoint);
                            if (!disableAllRays && showRoofRays) DrawPlane(roofWallPoint, roofWallNormal, Color.blue, 0.7f);

                            Vector3 roofWallRayOrigin = collisions.closestHorRaycast.origin;
                             Vector3 roofWallRayDir = horVel.normalized;

                            Ray roofWallRay = new Ray(roofWallRayOrigin, roofWallRayDir);
                            if (!disableAllRays && showRoofRays) Debug.DrawRay(roofWallRayOrigin, roofWallRayDir * 0.1f, Color.magenta);
                            float roofWallEnter;
                            if (roofWall.Raycast(roofWallRay, out roofWallEnter))
                            {
                                float distance = (roofWallRay.GetPoint(roofWallEnter) - roofWallRayOrigin).magnitude - collisions.closestHorRaycast.skinWidth;
                                float slopeAngle = GetSlopeAngle(roofWallNormal);
                                float wallAngle = GetWallAngle(roofWallNormal);
                                Raycast auxRay = new Raycast(collisions.closestHorRaycast.ray,roofWallNormal, distance,vel,roofWallRayOrigin,true, slopeAngle, wallAngle, collisions.closestHorRaycast.axis, collisions.closestHorRaycast.row,
                                    collisions.closestHorRaycast.column,horizontalRows, collisions.closestHorRaycast.skinWidth);
                                WallSlide(ref vel, auxRay);
                                collisions.collSt = CollisionState.climbing;
                                vel.y = (collisions.closestVerRaycast.distance) * directionY;
                            }
                            else if (!disableAllDebugs) Debug.LogError("Error: could not hit roofWall!");
                        }
                        //Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                        ////horVel = horVel.normalized * (vel.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad));
                        //float xz = vel.y * Mathf.Tan(collisions.realSlopeAngle * Mathf.Deg2Rad);
                        //horVel = horVel.normalized * xz;
                        //vel = new Vector3(horVel.x, vel.y, horVel.z);
                        //collisions.below = collisions.above = true;
                    }
                    else
                    {
                        vel.y = (collisions.closestVerRaycast.distance) * directionY;
                        collisions.below = directionY == -1;
                        collisions.above = directionY == 1;
                    }
                    break;
                #endregion
                #region Sliping
                case CollisionState.sliping:
                    SlipSlope(ref vel, collisions.closestVerRaycast);
                    //--------------------- CHECK FOR NEXT SLOPE/FLOOR -------------------------------------
                    Debug.DrawRay(collisions.closestVerRaycast.origin, Vector3.up * (Mathf.Abs(vel.y) + skinWidth) * Mathf.Sign(vel.y), Color.cyan);
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (Mathf.Abs(vel.y) + skinWidth);
                    Vector3 rayOrigin = collisions.closestVerRaycast.origin + horVelAux;
                    RaycastHit hit;
                    if (!disableAllRays) Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.yellow);

                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        if (!disableAllRays)
                        {
                            Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.magenta);
                        }

                        if (!disableAllDebugs) print("Slope Angle: " + collisions.slopeAngle + "; new slope angle: " + slopeAngle);
                        if (slopeAngle != collisions.slopeAngle)
                        {
                            if (!disableAllDebugs) Debug.LogWarning("SlipSlope: Clipping through floor avoided by stoping vel.y");
                            vel.y = -(hit.distance - skinWidth);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    break;
                #endregion
                #region Descending
                case CollisionState.descending:
                    if (collisions.collSt != CollisionState.climbing)
                    {
                        float distanceToSlopeStart = 0;
                        distanceToSlopeStart = collisions.closestVerRaycast.distance;
                        vel.y -= distanceToSlopeStart * -1;
                        DescendSlope(ref vel, collisions.closestVerRaycast);
                        vel.y += distanceToSlopeStart * -1;
                    }
                    else
                    {
                        Debug.LogError("Esto no es un error, solo quería saber si esta condición ocurría en algún momento.Estamos descending y climbing a la vez!?");
                        if (vel.y < 0) vel.y = 0;
                    }
                    //--------------------- CHECK FOR NEXT SLOPE/FLOOR -------------------------------------
                    horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (Mathf.Abs(vel.y) + skinWidth);
                    rayOrigin = collisions.closestVerRaycast.origin + horVelAux;
                    if (!disableAllRays) Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.yellow);

                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        if (!disableAllRays) Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.magenta);

                        if (slopeAngle != collisions.slopeAngle)
                        {
                            vel.y = -(hit.distance - skinWidth);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    break;
                #endregion
                #region Arista Vertical(PEAK)
                case CollisionState.crossingPeak:
                    if (collisions.collSt == CollisionState.climbing)
                    {
                    }
                    else
                    {
                        vel.y = 0;
                    }
                    collisions.collSt = CollisionState.crossingPeak;
                    collisions.below = true;
                    break;
                    #endregion
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
    #endregion

    #endregion

    #region --- AUXILIAR ---

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

        raycastOrigins.BottomCentre = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

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

        wallSlideRows = Mathf.Clamp(wallSlideRows, 2, int.MaxValue);
        wallSlideRaysPerRow = Mathf.Clamp(wallSlideRaysPerRow, 2, int.MaxValue);

        wallEdgeHighPrecisionHorRaySpacing = horizontalRaySpacing / (wallEdgeHighPrecissionRays - 1);
        wallEdgeHighPrecisionVerRaySpacing = horizontalRowSpacing / (wallEdgeHighPrecissionRays - 1);

        wallSlideRowSpacing = bounds.size.y / (wallSlideRows - 1);
        wallSlideRaySpacing = bounds.size.x / (wallSlideRaysPerRow - 1);

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
        public Vector3 BottomEnd;//TopEnd= center x, min y, max z
        public Vector3 BottomCentre;

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

        //HORIZONTAL
        public CollisionState collSt;
        public CollisionState lastCollSt;
        public float slopeAngle, slopeAngleOld, realSlopeAngle, wallAngle, wallAngleOld, wallAngle2, wallAngleOld2, floorAngle;
        public Vector3 startVel;
        public Raycast closestHorRaycast;
        public Raycast[,] horRaycastsX;
        public Raycast[,] horRaycastsZ;
        //public Vector3 horCollisionsPoint;
        public Vector3 wallNormal;
        public float wallEdgeSecondWallAngle;
        public float wallEdgeFirstWallAngle;
        public float wallEdgeNewWallAngle;
        public Vector3 oldWallNormal;
        public GameObject horWall;
        public GameObject verWall;
        public SlideState slideSt;
        public SlideState oldSlideSt;
        public SlideState oldSlideSt2;
        public FirstCollisionWithWallType fcww;
        public FirstCollisionWithWallType oldFcww;
        public bool finishedClimbing, lastFinishedClimbing;

        //VERTICAL
        public Raycast closestVerRaycast;
        public Raycast[,] verRaycastsY;
        public float distanceToFloor;
        public bool safeBelowStarted;
        float safeBelowTime, safeBelowMaxTime;
        public float roofAngle, oldRoofAngle;

        public void ResetVertical()
        {
            lastBelow = below;
            above = below = false;
            closestVerRaycast = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
            distanceToFloor = float.MaxValue;
            verRaycastsY = new Raycast[0, 0];
            verWall = null;
            oldRoofAngle = roofAngle;
            roofAngle = -600;
        }

        public void ResetHorizontal()
        {
            left = right = false;
            foward = behind = false;
            wallAngleOld = wallAngle;
            wallAngle = -500;
            wallAngleOld2 = wallAngle2;
            wallAngle2 = 0;
            //horCollisionsPoint = Vector3.zero;
            oldWallNormal = wallNormal;
            wallNormal = Vector3.zero;
            wallEdgeFirstWallAngle = -501;
            wallEdgeSecondWallAngle = -502;
            wallEdgeNewWallAngle = -503;
            horWall = null;
            oldSlideSt2 = oldSlideSt;
            oldSlideSt = slideSt;
            slideSt = SlideState.none;
            startVel = Vector3.zero;
            closestHorRaycast = new Raycast(new RaycastHit(), Vector3.zero, float.MaxValue, Vector3.zero, Vector3.zero);
            horRaycastsX = new Raycast[0, 0];
            horRaycastsZ = new Raycast[0, 0];
            lastFinishedClimbing = finishedClimbing;
            finishedClimbing = false;

            oldFcww = fcww;
            fcww = FirstCollisionWithWallType.none;
        }

        public void ResetAround()
        {
            around = false;
        }

        public void ResetClimbingSlope()
        {
            lastCollSt = collSt;
            collSt = CollisionState.none;
            floorAngle = -1;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
            realSlopeAngle = 0;
        }

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
    }
}

public struct Raycast
{
    public RaycastHit ray;
    public Vector3 normal;
    public float distance;
    public Vector3 vel;
    public Vector3 origin;
    public bool hit;
    public float slopeAngle;
    public float wallAngle;
    public Axis axis;
    public int row;//row in which the ray was thrown from
    public int column;
    public float rayHeightPercentage;//from 0 (feet) to 100(head)
    public float skinWidth;

    public Raycast(RaycastHit _ray, Vector3 _normal, float _dist, Vector3 _vel, Vector3 _origin, bool _hit = false, float _slopeAngle = 0, float _wallAngle = 0,
        Axis _axis = Axis.none, int _row = 0, int _column = 0, int horizontalRows = 0, float _skinWidth = 0.1f)
    {
        ray = _ray;
        normal = _normal;
        distance = _dist;
        vel = _vel;
        origin = _origin;
        hit = _hit;
        axis = _axis;
        slopeAngle = _slopeAngle;
        wallAngle = _wallAngle;
        row = _row;
        column = _column;
        rayHeightPercentage = horizontalRows == 0 ? 0 : Mathf.Clamp((row / horizontalRows), 0, 100);
        skinWidth = Mathf.Clamp(_skinWidth, 0, 0.1f);
    }

    //public Raycast()
    //{
    //    ray = new RaycastHit();
    //    distance = float.MaxValue;
    //    vel = Vector3.zero;
    //    origin = Vector3.zero;
    //    hit = false;
    //    axis = Axis.none;
    //    slopeAngle = 0;
    //    wallAngle = 0;
    //    row = 0;
    //    column = 0;
    //    rayHeightPercentage = 0 ;
    //    skinWidth = Mathf.Clamp(0.1f, 0, 0.1f);
    //}

    public bool IsSame(Raycast _otherRay)
    {
        if (axis == _otherRay.axis && row == _otherRay.row && column == _otherRay.column && distance == _otherRay.distance)
            return true;
        else return false;
    }
    public void Print(string name)
    {
        Debug.Log(name + " raycast-> axis = " + axis + "; row = " + row + "; column = " + column + "; origin = " + origin + "; distance = " + distance + "; normal = " + normal);
    }
}

public enum Axis
{
    none,
    X,
    Y,
    Z
}
