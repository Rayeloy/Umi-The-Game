using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour {

    PlayerMovement myPlayerMov;

    GameObject currentHook;
    public GameObject hookPrefab;
    public Vector3 hookLocalOrigin;
    public float hookMaxDistance;
    float currentDistance;
    Vector3 originPos;
    Vector3 hookPos;
    public float hookFowardSpeed;
    public float hookBackwardsSpeed;
    public float hookMinDistance;//Distance the hook stops when bringing the enemy

    bool doingHook;
    bool reelingStarted;
    bool enemyHooked;
    PlayerMovement enemy;

    private void Awake()
    {
        myPlayerMov = GetComponent<PlayerMovement>();
        if (myPlayerMov == null)
        {
            Debug.LogError("PlayerMovement is not on the object where the hook script is. This object is " + gameObject.name);
        }
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

            if (!reelingStarted)//TIRANDO HOOK
            {

            }
            else//RECOGIENDO EL HOOK
            {
                //Calculate dir to owner
                //move with speed to owner
                //check for collisions
                if (currentDistance <= hookMinDistance)
                {
                    StopHook();
                }
            }
        }



    }

    public void StartHook()
    {
        if (!doingHook)
        {
            doingHook = true;
            myPlayerMov.StartHooking();
            currentDistance = 0;
            originPos = transform.TransformPoint(hookLocalOrigin);
            hookPos = originPos;
            //Calculate trayectory
            //Instantiate hook
            //Throw hook in direction with speed
            //check for collisions on hook hitbox
        }
    }

    void StartReeling()
    {
        if (!reelingStarted)
        {
            reelingStarted = true;
        }
    }

    void StopHook()
    {
        if (doingHook)
        {
            doingHook = false;
            myPlayerMov.StopHooking();

            reelingStarted = false;
            enemyHooked = false;
            Destroy(currentHook);
            if (enemyHooked)
            {
                enemyHooked = false;
                enemy.StopHooked();
                enemy = null;
            }

        }
    }

    public void HookPlayer(PlayerMovement player)
    {
        if (!enemyHooked)
        {
            enemyHooked = true;
            enemy = player;
            enemy.StartHooked();
        }

    }


    void UpdateDistance()
    {
        originPos = transform.TransformPoint(hookLocalOrigin);
        hookPos = currentHook.transform.position;
        currentDistance = (hookPos - originPos).magnitude;
    }
}
