﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHook : MonoBehaviour
{
    PlayerMovement myPlayerMov;
    PlayerCombat myPlayerCombat;

    public LayerMask collisionMask;
    GameObject currentHook;
    Transform hookRopeEnd;
    Transform hookedObject;
    public GameObject hookPrefab;
    public Vector3 hookLocalOrigin;
    public float hookMaxDistance;
    float currentDistance;
    Vector3 originPos;
    Vector3 hookPos;
    public float hookFowardSpeed;
    public float hookBackwardsSpeed;
    public float hookGrapplingSpeed;
    public float hookMinDistance;//Distance the hook stops when bringing the enemy
    public float cdMaxTime;
<<<<<<< HEAD:Assets/Scripts/Player/PlayerHook.cs
    float cdTime;
=======
    public float cdTime
    {
    	get{return _cdTime;}
    	set
        {
            hookUI.fillAmount = Mathf.Clamp01( _cdTime/cdMaxTime );
            _cdTime = value;
        }
    }
    float _cdTime;
    bool hookReady;
>>>>>>> Menu:Assets/Scripts/Player/Hook.cs
    Vector3 reelingVel;

    HookState hookSt;
    public enum HookState
    {
        ready,
        throwing,
        reeling,
        grappling,
        cd
    }
    bool enemyHooked;
    bool objectHooked;// item 
    PlayerMovement enemy;

    [Space]
    public LayerMask collisionMask;

    [Header("Referencias")]
    public Image hookUI;

    private void Awake()
    {
        myPlayerMov = GetComponent<PlayerMovement>();
        myPlayerCombat = GetComponent<PlayerCombat>();
        if (myPlayerMov == null)
        {
            Debug.LogError("PlayerMovement is not on the object where the hook script is. This object is " + gameObject.name);
        }
        hookSt = HookState.ready;
    }

    private void Update()
    {
        if (hookSt != HookState.ready && hookSt != HookState.cd)
        {
            UpdateDistance();
            if (currentDistance >= hookMaxDistance)
            {
                StartReeling();
            }
            Vector3 hookRopeEndPos = hookRopeEnd.position;
            Debug.DrawLine(originPos, hookRopeEndPos, Color.red);
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
                MoveHook(reelingVel);
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
                    reelingVel = reelingDir * hookBackwardsSpeed;
                    //Calculate rotation
                    Quaternion rotation = Quaternion.LookRotation(-reelingVel);
                    currentHook.transform.localRotation = rotation;

                    if (enemyHooked)
                    {
                        enemy.currentVel = reelingVel;
                        enemy.currentSpeed = hookBackwardsSpeed;
                    }
                    else
                    {
                        MoveHook(reelingVel);
                    }

                    //check for collisions
                }
                break;
            case HookState.grappling:
                if (currentDistance <= hookMinDistance)
                {
                    FinishHook();
                }
                else
                {
                    reelingDir = (hookPos - originPos).normalized;
                    //move with speed to hook
                    reelingVel = reelingDir * hookGrapplingSpeed;
                    myPlayerMov.currentVel = reelingVel;
                    myPlayerMov.currentSpeed = hookGrapplingSpeed;
                }
                break;
            case HookState.cd:
                cdTime += Time.deltaTime;
                if (cdTime >= cdMaxTime)
                {
                    hookSt = HookState.ready;
                }
                break;

        }
    }

    void MoveHook(Vector3 vel)
    {
        Vector3 finalVel = (vel * Time.deltaTime);
        currentHook.transform.Translate(finalVel, Space.World);
    }

    public void StartHook()
    {
        if (hookSt == HookState.ready)
        {
            //VARIABLES
            enemy = null;
            hookedObject = null;
            hookSt = HookState.throwing;
            objectHooked = false;
            enemyHooked = false;
            currentDistance = 0;
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
            reelingVel = (endPoint - originPos).normalized * hookFowardSpeed;
            //Instantiate hook
            Quaternion rotation = Quaternion.LookRotation(reelingVel);
            if (StoringManager.instance.IsObjectStored(hookPrefab.name))
            {
                currentHook = StoringManager.instance.TakeObjectStored(hookPrefab.name, originPos, rotation).gameObject;
            }
            else
            {
                currentHook = Instantiate(hookPrefab, originPos, Quaternion.identity, StoringManager.instance.transform);
            }
            currentHook.transform.GetComponentInChildren<Hook>().KonoAwake(myPlayerMov, this);
            hookRopeEnd = currentHook.GetComponent<Hook>().hookRopeEnd;
            print("hookHBsmall= " + currentHook.transform.GetComponentInChildren<HitboxHookSmall>() + "; hookHBBig = " + currentHook.transform.GetComponentInChildren<HitboxHookBig>());
            myPlayerCombat.myPlayerHUD.StartThrowHook();
        }
    }

    public void HookPlayer(PlayerMovement player)
    {
        if (canHookSomething)
        {
            //print("HOOK PLAYER");
            enemyHooked = true;
            enemy = player;
            enemy.StartHooked();
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

    public void StopHook()
    {
        print("STOP HOOK");
        StartReeling();
        if (somethingHooked)
        {
            if (enemyHooked)
            {
                print("DROP ENEMY FROM HOOK");
                enemyHooked = false;
                enemy.StopHooked();
                enemy = null;

            }
            else if (objectHooked)
            {
                print("DROP OBJECT FROM HOOK");
                if (hookedObject.tag == "Flag")
                {
                    Flag flag = hookedObject.GetComponent<Flag>();
                    flag.DropFlag();
                }
                objectHooked = false;
                hookedObject.SetParent(StoringManager.instance.transform);
                hookedObject = null;

            }
        }
    }

    public void FinishHook()
    {
        //print("FINISH HOOK");
        switch (hookSt)
        {
            case HookState.reeling:
                hookSt = HookState.cd;
                myPlayerMov.StopHooking();
                HandinObject();
                StopHook();
                StoringManager.instance.StoreObject(currentHook.transform);
                currentHook = null;
                cdTime = 0;
                if (myPlayerCombat.aiming)
                {
                    myPlayerCombat.myPlayerHUD.StopThrowHook();
                }
                break;
            case HookState.grappling:
                FinishGrapple();
                break;
        }
    }

    Vector3 grappleOrigin;
    public void StartGrappling()
    {
        if (hookSt == HookState.throwing)
        {
            hookSt = HookState.grappling;
            //grappleOrigin = currentHook.transform.position;
            myPlayerMov.StopHooking();
            myPlayerMov.StartHooked();
            myPlayerCombat.StopAiming();

        }
    }

    void FinishGrapple()
    {
        hookSt = HookState.cd;
        myPlayerMov.StopHooked();
        StoringManager.instance.StoreObject(currentHook.transform);
        currentHook = null;
        cdTime = 0;
        if (myPlayerMov.Actions.Aim.IsPressed)
        {
            myPlayerCombat.StartAiming();
        }

    }

    void HandinObject()
    {
        if (objectHooked)
        {
            if (hookedObject.tag == "Flag")
            {
                print("Recieve flag with hook");
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

    Vector3 dirToHook;
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
        currentDistance = dirToHook.magnitude;
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
}
