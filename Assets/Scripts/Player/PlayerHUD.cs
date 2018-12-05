using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {

    public Image crosshair;
    public Image crosshairReduced;
    public Image Hook;
    public Image Boost;

    private void Start()
    {
        crosshair.enabled = false;
        crosshairReduced.enabled = false;
    }

    public void StartAim()
    {
        crosshair.enabled = true;
        crosshairReduced.enabled = false;
    }
    public void StartThrowHook()
    {
        crosshair.enabled = false;
        crosshairReduced.enabled = true;
    }

    public void StopThrowHook()
    {
        crosshair.enabled = true;
        crosshairReduced.enabled = false;
    }

    public void StopAim()
    {
        crosshair.enabled = false;
        crosshairReduced.enabled = false;
    }

    public void setHookUI (float f)
    {
        Hook.fillAmount = Mathf.Clamp01( f );
    }

    public void setBoostUI (float f)
    {
        Boost.fillAmount = Mathf.Clamp01( f );
    }
}
