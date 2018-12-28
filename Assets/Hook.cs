using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour {

    public Transform hookRopeEnd;
    public HitboxHookBig myHitboxBig;
    public HitboxHookSmall myHitboxSmall;
    LineRenderer myLineRenderer;

    /*public Hook()
    {

    }*/
    
    public void KonoAwake(PlayerMovement playerMov, PlayerHook playerHook)
    {
        myHitboxBig.KonoAwake(playerMov, playerHook);
        myHitboxSmall.KonoAwake(playerMov, playerHook);
        myLineRenderer = GetComponent<LineRenderer>();
    }
    public void UpdateRopeLine(Vector3 pos1, Vector3 pos2)
    {
        myLineRenderer.SetPosition(0, pos1);
        myLineRenderer.SetPosition(1, pos2);
    }
}
