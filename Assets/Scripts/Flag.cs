using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {
    [HideInInspector]
    public Vector3 respawnPos;
    [Tooltip("Not used yet. Can be used to differentiate flags")]
    public int flagNumber;
    [HideInInspector]
    public Transform currentOwner;
    [HideInInspector]
    public bool beingHooked;
    public Transform playerHooking;
	void Start () {
        respawnPos = transform.position;
        currentOwner = null;
        beingHooked = false;
        playerHooking = null;

    }
    /// <summary>
    /// Allows player to steal flag from another player that already has it from a hit. It also puts it on his back.
    /// </summary>
    /// <param name="player"></param>
    public void StealFlag(PlayerMovement player)//steal from somone who has it already
    {
        if (!player.haveFlag)
        {
            if (!beingHooked)
            {
                if (currentOwner != null)
                {
                    currentOwner.GetComponent<PlayerMovement>().LoseFlag();
                    currentOwner = player.transform;
                    PutFlagOnBack(player);
                }
                else
                {
                    Debug.LogWarning("Error: " + player.name + " can't steal a flag that is not owned by anyone. Switching to 'PickupFlag'");
                    PickupFlag(player);
                }
            }
        }
        else
        {
            Debug.LogError("Error: Can't steal a flag because the player "+player.name+" already has a flag");
        }
    }

    public void PickupFlag(PlayerMovement player)//from floor
    {
        if (!player.haveFlag && currentOwner==null && !beingHooked)
        {
            ClaimFlag(player);
            PutFlagOnBack(player);

        }
        else
        {
            Debug.LogWarning("Error: Can't pick up a flag because the player " + (player.name).ToString() +
                " already has a flag("+ (player.haveFlag).ToString() + ") || the flag has an owner("+ (currentOwner != null).ToString() + ") || the flag is being hooked("+ beingHooked + ").");
        }
    }

    public void HookFlag(PlayerMovement player)
    {
        if (!player.haveFlag)
        {
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
                beingHooked = true;
                playerHooking = player.transform;
            }
        }
    }

    public void DropFlag()
    {
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
    }

    public void StopBeingHooked()
    {
        beingHooked = false;
        playerHooking = null;
    }

    public void ClaimFlag(PlayerMovement player)
    {
        player.haveFlag = true;
        player.flag = this;
    }

    public void PutFlagOnBack(PlayerMovement player) 
    {
        player.PutOnFlag(this);
    }
}
