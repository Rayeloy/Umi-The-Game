using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class Hook : MonoBehaviour
{

    PlayerMovement myPlayerMov;

    GameObject currentHook;
    Transform hookedObject;
    public GameObject hookPrefab;
    public Vector3 hookLocalOrigin;
    public float hookMaxDistance;
    float currentDistance;
    Vector3 originPos;
    Vector3 hookPos;
    public float hookFowardSpeed;
    public float hookBackwardsSpeed;
    public float hookMinDistance;//Distance the hook stops when bringing the enemy
    public float cdMaxTime;
    float cdTime;
    bool hookReady;
    Vector3 reelingVel;

    bool doingHook;
    bool reelingStarted;
    bool enemyHooked;
    bool objectHooked;// item 
    PlayerMovement enemy;

    private void Awake()
    {
        myPlayerMov = GetComponent<PlayerMovement>();
        if (myPlayerMov == null)
        {
            Debug.LogError("PlayerMovement is not on the object where the hook script is. This object is " + gameObject.name);
        }
        hookReady = true;
    }

    private void Update()
    {
        if (doingHook)
        {

            UpdateDistance();
            if (currentDistance >= hookMaxDistance)
            {
                StartReeling();
            }
            Debug.DrawLine(originPos, hookPos, Color.red);
            RaycastHit hit;
            if (Physics.Linecast(originPos, hookPos, out hit, collisionMask, QueryTriggerInteraction.Ignore))
            {
                StopHook();
            }
            if (!reelingStarted)//TIRANDO HOOK
            {
                MoveHook(reelingVel);
            }
            else//RECOGIENDO EL HOOK
            {
                if (currentDistance <= hookMinDistance)
                {
                    FinishHook();
                }
                else
                {
                    //Calculate dir to owner
                    Vector3 reelingDir = (originPos - hookPos).normalized;
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
            }
        }
        else if (!hookReady)
        {
            cdTime += Time.deltaTime;
            if (cdTime >= cdMaxTime)
            {
                hookReady = true;
            }
        }
    }

    void MoveHook(Vector3 vel)
    {
        Vector3 finalVel = (vel * Time.deltaTime);
        currentHook.transform.Translate(finalVel, Space.World);
    }

    public LayerMask collisionMask;
    public void StartHook()
    {
        if (!doingHook && hookReady)
        {
            //VARIABLES
            hookReady = false;
            doingHook = true;
            enemy = null;
            hookedObject = null;
            reelingStarted = false;
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
            for (int i = 0; i < currentHook.transform.childCount; i++)
            {
                currentHook.transform.GetChild(i).GetComponent<Hitbox>().KonoAwake(myPlayerMov, this);
            }
        }
    }

    public void StartReeling()
    {
        if (!reelingStarted)
        {
            print("START REELING");
            reelingStarted = true;
        }
    }

    public void FinishHook()
    {
        if (doingHook)
        {
            print("FINISH HOOK");
            doingHook = false;
            myPlayerMov.StopHooking();
            StopHook();
            reelingStarted = false;
            StoringManager.instance.StoreObject(currentHook.transform);
            currentHook = null;
            cdTime = 0;
        }
    }

    public void HookPlayer(PlayerMovement player)
    {
        if (canHookEnemy)
        {
            print("HOOK PLAYER");
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
        if (canHookEnemy)
        {
            objectHooked = true;
            hookedObject = item;
            hookedObject.transform.SetParent(currentHook.transform);
            hookedObject.transform.localPosition = Vector3.zero;
            StartReeling();

        }
    }

    public void StopHook()
    {
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
                objectHooked = false;
                Flag flag = hookedObject.GetComponent<Flag>();
                if (flag != null)
                {
                    myPlayerMov.PickFlag(hookedObject.gameObject);
                }
                hookedObject = null;
            }
            currentHook.transform.SetParent(StoringManager.instance.transform);
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
    public bool canHookEnemy
    {
        get
        {
            return (!reelingStarted && !enemyHooked && !objectHooked);
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
