using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxHookBig : MonoBehaviour
{
    PlayerMovement myPlayerMov;
    PlayerHook myHook;

    public void KonoAwake(PlayerMovement playerMov, PlayerHook hook)
    {
        myPlayerMov = playerMov;
        myHook = hook;
    }
    private void OnTriggerEnter(Collider col)
    {
        //print("Hook: Collision with " + col.name);
        if (col.gameObject != myPlayerMov.gameObject)
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
                    /*case "Stage":
                        StageScript stage = col.GetComponent<StageScript>();
                        if (stage != null)
                        {
                            if (stage.hookable)
                            {
                                myHook.StartGrappling();
                            }
                        }
                        break;*/
                    case "HookPoint":
                        //print("Hook: Collision with HookPoint 1");
                        CollideWithHookPoint(col);
                        break;
                }
            }
            else if (myHook.grappleSt == GrappleState.throwing)
            {
                CollideWithHookPoint(col);
            }
        }
    }  

    void CollideWithHookPoint(Collider col)
    {
        if (col.name.Contains("SmallTrigger"))
        {
            //print("Hook: Collision with HookPoint 2");
            HookPoint hookPoint = col.transform.parent.GetComponent<HookPoint>();
            if (hookPoint != null)
            {
                RaycastHit hit;
                Vector3 rayOrigin = myHook.currentHook.transform.position;
                Vector3 rayEnd = col.transform.parent.position;
                LayerMask lm = LayerMask.GetMask("Stage");
                Debug.DrawLine(rayOrigin, rayEnd, Color.yellow, 3);
                //Debug.Log("hook pos = " + rayOrigin.ToString("F4"));

                if (Physics.Linecast(rayOrigin, rayEnd, out hit, lm, QueryTriggerInteraction.Collide))
                {
                    Vector3 hookPos = hookPoint.GetHookPoint(hit.point);//col.ClosestPointOnBounds(myHook.transform.position));
                    myHook.StartGrappling(hookPos, col.transform.parent.GetComponent<HookPoint>());
                }
                else
                {
                    //Debug.Log("Error: Can't find a collision point between the hook and the hookPoint. " +
                    //    "There must be one since we already collided and we are just trying to find a collisions point");
                    Vector3 hookPos = hookPoint.GetHookPoint(col.ClosestPointOnBounds(myHook.transform.position));
                    myHook.StartGrappling(hookPos, col.transform.parent.GetComponent<HookPoint>());
                }
            }
        }
    }
}
