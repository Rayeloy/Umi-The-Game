using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxHookSmall : MonoBehaviour
{
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
            print("SMALL HOOK HB");
            if (col.tag == "Stage")
            {
                myHook.StopHook();
            }
        }
    }
}
