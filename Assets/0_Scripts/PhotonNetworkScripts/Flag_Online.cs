﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// 
/// JUAN
/// 
///Networked Scene Objects
///
///It is perfectly fine to place PhotonViews on objects in a scene.They will be controlled by the Master Client
///by default and can be useful to have a "neutral" object to send room-related Remote Procedure Calls (RPCs).
///Important: When you load a scene with networked objects before being in a room, some PhotonView values are not
///useful yet.For example: You can't check isMine in Awake() when you're not in a room!
///
/// </summary>

public class Flag_Online : MonoBehaviour
{
    [Header("Referencias")]
    public Transform flagCamera;
    public Transform flagCameraLocalParent;
    [HideInInspector]
    public Vector3 respawnPos;
    [Tooltip("Not used yet. Can be used to differentiate flags")]
    public int flagNumber;
    [HideInInspector]
    public Transform currentOwner;
    [HideInInspector]
    public bool beingHooked;
    [HideInInspector]
    public Transform playerHooking;

    [Header("Flag Times")]
    public float maxTimeLocked;
    float timeLocked = 0;
    bool locked = false;
    public float maxTimeToRespawnFall;
    public float maxTimeToRespawnGoal;
    float maxTimeToRespawn;
    float timeToRespawn = 0;
    bool respawning = false;
    [Tooltip("NOT USED YET")]
    public float maxTimeToPick;
    float timeToPick = 0;

    //FLAG FALLING WHEN DROPPED ("FLAG PHYSICS")
    [Header("Flag Physics")]
    [Tooltip("Speed at which the flag falls when dropped by a player.")]
    public float fallSpeed;
    public LayerMask collisionMask;
    [Tooltip("Vertical distance you want the orca to be 'levitating' over the floor")]
    public float heightFromFloor;
    public bool grounded = false;
    Vector3 rayOrigin;
    float rayLength;
    Collider myCol;
    float skinWidth = 0.1f;
    public float timeToDespawnInWater;

    [Header("Idle Animation Param")]
    [Tooltip("Distance from the middle (default) position to the max height and min height. The total distance of the animation will be the double of this value")]
    public float idleAnimVertDist;
    [Tooltip("Not in use yet")]
    public float idleAnimHorDist;
    [Tooltip("How much seconds per half animation cycle (i.e. from bottom to top height). The shorter the time, the faster the animation.")]
    public float idleAnimFrequency;
    float idleAnimTime = 0;
    float maxHeight, minHeight;
    //Vector3 bodyOriginalLocalPos;
    bool idleAnimStarted = false;


