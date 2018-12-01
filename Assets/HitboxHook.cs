using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxHook : MonoBehaviour {
    PlayerMovement myPlayerMov;
    Hook myHook;

    public void KonoAwake(PlayerMovement playerMov, Hook hook)
    {
        myPlayerMov = playerMov;
        myHook = hook;
    }
    private void OnTriggerEnter(Collider col)
    {
        
        if (col.gameObject != myPlayerMov.gameObject)
        {
            if (tag == "HookBigHB")
            {
                if (myHook.canHookSomething)
                {
                    switch (col.tag)
                    {
                        case "Flag":
                            myHook.HookObject(col.transform);
                            break;
                        case "Player":
                            PlayerMovement otherPlayer = col.GetComponent<PlayerBody>().myPlayerMov;
                            if (myPlayerMov.team != otherPlayer.team)// IF ENEMY
                            {
                                if (!otherPlayer.inWater)// OUTSIDE WATER
                                {
                                    myHook.HookPlayer(otherPlayer);
                                }
                            }
                            else
                            {
                                if (otherPlayer.inWater)//IF ALLY IN WATER
                                {
                                    myHook.HookPlayer(otherPlayer);
                                }
                            }
                            break;
                        case "Stage":
                            StageScript stage = col.GetComponent<StageScript>();
                            if (stage != null)
                            {
                                if (stage.hookable)
                                {
                                    myHook.StartGrappling();
                                }
                            }
                            break;
                    }
                }
            }
            else if (tag == "HookSmallHB")
            {
                if (col.tag == "Stage")
                {
                    myHook.StopHook();
                }
            }
        }
    }  
}
