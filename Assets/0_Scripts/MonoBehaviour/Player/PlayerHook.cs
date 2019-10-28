using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HookState
{
    ready,
    throwing,
    reeling,
    grappling,
    cd
}
public enum GrappleState
{
    ready,
    throwing,
    grappling,
    cd
}
[RequireComponent(typeof(PlayerMovement))]
public class PlayerHook : MonoBehaviour
{
    [Header("Referencias")]
    PlayerHUD myPlayerHUD;
    PlayerMovement myPlayerMov;
    //PlayerCombat myPlayerCombat;
    PlayerCombatNew myPlayerCombatNew;

    [HideInInspector]
    public CameraController myCameraBase;
    Hook myHook;

    [HideInInspector]
    public HookState hookSt;
    [HideInInspector]
    public GameObject currentHook;
    Transform hookRopeEnd;
    Transform hookedObject;
    public Vector3 hookLocalOrigin;
    public float hookMaxDistance;
    float currentDistance;
    float lastCurrentDistance;
    Vector3 originPos;
    Vector3 hookPos;

    bool usingHook
    {
        get
        {
            return (hookSt == HookState.throwing || hookSt == HookState.reeling || hookSt == HookState.grappling);
        }
    }
    bool usingAutoGrapple
    {
        get
        {
            return (grappleSt == GrappleState.throwing || grappleSt == GrappleState.grappling);
        }
    }
    [HideInInspector]
    public bool canHookSomething
    {
        get
        {
            return (hookSt == HookState.throwing && !enemyHooked && !objectHooked);
        }
    }
    [HideInInspector]
    public bool somethingHooked
    {
        get
        {
            return (enemyHooked || objectHooked);
        }
    }

    [Header("--- HOOK ---")]
    public int hookPriority = 4;
    public float hookFowardSpeed;
    public float hookBackwardsSpeed;
    public float hookGrapplingSpeed;
    public float hookMinDistance;//Distance the hook stops when bringing the enemy
    public float cdMaxTime;
    public float cdTime
    {
        get { return _cdTime; }
        set
        {
            myPlayerHUD.SetHookUI(_cdTime / cdMaxTime);
            _cdTime = value;
        }
    }
    float _cdTime;
    bool hookReady;
    Vector3 hookMovingVel;
    Vector3 dirToHook;

    [HideInInspector]
    public bool enemyHooked;
    bool objectHooked;// item 
    [HideInInspector]
    public PlayerMovement enemy;

    [Space]
    public LayerMask collisionMask;

    // --- Grapple --- 
    [HideInInspector]
    public GrappleState grappleSt = GrappleState.ready;
    [Header(" --- AUTO GRAPPLE --- ")]
    [Tooltip("Layers that we can collide with, like walls, or the hookPoint. If it's a wall (checking via tags) we won't be able to grapple.")]
    public LayerMask grappleColMask;
    public float grappleStopMinDistance = 0.1f;
    public float minDistanceToGrapple;
    public float hookPointMinDistToCameraCenter;
    public float grappleMaxCDTime;
    float grappleCDTime = 0;
    bool canAutoGrapple = false;
    HookPoint currentHookPointInSight;
    HookPoint currentGrapplingHookPoint;
    Transform currentHookPointPos;
    //float currentGrappleDistance = 0;
    float timeGrappling = 0;
    Camera myCamera;
    Plane[] cameraPlanes;
    List<HookPoint> hookPointsInView;

    public void KonoAwake()
    {
        myPlayerMov = GetComponent<PlayerMovement>();
        myPlayerCombatNew = GetComponent<PlayerCombatNew>();
        if (myPlayerMov == null)
        {
            Debug.LogError("PlayerMovement is not on the object where the hook script is. This object is " + gameObject.name);
        }
        myPlayerHUD = myPlayerMov.myPlayerHUD;
        hookSt = HookState.ready;
        myCamera = myCameraBase.myCamera.GetComponent<Camera>();

        hookPointsInView = new List<HookPoint>();
        cdTime = cdMaxTime;
    }

