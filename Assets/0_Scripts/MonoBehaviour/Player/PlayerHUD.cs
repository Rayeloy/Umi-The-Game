using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


#region ----[ PUBLIC ENUMS ]----
public enum CameraVFXType
{
    None,
    Dash,
    RecieveHit,
    Water,
    PickFlag
}

public enum FlagArrowState
{
    none,
    deactivated,
    activated_OnScreen,
    activated_OffScreen
}
#endregion

public class PlayerHUD : MonoBehaviourPunCallbacks
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    [Header("Referencias")]
    [HideInInspector]
    public GameControllerBase gC;
    [HideInInspector]
    public PlayerMovement myPlayerMov;
    [HideInInspector]
    public PlayerCombatNew myPlayerCombat;
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

    [Header("Scoreboard")]
    public Image[] teamAPlayerIcons;
    public Image[] teamBPlayerIcons;
    public TMPro.TextMeshProUGUI[] maxScorePoints;

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
    Transform flagTransform;
    Flag flag;
    Vector3 flagPos;

    //Flag Arrow
    [Header("Flag Arrow && Flag Home Arrow")]
    [Tooltip("Minimum distance the flag needs to be separated from the player to show the flag arrow even when looking directly at it")]
    public float minDistanceToShowWhenOnCamera = 25;
    [Tooltip("Ball Pointer proportion when at max distance (minProportionWOCMaxDist)")]
    public float minProportionWhenOnCamera = 0.4f;
    [Tooltip("Ball Pointer proportion when at min distance (maxProportionWOCMinDist)")]
    public float maxProportionWhenOnCamera = 0.85f;
    [Tooltip("The Distance at which the Ball Pointer has the min proportion size When On Camera. This should be always higher than the maxProportionWOCMinDist")]
    public float minProportionWOCMaxDist = 70;
    [Tooltip("The Distance at which the Ball Pointer has the max proportion size When On Camera. This should be always lower than the minProportionWOCMaxDist")]
    public float maxProportionWOCMinDist = 30;

    [Header("Flag Arrow")]
    public Transform flagArrowWhale;
    public Transform flagArrowArrow;
    public Transform flagArrowRing;
    public Transform flagArrowFixedRing;
    //public Transform flagArrowParent;
    public Color[] flagArrowColors;//0 -> not picked; 1 -> respawning
    public UIAnimation flagArrowRespawnGlowAnim;
    public UIAnimation flagArrowPickFlagGlowAnim;
    public Color[] arrowToFlagGlowTeamColors;//0 -> Green team; 1-> Pink team
    public LayerMask arrowToFlagCameraLM;
    Vector3 flagArrowWhaleOriginalProportion;
    FlagArrowState flagArrowSt = FlagArrowState.deactivated;
    Transform flagArrowFollowTarget;
    Transform flagSpawn;
    bool flagArrowPickupStarted = false;
    bool flagArrowRespawnRestarted = false;

    [Header("Flag Home Arrow")]
    public RectTransform flagHomeArrowIcon;
    public RectTransform flagHomeArrowIconOutline;
    public RectTransform flagHomeArrowArrow;
    FlagArrowState flagHomeArrowSt = FlagArrowState.deactivated;
    Transform flagHomeTransform;
    Vector3 flagHomeArrowFlagHomePos;
    Vector3 flagHomeArrowIconOriginalProportion;



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
    public CameraVFX[] cameraVFX;
    CameraVFX currentCameraVFX;
    public RectTransform CameraVFXParent;


    //DASH HUD
    [Header("DASH HUD")]
    public RectTransform dashHUDParent;
    public RectTransform dashHUDStartPos;
    public RectTransform dashHUDEndPos;
    public RectTransform dashHUDWater;
    float dashHUDTotalDist = 0;
    public Image dashHUDBackground, dashHUDMinLine, dashHUDEdges;
    public Color[] dashHUDBackgroundColor, dashHUDMinLineColor, dashHUDEdgesColor;//0 -> COLOR WHEN BOOST READY; 1 -> COLOR WHEN BOOST NOT READY;
    bool dashHUDCantDoAnimStarted = false;
    public UIAnimation dashHUDCantDoAnimation;
    float dashHUDCantDoAnimTime = 0;

    //SKILLS HUD
    [Header("SKILLS HUD")]
    public Image[] skillHUDIcons;
    public Image[] skillHUDProgressBars;
    public Image[] skillHUDBackgrounds;
    public UIAnimation[] skillHUDCantDoAnimations;
    bool[] skillHUDCantDoAnimStarted;
    float[] skillHUDCantDoAnimTime;
    [Tooltip("0 -> COLOR WHEN READY; 1 -> COLOR WHEN CD; 2 -> COLOR WHEN ACTIVE; 3 -> COLOR WHEN CANT DO ANIM;")]
    public Color[] skillHUDBackgroundColors, skillHUDIconColors;//0 -> COLOR WHEN READY; 1 -> COLOR WHEN CD; 2 -> COLOR WHEN ACTIVE; 3 -> COLOR WHEN CANT DO ANIM;
    [Header(" - Activation Glow -")]
    public UIAnimation[] skillHUDActivationGlow;
    [Header(" - Active Glow -")]
    public UIAnimation[] skillHUDActiveGlow;
    public Animator[] orangeCircleAnimation;
    PlayerMovement flagCurrentOwner = null;


    #endregion

    #region ----[ PROPERTIES ]----
    Color whiteTransparent = new Color(1, 1, 1, 0);
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    public void KonoAwake()
    {
        hookPointHUDInfoList = new List<HookPointHUDInfo>();
        myCanvas = GetComponent<Canvas>();
        SetupCameraVFX();
    }
    #endregion

    #region Start
    public void KonoStart()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)
        {
            if (gC.online)
            {
                flag = GameObject.FindGameObjectWithTag("Flag").GetComponent<Flag>();
                Debug.Log("FLAG POS = "+FindObjectOfType<Flag>().transform.position);
                Debug.Log("flag = " + flag);
            }
            else
            {
                flag = (gC as GameController_FlagMode).flags[0];
            }
        }

        Interaction_Message.SetActive(false);
        crosshair.enabled = false;
        crosshairReduced.enabled = false;

        //SETUP HUDS
        SetupFlagSlider();
        SetUpScoreBoard();
        SetUpFlagHomeArrow();
        SetupArrowToFlag();
        pressButtonToGrappleMessage.SetActive(false);
        SetUpCameraCenter();
        SetupDashHUD();
        SetUpHookUI();

    }
    #endregion

    #region Update
    private void Update()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)// && !PhotonNetwork.IsConnected)
        {
            UpdateFlagSlider();
            // Flecha a Bola
            ArrowToFlagUpdate();
            UpdateFlagHomeArrow();
        }
        UpdateAllHookPointHUDs();
        UpdateDashHUDFuel();
        ProcessSkillsCD();
        ProcessDashHUDCantDoAnimation();
        ProcessSkillCantDoAnim();
    }
    #endregion

    #endregion

    #region ----[ CLASS FUNCTIONS ]----

    public void AdaptCanvasHeightScale()
    {
        float scale = myUICamera.rect.width - myUICamera.rect.height;
        if (scale != 0)
        {
            float scaleValue = scale;
            float invScaleValue = 1 + (1 - scale);//2- SCALE
            contador.localScale = new Vector3(contador.localScale.x * scaleValue, contador.localScale.y, 1);
            powerUpPanel.localScale = new Vector3(powerUpPanel.localScale.x * scaleValue, powerUpPanel.localScale.y, 1);
            crosshair.rectTransform.localScale = new Vector3(crosshair.rectTransform.localScale.x * scaleValue, crosshair.rectTransform.localScale.y, 1);
            crosshairReduced.rectTransform.localScale = new Vector3(crosshairReduced.rectTransform.localScale.x * scaleValue, crosshairReduced.rectTransform.localScale.y, 1);
            redTeamScoreText.rectTransform.localScale = new Vector3(redTeamScoreText.rectTransform.localScale.x * invScaleValue, redTeamScoreText.rectTransform.localScale.y, 1);
            blueTeamScoreText.rectTransform.localScale = new Vector3(blueTeamScoreText.rectTransform.localScale.x * invScaleValue, blueTeamScoreText.rectTransform.localScale.y, 1);
            timeText.rectTransform.localScale = new Vector3(timeText.rectTransform.localScale.x, timeText.rectTransform.localScale.y * scaleValue, 1);
            dashHUDParent.localScale = new Vector3(dashHUDParent.localScale.x * scaleValue, dashHUDParent.localScale.y, 1);
            //flagArrowParent.GetComponent<RectTransform>().localScale = new Vector3(flagArrowParent.GetComponent<RectTransform>().localScale.x * scaleValue, flagArrowParent.GetComponent<RectTransform>().localScale.y, 1);
            flagArrowWhale.GetComponent<RectTransform>().localScale = new Vector3(flagArrowWhale.GetComponent<RectTransform>().localScale.x * scaleValue, flagArrowWhale.GetComponent<RectTransform>().localScale.y, 1);
            flagArrowArrow.GetComponent<RectTransform>().localScale = new Vector3(flagArrowArrow.GetComponent<RectTransform>().localScale.x * scaleValue, flagArrowArrow.GetComponent<RectTransform>().localScale.y, 1);
            flagArrowFixedRing.GetComponent<RectTransform>().localScale = new Vector3(flagArrowFixedRing.GetComponent<RectTransform>().localScale.x * scaleValue, flagArrowFixedRing.GetComponent<RectTransform>().localScale.y, 1);
            //flagHomeArrowIcon.GetComponent<RectTransform>().localScale = new Vector3(flagHomeArrowIcon.GetComponent<RectTransform>().localScale.x * scaleValue, flagHomeArrowIcon.GetComponent<RectTransform>().localScale.y, 1);
            flagHomeArrowArrow.GetComponent<RectTransform>().localScale = new Vector3(flagHomeArrowArrow.GetComponent<RectTransform>().localScale.x * scaleValue, flagHomeArrowArrow.GetComponent<RectTransform>().localScale.y, 1);
            flagHomeArrowIconOutline.GetComponent<RectTransform>().localScale = new Vector3(flagHomeArrowIconOutline.GetComponent<RectTransform>().localScale.x * scaleValue, flagHomeArrowIconOutline.GetComponent<RectTransform>().localScale.y, 1);
            //flagAro.GetComponent<RectTransform>().localScale = new Vector3(flagAro.GetComponent<RectTransform>().localScale.x * scaleValue, flagAro.GetComponent<RectTransform>().localScale.y, 1);
            //CameraVFXParent.localScale = new Vector3(CameraVFXParent.localScale.x * scaleValue, CameraVFXParent.localScale.y, 1);
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

    #region --- SCOREBOARD ---
    void SetUpScoreBoard()
    {
        if(gC.gameMode == GameMode.CaptureTheFlag)
        {
            Color whiteHalfTransparent = new Color(1, 1, 1, 0.35f);
            int playerNumTeamACopy = gC.playerNumTeamA;
            for (int i = 0; i < teamAPlayerIcons.Length; i++)
            {
                teamAPlayerIcons[i].color = playerNumTeamACopy > 0 ? Color.white : whiteHalfTransparent;
                if (playerNumTeamACopy > 0) playerNumTeamACopy--;
            }
            int playerNumTeamBCopy = gC.playerNumTeamB;
            for (int i = 0; i < teamBPlayerIcons.Length; i++)
            {
                teamBPlayerIcons[i].color = playerNumTeamBCopy > 0 ? Color.white : whiteHalfTransparent;
                if (playerNumTeamBCopy > 0) playerNumTeamBCopy--;
            }
            maxScorePoints[0].text = "/" + (gC as GameController_FlagMode).myScoreManager.maxScore;
            maxScorePoints[1].text = "/" + (gC as GameController_FlagMode).myScoreManager.maxScore;
        }
        else
        {
            contador.gameObject.SetActive(false);
        }

    }
    #endregion

    #region --- FLAG SLIDER ---

    void SetupFlagSlider()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)
        {
            if (gC.online)
            {
                if(flag == null) flag = GameObject.FindObjectOfType<Flag>();
                Debug.Log("Estoy pasando por aquí");
                flagTransform = flag.transform;
            }
            else
            {
                flagTransform = (gC as GameController_FlagMode).flags[0].transform;
            }
            blueFlagHomePos = (gC as GameController_FlagMode).FlagHome_TeamA.position;
            redFlagHomePos = (gC as GameController_FlagMode).FlagHome_TeamB.position;
            blueFlagHomePos.y = 0;
            redFlagHomePos.y = 0;
            flagSliderStartX = flagSliderStart.localPosition.x;
            flagSliderEndX = flagSliderEnd.localPosition.x;
            sliderTotalDist = (flagSliderStart.localPosition - flagSliderEnd.localPosition).magnitude;
            flagSlider.localPosition = (flagSliderStart.localPosition - flagSliderEnd.localPosition) / 2;

            //Flecha bola
            flagArrowArrow.gameObject.SetActive(false);
            flagArrowWhale.gameObject.SetActive(false);
        }
    }

    void UpdateFlagSlider()
    {
        float progress = 0.5f;
        if (!flag.respawning)
        {
            flagPos = flagTransform.position;
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
    }

    #endregion

    #region --- FLAG ARROW ---
    void SetupArrowToFlag()
    {
        if(gC.gameMode == GameMode.CaptureTheFlag)
        {
            flagSpawn = (gC as GameController_FlagMode).flagsParent;
            flagArrowFollowTarget = flagTransform;
            DeactivateArrowToFlagOffScreen();
            if (minProportionWOCMaxDist <= maxProportionWhenOnCamera) Debug.LogError("Error: minProportionWOCMaxDist(" + minProportionWOCMaxDist + ") should be always higher than the " +
                "maxProportionWOCMinDist(" + maxProportionWhenOnCamera + "). Change the values please.");
            if (maxProportionWhenOnCamera <= minProportionWhenOnCamera) Debug.LogError("Error: maxProportionWhenOnCamera(" + maxProportionWhenOnCamera + ") should be always higher than the " +
        "minProportionWhenOnCamera(" + minProportionWhenOnCamera + "). Change the values please.");
            flagArrowWhaleOriginalProportion = flagArrowWhale.localScale;
        }
        else
        {
            flagArrowArrow.gameObject.SetActive(false);
            flagArrowWhale.gameObject.SetActive(false);
            flagArrowFixedRing.gameObject.SetActive(false);
        }
    }

    void ActivateArrowToFlagOffScreen()
    {
        if (flagArrowSt != FlagArrowState.activated_OffScreen)
        {
            DeactivateArrowToFlagOnScreen();
            if (!myPlayerMov.disableAllDebugs) Debug.LogError("Activate FLAG ARROW");
            flagArrowSt = FlagArrowState.activated_OffScreen;
            flagArrowArrow.gameObject.SetActive(true);
            flagArrowWhale.gameObject.SetActive(true);
            flagArrowFixedRing.gameObject.SetActive(false);
            if (flag.currentOwner != null && !flagArrowRing.gameObject.activeInHierarchy) flagArrowRing.gameObject.SetActive(true);
            else if (flag.currentOwner == null && flagArrowRing.gameObject.activeInHierarchy) flagArrowRing.gameObject.SetActive(false);
            ArrowToFlagSetTeamColorsAndRing();
        }
    }

    void DeactivateArrowToFlagOffScreen()
    {
        if (flagArrowSt == FlagArrowState.activated_OffScreen)
        {
            if (!myPlayerMov.disableAllDebugs) Debug.LogError("Deactivate FLAG ARROW");
            flagArrowSt = FlagArrowState.deactivated;
            flagArrowArrow.gameObject.SetActive(false);
            flagArrowWhale.gameObject.SetActive(false);
            flagArrowRing.gameObject.SetActive(false);
            flagArrowFixedRing.gameObject.SetActive(false);
        }
    }

    void ActivateArrowToFlagOnScreen()
    {
        if (flagArrowSt != FlagArrowState.activated_OnScreen)
        {
            DeactivateArrowToFlagOffScreen();
            if (!myPlayerMov.disableAllDebugs) Debug.LogError("Activate FLAG ARROW ON FLAG");
            flagArrowSt = FlagArrowState.activated_OnScreen;
            flagArrowArrow.gameObject.SetActive(false);
            flagArrowWhale.gameObject.SetActive(true);
            if (flag.currentOwner != null && !flagArrowFixedRing.gameObject.activeInHierarchy) flagArrowFixedRing.gameObject.SetActive(true);
            else if (flag.currentOwner == null && flagArrowFixedRing.gameObject.activeInHierarchy) flagArrowFixedRing.gameObject.SetActive(false);
            //flagArrowWhale.localScale = new Vector3(flagArrowWhale.localScale.x * 0.8f, flagArrowWhale.localScale.y * 0.8f, 1);
            ArrowToFlagSetTeamColorsAndRing();
        }
    }

    void DeactivateArrowToFlagOnScreen()
    {
        if (flagArrowSt == FlagArrowState.activated_OnScreen)
        {
            if (!myPlayerMov.disableAllDebugs) Debug.LogError("Deactivate FLAG ARROW ON FLAG");
            flagArrowSt = FlagArrowState.deactivated;
            flagArrowWhale.localScale = new Vector3(flagArrowWhaleOriginalProportion.x, flagArrowWhaleOriginalProportion.y, 1);
            flagArrowArrow.gameObject.SetActive(false);
            flagArrowWhale.gameObject.SetActive(false);
            flagArrowRing.gameObject.SetActive(false);
            flagArrowFixedRing.gameObject.SetActive(false);
        }
    }

    void ProcessArrowToFlag()
    {

        //if (flagArrowSt == FlagArrowState.activated_OffScreen || flagArrowSt == FlagArrowState.activated_OnScreen)
        //{
            ArrowToFlagStopPickup();
            ArrowToFlagStartPickup();
            ArrowToFlagSetNewOwner();
            ArrowToFlagStartRespawn();
            ArrowToFlagStopRespawn();
        //}

        //float pixelW = myCamera.pixelWidth;
        //float pixelH = myCamera.pixelHeight;
        //Vector3 dir = myCamera.WorldToScreenPoint(flag.position);

        //float offsetX = -((pixelW / 2) + (myCamera.rect.x * 2 * pixelW));
        //float offsetY = -((pixelH / 2) + (myCamera.rect.y * 2 * pixelH));
        //RectTransform arrowRect = Arrow.GetComponent<RectTransform>();
        //arrowRect.localPosition = new Vector3(dir.x + offsetX, dir.y + offsetY, 0);

        //ArrowPointing.z = Mathf.Atan2((arrowRect.localPosition.y - dir.y), (arrowRect.localPosition.x - dir.x)) * Mathf.Rad2Deg - 90;

        //arrowRect.localRotation = Quaternion.Euler(ArrowPointing);
        ////Arrow.position = new Vector3(Mathf.Clamp(dir.x, offsetPantalla, Screen.width - offsetPantalla), Mathf.Clamp(dir.y, offsetPantalla, Screen.height - offsetPantalla), 0);
        //RectTransform waleRect = Wale.GetComponent<RectTransform>();
        //waleRect.localPosition = arrowRect.localPosition;

        switch (flagArrowSt)
        {
            case FlagArrowState.activated_OffScreen:
                Vector3 flagViewportPos, flagArrowPos;
                flagArrowPos = flagViewportPos = myCamera.WorldToViewportPoint(flagArrowFollowTarget.position);

                flagArrowPos.x -= 0.5f;  // Translate to use center of viewport
                flagArrowPos.y -= 0.5f;
                flagArrowPos.z = 0;      // I think I can do this rather than do a 
                                         //   a full projection onto the plane

                float fAngle = Mathf.Atan2(flagArrowPos.x, flagArrowPos.y);

                float yProportion = myCamera.rect.height < 1 ? 0.21f : 0.22f;
                float xProportion = myCamera.rect.width < 1 ? 0.445f : 0.45f;
                flagArrowPos.x = xProportion * Mathf.Sin(fAngle) + 0.5f;  // Place on ellipse touching 
                flagArrowPos.y = yProportion * Mathf.Cos(fAngle) + 0.5f;  //   side of viewport
                if (flagViewportPos.z < myCamera.nearClipPlane)
                {
                    flagArrowPos.x = 1 - flagArrowPos.x;
                    flagArrowPos.y = 1 - flagArrowPos.y;
                }
                //Debug.LogWarning(" flagArrowPos = " + flagArrowPos.ToString("F4"));
                flagArrowPos.z = myCamera.nearClipPlane + 0.001f;  // Looking from neg to pos Z;
                float finalAngle = 180 + (-fAngle * Mathf.Rad2Deg) + (flagViewportPos.z < myCamera.nearClipPlane ? 0 : 180);
                flagArrowArrow.localEulerAngles = new Vector3(0.0f, 0.0f, finalAngle);
                flagArrowWhale.position = myCamera.ViewportToWorldPoint(flagArrowPos);
                flagArrowArrow.position = flagArrowWhale.position;
                break;

            case FlagArrowState.activated_OnScreen:
                float distToFlag = (flagArrowFollowTarget.position - myPlayerMov.transform.position).magnitude;
                distToFlag = Mathf.Clamp(distToFlag,maxProportionWOCMinDist,minProportionWOCMaxDist) - maxProportionWOCMinDist;
                float totalDist = minProportionWOCMaxDist - maxProportionWOCMinDist;
                float progress = distToFlag / totalDist;
                float totalPropDif = maxProportionWhenOnCamera - minProportionWhenOnCamera;
                float currentProportion = maxProportionWhenOnCamera - (progress * totalPropDif);
                //Debug.LogWarning("currentProportion = "+ currentProportion + "; progress = "+ progress + "; distToFlag = "+ distToFlag + "; totalDist = "+ totalDist + "; totalPropDif = " + totalPropDif);

                flagArrowWhale.localScale = new Vector3(flagArrowWhaleOriginalProportion.x * currentProportion, flagArrowWhaleOriginalProportion.y * currentProportion, 1);
                flagArrowPos = flagArrowFollowTarget.position; flagArrowPos.y += 3;
                Vector3 flagArrowViewportPos = myCamera.WorldToViewportPoint(flagArrowPos);
                flagArrowViewportPos.z = myCamera.nearClipPlane + 0.001f;
                flagArrowWhale.position = myCamera.ViewportToWorldPoint(flagArrowViewportPos);
                flagArrowFixedRing.position = flagArrowWhale.position;
                break;
        }
    }

    void ArrowToFlagUpdate()
    {
        if (myPlayerMov.haveFlag)
        {
            DeactivateArrowToFlagOnScreen();
            DeactivateArrowToFlagOffScreen();
        }
        else
        {
            Vector3 flagViewportPos = myCamera.WorldToViewportPoint(flagArrowFollowTarget.position+ Vector3.up*3);
            if (flagViewportPos.z > myCamera.nearClipPlane && (flagViewportPos.x >= 0.05f && flagViewportPos.x <= 0.95f && flagViewportPos.y >= 0.2f && flagViewportPos.y <= 0.75f))
            {
                DeactivateArrowToFlagOffScreen();
                float distToFlag = (flagArrowFollowTarget.position - myPlayerMov.transform.position).magnitude;
                if (distToFlag >= minDistanceToShowWhenOnCamera)
                {
                    ActivateArrowToFlagOnScreen();
                }
                else
                {
                    Debug.DrawLine(myCamera.transform.position, flagArrowFollowTarget.position, Color.yellow);
                    RaycastHit hit;
                    bool collided = false;
                    if (Physics.Linecast(myCamera.transform.position, flagArrowFollowTarget.position, out hit, arrowToFlagCameraLM, QueryTriggerInteraction.Collide))
                    {
                        //Debug.LogWarning("COLLISIONS BETWEEN CAMERA AND FLAG");
                        if (hit.collider.gameObject.tag == "Stage" || hit.collider.gameObject.tag == "Player")
                        {
                            collided = true;
                        }
                    }

                    if (collided)
                    {
                        ActivateArrowToFlagOnScreen();
                    }
                    else
                    {
                        //Debug.LogWarning("NO COLLISIONS BETWEEN CAMERA AND FLAG");
                        DeactivateArrowToFlagOnScreen();
                    }
                }
            }
            else
            {
                ActivateArrowToFlagOffScreen();
            }
            
            ProcessArrowToFlag();
        }    
    }

    void ArrowToFlagSetNewOwner()
    {
        if (flagArrowPickupStarted && flag.currentOwner != null && flag.currentOwner.GetComponent<PlayerMovement>() != flagCurrentOwner)
        {
            flagArrowPickupStarted = false;
            ArrowToFlagStartPickup();
        }
    }

    void ArrowToFlagStartPickup()
    {
        if (!flagArrowPickupStarted && flag.currentOwner != null)
        {
            flagArrowPickupStarted = true;
            flagCurrentOwner = flag.currentOwner.GetComponent<PlayerMovement>();
            GameInfo.instance.StartAnimation(flagArrowPickFlagGlowAnim, myCamera);
            ArrowToFlagSetTeamColorsAndRing();
        }
    }

    void ArrowToFlagSetTeamColorsAndRing()
    {
        if (flag.currentOwner!=null && !myPlayerMov.haveFlag)
        {
            switch (flagArrowSt)
            {
                case FlagArrowState.activated_OffScreen:
                    if (!flagArrowRing.gameObject.activeInHierarchy) flagArrowRing.gameObject.SetActive(true);
                    flagArrowRing.GetComponent<Image>().color = flag.currentOwner.GetComponent<PlayerMovement>().team == Team.A ?
                        arrowToFlagGlowTeamColors[0] : arrowToFlagGlowTeamColors[1];
                    flagArrowArrow.GetComponent<Image>().color = flag.currentOwner.GetComponent<PlayerMovement>().team == Team.A ?
    arrowToFlagGlowTeamColors[0] : arrowToFlagGlowTeamColors[1];
                    flagArrowPickFlagGlowAnim.rect.GetComponent<Image>().color = flag.currentOwner.GetComponent<PlayerMovement>().team == Team.A ?
                        arrowToFlagGlowTeamColors[0] : arrowToFlagGlowTeamColors[1];
                    break;
                case FlagArrowState.activated_OnScreen:
                    if (!flagArrowFixedRing.gameObject.activeInHierarchy) flagArrowFixedRing.gameObject.SetActive(true);
                    flagArrowFixedRing.GetComponent<Image>().color = flag.currentOwner.GetComponent<PlayerMovement>().team == Team.A ?
    arrowToFlagGlowTeamColors[0] : arrowToFlagGlowTeamColors[1];
                    flagArrowPickFlagGlowAnim.rect.GetComponent<Image>().color = flag.currentOwner.GetComponent<PlayerMovement>().team == Team.A ?
                        arrowToFlagGlowTeamColors[0] : arrowToFlagGlowTeamColors[1];
                    break;
            }
        }
        else
        {
            switch (flagArrowSt)
            {
                case FlagArrowState.activated_OffScreen:
                    if (flagArrowRing.gameObject.activeInHierarchy) flagArrowRing.gameObject.SetActive(false);
                    flagArrowRing.GetComponent<Image>().color = Color.white;
                    flagArrowArrow.GetComponent<Image>().color = Color.white;
                    break;
                case FlagArrowState.activated_OnScreen:
                    if (flagArrowFixedRing.gameObject.activeInHierarchy) flagArrowFixedRing.gameObject.SetActive(false);
                    flagArrowFixedRing.GetComponent<Image>().color = Color.white;
                    break;
            }
        }
    }

    void ArrowToFlagStopPickup()
    {
        if (flagArrowPickupStarted && flag.currentOwner == null)
        {
            flagArrowPickupStarted = false;
            flagCurrentOwner = null;
            GameInfo.instance.StopUIAnimation(flagArrowPickFlagGlowAnim);
            ArrowToFlagSetTeamColorsAndRing();
            flagArrowPickFlagGlowAnim.rect.GetComponent<Image>().color = whiteTransparent;
        }
    }

    void ArrowToFlagStartRespawn()
    {
        if (!flagArrowRespawnRestarted && flag.respawning)
        {
            flagArrowRespawnRestarted = true;
            flagArrowFollowTarget = flagSpawn;
            flagArrowArrow.GetComponent<Image>().color = flagArrowColors[0];
            flagArrowWhale.GetComponent<Image>().color = flagArrowColors[1];
            flagArrowPickFlagGlowAnim.rect.GetComponent<Image>().color = whiteTransparent;
            DeactivateFlagHomeArrowOffScreen();
            DeactivateFlagHomeArrowOnScreen();
        }
    }

    void ArrowToFlagStopRespawn()
    {
        if (flagArrowRespawnRestarted && !flag.respawning)
        {
            flagArrowRespawnRestarted = false;
            GameInfo.instance.StartAnimation(flagArrowRespawnGlowAnim, myCamera);
            flagArrowFollowTarget = flagTransform;
            flagArrowArrow.GetComponent<Image>().color = flagArrowColors[0];
            flagArrowWhale.GetComponent<Image>().color = flagArrowColors[0];
        }
    }

    #endregion

    #region --- FLAG HOME ARROW ---
    void SetUpFlagHomeArrow()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)
        {
            flagHomeTransform = myPlayerMov.team == Team.A ? (gC as GameController_FlagMode).FlagHome_TeamA : (gC as GameController_FlagMode).FlagHome_TeamB;
            flagHomeArrowIconOutline.gameObject.SetActive(false);
            flagHomeArrowArrow.gameObject.SetActive(false);
            flagHomeArrowIconOriginalProportion = flagHomeArrowIconOutline.localScale;
        }
        else
        {
            flagHomeArrowIconOutline.gameObject.SetActive(false);
            flagHomeArrowArrow.gameObject.SetActive(false);
        }
    }

    void UpdateFlagHomeArrow()
    {
        if (gC.gameMode == GameMode.CaptureTheFlag)
        {
            //Debug.Log("FLAGHOME ARROW UPDATE:  flagCurrentOwner= "+ flagCurrentOwner + "; flagCurrentOwner.team = "+ flagCurrentOwner.team + "; myPlayerMov.team = " + myPlayerMov.team);
            if (flag.currentOwner != null && flag.currentOwner.GetComponent<PlayerMovement>().team == myPlayerMov.team)
            {
                //Debug.Log("FLAGHOME ARROW UPDATE:  flagCurrentOwner= " + flagCurrentOwner + "; flagCurrentOwner.team = " + flagCurrentOwner.team + "; myPlayerMov.team = " + myPlayerMov.team);
                flagHomeArrowFlagHomePos = flagHomeTransform.position + Vector3.up * 3;
                Vector3 flagHomeViewportPos = myCamera.WorldToViewportPoint(flagHomeArrowFlagHomePos);
                if(!myPlayerMov.disableAllDebugs)Debug.Log("flagHomeViewportPos = " + flagHomeViewportPos.ToString("F4"));
                if (flagHomeViewportPos.z > myCamera.nearClipPlane && (flagHomeViewportPos.x >= 0.05f && flagHomeViewportPos.x <= 0.95f && flagHomeViewportPos.y >= 0.2f && flagHomeViewportPos.y <= 0.75f))
                {
                    DeactivateFlagHomeArrowOffScreen();
                    float distToFlagHome = (flagHomeTransform.position - myPlayerMov.transform.position).magnitude;
                    if (distToFlagHome >= minDistanceToShowWhenOnCamera)
                    {
                        if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("Trying to Activate Flag Home Arrow On Screen");
                        ActivateFlagHomeArrowOnScreen();
                    }
                    else
                    {
                        Debug.DrawLine(myCamera.transform.position, flagHomeTransform.position, Color.yellow);
                        RaycastHit hit;
                        bool collided = false;
                        if (Physics.Linecast(myCamera.transform.position, flagHomeTransform.position, out hit, arrowToFlagCameraLM, QueryTriggerInteraction.Collide))
                        {
                            if (hit.collider.gameObject.tag == "Stage" || hit.collider.gameObject.tag == "Player")
                            {
                                if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("COLLISIONS BETWEEN CAMERA AND FLAG: collided with " + hit.collider.gameObject);
                                collided = true;
                            }
                        }

                        if (collided)
                        {
                            if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("Trying to Activate Flag Home Arrow On Screen because there is a collision");
                            ActivateFlagHomeArrowOnScreen();
                        }
                        else
                        {
                            if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("NO COLLISIONS BETWEEN CAMERA AND FLAG");
                            DeactivateFlagHomeArrowOnScreen();
                        }
                    }
                }
                else
                {
                    ActivateFlagHomeArrowOffScreen();
                }

                ProcessFlagHomeArrow();
            }
            else
            {
                DeactivateFlagHomeArrowOffScreen();
                DeactivateFlagHomeArrowOnScreen();
            }
        }
    }

    void ProcessFlagHomeArrow()
    {
        switch (flagHomeArrowSt)
        {
            case FlagArrowState.activated_OffScreen:
                Vector3 flagHomeViewportPos, flagHomeArrowPos;
                flagHomeArrowPos = flagHomeViewportPos = myCamera.WorldToViewportPoint(flagHomeTransform.position);

                flagHomeArrowPos.x -= 0.5f;  // Translate to use center of viewport
                flagHomeArrowPos.y -= 0.5f;
                flagHomeArrowPos.z = 0;      // I think I can do this rather than do a 
                                         //   a full projection onto the plane

                float fAngle = Mathf.Atan2(flagHomeArrowPos.x, flagHomeArrowPos.y);

                float yProportion = myCamera.rect.height < 1 ? 0.21f : 0.22f;
                float xProportion = myCamera.rect.width < 1 ? 0.445f : 0.45f;
                flagHomeArrowPos.x = xProportion * Mathf.Sin(fAngle) + 0.5f;  // Place on ellipse touching 
                flagHomeArrowPos.y = yProportion * Mathf.Cos(fAngle) + 0.5f;  //   side of viewport
                if (flagHomeViewportPos.z < myCamera.nearClipPlane)
                {
                    flagHomeArrowPos.x = 1 - flagHomeArrowPos.x;
                    flagHomeArrowPos.y = 1 - flagHomeArrowPos.y;
                }
                //Debug.LogWarning(" flagArrowPos = " + flagArrowPos.ToString("F4"));
                flagHomeArrowPos.z = myCamera.nearClipPlane + 0.001f;  // Looking from neg to pos Z;
                float finalAngle = 180 + (-fAngle * Mathf.Rad2Deg) + (flagHomeViewportPos.z < myCamera.nearClipPlane ? 0 : 180);
                flagHomeArrowArrow.localEulerAngles = new Vector3(0.0f, 0.0f, finalAngle);
                flagHomeArrowIconOutline.position = myCamera.ViewportToWorldPoint(flagHomeArrowPos);
                flagHomeArrowArrow.position = flagHomeArrowIconOutline.position;
                break;

            case FlagArrowState.activated_OnScreen:
                float distToFlagHome = (flagHomeTransform.position - myPlayerMov.transform.position).magnitude;
                distToFlagHome = Mathf.Clamp(distToFlagHome, maxProportionWOCMinDist, minProportionWOCMaxDist) - maxProportionWOCMinDist;
                float totalDist = minProportionWOCMaxDist - maxProportionWOCMinDist;
                float progress = distToFlagHome / totalDist;
                float totalPropDif = maxProportionWhenOnCamera - minProportionWhenOnCamera;
                float currentProportion = maxProportionWhenOnCamera - (progress * totalPropDif);
                //Debug.LogWarning("currentProportion = "+ currentProportion + "; progress = "+ progress + "; distToFlag = "+ distToFlag + "; totalDist = "+ totalDist + "; totalPropDif = " + totalPropDif);

                flagHomeArrowIconOutline.localScale = new Vector3(flagHomeArrowIconOriginalProportion.x * currentProportion, flagHomeArrowIconOriginalProportion.y * currentProportion, 1);
                Vector3 flagHomeArrowViewportPos = myCamera.WorldToViewportPoint(flagHomeArrowFlagHomePos);
                flagHomeArrowViewportPos.z = myCamera.nearClipPlane + 0.001f;
                flagHomeArrowIconOutline.position = myCamera.ViewportToWorldPoint(flagHomeArrowViewportPos);
                //flagArrowFixedRing.position = flagArrowWhale.position;
                break;
        }
    }

    void ActivateFlagHomeArrowOffScreen()
    {
        if (flagHomeArrowSt != FlagArrowState.activated_OffScreen)
        {
            DeactivateFlagHomeArrowOnScreen();
            //if (!myPlayerMov.disableAllDebugs) Debug.LogError("Activate FLAG HOME ARROW OFF SCREEN");
            if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("Activate FLAG HOME ARROW OFF SCREEN");
            flagHomeArrowSt = FlagArrowState.activated_OffScreen;
            flagHomeArrowIconOutline.gameObject.SetActive(true);
            flagHomeArrowArrow.gameObject.SetActive(true);
            //ArrowToFlagSetTeamColorsAndRing();
            FlagHomeArrowSetTeamColors();
        }
    }

    void DeactivateFlagHomeArrowOffScreen()
    {
        if (flagHomeArrowSt == FlagArrowState.activated_OffScreen)
        {
            //if (!myPlayerMov.disableAllDebugs) Debug.LogError("Deactivate FLAG HOME ARROW OFF SCREEN");
            if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("Deactivate FLAG HOME ARROW OFF SCREEN");
            flagHomeArrowSt = FlagArrowState.deactivated;
            flagHomeArrowIconOutline.gameObject.SetActive(false);
            flagHomeArrowArrow.gameObject.SetActive(false);
        }
    }

    void ActivateFlagHomeArrowOnScreen()
    {
        if (flagHomeArrowSt != FlagArrowState.activated_OnScreen)
        {
            DeactivateFlagHomeArrowOnScreen();
            //if (!myPlayerMov.disableAllDebugs) Debug.LogError("Activate FLAG HOME ARROW ON SCREEN");
            if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("Activate FLAG HOME ARROW ON SCREEN");
            flagHomeArrowSt = FlagArrowState.activated_OnScreen;
            flagHomeArrowArrow.gameObject.SetActive(false);
            flagHomeArrowIconOutline.gameObject.SetActive(true);
            //flagArrowWhale.localScale = new Vector3(flagArrowWhale.localScale.x * 0.8f, flagArrowWhale.localScale.y * 0.8f, 1);
            FlagHomeArrowSetTeamColors();
        }
    }

    void DeactivateFlagHomeArrowOnScreen()
    {
        if (flagHomeArrowSt == FlagArrowState.activated_OnScreen)
        {
            //if (!myPlayerMov.disableAllDebugs) Debug.LogError("Deactivate FLAG HOME ARROW ON SCREEN");
            if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("Deactivate FLAG HOME ARROW ON SCREEN");
            flagHomeArrowSt = FlagArrowState.deactivated;
            flagHomeArrowIconOutline.localScale = new Vector3(flagHomeArrowIconOriginalProportion.x, flagHomeArrowIconOriginalProportion.y, 1);
            flagHomeArrowArrow.gameObject.SetActive(false);
            flagHomeArrowIconOutline.gameObject.SetActive(false);
        }
    }

    void FlagHomeArrowSetTeamColors()
    {
        switch (flagHomeArrowSt)
        {
            case FlagArrowState.activated_OffScreen:
                //if (!flagHomeArrowArrow.gameObject.activeInHierarchy) flagHomeArrowArrow.gameObject.SetActive(true);
                flagHomeArrowArrow.GetComponent<Image>().color = flag.currentOwner.GetComponent<PlayerMovement>().team == Team.A ?
                        arrowToFlagGlowTeamColors[0] : arrowToFlagGlowTeamColors[1];
                flagHomeArrowIcon.GetComponent<Image>().color = flag.currentOwner.GetComponent<PlayerMovement>().team == Team.A ?
arrowToFlagGlowTeamColors[0] : arrowToFlagGlowTeamColors[1];
                //flagArrowPickFlagGlowAnim.rect.GetComponent<Image>().color = flag.currentOwner.GetComponent<PlayerMovement>().team == Team.A ?
                //    arrowToFlagGlowTeamColors[0] : arrowToFlagGlowTeamColors[1];
                break;
            case FlagArrowState.activated_OnScreen:
                //if (flagHomeArrowArrow.gameObject.activeInHierarchy) flagArrowFixedRing.gameObject.SetActive(false);
                flagHomeArrowIcon.GetComponent<Image>().color = flag.currentOwner.GetComponent<PlayerMovement>().team == Team.A ?
arrowToFlagGlowTeamColors[0] : arrowToFlagGlowTeamColors[1];
                break;
        }
    }

    #endregion

    #region --- GRAPPLE//HOOK POINT ---

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

    void AddHookPointHUD(HookPoint hookPoint)
    {
        if (!myPlayerMov.disableAllDebugs) print("CREATE NEW hookPointHUDObject");
        GameObject hookPointHUDObject = Instantiate(hookPointHUDPrefab, transform, false);
        hookPointHUDObject.transform.SetParent(hookPointsParent, true);
        HookPointHUDInfo auxHookPointHUDInfo = new HookPointHUDInfo(hookPointHUDObject.GetComponent<HookPointHUD>());
        auxHookPointHUDInfo.hookPointHUD.KonoAwake(hookPoint, myUICamera, myCanvas);
        auxHookPointHUDInfo.showing = true;
        hookPointHUDInfoList.Add(auxHookPointHUDInfo);
    }

    public void ShowHookPointHUD(HookPoint hookPoint)
    {
        bool found = false;
        for (int i = 0; i < hookPointHUDInfoList.Count && !found; i++)
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

    public void SetChosenHookPointHUD(HookPoint hookPoint)
    {
        if (!myPlayerMov.disableAllDebugs) print("SetChosenHookPointHUD(" + hookPoint.name + ")");
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

    #endregion

    #region --- CAMERA VFX ---
    // --- EFECTOS DE CÁMARA ---

    void SetupCameraVFX()
    {
        if (!CameraVFXParent.gameObject.activeInHierarchy)
        {
            CameraVFXParent.gameObject.SetActive(true);
        }
        CameraVFXAwakes();
    }

    void CameraVFXAwakes()
    {
        for (int i = 0; i < cameraVFX.Length; i++)
        {
            cameraVFX[i].KonoAwake();
        }
    }

    void StartCamVFX(CameraVFX camVFX)
    {
        camVFX.Activate();
        currentCameraVFX = camVFX;
    }

    public void StartCamVFX(CameraVFXType camVFXType)
    {
        for (int i = 0; i < cameraVFX.Length; i++)
        {
            if (cameraVFX[i].effectType == camVFXType)
            {
                if (currentCameraVFX != null)
                {
                    if (cameraVFX[i].priority >= currentCameraVFX.priority)
                    {
                        StopCamVFX(currentCameraVFX);
                        StartCamVFX(cameraVFX[i]);
                    }
                }
                else
                {
                    StartCamVFX(cameraVFX[i]);
                }
            }
        }
    }

    public void StopCamVFX(CameraVFXType camVFXType)
    {
        for (int i = 0; i < cameraVFX.Length; i++)
        {
            if (cameraVFX[i].effectType == camVFXType)
            {
                cameraVFX[i].Deactivate();
            }
        }
        currentCameraVFX = null;
    }

    public void StopCamVFX(CameraVFX camVFX)
    {
        for (int i = 0; i < cameraVFX.Length; i++)
        {
            if (cameraVFX[i] == camVFX)
            {
                cameraVFX[i].Deactivate();
            }
        }
        currentCameraVFX = null;
    }

    #endregion

    #region --- DASH HUD ---
    // --- DASH HUD --- 

    void SetupDashHUD()
    {
        dashHUDTotalDist = Mathf.Abs(dashHUDStartPos.localPosition.y - dashHUDEndPos.localPosition.y);
        SetDashHUDProgress(1);
        dashHUDBackground.color = dashHUDBackgroundColor[0];
        dashHUDMinLine.color = dashHUDMinLineColor[0];
        dashHUDEdges.color = dashHUDEdgesColor[0];

        float cameraProportion = Mathf.Min(myUICamera.rect.width, myUICamera.rect.height);
        dashHUDCantDoAnimStarted = false;
    }

    void UpdateDashHUDFuel()
    {
        if (myPlayerMov.boostReady)
        {
            dashHUDBackground.color = dashHUDBackgroundColor[0];
            dashHUDMinLine.color = dashHUDMinLineColor[0];
        }
        else
        {
            dashHUDBackground.color = dashHUDBackgroundColor[1];
            if (myPlayerMov.boostCurrentFuel < myPlayerMov.boostMinFuelNeeded * myPlayerMov.boostCapacity)
            {
                dashHUDMinLine.color = dashHUDMinLineColor[1];
            }
            else
            {
                dashHUDMinLine.color = dashHUDMinLineColor[0];
            }
        }
        float progress = myPlayerMov.boostCurrentFuel / myPlayerMov.boostCapacity;
        SetDashHUDProgress(progress);
    }

    void SetDashHUDProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        float currentY = progress * dashHUDTotalDist;
        dashHUDWater.localPosition = new Vector3(dashHUDStartPos.localPosition.x, dashHUDStartPos.localPosition.y + currentY, 1);
    }

    public void StartDashHUDCantDoAnimation()
    {
        if (!dashHUDCantDoAnimStarted)
        {
            if (!myPlayerMov.disableAllDebugs) print("Start DashHUD Cant Use Animation");
            dashHUDCantDoAnimStarted = true;
            dashHUDCantDoAnimTime = 0;
            dashHUDEdges.color = dashHUDEdgesColor[1];
            GameInfo.instance.StartAnimation(dashHUDCantDoAnimation, myUICamera);
        }
    }

    void ProcessDashHUDCantDoAnimation()
    {
        if (dashHUDCantDoAnimStarted)
        {
            dashHUDCantDoAnimTime += Time.deltaTime;
            if (dashHUDCantDoAnimTime >= dashHUDCantDoAnimation.duration)
            {
                StopDashHUDCantDoAnimation();
            }
        }
    }

    public void StopDashHUDCantDoAnimation()
    {
        if (dashHUDCantDoAnimStarted)
        {
            dashHUDCantDoAnimStarted = false;
            dashHUDEdges.color = dashHUDEdgesColor[0];
        }
    }
    #endregion

    #region --- HOOK ---
    // --- Hook ---

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
    #endregion

    #region --- HOOK CD AND BOOST CD ---
    // --- Hook CD and Boost CD ---

    void SetUpHookUI()
    {
        Hook.fillAmount = 1;
    }

    public void SetHookUI(float f)
    {
        //print("SetHookUI: fillAmout= " + f);
        Hook.fillAmount = Mathf.Clamp01(f);
    }

    public void SetBoostUI(float f)
    {
        //print("SetBoostUI: fillAmout= " + f);
        Boost.fillAmount = Mathf.Clamp01(f);
    }
    #endregion

    #region --- SKILLS ---
    public void SetupSkillsHUD(WeaponSkill[] equippedSkills)
    {
        skillHUDCantDoAnimTime = new float[equippedSkills.Length];
        skillHUDCantDoAnimStarted = new bool[equippedSkills.Length];
        for (int i = 0; i < equippedSkills.Length; i++)
        {
            skillHUDIcons[i].sprite = equippedSkills[i].myWeaponSkillData.weaponSkillHUDImage;
            skillHUDIcons[i].color = Color.white;
            skillHUDProgressBars[i].fillAmount = 1;
            skillHUDCantDoAnimTime[i] = 0;
            skillHUDCantDoAnimStarted[i] = false;
            SetSkillHUDColors(i);
            //orangeCircleAnimation[i].gameObject.SetActive(false);
        }
    }

    void ProcessSkillsCD()
    {
        for (int i = 0; i < myPlayerCombat.equipedWeaponSkills.Length; i++)
        {
            WeaponSkill skill = myPlayerCombat.equipedWeaponSkills[i];
            switch (skill.weaponSkillSt)
            {
                case WeaponSkillState.cd:
                    float progress = skill.currentCDTime / skill.myWeaponSkillData.cd;
                    //Set Progress bar
                    skillHUDProgressBars[i].fillAmount = progress;
                    break;
                case WeaponSkillState.active:
                    //if (orangeCircleAnimation[i].)
                    //{

                    //}
                    break;
                    //case WeaponSkillState.ready:
                    //    break;
            }
        }
    }

    void SetSkillHUDColors(int index)
    {
        switch (myPlayerCombat.equipedWeaponSkills[index].weaponSkillSt)
        {
            case WeaponSkillState.ready:
                skillHUDIcons[index].color = skillHUDIconColors[0];
                skillHUDBackgrounds[index].color = skillHUDBackgroundColors[0];
                break;
            case WeaponSkillState.cd:
                skillHUDIcons[index].color = skillHUDIconColors[1];
                skillHUDBackgrounds[index].color = skillHUDBackgroundColors[1];
                break;
            case WeaponSkillState.active:
                skillHUDIcons[index].color = skillHUDIconColors[2];
                skillHUDBackgrounds[index].color = skillHUDBackgroundColors[2];
                break;
        }
    }

    public void StartSkillActive(int index)
    {
        skillHUDProgressBars[index].fillAmount = 1;
        SetSkillHUDColors(index);
        GameInfo.instance.StartAnimation(skillHUDActiveGlow[index], myUICamera);
        //orangeCircleAnimation[index].gameObject.SetActive(true);
        orangeCircleAnimation[index].SetTrigger("startAnim");
    }

    public void StartSkillCD(int index)
    {
        GameInfo.instance.StopUIAnimation(skillHUDActiveGlow[index]);
        skillHUDProgressBars[index].fillAmount = 0;
        SetSkillHUDColors(index);
        //orangeCircleAnimation[index].gameObject.SetActive(false);
    }

    public void SetSkillReady(int index)
    {
        skillHUDProgressBars[index].fillAmount = 1;
        SetSkillHUDColors(index);
        GameInfo.instance.StartAnimation(skillHUDActivationGlow[index], myUICamera);
    }

    public void StartSkillCantDoAnim(int index)
    {
        Debug.Log("INDEX = " + index + "; skillHUDCantDoAnimStarted[].Lenght = " + skillHUDCantDoAnimStarted.Length);
        if (!skillHUDCantDoAnimStarted[index])
        {
            skillHUDCantDoAnimStarted[index] = true;
            skillHUDCantDoAnimTime[index] = 0;
            skillHUDIcons[index].color = skillHUDIconColors[3];
            skillHUDBackgrounds[index].color = skillHUDBackgroundColors[3];
            GameInfo.instance.StartAnimation(skillHUDCantDoAnimations[index], myUICamera);
        }
    }

    void ProcessSkillCantDoAnim()
    {
        for (int i = 0; i < skillHUDCantDoAnimations.Length; i++)
        {
            if (skillHUDCantDoAnimStarted[i])
            {
                skillHUDCantDoAnimTime[i] += Time.deltaTime;
                if (skillHUDCantDoAnimTime[i] >= skillHUDCantDoAnimations[i].duration)
                {
                    StopSkillCantDoAnim(i);
                }
            }
        }
    }

    void StopSkillCantDoAnim(int index)
    {
        if (skillHUDCantDoAnimStarted[index])
        {
            skillHUDCantDoAnimStarted[index] = false;
            SetSkillHUDColors(index);
        }
    }

    #endregion

    #endregion

    #region ----[ PUN CALLBACKS ]----
    #endregion

    #region ----[ RPC ]----

    #endregion

    #region ----[ NETWORK FUNCTIONS ]----
    #endregion

    #region ----[ IPUNOBSERVABLE ]----
    #endregion
}

#region ----[ STRUCTS & CLASSES ]----
[System.Serializable]
public class CameraVFX
{
    public CameraVFXType effectType;
    public GameObject prefab;
    [Tooltip("The higher, the bigger prority")]
    public int priority;
    public float speed = 1;

    public void KonoAwake()
    {
        prefab.GetComponent<Animator>().speed = speed;
        prefab.SetActive(false);
    }

    public void Activate()
    {
        if (!prefab.activeInHierarchy)
        {
            //Debug.Log("ACTIVATE CAMERA VFX : " + effectType);
            prefab.SetActive(true);
            //prefab.GetComponent<Animator>().Play("dashCameraVFX",0);
        }
    }

    public void Deactivate()
    {
        if (prefab.activeInHierarchy)
        {
            prefab.SetActive(false);
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
//public enum HUDSkillState
//{
//    cd,
//    ready
//}
//public class HUDSkill
//{
//    public HUDSkillState hudSkillSt = HUDSkillState.ready;
//    public float currentCD = 0;
//    public void StartCD()
//    {
//        if(hudSkillSt == HUDSkillState.ready)
//        currentCD = 0;
//    }
//}
#endregion


