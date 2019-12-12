using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxHookSmallCMF : MonoBehaviour
{
    PlayerMovementCMF myPlayerMov;
    PlayerHookCMF myHook;

    public void KonoAwake(PlayerMovementCMF playerMov, PlayerHookCMF hook)
    {
        myPlayerMov = playerMov;
        myHook = hook;
    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject != myPlayerMov.gameObject)
        {
            if (col.tag == "Stage")
            {
                myHook.StopHook();
            }
        }
    }
}
