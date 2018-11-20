using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {

    public Image crosshair;
    public Image crosshairReduced;

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
}
