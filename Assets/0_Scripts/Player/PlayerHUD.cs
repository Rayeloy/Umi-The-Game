using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {

    [Header("Referencias")]
    public GameControllerBase gC;

    public Image crosshair;
    public Image crosshairReduced;
    public Image Hook;
    public Image Boost;
    public Slider flagSlider;
    Vector3 blueFlagHomePos;
    Vector3 redFlagHomePos;
    Transform flag;
    Vector3 flagPos;

    private void Start()
    {
        crosshair.enabled = false;
        crosshairReduced.enabled = false;
        if (gC.gameMode == GameMode.CaptureTheFlag)
        {
            SetupFlagSlider();
        }
    }

    private void Update()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)
        {
            UpdateFlagSlider();
        }
    }

    void SetupFlagSlider()
    {
        flag = (gC as GameController_FlagMode).flags[0].transform;
        blueFlagHomePos = (gC as GameController_FlagMode).blueTeamFlagHome.position;
        redFlagHomePos = (gC as GameController_FlagMode).redTeamFlagHome.position;
        blueFlagHomePos.y = 0;
        redFlagHomePos.y = 0;
    }

    void UpdateFlagSlider()
    {
        flagPos = flag.position;
        flagPos.y = 0;
        Vector3 blueToFlagDir = blueFlagHomePos - flagPos;
        float distFromBlue = blueToFlagDir.magnitude;
        Vector3 union = (blueFlagHomePos - redFlagHomePos).normalized;
        float angle = Vector3.Angle(blueToFlagDir.normalized, union);
        //cos(angle) = finalDist/distFromBlue
        float currentDist = Mathf.Cos(angle * Mathf.Deg2Rad) * distFromBlue;
        float totalDist = (blueFlagHomePos - redFlagHomePos).magnitude;
        float progress = Mathf.Clamp01(currentDist / totalDist);
        flagSlider.value = progress;
        /*
        float distFromRed = (redFlagHomePos - flagPos).magnitude;
        float diff = distFromBlue - distFromRed;
        float coef = diff + totalDist / 2;
        */
        //print("totalDist = " + totalDist + "; distFromBlue = " + distFromBlue + "; distFromRed = " + distFromRed + "; diff = " + diff + "; coef = " + coef + "; progress = " + progress);
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
