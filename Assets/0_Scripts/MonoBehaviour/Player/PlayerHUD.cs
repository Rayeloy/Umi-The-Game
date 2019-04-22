using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum CameraVFX
{
    None,
    Dash,
    RecieveHit,
    Water,
    PickFlag
}
public class PlayerHUD : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Offset de la pantalla al que se bloquea la flecha de la bola")]
    float offsetPantalla;

    [Header("Referencias")]
    [HideInInspector]
    public GameControllerBase gC;
    [HideInInspector]
    public PlayerMovement myPlayerMov;
    [HideInInspector]
    public Camera myCamera;
    [HideInInspector]
    public Camera myUICamera;
    Canvas myCanvas;

    public RectTransform contador;
    public RectTransform powerUpPanel;
    public Image crosshair;
    public Image crosshairReduced;
    public Image Hook;
    public Image Boost;
    public TMPro.TextMeshProUGUI redTeamScoreText;
    public TMPro.TextMeshProUGUI blueTeamScoreText;
    public TMPro.TextMeshProUGUI timeText;
    public Text attackNameText;

    //pickup weapon
    public GameObject Interaction_Message;
    public Text changeYourWeaponToText;
    //public Text pressText;
    public Image interactButtonImage;

    //Flag Slider
    [Header("Flag Slider")]
    public RectTransform flagSlider;
    public RectTransform flagSliderStart;
    public RectTransform flagSliderEnd;
    float flagSliderStartX;
    float flagSliderEndX;
    Vector3 blueFlagHomePos;
    Vector3 redFlagHomePos;
    float sliderTotalDist;
    Transform flag;
    Flag flagForSlider;
    Vector3 flagPos;

    [Header("Flag Arrow")]
    [SerializeField]
    Transform Arrow;
    [SerializeField]
    Transform Wale;

    //Grapple
    [Header("AutoGrapple")]
    public Transform hookPointsParent;
    public GameObject hookPointHUDPrefab;
    public GameObject cameraCenterPrefab;
    public GameObject pressButtonToGrappleMessage;
    [HideInInspector]
    public Vector2 cameraCenterPix;
    List<HookPointHUDInfo> hookPointHUDInfoList;

    //CameraVFX
    [Header("Camera VFX")]
    public GameObject dashCamVFX;
    public GameObject RecieveHitCamVFX;
    public GameObject PickFlagCamVFX;


    public void KonoAwake()
    {
        hookPointHUDInfoList = new List<HookPointHUDInfo>();
        myCanvas = GetComponent<Canvas>();
    }

    public void KonoStart()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)
        {
            flagForSlider = (gC as GameController_FlagMode).flags[0];
        }
        if (gC.gameMode == GameMode.Tutorial)
        {
            Arrow.gameObject.SetActive(false);
            Wale.gameObject.SetActive(false);
        }
        Interaction_Message.SetActive(false);
        crosshair.enabled = false;
        crosshairReduced.enabled = false;
        if (gC.gameMode == GameMode.CaptureTheFlag)// && !PhotonNetwork.IsConnected)
        {
            SetupFlagSlider();
        }
        pressButtonToGrappleMessage.SetActive(false);
        SetUpCameraCenter();
        SetupDashHUD();
    }

    private void Update()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)// && !PhotonNetwork.IsConnected)
        {
            UpdateFlagSlider();
        }
        UpdateAllHookPointHUDs();
        UpdateDashHUDCD();
    }

    public void AdaptCanvasHeightScale()
    {
        float scale = myUICamera.rect.width - myUICamera.rect.height;
        if (scale != 0)
        {
            float scaleValue = scale;
            float invScaleValue = 1 + (1 - scale);
            contador.localScale = new Vector3(contador.localScale.x * scaleValue, contador.localScale.y, 1);
            powerUpPanel.localScale = new Vector3(powerUpPanel.localScale.x * scaleValue, powerUpPanel.localScale.y, 1);
            crosshair.rectTransform.localScale = new Vector3(crosshair.rectTransform.localScale.x * scaleValue, crosshair.rectTransform.localScale.y, 1);
            crosshairReduced.rectTransform.localScale = new Vector3(crosshairReduced.rectTransform.localScale.x * scaleValue, crosshairReduced.rectTransform.localScale.y, 1);
            redTeamScoreText.rectTransform.localScale = new Vector3(redTeamScoreText.rectTransform.localScale.x * invScaleValue, redTeamScoreText.rectTransform.localScale.y, 1);
            blueTeamScoreText.rectTransform.localScale = new Vector3(blueTeamScoreText.rectTransform.localScale.x * invScaleValue, blueTeamScoreText.rectTransform.localScale.y, 1);
            timeText.rectTransform.localScale = new Vector3(timeText.rectTransform.localScale.x, timeText.rectTransform.localScale.y * scaleValue, 1);
        }
    }

    public void SetUpCameraCenter()
    {
        cameraCenterPix = new Vector2(myCamera.pixelWidth / 2, myCamera.pixelHeight / 2);
        cameraCenterPix += new Vector2(myCamera.pixelWidth / myCamera.rect.width * myCamera.rect.x, myCamera.pixelHeight / myCamera.rect.height * myCamera.rect.y);
        //Debug.Log("I'm player " + myPlayerMov.name +" and my center in the camera is  = " + cameraCenterPix.ToString("F4"));
        cameraCenterPrefab.SetActive(false);
        //cameraCenterPrefab.GetComponent<RectTransform>().anchoredPosition = cameraCenterPix;
        //Debug.Log("cameraCenterPrefab.rectTransform.localPosition = " + cameraCenterPrefab.GetComponent<RectTransform>().localPosition);
        //cameraCenterPrefab.GetComponent<RectTransform>().position = cameraCenterPix;
    }

    public void SetPickupWeaponTextMessage(WeaponData _weap)
    {
        //if (!changeYourWeaponToText.enabled) changeYourWeaponToText.enabled = true;
        //if (!pressText.enabled) pressText.enabled = true;
        //if (!interactButtonImage.enabled) interactButtonImage.enabled = true;

        if (!Interaction_Message.activeInHierarchy) Interaction_Message.SetActive(true);
        //print("Setting Pickup Weapon Text Message");
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
        flagSliderStartX = flagSliderStart.localPosition.x;
        flagSliderEndX = flagSliderEnd.localPosition.x;
        sliderTotalDist = (flagSliderStart.localPosition - flagSliderEnd.localPosition).magnitude;
        flagSlider.localPosition = (flagSliderStart.localPosition - flagSliderEnd.localPosition)/2;

        //Flecha bola
        Arrow.gameObject.SetActive(false);
        Wale.gameObject.SetActive(false);
    }

    Vector3 ArrowPointing;
    void UpdateFlagSlider()
    {
        float progress = 0.5f;
        if (!flagForSlider.respawning)
        {
            flagPos = flag.position;
            flagPos.y = 0;
            Vector3 blueToFlagDir = blueFlagHomePos - flagPos;
            float distFromBlue = blueToFlagDir.magnitude;
            Vector3 union = (blueFlagHomePos - redFlagHomePos).normalized;
            float angle = Vector3.Angle(blueToFlagDir.normalized, union);
            float currentDist = Mathf.Cos(angle * Mathf.Deg2Rad) * distFromBlue;
            float totalDist = (blueFlagHomePos - redFlagHomePos).magnitude;
            progress = Mathf.Clamp01(currentDist / totalDist);
        }

        float currentX = progress * sliderTotalDist;
        Vector3 newPos = flagSliderStart.localPosition;
        newPos.x += currentX;
        flagSlider.localPosition = newPos;

        // Flecha a Bola
        ArrowToFlag();
    }

    void ArrowToFlag()
    {

        Vector3 dir = myCamera.WorldToScreenPoint(flag.position);

        if (dir.x > offsetPantalla && dir.x < Screen.width - offsetPantalla && dir.y > offsetPantalla && dir.y < Screen.height - offsetPantalla)
        {
            Arrow.gameObject.SetActive(false);
            Wale.gameObject.SetActive(false);
        }
        else
        {
            Arrow.gameObject.SetActive(true);
            Wale.gameObject.SetActive(true);

            ArrowPointing.z = Mathf.Atan2((Arrow.transform.position.y - dir.y), (Arrow.transform.position.x - dir.x)) * Mathf.Rad2Deg - 90;

            Arrow.transform.rotation = Quaternion.Euler(ArrowPointing);
            Arrow.transform.position = new Vector3(Mathf.Clamp(dir.x, offsetPantalla, Screen.width - offsetPantalla), Mathf.Clamp(dir.y, offsetPantalla, Screen.height - offsetPantalla), 0);

            Wale.transform.position = Arrow.transform.position;
        }
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

    public void setHookUI(float f)
    {
        //print("SetHookUI: fillAmout= " + f);
        Hook.fillAmount = Mathf.Clamp01(f);
    }

    public void setBoostUI(float f)
    {
        //print("SetBoostUI: fillAmout= " + f);
        Boost.fillAmount = Mathf.Clamp01(f);
    }


    // --- GRAPPLE//Hook Point ---
    public void ShowGrappleMessage()
    {
        //Debug.LogWarning("SHOW GRAPPLE MESSAGE");
        pressButtonToGrappleMessage.SetActive(true);
    }

    public void HideGrappleMessage()
    {
        pressButtonToGrappleMessage.SetActive(false);
    }

    //public void ShowGrapplePoints(List<HookPoint> hookPoints)
    //{
    //    for (int i = 0; i < hookPoints.Count; i++)
    //    {
    //        bool found = false;
    //        for (int j = 0; j < hookPointHUDInfoList.Count && !found; j++)
    //        {
    //            if (hookPointHUDInfoList[j].hookPointHUD.myHookPoint == hookPoints[i])
    //            {
    //                found = true;
    //                if ()
    //                {

    //                }
    //            }
    //        }
    //        if (!found)
    //        {
    //            AddHookPointHUD(hookPoints[i]);
    //        }
    //    }
    //}

    void UpdateAllHookPointHUDs()
    {
        for (int i = 0; i < hookPointHUDInfoList.Count; i++)
        {
            if (hookPointHUDInfoList[i].showing)
            {
                hookPointHUDInfoList[i].hookPointHUD.KonoUpdate();
            }
        }
    }

    public void HideHookPointHUD(HookPoint hookPoint)
    {
        for (int i = 0; i < hookPointHUDInfoList.Count; i++)
        {
            if (hookPointHUDInfoList[i].hookPointHUD.myHookPoint == hookPoint)
            {
                if (hookPointHUDInfoList[i].showing)
                {
                    hookPointHUDInfoList[i].showing = false;
                    if (hookPointHUDInfoList[i].chosenOne)
                    {
                        hookPointHUDInfoList[i].SwitchChosenOne();
                    }
                    hookPointHUDInfoList[i].hookPointHUD.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowHookPointHUD(HookPoint hookPoint)
    {
        bool found = false;
        for(int i=0; i < hookPointHUDInfoList.Count && !found; i++)
        {
            if (hookPointHUDInfoList[i].hookPointHUD.myHookPoint == hookPoint)
            {
                if (!hookPointHUDInfoList[i].showing)
                {
                    hookPointHUDInfoList[i].showing = true;
                    hookPointHUDInfoList[i].hookPointHUD.gameObject.SetActive(true);
                    found = true;
                }
            }
        }
        if (!found)//Create new hookpointHUD
        {
            AddHookPointHUD(hookPoint);
        }

    }

    void AddHookPointHUD(HookPoint hookPoint)
    {
        print("CREATE NEW hookPointHUDObject");
        GameObject hookPointHUDObject = Instantiate(hookPointHUDPrefab, transform, false);
        hookPointHUDObject.transform.SetParent(hookPointsParent, true);
        HookPointHUDInfo auxHookPointHUDInfo = new HookPointHUDInfo(hookPointHUDObject.GetComponent<HookPointHUD>());
        auxHookPointHUDInfo.hookPointHUD.KonoAwake(hookPoint, myUICamera, myCanvas);
        auxHookPointHUDInfo.showing = true;
        hookPointHUDInfoList.Add(auxHookPointHUDInfo);
    }

    public void SetChosenHookPointHUD(HookPoint hookPoint)
    {
        print("SetChosenHookPointHUD("+hookPoint.name+")");
        for (int i = 0; i < hookPointHUDInfoList.Count; i++)
        {
            if (hookPointHUDInfoList[i].hookPointHUD.myHookPoint == hookPoint && !hookPointHUDInfoList[i].chosenOne)
            {
                hookPointHUDInfoList[i].SwitchChosenOne();
            }
            else
            {
                if (hookPointHUDInfoList[i].chosenOne)
                {
                    hookPointHUDInfoList[i].SwitchChosenOne();
                }
            }
        }
    }


    //EFECTOS DE CÁMARA

    GameObject currentCameraEffect;
    CameraVFX currentCamVFX = CameraVFX.None;
    public void StartCamVFX(CameraVFX camVFX)
    {
        if (currentCamVFX != CameraVFX.None)
        {
            switch (camVFX)
            {
                case CameraVFX.Dash:
                    currentCamVFX = CameraVFX.Dash;
                    break;
                case CameraVFX.RecieveHit:
                    currentCamVFX = CameraVFX.RecieveHit;
                    break;
                case CameraVFX.PickFlag:
                    currentCamVFX = CameraVFX.PickFlag;
                    break;
                case CameraVFX.Water:
                    currentCamVFX = CameraVFX.Water;
                    break;
            }
        }
    }

    //DASH HUD
    public RectTransform dashHUDStartPos;
    public RectTransform dashHUDEndPos;
    public RectTransform dashHUDWater;
    float dashHUDTotalDist = 0;

    bool dashHUDDepleteAnimStarted = false;
    public float dashHUDDepleteMaxTime = 0.5f;
    float dashHUDDepleteTime = 0;

    void SetupDashHUD()
    {
        dashHUDTotalDist = Mathf.Abs(dashHUDStartPos.localPosition.y - dashHUDEndPos.localPosition.y);
    }

    void UpdateDashHUDCD()
    {
        float progress = 0;
        if (myPlayerMov.boostReady)
        {
            progress = 1;
        }
        else if (dashHUDDepleteAnimStarted)
        {
            ProcessDashHUDDepleteAnimation();
        }
        else
        {
            progress = myPlayerMov.boostTime/myPlayerMov.boostCD;
            float currentY = progress * dashHUDTotalDist;
            dashHUDWater.localPosition = new Vector3(dashHUDStartPos.localPosition.x, dashHUDStartPos.localPosition.y + currentY, 1);
            print("DASH CD = " + myPlayerMov.boostTime+"; progress = "+progress);
        }
    }

    public void StartDashHUDDepleteAnimation()
    {
        if (!dashHUDDepleteAnimStarted)
        {
            dashHUDDepleteMaxTime = Mathf.Clamp(dashHUDDepleteMaxTime, 0, myPlayerMov.boostDuration);
            dashHUDDepleteTime = 0;
            dashHUDDepleteAnimStarted = true;
        }
    }

    void ProcessDashHUDDepleteAnimation()
    {
        if (dashHUDDepleteAnimStarted)
        {
            dashHUDDepleteTime += Time.deltaTime;

            //Move Down water
            float progress = dashHUDDepleteTime / dashHUDDepleteMaxTime;
            progress = 1 - progress;
            float currentY = progress * dashHUDTotalDist;
            dashHUDWater.localPosition = new Vector3(dashHUDStartPos.localPosition.x, dashHUDStartPos.localPosition.y + currentY, 1);
            print("DASH Deplete Anim progress = " + progress);

            if (dashHUDDepleteTime >= dashHUDDepleteMaxTime)
            {
                StopDashHUDDepleteAnimation();
            }
        }
    }
    void StopDashHUDDepleteAnimation()
    {
        if (dashHUDDepleteAnimStarted)
        {
            dashHUDDepleteAnimStarted = false;
        }
    }
}
public class HookPointHUDInfo
{
    public HookPointHUD hookPointHUD;
    public bool showing = false;
    public bool chosenOne = false;
    Color green = new Color(0.46f, 1, 0, 0.75f);
    Color gray = new Color(0.5f, 0.5f, 0.5f, 0.75f);

    public HookPointHUDInfo()
    {
        hookPointHUD = null;
        showing = false;
        chosenOne = false;
        hookPointHUD.GetComponent<Image>().color = gray;
    }

    public HookPointHUDInfo(HookPointHUD _hookPointHUD)
    {
        hookPointHUD = _hookPointHUD;
        showing = true;
        chosenOne = false;
        hookPointHUD.GetComponent<Image>().color = gray;
    }

    public void SwitchChosenOne()
    {
        if (chosenOne)
        {
            chosenOne = false;
            hookPointHUD.GetComponent<Image>().color = gray;
        }
        else
        {
            chosenOne = true;
            hookPointHUD.GetComponent<Image>().color = green;
        }
    }
}
