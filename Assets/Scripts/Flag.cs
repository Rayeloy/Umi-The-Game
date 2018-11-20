using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Flag : MonoBehaviour {
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
    public float maxTimeLocked;
    float timeLocked = 0;
    bool locked = false;
    public float maxTimeToRespawn;
    float timeToRespawn = 0;
    bool respawning = false;
    [Tooltip("NOT USED YET")]
    public float maxTimeToPick;
    float timeToPick = 0;


	void Start () {
        respawnPos = transform.position;
        currentOwner = null;
        beingHooked = false;
        playerHooking = null;

    }
    private void Update()
    {
        ProcessLocked();
        ProcessRespawn();
    }

    public void StartRespawn()
    {
        timeToRespawn = 0;
        respawning = true;
        StoringManager.instance.StoreObject(transform);
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
        respawning = false;
        GameController.instance.RespawnFlag(respawnPos);
    }
    /// <summary>
    /// Allows player to steal flag from another player that already has it from a hit. It also puts it on his back.
    /// </summary>
    /// <param name="player"></param>
    public void StealFlag(PlayerMovement player)//steal from somone who has it already
    {
        print("StealFlag");
        if (!player.haveFlag)
        {
            if (!beingHooked)
            {
                if (currentOwner != null)
                {
                    currentOwner.GetComponent<PlayerMovement>().LoseFlag();
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
            Debug.LogError("Error: Can't steal a flag because the player "+player.name+" already has a flag");
        }
    }

    public void PickupFlag(PlayerMovement player)//from floor
    {
        print("PickupFlag");
        if (!player.haveFlag && currentOwner==null && !beingHooked && !locked)
        {
            ClaimFlag(player);
            PutFlagOnBack(player);

        }
        else
        {
            Debug.LogWarning("Error: Can't pick up a flag because the player " + (player.name).ToString() +
                " already has a flag("+ (player.haveFlag).ToString() + ") || the flag has an owner("+ (currentOwner != null).ToString() + 
                ") || the flag is being hooked("+ beingHooked + ") || the flag is locked("+locked+").");
        }
    }

    public bool HookFlag(PlayerMovement player)
    {
        print("HookFlag");
        if (!player.haveFlag && !locked)
        {
            if(!(currentOwner != null && player.team == currentOwner.GetComponent<PlayerMovement>().team))
            {
                if (currentOwner == null)
                {
                    print("NO OWNER");
                }
                else
                {
                    print("current owner = " + currentOwner + "; owner team= " + currentOwner.GetComponent<PlayerMovement>().team + "; hooker team = " + player.team);
                }
                if (beingHooked)
                {
                    playerHooking.GetComponent<Hook>().StopHook();
                    playerHooking = player.transform;
                }
                else
                {
                    if (currentOwner != null)
                    {
                        currentOwner.GetComponent<PlayerMovement>().LoseFlag();
                        currentOwner = null;
                    }
                    print("START BEING HOOKED");
                    beingHooked = true;
                    playerHooking = player.transform;
                }
                return true;
            }
        }
        else
        {
            Debug.LogWarning("Error: Can't hook flag because player has already a flag ("+player.haveFlag+") || flag is locked ("+locked+").");
        }
        return false;
    }

    public void DropFlag()
    {
        print("DropFlag");
        if (beingHooked)
        {
            playerHooking.GetComponent<Hook>().StopHook();
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
                currentOwner.GetComponent<PlayerMovement>().LoseFlag();
                currentOwner = null;
            }
        }
        transform.SetParent(StoringManager.instance.transform);
        StartLocked();
    }

    public void SetAway(bool instant=false)
    {
        if (currentOwner != null)//solo se puede poner away si se pierde en el agua o se marca un punto, ambos casos con un owner
        {
            currentOwner.GetComponent<PlayerMovement>().LoseFlag();
            currentOwner = null;
            if (!instant)
            {
                StartRespawn();
            }
            else
            {
                FinishRespawn();
            }

        }

    }

    public void StopBeingHooked()
    {
        print("Stop being Hooked");
        beingHooked = false;
        playerHooking = null;
    }

    public void ClaimFlag(PlayerMovement player)
    {
        currentOwner = player.transform;
        player.haveFlag = true;
        player.flag = this;
        if (GameController.instance.gameMode == GameController.GameMode.Tutorial)
        {
            SceneManager.LoadScene("Menus");
        }

    }

    public void PutFlagOnBack(PlayerMovement player) 
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
}