    public void KonoUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            FinishAutoGrapple();
        }
        UpdateHookPoints();
        if (canAutoGrapple && myPlayerMov.Actions.R1.WasPressed && !myPlayerMov.inWater)
        {
            CheckStartAutoGrapple();
        }

        ProcessAutoGrapple();

        //TO DO: Revisar este código porque me parece mal colocado o innecesario.
        ProcessHook();
    }

    public void ResetHook()
    {
        FinishHook();
        cdTime = 0;
    }

    void UpdateDistance()
    {
        originPos = myPlayerMov.rotateObj.TransformPoint(hookLocalOrigin);
        if (!somethingHooked)
        {
            hookPos = currentHook.transform.position;
        }
        else if (enemyHooked)
        {
            hookPos = enemy.transform.position;
        }
        else if (objectHooked)
        {
            hookPos = hookedObject.position;
        }
        dirToHook = (hookPos - originPos);
        lastCurrentDistance = currentDistance;
        currentDistance = dirToHook.magnitude;
    }

    public void StartGrappling(HookPoint hookPoint, Transform _currentHookPointPos)
    {
        if (!myPlayerMov.disableAllDebugs) print("START GRAPPLING");
        currentGrapplingHookPoint = hookPoint;
        currentHookPointPos=_currentHookPointPos;
        myPlayerMov.StopBoost();
        if (grappleSt == GrappleState.throwing)
        {
            StartGrapplingAutoGrapple();
        }
        else if (hookSt == HookState.throwing)
        {
            StartHookGrappling();
        }
        else
        {
            Debug.LogError("Error: PlayerHook: hook collided with a hookPoint but the hook is not in hook mode nor automatic grapple mode");
        }
    }

    #region --- Hook ---
    public void StartHook()
    {
        if (hookSt == HookState.ready && !usingAutoGrapple)
        {
            //VARIABLES
            enemy = null;
            hookedObject = null;
            hookSt = HookState.throwing;
            objectHooked = false;
            enemyHooked = false;
            currentDistance = 0;
            lastCurrentDistance = 0;
            originPos = myPlayerMov.rotateObj.transform.TransformPoint(hookLocalOrigin);
            hookPos = originPos;
            myPlayerMov.StartHooking();

            //Calculate trayectory
            Vector3 rayOrigin = myPlayerMov.myCamera.myCamera.transform.position;
            float minDistance = (myPlayerMov.myCamera.transform.position - myPlayerMov.myCamera.myCamera.transform.position).magnitude;
            rayOrigin += myPlayerMov.currentCamFacingDir * minDistance;
            Vector3 endPoint;
            RaycastHit hit;
            Debug.DrawRay(rayOrigin, myPlayerMov.currentCamFacingDir * hookMaxDistance, Color.white, 3);
            if (Physics.Raycast(rayOrigin, myPlayerMov.currentCamFacingDir, out hit, hookMaxDistance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                endPoint = hit.point;
            }
            else
            {
                endPoint = rayOrigin + (myPlayerMov.currentCamFacingDir * hookMaxDistance);
            }
            hookMovingVel = (endPoint - originPos).normalized * hookFowardSpeed;

            //Instantiate hook
            Quaternion rotation = Quaternion.LookRotation(hookMovingVel);
            currentHook = StoringManager.instance.Spawn(StoringManager.instance.hookPrefab, originPos, rotation).gameObject;
            myHook = currentHook.GetComponent<Hook>();
            myHook.GetComponent<LineRenderer>().enabled = true;
            myHook.KonoAwake(myPlayerMov, this);
            hookRopeEnd = myHook.hookRopeEnd;
            myPlayerHUD.StartThrowHook();
        }
    }

    void ProcessHook()
    {
        if (hookSt != HookState.ready && hookSt != HookState.cd)
        {
            UpdateDistance();
            if (currentDistance >= hookMaxDistance)
            {
                StopHook();
            }
            Vector3 hookRopeEndPos = hookRopeEnd.position;
            myHook.UpdateRopeLine(originPos, hookRopeEndPos);
            //Debug.DrawLine(originPos, hookRopeEndPos, Color.red);
            RaycastHit hit;
            if (Physics.Linecast(originPos, hookRopeEndPos, out hit, collisionMask, QueryTriggerInteraction.Ignore))
            {
                StopHook();
            }
        }
        Vector3 reelingDir;
        switch (hookSt)
        {
            case HookState.throwing:
                MoveHook(hookMovingVel);
                break;
            case HookState.reeling:
                if (currentDistance <= hookMinDistance)
                {
                    FinishHook();
                }
                else
                {
                    //Calculate dir to owner
                    reelingDir = (originPos - hookPos).normalized;
                    //move with speed to owner
                    hookMovingVel = reelingDir * hookBackwardsSpeed;
                    //Calculate rotation
                    Quaternion rotation = Quaternion.LookRotation(-hookMovingVel);
                    currentHook.transform.localRotation = rotation;

                    if (enemyHooked)
                    {
                        enemy.currentVel = hookMovingVel;
                        enemy.currentSpeed = hookBackwardsSpeed;
                    }
                    else
                    {
                        MoveHook(hookMovingVel);
                    }

                    //check for collisions
                }
                break;
            case HookState.grappling:
                currentHook.transform.position = currentHookPointPos.position;
                timeGrappling += Time.deltaTime;
                float lastFrameDist = Mathf.Abs(lastCurrentDistance - currentDistance);
                if (currentDistance <= hookMinDistance || (lastFrameDist < 0.01f && timeGrappling > 0.1f))//CHECK IF ARRIVED OR STUCK
                {
                    FinishHook();
                }
                else
                {
                    reelingDir = (hookPos - originPos).normalized;
                    //move with speed to hook
                    hookMovingVel = reelingDir * hookGrapplingSpeed;
                    myPlayerMov.currentVel = hookMovingVel;
                    myPlayerMov.currentSpeed = hookGrapplingSpeed;
                }
                break;
            case HookState.cd:
                cdTime += Time.deltaTime;
                if (cdTime >= cdMaxTime)
                {
                    hookSt = HookState.ready;
                    cdTime = cdMaxTime;
                }
                break;
        }
    }

    void MoveHook(Vector3 vel)
    {
        Vector3 finalVel = (vel * Time.deltaTime);
        currentHook.transform.Translate(finalVel, Space.World);
    }

    public void HookPlayer(PlayerMovement player)
    {
        if (canHookSomething && player.StartHooked())
        {
            //print("HOOK PLAYER");
            enemyHooked = true;
            enemy = player;
            
            currentHook.transform.SetParent(enemy.transform);
            currentHook.transform.localPosition = Vector3.zero;
            StartReeling();
        }

    }

    public void HookObject(Transform item)
    {
        if (canHookSomething)
        {
            if (item.tag == "Flag")
            {
                Flag flag = item.GetComponent<Flag>();
                if (!flag.HookFlag(myPlayerMov))
                {
                    print("FLAG NOT HOOKED");
                    return;
                }
            }
            objectHooked = true;
            hookedObject = item;
            print("Current hook = " + currentHook);
            hookedObject.SetParent(currentHook.transform);
            hookedObject.transform.localPosition = Vector3.zero;
            StartReeling();
        }
    }

    public void StartReeling()
    {
        if (hookSt == HookState.throwing)
        {
            //print("START REELING");
            hookSt = HookState.reeling;
        }
    }

    public void StartHookGrappling()
    {
        if (hookSt == HookState.throwing)
        {
            timeGrappling = 0;
            hookSt = HookState.grappling;
            //grappleOrigin = currentHook.transform.position;
            myPlayerMov.StopHooking();
            myPlayerMov.StartHooked();
            myPlayerCombatNew.StopAiming();
            //currentHookPoint = hookPoint;

        }
    }

    public void StopHook()
    {
        if (usingHook)
        {
            Debug.Log("STOP HOOK");
            StartReeling();
            if (somethingHooked)
            {
                if (enemyHooked)
                {
                    enemyHooked = false;
                    enemy.StopHooked(0);
                    enemy.StartRecieveHit(myPlayerMov, Vector3.zero,EffectType.softStun,0.2f);
                    enemy = null;
                }
                else if (objectHooked)
                {
                    objectHooked = false;
                    Debug.LogError("StopHook while reeling an object!");
                    //print("DROP OBJECT FROM HOOK");
                    if (hookedObject.tag == "Flag")
                    {
                        Flag flag = hookedObject.GetComponent<Flag>();
                        flag.DropFlag();
                    }
                    hookedObject.SetParent(StoringManager.instance.transform);
                    hookedObject = null;
                }
            }
        }
    }

    public void FinishHook()
    {
        //print("FINISH HOOK");
        switch (hookSt)
        {
            case HookState.reeling:

                myPlayerMov.StopHooking();
                HandinObject();
                StopHook();
                StoringManager.instance.StoreObject(currentHook.transform);
                currentHook = null;
                cdTime = 0;
                if (myPlayerCombatNew.aiming)
                {
                    myPlayerHUD.StopThrowHook();
                }
                hookSt = HookState.cd;
                break;
            case HookState.grappling:
                FinishHookGrappling();
                break;
        }
        if (myHook != null)
        {
            myHook.GetComponent<LineRenderer>().enabled = false;
        }
    }

    void FinishHookGrappling()
    {
        currentGrapplingHookPoint = null;
        currentHookPointPos = null;
        hookSt = HookState.cd;
        myPlayerMov.StopHooked(0.5f);
        StoringManager.instance.StoreObject(currentHook.transform);
        currentHook = null;
        cdTime = 0;
        if (myPlayerMov.Actions.L2.IsPressed)
        {
            myPlayerCombatNew.StartAiming();
        }

    }

    void HandinObject()
    {
        if (objectHooked)
        {
            if (hookedObject.tag == "Flag")
            {
                //print("Recieve flag with hook");
                Flag flag = hookedObject.GetComponent<Flag>();
                flag.StopBeingHooked();
                flag.PickupFlag(myPlayerMov);
            }
            else
            {
                StoringManager.instance.StoreObject(hookedObject);
                //OBTAIN ITEM
            }
            objectHooked = false;
            hookedObject = null;
        }
        if (enemyHooked)
        {
            if (enemy.haveFlag)
            {
                enemy.flag.StealFlag(myPlayerMov);
            }
        }
    }
    #endregion

    #region --- Automatic Grapple ---
    //AUTOMATIC GRAPPLE
    public void CheckStartAutoGrapple()
    {
        if (grappleSt == GrappleState.ready && !usingHook )
        {
            //Debug.Log("Grapple: Starting Grapple");
            originPos = myPlayerMov.rotateObj.transform.TransformPoint(hookLocalOrigin);

            //Calculate trayectory
            Vector3 rayOrigin = originPos;
            float dist = (minDistanceToGrapple + 0.1f);
            Vector3 rayEnd = currentHookPointInSight.transform.position;
            //Debug.Log("Grapple: Checking for walls in the middle; rayOrigin = " + rayOrigin.ToString("F4"));

            Debug.DrawLine(rayOrigin, rayEnd, Color.white, 5);
            RaycastHit hit;
            if (Physics.Linecast(rayOrigin, rayEnd, out hit, grappleColMask, QueryTriggerInteraction.Collide))
            {
                //print("AUTOGRAPPLE: collided with something in the middle: " + hit.transform.name);
                if (hit.transform.tag == "Stage")
                {
                    Debug.LogWarning("Warning: Can't grapple because there is a wall or something similar in the middle (" + hit.transform.name + ")");
                    return;
                }
                else
                {
                    StartAutoGrapple();
                }
            }
            else
            {
                float distFromOriginToHookPoint = (originPos - rayEnd).magnitude;
                float hookPointRadius = currentHookPointInSight.GetComponentInChildren<SphereCollider>().bounds.extents.x;
                if (distFromOriginToHookPoint <= hookPointRadius)
                {
                    StartAutoGrapple();
                }
                else
                {
                    Debug.LogError("Error: For some reason the automatic grapple can't hit the hookPoint with a raycast to check for walls");
                }
            }
        }
    }

    void StartAutoGrapple()
    {
        //print("START THROWING AUTOGRAPPLE");
        //VARIABLES
        grappleSt = GrappleState.throwing;
        //currentGrappleDistance = 0;
        lastCurrentDistance = 0;
        hookPos = originPos;
        myPlayerMov.StartHooking();

        Vector3 endPoint = currentHookPointInSight.transform.position;
        //throwing grapple anim start
        hookMovingVel = (endPoint - originPos).normalized * hookFowardSpeed;

        //Instantiate hook
        Quaternion rotation = Quaternion.LookRotation(hookMovingVel);
        currentHook = StoringManager.instance.Spawn(StoringManager.instance.hookPrefab, originPos, rotation).gameObject;
        myHook = currentHook.GetComponent<Hook>();
        myHook.GetComponent<LineRenderer>().enabled = true;
        myHook.KonoAwake(myPlayerMov, this);
        hookRopeEnd = myHook.hookRopeEnd;
        //myPlayerHUD.StartThrowHook();
    }

    void ProcessAutoGrapple()
    {
        if (grappleSt != GrappleState.ready && grappleSt != GrappleState.cd)
        {
            UpdateDistance();
            Vector3 hookRopeEndPos = hookRopeEnd.position;
            myHook.UpdateRopeLine(originPos, hookRopeEndPos);
            //Debug.DrawLine(originPos, hookRopeEndPos, Color.red);
            RaycastHit hit;
            if (Physics.Linecast(originPos, hookRopeEndPos, out hit, collisionMask, QueryTriggerInteraction.Ignore))
            {
                FinishAutoGrapple();
            }
        }
        Vector3 reelingDir;
        switch (grappleSt)
        {
            case GrappleState.throwing:
                MoveHook(hookMovingVel);
                //UpdateDistance();
                Vector3 hookRopeEndPos = hookRopeEnd.position;
                myHook.UpdateRopeLine(originPos, hookRopeEndPos);
                //Debug.DrawLine(originPos, hookRopeEndPos, Color.red);
                RaycastHit hit;
                if (Physics.Linecast(originPos, hookRopeEndPos, out hit, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    StartGrapplingAutoGrapple();
                }
                else if(currentDistance >= minDistanceToGrapple)
                {
                    FinishAutoGrapple();
                }

                break;
            case GrappleState.grappling:
                //print("AUTO GRAPPLING");
                currentHook.transform.position = currentHookPointPos.position;
                //UpdateDistance();
                hookRopeEndPos = hookRopeEnd.position;
                myHook.UpdateRopeLine(originPos, hookRopeEndPos);
                timeGrappling += Time.deltaTime;

                float lastFrameDist = Mathf.Abs(lastCurrentDistance - currentDistance);
                if (currentDistance <= grappleStopMinDistance || (lastFrameDist < 0.01f && timeGrappling > 0.1f))//CHECK IF ARRIVED OR STUCK
                {
                    if (!myPlayerMov.disableAllDebugs) print("FINISH AUTOGRAPPLE: currentDistance = " + currentDistance + "; grappleStopMinDistance = " + grappleStopMinDistance + "; lastFrameDist = "+lastFrameDist+"; timeGrappling = "+timeGrappling);
                    FinishAutoGrapple();
                }
                else
                {
                    reelingDir = (hookPos - originPos).normalized;
                    //move with speed to hook
                    hookMovingVel = reelingDir * hookGrapplingSpeed;
                    myPlayerMov.currentVel = hookMovingVel;
                    myPlayerMov.currentSpeed = hookGrapplingSpeed;
                    //print("MOVING TO HOOKPOINT WITH SPEED = " + myPlayerMov.currentSpeed);
                }
                break;
            case GrappleState.cd:
                grappleCDTime += Time.deltaTime;
                if (grappleCDTime >= grappleMaxCDTime)
                {
                    grappleSt = GrappleState.ready;
                }
                break;
        }
    }

    public void StartGrapplingAutoGrapple()
    {
        if (grappleSt == GrappleState.throwing)
        {
            //print("START AUTOGRAPPLING");
            timeGrappling = 0;
            grappleSt = GrappleState.grappling;
            myPlayerMov.StopHooking();
            myPlayerMov.StartHooked();
        }
    }

    public void FinishAutoGrapple()
    {
        if (usingAutoGrapple)
        {
            //print("FINISH AUTOGRAPPLE");
            currentGrapplingHookPoint = null;
            currentHookPointPos = null;
            grappleSt = GrappleState.cd;
            myPlayerMov.StopHooked(0.5f);
            myPlayerMov.StopHooking();
            StoringManager.instance.StoreObject(currentHook.transform);
            currentHook = null;
            grappleCDTime = 0;
            if (myPlayerMov.Actions.L2.IsPressed)
            {
                myPlayerCombatNew.StartAiming();
            }
            if (myHook != null)
            {
                myHook.GetComponent<LineRenderer>().enabled = false;
            }
        }

    }

    void UpdateHookPoints()
    {
        List<HookPoint> hookPoints = myPlayerMov.myPlayerObjectDetection.hookPoints;
        HookPoint closestHookPoint = null;//HookPoint that is in range of grapple, in front of the camera (in view) and has the lowest angle with the camera Z+
        float lowestDistToCamCenter = float.MaxValue;

        Vector2 minDist = new Vector2(hookPointMinDistToCameraCenter * myCamera.rect.width, hookPointMinDistToCameraCenter * myCamera.rect.height);
        if (myPlayerMov.gC.HasPlayerFlatCamera(myPlayerMov))
         {
            //print("FLAT CAMERA ("+name+")");
            minDist.y += (1 * myCamera.pixelHeight);
            minDist.x -= (0.1f * myCamera.pixelWidth);
         }
        for (int i = 0; i < hookPoints.Count; i++)
        {
            bool success = false;
            float dist = (hookPoints[i].transform.position - myPlayerMov.transform.position).magnitude;
            if (dist <= minDistanceToGrapple)
            {
                Collider col = hookPoints[i].smallTrigger.GetComponent<Collider>();
                cameraPlanes = GeometryUtility.CalculateFrustumPlanes(myCamera);
                if (GeometryUtility.TestPlanesAABB(cameraPlanes, col.bounds))
                {
                    Vector2 hookScreenPos = myCamera.WorldToScreenPoint(hookPoints[i].transform.position);
                    Vector3 distToCameraCenter = (hookScreenPos - myPlayerHUD.cameraCenterPix);
                    float screenScale = (float)myPlayerMov.myUICamera.pixelHeight / (float)myPlayerMov.myUICamera.pixelWidth;
                    if (Mathf.Abs(distToCameraCenter.x) <= minDist.x && Mathf.Abs(distToCameraCenter.y) <= (minDist.y * screenScale))
                    {
                        success = true;
                        if (!hookPointsInView.Contains(hookPoints[i]))
                        {
                            hookPointsInView.Add(hookPoints[i]);
                            myPlayerHUD.ShowHookPointHUD(hookPoints[i]);
                        }

                        float newDist = (myPlayerHUD.cameraCenterPix - hookScreenPos).magnitude;
                        if (newDist < lowestDistToCamCenter)
                        {
                            lowestDistToCamCenter = newDist;
                            closestHookPoint = hookPoints[i];
                        }
                    }
                }
            }
            if (!success && hookPointsInView.Contains(hookPoints[i]))
            {
                myPlayerHUD.HideHookPointHUD(hookPoints[i]);
                hookPointsInView.Remove(hookPoints[i]);
            }

        }
        if (closestHookPoint != null)
        {
            if (closestHookPoint != currentHookPointInSight)
            {
                canAutoGrapple = true;
                currentHookPointInSight = closestHookPoint;
                //myPlayerHUD.ShowGrappleMessage();
                myPlayerHUD.SetChosenHookPointHUD(closestHookPoint);
            }
        }
        else
        {
            canAutoGrapple = false;
            //myPlayerHUD.HideGrappleMessage();
            currentHookPointInSight = null;
        }
    }
    #endregion
}