    private void Awake()
    {
        myCol = GetComponent<SphereCollider>();
        grounded = false;
        skinWidth = myCol.bounds.extents.y;
        //bodyOriginalLocalPos= new Vector3(-0.016f, -0.303f, 0);
    }
    void Start()
    {
        respawnPos = transform.position;
        currentOwner = null;
        beingHooked = false;
        playerHooking = null;

    }
    private void Update()
    {
        ProcessLocked();
        ProcessRespawn();

        if (currentOwner == null && !beingHooked && !respawning)
        {
            if (!grounded)//fall
            {
                Fall();
                UpdateRayParameters();
                Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.yellow);
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Collide))
                {
                    if (hit.transform.tag != "Water")
                    {
                        grounded = true;
                        float distToDesiredHeight = heightFromFloor - hit.distance;
                        transform.position = new Vector3(transform.position.x, transform.position.y + distToDesiredHeight, transform.position.z);
                        StartIdleAnimation();
                    }
                    else
                    {
                        if (hit.distance <= myCol.bounds.extents.y)
                        {
                            SetAway(false);
                        }
                    }
                }
            }
            else//animation
            {
                ProcessIdleAnimation();
            }
        }
    }

    void UpdateRayParameters()
    {
        Bounds bounds = myCol.bounds;
        bounds.Expand(skinWidth * -2);
        rayOrigin = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        rayLength = heightFromFloor + skinWidth;
    }

    void Fall()
    {
        Vector3 vel = Vector3.down * fallSpeed * Time.deltaTime;
        transform.Translate(vel, Space.World);
    }

    void StartIdleAnimation()
    {
        if (!idleAnimStarted)
        {
            idleAnimStarted = true;
            idleAnimTime = 0;
            maxHeight = transform.position.y + idleAnimVertDist;
            minHeight = transform.position.y - idleAnimVertDist;
            progress = 0.5f;
            idleAnimUp = true;
        }
    }

    bool idleAnimUp = true;
    float progress = 0;
    void ProcessIdleAnimation()
    {
        if (idleAnimStarted)
        {
            idleAnimTime += Time.deltaTime;
            progress = idleAnimTime / idleAnimFrequency;
            progress = Mathf.Clamp01(progress);
            float newY = 0;
            if (idleAnimUp)
            {
                newY = EasingFunction.EaseInOutQuad(minHeight, maxHeight, progress);
            }
            else
            {
                newY = EasingFunction.EaseInOutQuad(maxHeight, minHeight, progress);
            }
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            if (idleAnimTime >= idleAnimFrequency)
            {
                idleAnimTime = 0;
                idleAnimUp = !idleAnimUp;
            }
        }
    }

    void StopIdleAnimation()
    {
        idleAnimStarted = false;
    }
    /// <summary>
    /// Sobrecarga para introducir cantidad al gusto de tiempo de respawn, no se usa normalmente.
    /// </summary>
    /// <param name="respawnTime"></param>
    public void StartRespawn(float respawnTime)
    {
        StartRespawnIssho(respawnTime);
    }

    public void StartRespawn(bool respawnFromGoal = false)
    {
        float respawnTime = respawnFromGoal ? maxTimeToRespawnGoal : maxTimeToRespawnFall;
        StartRespawnIssho(respawnTime);
    }


    void StartRespawnIssho(float _maxTimeToRespawn)
    {
        if (!respawning)
        {
            maxTimeToRespawn = _maxTimeToRespawn;
            timeToRespawn = 0;
            respawning = true;
            StoringManager.instance.StoreObject(transform);
            flagCamera.SetParent(GameController.instance.centerCameraParent);
            flagCamera.localPosition = Vector3.zero;
            flagCamera.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("Error: can't start a respawn process for the flag since there is one already happening.");
        }
    }


    void ProcessRespawn()
    {
        if (respawning)
        {
            timeToRespawn += Time.deltaTime;
            if (timeToRespawn >= maxTimeToRespawn)
            {
                FinishRespawn();
            }
        }
    }

    public void FinishRespawn()
    {
        ResetFlag();
    }

    void SpawnFakeFlag()
    {
        print("SPAWN FAKE FLAG");
        Transform fakeFlag = StoringManager.instance.Spawn(StoringManager.instance.fakeFlagPrefab, StoringManager.instance.transform, transform.position, transform.rotation);
        fakeFlag.GetComponent<FakeFlag>().KonoAwake(timeToDespawnInWater, fallSpeed);

    }

    /// <summary>
    /// Allows player to steal flag from another player that already has it from a hit. It also puts it on his back.
    /// </summary>
    /// <param name="player"></param>
    public void StealFlag(PlayerMovement_Online player)//steal from somone who has it already
    {
        print("StealFlag");
        if (!player.haveFlag)
        {
            if (!beingHooked)
            {
                if (currentOwner != null)
                {
                    currentOwner.GetComponent<PlayerMovement_Online>().LoseFlag();
                    ClaimFlag(player);
                    PutFlagOnBack(player);
                }
                else
                {
                    Debug.LogWarning("Error: " + player.name + " can't steal a flag that is not owned by anyone. Switching to 'PickupFlag'");
                    PickupFlag(player);
                }
            }
            else
            {
                Debug.LogError("Error: Can't steal a flag that is being hooked.");
            }
        }
        else
        {
            Debug.LogError("Error: Can't steal a flag because the player " + player.name + " already has a flag");
        }
    }

    public void PickupFlag(PlayerMovement_Online player)//from floor
    {
        print("PickupFlag");
        if (!player.haveFlag && currentOwner == null && !beingHooked && !locked)
        {
            StopIdleAnimation();
            ClaimFlag(player);
            PutFlagOnBack(player);

        }
        else
        {
            Debug.LogWarning("Error: Can't pick up a flag because the player " + (player.name).ToString() +
                " already has a flag(" + (player.haveFlag).ToString() + ") || the flag has an owner(" + (currentOwner != null).ToString() +
                ") || the flag is being hooked(" + beingHooked + ") || the flag is locked(" + locked + ").");
        }
    }

    public bool HookFlag(PlayerMovement_Online player)
    {
        print("HookFlag");
        if (!player.haveFlag && !locked)
        {
            if (!(currentOwner != null && player.team == currentOwner.GetComponent<PlayerMovement_Online>().team))//si no hemos enganchado la bandera de un compañero de equipo
            {
                if (beingHooked)
                {
                    playerHooking.GetComponent<PlayerHook_Online>().StopHook();
                    playerHooking = player.transform;
                }
                else
                {
                    if (currentOwner != null)
                    {
                        currentOwner.GetComponent<PlayerMovement_Online>().LoseFlag();
                        currentOwner = null;
                    }
                    print("START BEING HOOKED");
                    beingHooked = true;
                    playerHooking = player.transform;
                    StopIdleAnimation();
                }
                return true;
            }
        }
        else
        {
            Debug.LogWarning("Error: Can't hook flag because player has already a flag (" + player.haveFlag + ") || flag is locked (" + locked + ").");
        }
        return false;
    }

    public void DropFlag()
    {
        print("DropFlag");
        if (beingHooked)
        {
            playerHooking.GetComponent<PlayerHook_Online>().StopHook();
            playerHooking = null;
            beingHooked = false;
            if (currentOwner != null)
            {
                Debug.LogError("Error: Flag can't be hooked and owned at the same time.");
            }
        }
        else
        {
            if (currentOwner != null)
            {
                currentOwner.GetComponent<PlayerMovement_Online>().LoseFlag();
                currentOwner = null;
            }
        }
        grounded = false;
        transform.SetParent(StoringManager.instance.transform);
        StartLocked();
    }

    public void SetAway(bool respawnFromGoal, bool instant = false)
    {
        //solo se puede poner away si se pierde en el agua o se marca un punto
        if (currentOwner != null)
        {
            currentOwner.GetComponent<PlayerMovement_Online>().LoseFlag();
            currentOwner = null;
        }

        if (!respawnFromGoal)
        {
            print("RESPAWN FROM FALLING TO WATER");
            grounded = true;
            StopIdleAnimation();
            SpawnFakeFlag();
        }

        if (!instant)
        {
            StartRespawn(respawnFromGoal);
        }
        else
        {
            FinishRespawn();
        }

    }

    public void StopBeingHooked()
    {
        print("Stop being Hooked");
        beingHooked = false;
        playerHooking = null;
    }

    public static int flagsCaptured = 0;
    public void ClaimFlag(PlayerMovement_Online player)
    {
        currentOwner = player.transform;
        player.haveFlag = true;
        player.flag = this;
        if (GameController.instance.gameMode == GameController.GameMode.Tutorial)
        {
            flagsCaptured++;
            currentOwner.gameObject.GetComponent<PlayerMovement_Online>().noInput = true;
            if (flagsCaptured >= GameController.instance.playerNum)
                GameController.instance.GoBackToMenu();
        }

    }

    public void PutFlagOnBack(PlayerMovement_Online player)
    {
        player.PutOnFlag(this);
    }

    void StartLocked()
    {
        locked = true;
        timeLocked = 0;
    }

    void ProcessLocked()
    {
        if (locked)
        {
            timeLocked += Time.deltaTime;
            if (timeLocked >= maxTimeLocked)
            {
                StopLocked();
            }
        }
    }

    void StopLocked()
    {
        locked = false;
    }

    public void ResetFlag()
    {
        if (respawning)
        {
            StoringManager.instance.UnstoreObject(transform);
        }
        respawning = false;
        currentOwner = null;
        beingHooked = false;
        playerHooking = null;
        locked = false;

        transform.position = respawnPos;
        transform.rotation = Quaternion.identity;
        flagCamera.SetParent(flagCameraLocalParent);
        flagCamera.localPosition = Vector3.zero;
        flagCamera.localRotation = Quaternion.identity;

        grounded = false;
    }



    //private void OnTriggerEnter(Collider col)
    //{
    //    if (col.tag == "Water")
    //    {
    //    }

    //}
}
