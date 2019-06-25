using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller3D : MonoBehaviour
{
    public bool disableAllRays;
    public LayerMask collisionMask;
    public LayerMask collisionMaskAround;
    public float FloorMaxDistanceCheck = 5;
    Color purple = new Color(0.749f, 0.380f, 1f);

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

    private void Awake()
    {
        if (minClimbAngle != minDescendAngle) Debug.LogWarning("Warning: minClimbAngle and minDescendAngle values are different, " +
             "are you sure you want this? It will generate extrange behaviours.");
        if (maxClimbAngle != maxDescendAngle) Debug.LogWarning("Warning: maxClimbAngle and maxDescendAngle values are different, " +
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
        //print("Start Vel = " + vel.ToString("F4"));
        if (!disableAllRays)
        {
            Debug.DrawRay(raycastOrigins.Center, vel * 5, Color.blue);
            Vector3 horVel = new Vector3(vel.x, 0, vel.z);
            Debug.DrawRay(raycastOrigins.Center, horVel.normalized * 2, Color.yellow);
        }
        if (vel.x != 0 || vel.z != 0)
        {
            HorizontalCollisions(ref vel, Color.red, false);
        }
        //Debug.Log("Middle Vel = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
        if (vel.y != 0 || vel.x != 0 || vel.z != 0)
        {
            NewVerticalCollisions2(ref vel);
        }
        //Debug.Log("End Vel = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
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

    float GetSlopeAngle(RaycastHit hit)
    {
        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        return slopeAngle;
    }

    void ClimbSlope(ref Vector3 vel, Raycast rayCast)
    {
        //Debug.Log("Start ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
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
            collisions.collSt = CollisionState.climbing;
            collisions.slopeAngle = rayCast.slopeAngle;
            collisions.realSlopeAngle = Mathf.Asin(climbVel.y / climbVel.magnitude) * Mathf.Rad2Deg;
            //print("REAL SLOPE ANGLE = " + collisions.realSlopeAngle);
            //print("CLIMBING: Angle = " + rayCast.slopeAngle + "; old Vel= " + horVel.ToString("F5") + "; vel= " + vel.ToString("F5") +
            //    "; old magnitude=" + horVel.magnitude + "; new magnitude = " + vel.magnitude);
            if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 0.5f);

        }
        //Debug.Log("Finish ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
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
            collisions.collSt = CollisionState.descending;
            collisions.slopeAngle = rayCast.slopeAngle;
            if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 0.5f);

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
        if (!disableAllRays) Debug.DrawRay(rayCast.origin, slipDir * 2, Color.green, 1);
        Vector3 slipVel = (slipDir * vel.y) + horVel;
        //slipVel.y = vel.y;
        //float angWithWall = Vector3.Angle(wallHorNormal, horVel);

        vel = slipVel;
        collisions.below = false;
        collisions.collSt = CollisionState.sliping;
        collisions.slopeAngle = rayCast.slopeAngle;
        collisions.verWall = collisions.closestVerRaycast.ray.transform.gameObject;
        if (!disableAllRays) Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 1);
    }

    CollisionState CheckSlopeType(Vector3 vel, Raycast ray)
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
            if (ray.row == 0)
            {
                if (climbVel.y > 0 && ray.slopeAngle <= maxClimbAngle && ray.slopeAngle > minClimbAngle)
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
            if (climbVel.y > 0 && ray.slopeAngle <= maxClimbAngle && ray.slopeAngle > minClimbAngle)
            {
                return CollisionState.climbing;
            }
            else if (climbVel.y < 0 && ray.slopeAngle <= maxDescendAngle && ray.slopeAngle > minDescendAngle)
            {
                return CollisionState.descending;
            }
            else if (ray.axis == Axis.Y && ((vel.y <= 0 && ray.slopeAngle > maxDescendAngle) || (vel.y > 0 && ray.slopeAngle != 0)))
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
                if (climbVel.y > 0 && ray.slopeAngle <= maxClimbAngle && ray.slopeAngle > minClimbAngle)
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

    #region -- HORIZONTAL COLLISIONS --
    void WallSlide(ref Vector3 vel, Raycast rayCast)
    {
        Debug.LogWarning("WALL SLIDE");
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        float wallAngle = rayCast.wallAngle;
        Vector3 normal = -new Vector3(rayCast.ray.normal.x, 0, rayCast.ray.normal.z).normalized;
        //if (showHorizontalRays && !disableAllRays)
        //{
        //    DrawPlane(rayCast.ray.point, normal, purple);
        //}
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

    void HorizontalCollisions(ref Vector3 vel, Color rayColor, bool wallSlide)
    {
        Raycast closestRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
        #region Raycasts
        int rows, raysPerRow;
        float rowSpacing, raySpacing;
        rows = wallSlide ? wallSlideRows : horizontalRows;
        raysPerRow = wallSlide ? wallSlideRaysPerRow : horizontalRaysPerRow;
        rowSpacing = wallSlide ? wallSlideRowSpacing : horizontalRowSpacing;
        raySpacing = wallSlide ? wallSlideRaySpacing : horizontalRaySpacing;

        collisions.horRaycastsX = new Raycast[horizontalRows, horizontalRaysPerRow];
        collisions.horRaycastsZ = new Raycast[horizontalRows, horizontalRaysPerRow];
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);//DO NOT CHANGE ORDER
        float rayLength = horVel.magnitude + skinWidth;//DO NOT CHANGE ORDER
        horVel = horVel.normalized;//DO NOT CHANGE ORDER
        float directionX = 0, directionZ = 0;

        //VARIABLES FOR "Cutting Out Raycast's Skinwidth in Borders"
        float corsbAngle = 0;

        //VARAIBLES FOR WALLSLIDE
        Vector3 lastWallNormal = new Vector3(collisions.wallNormal.x, 0, collisions.wallNormal.z).normalized;

        #region Raycasts X
        if (vel.x != 0)
        {
            directionX = Mathf.Sign(vel.x);
            Vector3 rowsOriginX = directionX == 1 ? raycastOrigins.BottomRFCornerReal : raycastOrigins.BottomLFCornerReal;
            corsbAngle = Vector3.Angle(Vector3.forward, horVel);

            //print("CONTROLLER 3D: " + directionX + "*X corsbAngle = " + corsbAngle + "; corsbBorderHorRaysPerRow = " + corsbBorderHorRaysPerRow +
            //    "; (raysPerRow - corsbBorderHorRaysPerRow) = " + (raysPerRow - corsbBorderHorRaysPerRow));
            if (wallSlide) rowsOriginX += lastWallNormal * precisionSpaceFromSlideWall;//LEAVE SAFE SPACE FROM WALL 
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
                        Debug.DrawLine(lastOriginX, rayOriginX, wallSlide ? Color.cyan : Color.blue);
                    }
                    lastOriginX = rayOriginX;
                    rayOriginX += (-horVel * corsbSkinWidth);
                    RaycastHit hit;
                    if (showHorizontalRays && !disableAllRays)
                    {
                        Debug.DrawRay(rayOriginX, horVel * corsbRayLength, (corsbSkinWidth < skinWidth ? corsbColor : rayColor));
                    }

                    if (Physics.Raycast(rayOriginX, horVel, out hit, corsbRayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up);// Vector3.Angle(hit.normal, Vector3.forward);
                        Raycast auxRay = new Raycast(hit, (hit.distance - corsbSkinWidth), vel, rayOriginX, true, slopeAngle, wallAngle, Axis.X,
                            i, j, rows, corsbSkinWidth);
                        //WE STORE ALL THE RAYCASTS INFO
                        collisions.horRaycastsX[i, j] = auxRay;

                        bool newClosestRay = IsCloserHorizontalRay(closestRaycast, auxRay);
                        if (newClosestRay)
                        {
                            closestRaycast = auxRay;
                        }
                        //Closest climbing ray
                        SaveClosestHorizontalClimbingRay(auxRay);
                    }
                    else
                    {
                        //WE STORE ALL THE RAYCASTS INFO
                        Raycast auxRay = new Raycast(hit, (hit.distance - corsbSkinWidth), vel, rayOriginX, false, 0, 0, Axis.X, i, j, rows, corsbSkinWidth);
                        collisions.horRaycastsX[i, j] = auxRay;
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
            //    "; (raysPerRow - corsbBorderHorRaysPerRow) = " + (raysPerRow - corsbBorderHorRaysPerRow)+ "; horizontalRaySpacing = " + horizontalRaySpacing);
            if (wallSlide) rowsOriginZ += lastWallNormal * precisionSpaceFromSlideWall;//LEAVE SAFE SPACE FROM WALL 
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
                        Debug.DrawLine(lastOriginZ, rayOriginZ, wallSlide ? Color.cyan : Color.blue);
                    }
                    lastOriginZ = rayOriginZ;
                    rayOriginZ += (-horVel * corsbSkinWidth);
                    RaycastHit hit;
                    if (showHorizontalRays && !disableAllRays)
                    {
                        Debug.DrawRay(rayOriginZ, horVel * corsbRayLength, (corsbSkinWidth < skinWidth ? corsbColor : rayColor));
                    }

                    if (Physics.Raycast(rayOriginZ, horVel, out hit, corsbRayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        float wallAngle = SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up);// Vector3.Angle(hit.normal, Vector3.forward);
                        Raycast auxRay = new Raycast(hit, (hit.distance - corsbSkinWidth), vel, rayOriginZ, true, slopeAngle, wallAngle, Axis.Z,
                            i, j, rows, corsbSkinWidth);
                        //WE STORE ALL THE RAYCASTS INFO
                        collisions.horRaycastsZ[i, j] = auxRay;//when wall Slide this is overwritten by the second set of rays (wallSlideCollisions)

                        bool newClosestRay = !(i == 0 && j == 0) ? IsCloserHorizontalRay(closestRaycast, auxRay) : true;
                        if (newClosestRay)
                        {
                            closestRaycast = auxRay;
                        }
                        //Closest climbing ray
                        SaveClosestHorizontalClimbingRay(auxRay);
                    }
                    else
                    {
                        //WE STORE ALL THE RAYCASTS INFO
                        Raycast auxRay = new Raycast(hit, (hit.distance - corsbSkinWidth), vel, rayOriginZ, false, 0, 0, Axis.Z, i, j, rows, corsbSkinWidth);
                        collisions.horRaycastsZ[i, j] = auxRay;
                    }
                }
            }
        }
        #endregion
        #endregion

        if (closestRaycast.hit)//si ha habido una collision horizontal
        {
            if (wallSlide)
            {
                Debug.LogWarning("Wall Slide collisions had a hit");
            }
            CollisionState value = CheckSlopeType(vel, closestRaycast);
            //CollisionState value = CheckSlopeType(ref vel, closestRaycast);

            if (value == CollisionState.wall)
            {
                #region --- SLOPE'S BORDER WALL PROBLEMATIC ---
                if (collisions.closestHorRaycastClimb.hit)
                {
                    bool success = false;
                    //Debug.LogWarning("Climbing slope and colliding with wall at the same time!: transform of wall = " + closestRaycast.ray.transform
                    //    + "; transform of slope = " + collisions.closestHorRaycastClimb.ray.transform);
                    if (closestRaycast.ray.transform == collisions.closestHorRaycastClimb.ray.transform)
                    {
                        success = true;
                    }
                    if (success)
                    {
                        closestRaycast = collisions.closestHorRaycastClimb;
                        value = CollisionState.climbing;
                    }
                }
                #endregion

                #region --- CHECK FOR CLIMBSTEP ---
                //check for climbStep
                float rayHeight = closestRaycast.row * rowSpacing;
                if (rayHeight <= maxHeightToClimbStep)
                {
                    //CHECK IF THE WALL IS SHORT
                    bool success = true;
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (horVelAux.magnitude + closestRaycast.skinWidth);
                    Vector3 rayOrigin = closestRaycast.origin + Vector3.up * vel.y;
                    RaycastHit hit;
                    if (!disableAllRays) Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.yellow, 4);

                    if (Physics.Raycast(rayOrigin, horVelAux, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {

                        //for(int i = closestRaycast.row+1; i< collisions.horRaycastsX.GetLength(0); i++)
                        //{
                        //    //check if there was a hit 
                        //}
                        if (success)
                        {
                            //value = CollisionState.climbStep;
                        }
                    }
                }
                #endregion
            }

            //print("COLLISION HOR: " + value + "; slopeAngle=" + closestRaycast.slopeAngle);
            switch (value)//con que tipo de objeto colisionamos? pared/cuesta arriba/cuesta abajo
            {
                #region Wall
                case CollisionState.wall:
                    //Debug.LogWarning("WALL: START");
                    float auxRayLength = new Vector3(closestRaycast.vel.x, 0, closestRaycast.vel.z).magnitude;
                    if (!disableAllRays && showHorizontalRays) Debug.DrawRay(closestRaycast.origin, horVel * (auxRayLength), Color.white);
                    #region -- Wall Edges --
                    bool wallEdgeFound = false;
                    wallEdgeFound = WallEdgeAll(vel, ref closestRaycast);
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
                    #endregion
                    if (validWall)
                    {
                        Debug.Log("Valid Wall: oldWallNormal = " + collisions.oldWallNormal + "; closestRaycast.ray.normal = " + closestRaycast.ray.normal);
                        float angleBetweenWalls = SignedRelativeAngle(collisions.oldWallNormal, closestRaycast.ray.normal, Vector3.up);
                        int clockWise = 0;
                        if (angleBetweenWalls > 0)
                        {
                            clockWise = 1;
                        }
                        else if (angleBetweenWalls < 0)
                        {
                            clockWise = -1;
                        }
                        SlideState auxSlideSt = wallSlide ? collisions.slideSt : collisions.oldSlideSt;
                        Debug.LogWarning("WALL: old wall angle = " + collisions.wallAngleOld + "; new wall angle = " + closestRaycast.wallAngle +
                        "; SlideSt = " + auxSlideSt + "; clockWise = " + clockWise);
                        if (collisions.wallAngleOld == -500 || (AreAnglesDifferent(collisions.wallAngleOld, closestRaycast.wallAngle) &&
                        ((auxSlideSt == SlideState.right && clockWise == 1) || (auxSlideSt == SlideState.left && clockWise == -1))))//NEW WALL
                        {
                            if ((collisions.slideSt != SlideState.none && wallSlide) || collisions.slideSt == SlideState.none && !wallSlide)
                            {
                                Debug.LogError("WALL: APPROACHING NEW WALL: " + "distance = " + closestRaycast.distance);
                                if (collisions.closestHorRaycastClimb.hit)//CLIMBING AND FOUND A WALL
                                {
                                    ClimbSlope(ref vel, collisions.closestHorRaycastClimb);
                                    horVel = new Vector3(vel.x, 0, vel.z).normalized;
                                    horVel = horVel * (closestRaycast.distance);
                                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                                    float y = Mathf.Tan(collisions.realSlopeAngle * Mathf.Deg2Rad) * horVel.magnitude;
                                    vel.y = y;
                                    Debug.LogError("Found a wall while climbing. vel = " + vel.ToString("F6") + "; slopeAngle = " + collisions.slopeAngle + "; realSlopeAngle = " + collisions.realSlopeAngle);
                                }
                                else
                                {
                                    horVel = horVel * (closestRaycast.distance);
                                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                                }
                                collisions.wallAngle = closestRaycast.wallAngle;
                                collisions.wallNormal = closestRaycast.ray.normal;
                            }
                        }
                        else//COLLIDING WITH WALL and not the first frame (stop frame, the frame we use to stop in time before going through the wall)
                        {
                            if (closestRaycast.distance < 0)//SI ESTAMOS METIDOS DENTRO DEL MURO POR ALGUN MOTIVO
                            {
                                Vector3 moveOutVel = horVel.normalized * closestRaycast.distance;
                                transform.Translate(moveOutVel, Space.World);
                            }
                            //Debug.LogWarning("WALL SLIDE: START");
                            if (!wallSlide)
                            {
                                //Look for horizontal WallEdge
                                Raycast otherWallRay = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
                                //WallEdgeHor(vel, closestRaycast, otherWallRay);
                                WallSlide(ref vel, closestRaycast);
                                //GUARDAMOS PARÁMETROS DE COLLISIONS
                                collisions.horWall = closestRaycast.ray.transform.gameObject;
                                collisions.wallNormal = closestRaycast.ray.normal;
                                //collisions.horCollisionsPoint = closestRaycast.ray.point;
                            }
                            else
                            {
                                //SecondWallSlide?
                                //FRENAR PORQUE ESTAMOS CHOCANDO CON UN MURO y no hacemos slide
                                horVel = horVel * (closestRaycast.distance);
                                vel = new Vector3(horVel.x, vel.y, horVel.z);
                            }
                            //GUARDAMOS PARÁMETROS DE COLLISIONS
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
                                HorizontalCollisions(ref vel, Color.yellow, true);
                            }
                            //WallSlideCollisions(ref vel);
                        }
                    }
                    break;
                #endregion
                #region Climbing
                case CollisionState.climbing:
                    //Debug.Log("Start climbing = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
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
                    if (!disableAllRays) Debug.DrawRay(closestRaycast.origin, horVel * rayLength, Color.cyan, 4);

                    float distanceToSlopeStart = 0;
                    if (collisions.slopeAngleOld != closestRaycast.slopeAngle)
                    {
                        distanceToSlopeStart = closestRaycast.distance;
                        horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                    }
                    Debug.Log("Start ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
                    ClimbSlope(ref vel, closestRaycast);
                    Debug.Log("Finish ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
                    horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                    Debug.Log("After ClimbSlope = " + vel.ToString("F4") + "; CollisionState = " + collisions.collSt + "; below = " + collisions.below);
                    //--------------------- CHECK FOR NEXT SLOPE -------------------------------------
                    //TO DO: NEW SLOPE CHECK MUST BE DONE BY THROWING every ray possible in line i=0, not by throwing 1 ray only. 
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = (horVelAux.magnitude + closestRaycast.skinWidth);
                    Vector3 rayOrigin = closestRaycast.origin + Vector3.up * vel.y;
                    RaycastHit hit;
                    if (!disableAllRays) Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.yellow, 4);

                    if (Physics.Raycast(rayOrigin, horVelAux, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        if (!disableAllRays) Debug.DrawRay(rayOrigin, horVelAux * rayLength, Color.magenta, 4);

                        if (slopeAngle != collisions.slopeAngle)//NEW SLOPE FOUND
                        {
                            horVelAux = horVelAux.normalized * (hit.distance - closestRaycast.skinWidth);
                            float y = vel.y;
                            vel = new Vector3(horVelAux.x, y, horVelAux.z);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    else
                    {
                        //Debug.LogWarning("Climbing slope finished!");
                        collisions.finishedClimbing = true;
                    }
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

    bool IsCloserHorizontalRay(Raycast lastClosestHorRay, Raycast newRay)
    {
        bool success = false;
        if (!(newRay.slopeAngle > minClimbAngle && newRay.slopeAngle <= maxClimbAngle))//choco con un muro
        {
            if (!(lastClosestHorRay.slopeAngle > minClimbAngle && lastClosestHorRay.slopeAngle <= maxClimbAngle))//hay ya un rayo con colision de muro como closest ray
            {
                if (newRay.distance < lastClosestHorRay.distance)
                {
                    //Debug.LogError(" I COLLIDED WITH A WALL FOR THE FIRST TIME; there was already another raycast saved as wall collision");
                    success = true;
                }
            }
            else
            {
                //Debug.LogError(" I COLLIDED WITH A WALL FOR THE FIRST TIME; maybe I was climbing?");
                success = true;
            }
        }
        else//SLOPE
        {
            if (newRay.distance < lastClosestHorRay.distance)
            {
                success = true;
            }
        }
        return success;
    }

    void SaveClosestHorizontalClimbingRay(Raycast newRay)
    {
        if (newRay.row == 0 && newRay.slopeAngle > minClimbAngle && newRay.slopeAngle <= maxClimbAngle)
        {
            if (newRay.distance < collisions.closestHorRaycastClimb.distance)
            {
                collisions.closestHorRaycastClimb = newRay;
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
    /// <returns></returns>
    bool WallEdges(Vector3 vel, ref Raycast closestHorRaycast)
    {
        //ONLY ENTER IF closesthorRatCast = wall;
        Raycast auxClosestRay = closestHorRaycast;
        bool success = false;
        Raycast differentWallRay = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
        Debug.LogWarning("WALL EDGE: START");

        #region --- Primera comprobacion (raycasts actuales) ---
        //for (int i = 0; i < collisions.horRaycastsX.GetLength(0); i++)
        //{
        //    for (int j = 0; j < collisions.horRaycastsX.GetLength(1); j++)
        //    {
        //        if (i == 0 && j == 0) Debug.LogWarning("WALL EDGE: PRIMERA COMPROBACION START");
        //        if (collisions.horRaycastsX[i, j].hit)
        //        {
        //            CollisionState slopeType = CheckSlopeTypeWallEdges(vel, collisions.horRaycastsX[i, j]);
        //            if (slopeType == CollisionState.wall && AreAnglesDifferent(collisions.horRaycastsX[i, j].wallAngle, closestHorRaycast.wallAngle) &&
        //                collisions.horRaycastsX[i, j].distance < differentWallRay.distance)
        //            {
        //                differentWallRay.distance = collisions.horRaycastsX[i, j].distance;
        //                differentWallRay = collisions.horRaycastsX[i, j];
        //            }
        //        }
        //        if (collisions.horRaycastsZ[i, j].hit)
        //        {
        //            CollisionState slopeType = CheckSlopeTypeWallEdges(vel, collisions.horRaycastsZ[i, j]);
        //            if (slopeType == CollisionState.wall && AreAnglesDifferent(collisions.horRaycastsZ[i, j].wallAngle, closestHorRaycast.wallAngle) &&
        //                collisions.horRaycastsZ[i, j].distance < differentWallRay.distance)
        //            {
        //                differentWallRay.distance = collisions.horRaycastsZ[i, j].distance;
        //                differentWallRay = collisions.horRaycastsZ[i, j];
        //            }
        //        }
        //    }
        //}
        #endregion

        #region --- Segunda comprobacion / HIGH PRECISION (lanzando nuevos raycasts) ---
        if (!differentWallRay.hit)
        {
            Debug.LogWarning("WALL EDGE: SEGUNDA COMPROBACION START");
            bool seikai = false;//seikai = correcto en japonés. es que quería usar "success" pero ya estaba usado
            int column = closestHorRaycast.column;
            int row = closestHorRaycast.row;
            Axis originalAxis = closestHorRaycast.axis;
            for (int sentido = -1; sentido < 2 && !seikai; sentido = sentido == -1 ? sentido = 1 : sentido = 2)//1=+;-1=- (en el orden de creación de rayos: BACK Y RIGHT ) 
            {
                float auxSkinWidth, raylength;
                Vector3 horVel, rayOrigin;
                Axis finalAxis = originalAxis;
                int finalSentido = sentido;
                bool throwRays = false;
                if (!((column == 0 && sentido == -1) || (column == (horizontalRaysPerRow - 1) && sentido == 1)))
                {
                    throwRays = true;
                    auxSkinWidth = originalAxis == Axis.X ?
                        collisions.horRaycastsX[row, column + sentido].skinWidth : collisions.horRaycastsZ[row, column + sentido].skinWidth;
                    horVel = new Vector3(vel.x, 0, vel.z);// DO NOT CHANGE ORDER
                    raylength = horVel.magnitude + auxSkinWidth;// DO NOT CHANGE ORDER
                    horVel.Normalize();// DO NOT CHANGE ORDER
                    rayOrigin = closestHorRaycast.origin + (horVel * closestHorRaycast.skinWidth);
                }
                #region Cambio de eje
                else//CAMBIO DE EJE
                {
                    auxSkinWidth = closestHorRaycast.skinWidth;
                    horVel = new Vector3(vel.x, 0, vel.z);// DO NOT CHANGE ORDER
                    raylength = horVel.magnitude + auxSkinWidth;// DO NOT CHANGE ORDER
                    horVel.Normalize();// DO NOT CHANGE ORDER
                    rayOrigin = closestHorRaycast.origin + (horVel * closestHorRaycast.skinWidth);
                    if (originalAxis == Axis.X)
                    {
                        finalAxis = Axis.Z;
                        if (vel.x > 0)
                        {
                            if (column == 0 && vel.z > 0)//AL PRINCIPIO, en este caso es ARRIBA
                            {
                                throwRays = true;
                                column = horizontalRaysPerRow - 1;
                            }
                            else if (column == horizontalRaysPerRow - 1 && vel.z < 0)//AL FINAL, en este caso es ABAJO
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
                            else if (column == horizontalRaysPerRow - 1 && vel.z < 0)//AL FINAL, en este caso es ABAJO
                            {
                                throwRays = true;
                                column = 0;
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
                            else if (column == horizontalRaysPerRow - 1 && vel.x > 0)//AL FINAL, en este caso es a la DCHA
                            {
                                throwRays = true;
                                column = 0;
                            }
                        }
                        else if (vel.z < 0)
                        {
                            if (column == 0 && vel.x < 0) //AL PRINCIPIO, en este caso es a la IZDA
                            {
                                throwRays = true;
                                column = horizontalRaysPerRow - 1;
                            }
                            else if (column == horizontalRaysPerRow - 1 && vel.x > 0)//AL FINAL, en este caso es a la DCHA
                            {
                                throwRays = true;
                                finalSentido = -1;
                            }
                        }

                    }
                }
                #endregion
                for (int j = 1; j < wallEdgeHighPrecissionRays - 1 && !seikai && throwRays; j++)
                {
                    Vector3 localRayOrigin = rayOrigin;
                    localRayOrigin += finalAxis == Axis.X ?
                    Vector3.back * finalSentido * wallEdgeHighPrecisionHorRaySpacing * j : Vector3.right * finalSentido * wallEdgeHighPrecisionHorRaySpacing * j;
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
                        float wallAngle = SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up); //Vector3.Angle(hit.normal, Vector3.forward);
                        Raycast auxRay = new Raycast(hit, (hit.distance - auxSkinWidth), vel, localRayOrigin, true, slopeAngle, wallAngle,
                            originalAxis, row, column, horizontalRows, auxSkinWidth);
                        CollisionState slopeType = CheckSlopeTypeWallEdges(vel, auxRay);
                        if (slopeType == CollisionState.wall && AreAnglesDifferent(auxRay.wallAngle, closestHorRaycast.wallAngle))
                        {
                            //Debug.LogWarning("WALL EDGE: SEGUNDA COMPROBACION -> found!");
                            differentWallRay = auxRay;
                            if (differentWallRay.distance < auxClosestRay.distance) auxClosestRay = differentWallRay;
                            seikai = true;
                        }
                    }
                }
                //sentido = sentido == -1 ? sentido = 1 : sentido = 2;//at the end
            }
        }
        //else// rellenar de forma que se tiren rayos de precisión entre  los dos rayos encontrados en la comprobación 1
        //{

        //}
        #endregion

        if (differentWallRay.hit)//DOS TIPOS DE MURO ENCONTRADOS
        {
            #region --- Comprobar si convexo o cóncavo ---
            Debug.LogWarning("WALL EDGE: COMPROBACION CONCAVO/CONVEXO START");
            Debug.Log("WALL EDGE: Rayo 1 -> (" + closestHorRaycast.row + "," + closestHorRaycast.column + "), wallAngle = " + closestHorRaycast.wallAngle +
                "; Rayo 2 -> (" + differentWallRay.row + "," + differentWallRay.column + "), wallAngle = " + differentWallRay.wallAngle);
            // --- Comprobar que forman un ángulo convexo ---
            // -- Para ello primero comprobamos cual de los dos puntos es el del muro de nuestra izda y cual el de la dcha -- 
            // the vector that we want to measure an angle from
            Vector3 referenceForward = closestHorRaycast.ray.point - closestHorRaycast.origin;/* some vector that is not Vector3.up */
            referenceForward.y = 0; referenceForward.Normalize();
            // the vector of interest 
            //Vector3 vector1 = closestHorRaycast.ray.point - coll.bounds.center;// some vector that we're interested in 
            Vector3 vector2 = differentWallRay.ray.point - closestHorRaycast.origin;// some vector that we're interested in 
                                                                                    //vector1.y = vector2.y = 0;vector1 = vector1.normalized; vector2 = vector2.normalized;
            vector2.y = 0; vector2 = vector2.normalized;
            if (!disableAllRays && showWallEdgeRays)
            {
                Debug.DrawRay(closestHorRaycast.origin, referenceForward * 1f, Color.yellow);
                Debug.DrawRay(closestHorRaycast.origin, vector2 * 1f, Color.black);
            }

            //float angle1 = SignedRelativeAngle(referenceForward, vector1);
            float angle2 = SignedRelativeAngle(referenceForward, vector2, Vector3.up);
            bool angle1IsRight = (angle2 < 0);
            Vector3 wallVectorDcha;
            Vector3 wallVectorIzda;
            if (angle1IsRight)
            {
                wallVectorDcha = new Vector3(-closestHorRaycast.ray.normal.z, 0, closestHorRaycast.ray.normal.x).normalized;
                wallVectorIzda = new Vector3(differentWallRay.ray.normal.z, 0, -differentWallRay.ray.normal.x).normalized;
            }
            else
            {
                wallVectorIzda = new Vector3(closestHorRaycast.ray.normal.z, 0, -closestHorRaycast.ray.normal.x).normalized;
                wallVectorDcha = new Vector3(-differentWallRay.ray.normal.z, 0, differentWallRay.ray.normal.x).normalized;
            }
            if (!disableAllRays && showWallEdgeRays)
            {
                Debug.DrawRay(differentWallRay.ray.point, wallVectorIzda * 2, angle1IsRight ? Color.magenta : Color.black);
                Debug.DrawRay(closestHorRaycast.ray.point, wallVectorDcha * 2, angle1IsRight ? Color.black : Color.magenta);
            }
            //Debug.LogWarning("WALL EDGE: collPoint1 = " + closestHorRaycast.ray.point.ToString("F4") + "; collPoint2 = " + differentWallRay.ray.point.ToString("F4") +
            //    "; vector2 = " + vector2 + "; angle2 = " + angle2 + "; angle1IsRight = " + angle1IsRight);

            float cornerAngle = SignedRelativeAngle(wallVectorIzda, wallVectorDcha, Vector3.up);
            //Debug.LogWarning("WALL EDGE: COMPROBACION CONCAVO/CONVEXO -> cornerAngle = "+ cornerAngle);
            #endregion

            if (cornerAngle >= 0 && Mathf.Abs(cornerAngle) != 180)// -- ES UNA ESQUINA CONVEXA -- 
            {
                #region --- Crear el plano ---
                Debug.LogWarning("WALL EDGE: Estamos en una esquina convexa!");
                // --- CREAR EL PLANO --- 
                //crear un plano teniendo en cuenta que ángulo A + B + A = 180º
                //como en este dibujo:           \  B /   
                //                        Muro 1->\  /<-Muro 2
                //                              A (\/) A    <- Pico de un muro
                //                        --------------------- Plano

                //float sideAngle = (180 - cornerAngle) / 2;
                Vector3 planeNormal = -(wallVectorIzda + wallVectorDcha);
                Plane imaginaryWall = new Plane(planeNormal, auxClosestRay.ray.point);
                if (showWallEdgeRays && !disableAllRays)
                {
                    DrawPlane(auxClosestRay.ray.point, planeNormal, Color.green);
                }
                #endregion

                #region --- Raycast colisionando con ese plano ---
                // --- RAYCAST COLISIONANDO CON ESE PLANO --- 
                float enter = 0.0f;
                Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                float auxSkinWidth = auxClosestRay.skinWidth;
                float raylength = horVel.magnitude + auxSkinWidth;
                Vector3 origin = auxClosestRay.origin;
                Ray ray = new Ray(origin, horVel.normalized * raylength);
                if (imaginaryWall.Raycast(ray, out enter))
                {
                    float slopeAngle = GetSlopeAngle(planeNormal);
                    float wallAngle = SignedRelativeAngle(Vector3.forward, planeNormal, Vector3.up); ; Vector3.Angle(planeNormal, Vector3.forward);
                    float distance = (ray.GetPoint(enter) - origin).magnitude - auxSkinWidth;
                    auxClosestRay.ray.normal = planeNormal;
                    auxClosestRay.ray.point = ray.GetPoint(enter);
                    Debug.LogWarning("WALL EDGE: We hit the imaginary wall: enter = " + enter + "; ray dir = " + horVel + "; raylength = " + raylength
    + "; origin = " + origin + "; slopeAngle = " + slopeAngle + "; oldSlopeAngle = " + auxClosestRay.slopeAngle
    + "; wallAngle = " + wallAngle + "; oldWallAngle = " + auxClosestRay.wallAngle + "; distance = " + distance
    + "; oldDistance = " + auxClosestRay.distance);
                    closestHorRaycast = new Raycast(auxClosestRay.ray, distance, vel, origin, true, slopeAngle, wallAngle,
                        auxClosestRay.axis, auxClosestRay.row, auxClosestRay.column, horizontalRows, auxSkinWidth);
                    success = true;
                }
                else
                {
                    Debug.LogError("Error: Wall edge is trying to collide with the plane we just calculated, but there was no collision.");
                }
                #endregion
            }
        }
        return success;
    }

    bool WallEdgeAll(Vector3 vel, ref Raycast closestHorRaycast)
    {
        bool success = false;
        int found = 0;
        Raycast auxClosestRay = closestHorRaycast;
        Raycast differentWallRay = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
        bool closestRayIsRight = false;
        Debug.LogWarning("WALL EDGE ALL: START");

        #region --- Horizontalmente ---
        //Segunda comprobacion / HIGH PRECISION (lanzando nuevos raycasts) ---
        Debug.LogWarning("WALL EDGE ALL: HORIZONTAL");
        //bool seikai = false;//seikai = correcto en japonés. es que quería usar "success" pero ya estaba usado
        int column = closestHorRaycast.column;
        int row = closestHorRaycast.row;
        Axis originalAxis = closestHorRaycast.axis;
        for (int sentido = -1; sentido < 2 && found == 0; sentido = sentido == -1 ? sentido = 1 : sentido = 2)//1=+;-1=- (en el orden de creación de rayos: BACK Y RIGHT ) 
        {
            #region Parámetros
            float auxSkinWidth, originSkinWidth, endSkinWidth, raylength;
            Vector3 horVel, rayOrigin;
            Axis finalAxis = originalAxis;
            int finalSentido = sentido;
            bool throwRays = false;
            #region Parámetros sin cambio de eje
            if (!((column == 0 && sentido == -1) || (column == (horizontalRaysPerRow - 1) && sentido == 1)))
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
                            column = horizontalRaysPerRow - 1;
                        }
                        else if (column == horizontalRaysPerRow - 1 && vel.z < 0)//AL FINAL, en este caso es ABAJO
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
                        else if (column == horizontalRaysPerRow - 1 && vel.z < 0)//AL FINAL, en este caso es ABAJO
                        {
                            throwRays = true;
                            column = 0;
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
                        else if (column == horizontalRaysPerRow - 1 && vel.x > 0)//AL FINAL, en este caso es a la DCHA
                        {
                            throwRays = true;
                            column = 0;
                        }
                    }
                    else if (vel.z < 0)
                    {
                        if (column == 0 && vel.x < 0) //AL PRINCIPIO, en este caso es a la IZDA
                        {
                            throwRays = true;
                            column = horizontalRaysPerRow - 1;
                        }
                        else if (column == horizontalRaysPerRow - 1 && vel.x > 0)//AL FINAL, en este caso es a la DCHA
                        {
                            throwRays = true;
                            finalSentido = -1;
                        }
                    }

                }
                endSkinWidth = finalAxis == Axis.X ?
                    collisions.horRaycastsX[row, column].skinWidth : collisions.horRaycastsZ[row, column].skinWidth;
            }
            #endregion
            #endregion
            #region lanzar rayos
            for (int j = 1; j < wallEdgeHighPrecissionRays - 1 && found == 0 && throwRays; j++)
            {
                float skinWidthDiff = originSkinWidth - endSkinWidth;
                float progress = j / wallEdgeHighPrecissionRays;
                auxSkinWidth = originSkinWidth + skinWidthDiff * progress;
                raylength = horVel.magnitude + auxSkinWidth;// DO NOT CHANGE ORDER
                Vector3 localRayOrigin = rayOrigin;
                localRayOrigin += finalAxis == Axis.X ?
                Vector3.back * finalSentido * wallEdgeHighPrecisionHorRaySpacing * j : Vector3.right * finalSentido * wallEdgeHighPrecisionHorRaySpacing * j;
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
                    float wallAngle = SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up); //Vector3.Angle(hit.normal, Vector3.forward);
                    Raycast auxRay = new Raycast(hit, (hit.distance - auxSkinWidth), vel, localRayOrigin, true, slopeAngle, wallAngle,
                        originalAxis, row, column, horizontalRows, auxSkinWidth);
                    CollisionState slopeType = CheckSlopeTypeWallEdges(vel, auxRay);
                    if (slopeType == CollisionState.wall && AreAnglesDifferent(auxRay.wallAngle, closestHorRaycast.wallAngle))
                    {
                        //Debug.LogWarning("WALL EDGE: SEGUNDA COMPROBACION -> found!");
                        //CHECK FOR PERFECT EDGE Collision point 
                        differentWallRay = auxRay;
                        if (differentWallRay.distance < auxClosestRay.distance) auxClosestRay = differentWallRay;
                        found = 1;
                            if(finalSentido == 1)
                            {
                                closestRayIsRight = false;
                            }else if (finalSentido == -1)
                            {
                                closestRayIsRight = true;
                            }
                    }
                }
            }
            #endregion
        }

        #endregion

        #region --- Verticalmente ---
        if (found == 0)//solo buscamos este si no hemos encontrado nada en el horizontal
        {
            Debug.LogWarning("WALL EDGE ALL: VERTICAL");
            for (int sentido = -1; sentido < 2 && found == 0; sentido = sentido == -1 ? sentido = 1 : sentido = 2)//1=+;-1=- (en el orden de creación de rayos: BACK Y RIGHT ) 
            {
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
                        localRayOrigin += Vector3.up * sentido * wallEdgeHighPrecisionVerRaySpacing * j;
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
                            float wallAngle = SignedRelativeAngle(Vector3.forward, hit.normal, Vector3.up); //Vector3.Angle(hit.normal, Vector3.forward);
                            Raycast auxRay = new Raycast(hit, (hit.distance - auxSkinWidth), vel, localRayOrigin, true, slopeAngle, wallAngle,
                                originalAxis, row, column, horizontalRows, auxSkinWidth);
                            CollisionState slopeType = CheckSlopeTypeWallEdges(vel, auxRay);
                            if (slopeType == CollisionState.wall && AreAnglesDifferent(auxRay.wallAngle, closestHorRaycast.wallAngle))
                            {
                                Debug.LogWarning("WALL EDGE ALL: VERTICAL ->  found! slopeType = "+ slopeType + "; auxRay.wallAngle = " + auxRay.wallAngle +
                                "; closestHorRaycast.wallAngle = " + closestHorRaycast.wallAngle);
                                differentWallRay = auxRay;
                                if (differentWallRay.distance < auxClosestRay.distance) auxClosestRay = differentWallRay;
                                found = 2;
                                if (sentido == 1)
                                {
                                    closestRayIsRight = true;
                                }
                                else if (sentido == -1)
                                {
                                    closestRayIsRight = false;
                                }
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
            Debug.LogWarning("WALL EDGE ALL: COMPROBACION CONCAVO/CONVEXO START");
            Debug.Log("WALL EDGE ALL: closestHorRaycast -> (" + closestHorRaycast.row + "," + closestHorRaycast.column + "), wallAngle = " + closestHorRaycast.wallAngle +
                "; differentWallRay -> (" + differentWallRay.row + "," + differentWallRay.column + "), wallAngle = " + differentWallRay.wallAngle);

            // --- Comprobar que forman un ángulo convexo ---
            // -- Para ello primero comprobamos cual de los dos puntos es el del muro de nuestra izda y cual el de la dcha -- 
            // the vector that we want to measure an angle from
            Vector3 referenceForward = (closestHorRaycast.ray.point - closestHorRaycast.origin).normalized;/* some vector that is not Vector3.up */
            //referenceForward.y = 0;//TO TEST: I don't know if this is needed
            // the vector of interest 
            Vector3 vector2 = (differentWallRay.ray.point - closestHorRaycast.origin).normalized;/* some vector that we're interested in */
                                                                                    //vector1.y = vector2.y = 0;vector1 = vector1.normalized; vector2 = vector2.normalized;
                                                                                    //vector2.y = 0;
            if (!disableAllRays && showWallEdgeRays)
            {
                Debug.DrawRay(closestHorRaycast.origin, referenceForward * 1f, Color.yellow);
                Debug.DrawRay(closestHorRaycast.origin, vector2 * 1f, Color.black);
            }
            Debug.Log("FOUND = " + found);
            //Vector3 referenceUp;
            //if (found == 2)
            //{
            //    closestRayIsRight = closestHorRaycast.ray.point.y >= differentWallRay.ray.point.y ? false : true;
            //}
            //else
            //{
            //    referenceUp = Vector3.up;
            //    float angleBetweenVectors = SignedRelativeAngle(referenceForward, vector2, Vector3.up);
            //    closestRayIsRight = (angleBetweenVectors < 0);
            //}

            #region wallVectors method (deprecated)
            //Vector3 wallVectorDcha;
            //Vector3 wallVectorIzda;
            //if (closestRayIsRight)
            //{
            //    wallVectorDcha = new Vector3(-closestHorRaycast.ray.normal.z, 0, closestHorRaycast.ray.normal.x).normalized;
            //    wallVectorIzda = new Vector3(differentWallRay.ray.normal.z, 0, -differentWallRay.ray.normal.x).normalized;
            //}
            //else
            //{
            //    wallVectorIzda = new Vector3(closestHorRaycast.ray.normal.z, 0, -closestHorRaycast.ray.normal.x).normalized;
            //    wallVectorDcha = new Vector3(-differentWallRay.ray.normal.z, 0, differentWallRay.ray.normal.x).normalized;
            //}
            //if (!disableAllRays && showWallEdgeRays)
            //{
            //    Debug.DrawRay(differentWallRay.ray.point, wallVectorIzda * 2, closestRayIsRight ? Color.magenta : Color.black);
            //    Debug.DrawRay(closestHorRaycast.ray.point, wallVectorDcha * 2, closestRayIsRight ? Color.black : Color.magenta);
            //}
            ////Debug.LogWarning("WALL EDGE: collPoint1 = " + closestHorRaycast.ray.point.ToString("F4") + "; collPoint2 = " + differentWallRay.ray.point.ToString("F4") +
            ////    "; vector2 = " + vector2 + "; angle2 = " + angle2 + "; angle1IsRight = " + angle1IsRight);

            //float cornerAngle = SignedRelativeAngle(wallVectorIzda, wallVectorDcha, Vector3.up);
            #endregion
            Vector3 edgeVector = closestRayIsRight ? Vector3.Cross(closestHorRaycast.ray.normal, differentWallRay.ray.normal).normalized :
                Vector3.Cross(differentWallRay.ray.normal, closestHorRaycast.ray.normal).normalized;
            Debug.LogWarning("WALL EDGE ALL: closestRayIsRight = " + closestRayIsRight+
                "; edgeVector = " + edgeVector.ToString("F4"));
            Debug.DrawRay(auxClosestRay.ray.point, edgeVector * 2, Color.magenta);
            float cornerAngle = SignedRelativeAngle(closestHorRaycast.ray.normal, differentWallRay.ray.normal, edgeVector);
            //Debug.LogWarning("WALL EDGE: COMPROBACION CONCAVO/CONVEXO -> cornerAngle = "+ cornerAngle);
            #endregion

            if (((closestRayIsRight && cornerAngle >= 0) || (!closestRayIsRight && cornerAngle < 0)) && Mathf.Abs(cornerAngle) != 180)// -- ES UNA ESQUINA CONVEXA -- 
            {
                #region --- Crear el plano ---
                Debug.LogWarning("WALL EDGE ALL: Estamos en una esquina convexa!");
                // --- CREAR EL PLANO --- 
                //crear un plano teniendo en cuenta que ángulo A + B + A = 180º
                //como en este dibujo:           \  B /   
                //                        Muro 1->\  /<-Muro 2
                //                              A (\/) A    <- Pico de un muro
                //                        --------------------- Plano

                //float sideAngle = (180 - cornerAngle) / 2;
                #region -- Calculate Plane Normal --
                Vector3 planeNormal;
                if (edgeVector!=Vector3.up && edgeVector != Vector3.down)
                {
                    Vector3 firstPlaneVector, secondPlaneVector;
                    Vector3 horEdgeVector = edgeVector; horEdgeVector.y = 0;
                    if (edgeVector.y > 0)
                    {
                        firstPlaneVector = edgeVector;
                        secondPlaneVector = horEdgeVector;
                    }
                    else if (edgeVector.y < 0)
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
                    Debug.Log("WALL EDGE ALL: planeNormal = " + planeNormal.ToString("F4") + "; firstPlaneVector = " + firstPlaneVector.ToString("F4") +
    "; secondPlaneVector = " + secondPlaneVector.ToString("F4"));
                }
                else
                {
                    Vector3 wallVectorDcha;
                    Vector3 wallVectorIzda;
                    if (closestRayIsRight)
                    {
                        wallVectorDcha = new Vector3(-closestHorRaycast.ray.normal.z, 0, closestHorRaycast.ray.normal.x).normalized;
                        wallVectorIzda = new Vector3(differentWallRay.ray.normal.z, 0, -differentWallRay.ray.normal.x).normalized;
                    }
                    else
                    {
                        wallVectorIzda = new Vector3(closestHorRaycast.ray.normal.z, 0, -closestHorRaycast.ray.normal.x).normalized;
                        wallVectorDcha = new Vector3(-differentWallRay.ray.normal.z, 0, differentWallRay.ray.normal.x).normalized;
                    }
                    if (!disableAllRays && showWallEdgeRays)
                    {
                        Debug.DrawRay(differentWallRay.ray.point, wallVectorIzda * 2, closestRayIsRight ? Color.magenta : Color.black);
                        Debug.DrawRay(closestHorRaycast.ray.point, wallVectorDcha * 2, closestRayIsRight ? Color.black : Color.magenta);
                    }
                    planeNormal = -(wallVectorIzda + wallVectorDcha);
                }

                planeNormal.y = 0;
                if (planeNormal == -collisions.oldWallNormal)
                {
                    Debug.LogWarning("plane normal == - old plane normal!!!!");
                    planeNormal = -planeNormal;
                }
                #endregion
                #region --- CHECK FOR PERFECT EDGE COLLISION POINT ---
                bool hasHit = true;
                //rayOrigin = auxRay.ray.point;
                //rayEnd =
                //        for (int k = 0; hasHit; j++)
                //{
                //    if ()
                //    {
                //        hasHit = false;
                //    }
                //}
                #endregion
                Plane imaginaryWall = new Plane(planeNormal, auxClosestRay.ray.point);

                if (showWallEdgeRays && !disableAllRays)
                {
                    DrawPlane(auxClosestRay.ray.point, planeNormal, Color.green);
                }
                #endregion

                #region --- Raycast colisionando con ese plano ---
                // --- RAYCAST COLISIONANDO CON ESE PLANO --- 
                float enter = 0.0f;
                Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                float auxSkinWidth = auxClosestRay.skinWidth;
                float raylength = horVel.magnitude + auxSkinWidth;
                Vector3 origin = auxClosestRay.origin;
                Ray ray = new Ray(origin, horVel.normalized * raylength);
                if (imaginaryWall.Raycast(ray, out enter))
                {
                    float slopeAngle = GetSlopeAngle(planeNormal);
                    float wallAngle = SignedRelativeAngle(Vector3.forward, planeNormal, Vector3.up);// Vector3.Angle(planeNormal, Vector3.forward);
                    Debug.Log("WALL EDGE ALL: imaginary plane wallAngle = "+wallAngle+"; plane Normal = "+ planeNormal.ToString("F4"));
                    float distance = (ray.GetPoint(enter) - origin).magnitude - auxSkinWidth;
                    auxClosestRay.ray.normal = planeNormal;
                    auxClosestRay.ray.point = ray.GetPoint(enter);
                    Debug.LogWarning("WALL EDGE ALL: We hit the imaginary wall: enter = " + enter + "; ray dir = " + horVel + "; raylength = " + raylength
    + "; origin = " + origin + "; slopeAngle = " + slopeAngle + "; oldSlopeAngle = " + auxClosestRay.slopeAngle
    + "; wallAngle = " + wallAngle + "; oldWallAngle = " + auxClosestRay.wallAngle + "; distance = " + distance
    + "; oldDistance = " + auxClosestRay.distance);
                    closestHorRaycast = new Raycast(auxClosestRay.ray, distance, vel, origin, true, slopeAngle, wallAngle,
                        auxClosestRay.axis, auxClosestRay.row, auxClosestRay.column, horizontalRows, auxSkinWidth);
                    success = true;
                }
                else
                {
                    Debug.LogError("Error: Wall edge is trying to collide with the plane we just calculated, but there was no collision.");
                }
                #endregion
            }
        }
        return success;
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

    #endregion

    #region -- VERTICAL COLLISIONS --
    void NewVerticalCollisions2(ref Vector3 vel)
    {
        #region Raycasts
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
                    if (showVerticalRays && !disableAllRays)
                    {
                        Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.red);
                    }

                    if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore)) // throw raycast here
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        //print("Vertical Hit");
                        if (directionY == 1)
                        {
                            slopeAngle = slopeAngle == 180 ? slopeAngle = 0 : slopeAngle - 90;
                        }
                        float wallAngle = Vector3.Angle(hit.normal, Vector3.forward);
                        Raycast auxRay = new Raycast(hit, (hit.distance - skinWidth), vel, rayOrigin, true, slopeAngle, 0, Axis.Y, i, j, 0);
                        collisions.verRaycastsY[i, j] = auxRay;
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

                    }
                }
            }
        }
        #endregion

        if (collisions.closestVerRaycast.hit)//si ha habido una collision vertical
        {
            CollisionState value;
            //print("COLLISION VER: " + value + "; slopeAngle=" + collisions.closestVerRaycast.slopeAngle);
            if (!peak)
            {
                value = CheckSlopeType(vel, collisions.closestVerRaycast);
                value = value == CollisionState.climbing ? CollisionState.none : value;
                //print("Vertical Raycasts: value = "+value+ "; collisions.lastCollSt = " + collisions.lastCollSt + "; vel.y = " + vel.y);
                if (value == CollisionState.none && collisions.lastCollSt == CollisionState.crossingPeak && vel.y <= 0)
                {
                    value = CollisionState.crossingPeak;
                }
            }
            else
            {
                value = CollisionState.crossingPeak;
            }
            //print("Vertical collisions value = " + value);
            switch (value)//con que tipo de objeto collisionamos? suelo/cuesta arriba/cuesta abajo
            {
                #region None (FLOOR/ROOF)
                case CollisionState.none:
                    vel.y = (collisions.closestVerRaycast.distance) * directionY;
                    //rayLength = collisions.closestVerRaycast.distance;
                    if (collisions.collSt == CollisionState.climbing)//Subiendo chocamos con un techo
                    {
                        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                        //horVel = horVel.normalized * (vel.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad));
                        float xz = vel.y * Mathf.Tan(collisions.realSlopeAngle * Mathf.Deg2Rad);
                        horVel = horVel.normalized * xz;
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                        collisions.below = collisions.above = true;
                        Debug.LogError("While climbing we have collided with a roof: vel = " + vel.ToString("F6"));
                    }
                    else
                    {
                        collisions.below = directionY == -1;
                        collisions.above = directionY == 1;
                    }

                    break;
                #endregion
                #region Sliping
                case CollisionState.sliping:
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

    /// <summary>
    /// Auxiliar function that draws a plane given it's normal, a point in it, and a color. The normal will always be drawn red.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normal"></param>
    /// <param name="planeColor"></param>
    void DrawPlane(Vector3 position, Vector3 normal, Color planeColor)
    {

        Vector3 v3;
        normal = (normal.normalized) * 2;
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

        public CollisionState collSt;
        public CollisionState lastCollSt;
        public float slopeAngle, slopeAngleOld, realSlopeAngle, wallAngle, wallAngleOld, wallAngle2, wallAngleOld2, floorAngle;
        public Vector3 startVel;
        public Raycast closestHorRaycast;
        public Raycast[,] horRaycastsX;
        public Raycast[,] horRaycastsZ;
        public Raycast[,] verRaycastsY;
        public Raycast closestHorRaycastSlide;
        public Raycast closestHorRaycastClimb;
        public Raycast closestVerRaycast;
        public float distanceToFloor;
        //public Vector3 horCollisionsPoint;
        public Vector3 wallNormal;
        public Vector3 oldWallNormal;
        public GameObject horWall;
        public GameObject verWall;
        public SlideState slideSt;
        public SlideState oldSlideSt;


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
            wallAngle = -500;
            wallAngleOld2 = wallAngle2;
            wallAngle2 = 0;
            //horCollisionsPoint = Vector3.zero;
            oldWallNormal = wallNormal;
            wallNormal = Vector3.zero;
            horWall = null;
            oldSlideSt = slideSt;
            slideSt = SlideState.none;
            startVel = Vector3.zero;
            closestHorRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            horRaycastsX = new Raycast[0, 0];
            horRaycastsZ = new Raycast[0, 0];
            closestHorRaycastSlide = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            closestHorRaycastClimb = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            lastFinishedClimbing = finishedClimbing;
            finishedClimbing = false;
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
    public Vector3 origin;
    public float distance;
    public Vector3 vel;
    public bool hit;
    public float slopeAngle;
    public float wallAngle;
    public Axis axis;
    public int row;//row in which the ray was thrown from
    public int column;
    public float rayHeightPercentage;//from 0 (feet) to 100(head)
    public float skinWidth;

    public Raycast(RaycastHit _ray, float _dist, Vector3 _vel, Vector3 _origin, bool _hit = false, float _slopeAngle = 0, float _wallAngle = 0,
        Axis _axis = Axis.none, int _row = 0, int _column = 0, int horizontalRows = 0, float _skinWidth = 0.1f)
    {
        ray = _ray;
        distance = _dist;
        vel = _vel;
        origin = _origin;
        hit = _hit;
        axis = _axis;
        slopeAngle = _slopeAngle;
        wallAngle = _wallAngle;
        row = _row;
        column = _column;
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
