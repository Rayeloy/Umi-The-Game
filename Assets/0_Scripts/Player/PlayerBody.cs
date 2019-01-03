using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour {

    public PlayerMovement myPlayerMov;

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
        }
    }

    #endregion
}
