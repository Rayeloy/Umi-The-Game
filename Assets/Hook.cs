using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour {

    public Transform hookRopeEnd;
    public HitboxHookBig myHitboxBig;
    public HitboxHookSmall myHitboxSmall;

    public void KonoAwake(PlayerMovement playerMov, PlayerHook playerHook)
    {
        myHitboxBig.KonoAwake(playerMov, playerHook);
        myHitboxSmall.KonoAwake(playerMov, playerHook);
    }
}
