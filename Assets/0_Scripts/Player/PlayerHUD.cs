using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class PlayerHUD : MonoBehaviourPunCallbacks {

    [Header("Referencias")]
    public GameControllerBase gC;

    public RectTransform contador;
    public RectTransform powerUpPanel;
    public Image crosshair;
    public Image crosshairReduced;
    public Image Hook;
    public Image Boost;
    public Slider flagSlider;
    public TMPro.TextMeshProUGUI redTeamScoreText;
    public TMPro.TextMeshProUGUI blueTeamScoreText;
    public TMPro.TextMeshProUGUI timeText;
    public Text attackNameText;

    //pickup weapon
    public GameObject Interaction_Message;
    public Text changeYourWeaponToText;
    //public Text pressText;
    public Image interactButtonImage;

    Vector3 blueFlagHomePos;
    Vector3 redFlagHomePos;
    Transform flag;
    Vector3 flagPos;

    public void KonoStart()
    {
        Interaction_Message.SetActive(false);
        crosshair.enabled = false;
        crosshairReduced.enabled = false;
        if (gC.gameMode == GameMode.CaptureTheFlag && !PhotonNetwork.IsConnected)
        {
            SetupFlagSlider();
        }
    }

    private void Update()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag && !PhotonNetwork.IsConnected)
        {
            UpdateFlagSlider();
        }
    }

    public void SetPickupWeaponTextMessage(WeaponData _weap)
    {
        //if (!changeYourWeaponToText.enabled) changeYourWeaponToText.enabled = true;
        //if (!pressText.enabled) pressText.enabled = true;
        //if (!interactButtonImage.enabled) interactButtonImage.enabled = true;

        if (!Interaction_Message.activeInHierarchy) Interaction_Message.SetActive(true);
        print("Setting Pickup Weapon Text Message");
        changeYourWeaponToText.text = "to change your weapon to " + _weap.weaponName;
    }

    public void DisablePickupWeaponTextMessage()
    {
        //changeYourWeaponToText.enabled = false;
        //pressText.enabled = false;
        //interactButtonImage.enabled = false;
        if (Interaction_Message.activeInHierarchy) Interaction_Message.SetActive(false);
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
