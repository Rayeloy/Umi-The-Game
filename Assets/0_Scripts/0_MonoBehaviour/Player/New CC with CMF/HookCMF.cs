using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookCMF : MonoBehaviour
{

    public Transform hookRopeEnd;
    public HitboxHookBigCMF myHitboxBig;
    public HitboxHookSmallCMF myHitboxSmall;
    LineRenderer myLineRenderer;

    public void KonoAwake(PlayerMovementCMF playerMov, PlayerHookCMF playerHook)
    {
        if (myHitboxBig.isActiveAndEnabled)
        {
            myHitboxBig.KonoAwake(playerMov, playerHook);
        }
        if (myHitboxSmall.isActiveAndEnabled)
        {
            myHitboxSmall.KonoAwake(playerMov, playerHook);
        }
        myLineRenderer = GetComponent<LineRenderer>();
    }
    public void UpdateRopeLine(Vector3 pos1, Vector3 pos2)
    {
        myLineRenderer.SetPosition(0, pos1);
        myLineRenderer.SetPosition(1, pos2);
    }
}
