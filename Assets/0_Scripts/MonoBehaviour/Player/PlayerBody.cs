using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour {

    public PlayerMovement myPlayerMov;
    PlayerWeapons myPlayerWeapons;

    public void KonoAwake()
    {
        myPlayerWeapons = myPlayerMov.myPlayerWeap;
    }

    #region  TRIGGER COLLISIONS ---------------------------------------------
    private void OnTriggerStay(Collider col)
    {
        switch (col.tag)
        {
            case "Water":
                float waterSurface = col.GetComponent<Collider>().bounds.max.y;
                if (transform.position.y <= waterSurface)
                {
                    myPlayerMov.EnterWater();
                }
                else
                {
                    myPlayerMov.ExitWater();
                }
                break;
            case "Flag":
                col.GetComponent<Flag>().PickupFlag(myPlayerMov);
                break;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        switch (col.tag)
        {
            case "KillTrigger":
                myPlayerMov.Die();
                break;
            case "FlagHome":
                //print("I'm " + name + " and I touched a respawn");
                myPlayerMov.CheckScorePoint(col.GetComponent<FlagHome>());
                break;
            case "PickUp":
                myPlayerMov.myPlayerPickups.CogerPickup(col.gameObject);
                break;
            case "WeaponPickup":
                myPlayerWeapons.AddWeaponNearby(col.GetComponent<Weapon>());
                break;
            case "Player":
                Debug.LogWarning("Hitting player! checking team");
                PlayerMovement otherPlayer = col.transform.GetComponentInParent<PlayerMovement>();
                if(otherPlayer!=null && myPlayerMov.team!= otherPlayer.team)
                {
                    if (myPlayerMov.myPlayerHook.enemyHooked && myPlayerMov.myPlayerHook.enemy == otherPlayer)
                    {
                        Debug.LogError("Player hooked stopped due to colliding with the player hooking him.");
                        myPlayerMov.myPlayerHook.FinishHook();
                    }
                    else
                    {
                        Debug.LogError("Nope.");
                    }

                }
                break;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        switch (col.tag)
        {
            case "WeaponPickup":
                myPlayerWeapons.RemoveWeaponNearby(col.GetComponent<Weapon>());
                break;
        }
    }

    #endregion
}
