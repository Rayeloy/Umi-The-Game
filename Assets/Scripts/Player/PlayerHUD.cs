using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {

    public Image crosshair;

    private void Start()
    {
        crosshair.enabled = false;
    }

    public void StartAim()
    {
        crosshair.enabled = true;
    }

    public void StopAim()
    {
        crosshair.enabled = false;
    }
}
