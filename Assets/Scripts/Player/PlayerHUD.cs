using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {

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
        flag = GameController.instance.flags[0].transform;
        blueFlagHomePos = GameController.instance.blueTeamFlagHome.position;
        redFlagHomePos = GameController.instance.redTeamFlagHome.position;
        blueFlagHomePos.y = 0;
        redFlagHomePos.y = 0;
    }

    private void Update()
    {
        UpdateFlagSlider();
    }

    void UpdateFlagSlider()
    {
        flagPos = flag.position;
        flagPos.y = 0;
        float distFromBlue = (blueFlagHomePos-flagPos).magnitude;
        float distFromRed = (redFlagHomePos - flagPos).magnitude;
        float diff = distFromBlue - distFromRed;
        float totalDist = (blueFlagHomePos - redFlagHomePos).magnitude;
        float coef = diff + totalDist / 2;
        float progress = Mathf.Clamp01(coef / totalDist);
        flagSlider.value = progress;
        print("totalDist = " + totalDist + "; distFromBlue = " + distFromBlue + "; distFromRed = " + distFromRed + "; diff = " + diff + "; coef = " + coef + "; progress = " + progress);
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
