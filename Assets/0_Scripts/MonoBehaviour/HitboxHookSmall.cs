using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxHookSmall : MonoBehaviour
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
        if (col.gameObject != myPlayerMov.gameObject)
        {
            if (col.tag == "Stage")
            {
                myHook.StopHook();
            }
        }
    }
}
