using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using UnityEngine.UI;

//Esta clase es para guardar datos del juego entre escenas
public enum UIAnimType
{
    none,
    shake,
    color_alpha,
    scale,
    movement
}
public class GameInfo : MonoBehaviour
{
    public static GameInfo instance;
    public GameMode currentGameMode;
    public GameObject inControlManager;
    [HideInInspector]
    public RenController currentRenCont = null;
    RenController lastRenCont = null;
    public List<PlayerActions> playerActionsList;
    public List<Team> playerTeamList;
    public int nPlayers;
    [HideInInspector]
    public bool gameIsPaused = false;

    //1 player & online controls
    //PlayerActions keyboardListener;
    //PlayerActions joystickListener;
    public PlayerActions myControls;

    List<UIAnimation> uIAnimations;

    GameObject[] goList;

    public void Awake()
    {
        goList = GameObject.FindGameObjectsWithTag(tag);
        if (goList.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        //Application.targetFrameRate = 60;
        DontDestroyOnLoad(this);
        instance = this;
        currentGameMode = GameMode.None;
        playerActionsList = new List<PlayerActions>();
        playerTeamList = new List<Team>();

        uIAnimations = new List<UIAnimation>();
        myControls = PlayerActions.CreateDefaultBindings();
        inControlManager = FindObjectOfType<InControlManager>().gameObject;
    }

    private void Update()
    {
        //UpdateControls();
        ProcessUIAnimations();
        //if (myControls.L1.WasPressed) Debug.Log("L1 was pressed");
    }

    void OnEnable()
    {
        //keyboardListener = PlayerActions.CreateWithKeyboardBindings();
        //joystickListener = PlayerActions.CreateWithJoystickBindings();
    }

    void OnDisable()
    {
        //joystickListener.Destroy();
        //keyboardListener.Destroy();
    }

    public Team NoneTeamSelect()
    {
        int nAzul = 0;
        int nRojo = 0;
        foreach (Team t in playerTeamList)
        {
            if (t == Team.A)
                nAzul++;
            else if (t == Team.B)
                nRojo++;
        }
        if (nAzul == nRojo) //Mismo num de jugadores rojos que azules, random
        {
            if (Random.value < 0.5f)
                return Team.A;
            else
                return Team.B;
        }
        else if (nAzul > nRojo)  //mas numero de jugadores azules
            return Team.B;
        else                     //mas numero de jugadores rojos
            return Team.A;
    }

    public void PrintTeamList()
    {
        string print = "GameInfo PlayerTeamList: (";
        for (int i = 0; i < playerTeamList.Count; i++)
        {
            print += playerTeamList[i];
            if (i + 1 < playerTeamList.Count) print += ",";
        }
        print += ")";
        Debug.Log(print);
    }

    #region Controls
    //bool ButtonWasPressedOnListener(PlayerActions actions)
    //{
    //    return actions.AnyButtonWasPressed();
    //}

    void UpdateControls()
    {
        //if (ButtonWasPressedOnListener(joystickListener))
        //{
        //    Debug.Log("NEW CONTROLS: Joystick");
        //    Debug.Log("Input Device = " + InputManager.ActiveDevice.Name);
        //    InputDevice inputDevice = InputManager.ActiveDevice;
        //    SetMyControls(inputDevice);
        //}

        //if (ButtonWasPressedOnListener(keyboardListener))
        //{
        //    Debug.Log("NEW CONTROLS: Keyboard");
        //    Debug.Log("Input Device = " + InputManager.ActiveDevice.Name);
        //    SetMyControls(null);
        //}
    }

    public void ErasePlayerControls()
    {
        while (playerActionsList.Count > 0)
        {
            playerActionsList[playerActionsList.Count - 1].Destroy();
            playerActionsList.RemoveAt(playerActionsList.Count - 1);
        }
    }

    //NO SE USA
    //void SetMyControls(InputDevice inputDevice)
    //{
    //    if (inputDevice == null)
    //    {
    //        if (myControls.controlsType != InputDeviceClass.Keyboard)
    //        {
    //            myControls = PlayerActions.CreateWithKeyboardBindings();
    //        }
    //    }
    //    else
    //    {
    //        PlayerActions actions = PlayerActions.CreateWithJoystickBindings();
    //        actions.Device = inputDevice;
    //        myControls = actions;
    //    }
    //}
    #endregion

    #region UIAnimations

    public void StartAnimation(UIAnimation uIAnimation, Camera canvasCamera)
    {
        for (int i = 0; i < uIAnimations.Count; i++)
        {
            if (uIAnimations[i].rect == uIAnimation.rect)
            {
                Debug.LogError("UIAnimation Error: you are trying to animate the same RectTransform at the same time with " +
                    "more than 1 animation. Are you sure this is correct?");
            }
        }
        if (!uIAnimations.Contains(uIAnimation))
        {
            if (canvasCamera != null)
            {
                float prop = Mathf.Min(canvasCamera.rect.width, canvasCamera.rect.height);
                uIAnimation.currentXAmplitude = uIAnimation.xAmplitude * prop;
            }
            uIAnimations.Add(uIAnimation);
            uIAnimation.StartAnimation();
        }
        else
        {
            uIAnimation.RestartAnimation();
        }
    }

    void ProcessUIAnimations()
    {
        for (int i = 0; i < uIAnimations.Count; i++)
        {
            if (!uIAnimations[i].ProcessAnimation())
            {
                uIAnimations.RemoveAt(i);
            }
        }
    }

    public void StopUIAnimation(UIAnimation uIAnimation)
    {
        for (int i = 0; i < uIAnimations.Count; i++)
        {
            if (uIAnimations[i] == uIAnimation)
            {
                uIAnimations[i].StopAnimation();
                uIAnimations.RemoveAt(i);
            }
        }
    }
    #endregion

    #region RenController
    public void SetRenController(RenController newRenController)
    {
        lastRenCont = currentRenCont;

        if (currentRenCont != null) currentRenCont.disabled = true;
        newRenController.disabled = false;
        currentRenCont = newRenController;
    }

    public void QuitRenController()
    {
        if (currentRenCont != null) currentRenCont.disabled = true;
        currentRenCont = null;
    }

    public void ReturnToLastRenController()
    {
        RenController newLastRenCont = currentRenCont;

        if (currentRenCont != null) currentRenCont.disabled = true;
        if (lastRenCont != null) lastRenCont.disabled = false;
        currentRenCont = lastRenCont;

        lastRenCont = newLastRenCont;
    }
    #endregion
}

[System.Serializable]
public class UIAnimation
{
    public UIAnimType type = UIAnimType.none;
    [HideInInspector] public bool playing = false;
    public RectTransform rect;
    public float duration = 0.5f;
    public bool endless = false;
    public bool cycleAnimDir = true;
    public float frequency = 0.06f;
    [Range(0, 1)]
    public float cycleStartPoint = 0.5f;
    int animDir;//1 going right; -1 going left
    public Ease easeFunction = Ease.None;

    [Header("--- SHAKE ---")]
    public float xAmplitude = 7f;
    [HideInInspector] public float currentXAmplitude;
    float currentDuration, currentCycleTime, totalSpace;
    Vector3 originalLocalPos;

    [Header("--- COLOR_ALPHA ---")]
    [Range(0, 1)]
    public float alphaMin = 0;
    [Range(0, 1)]
    public float alphaMax = 1;
    Image image;

    [Header("--- SCALE ---")]
    public float initialScale;
    public float finalScale;
    private Vector2 originalScale;

    [Header("--- MOVEMENT ---")]
    public Vector2 initialPos;
    public Vector2 finalPos;
    private Vector2 originalPos;


    public UIAnimation(UIAnimType _type, ref RectTransform _rect, float _xAmplitude = 7f, float _frequency = 0.06f,
        float _duration = 0.5f, float _cycleStartPoint = 0.5f)
    {
        type = _type;
        rect = _rect;
        xAmplitude = _xAmplitude;
        currentXAmplitude = xAmplitude;
        frequency = _frequency;//cycle max time
        duration = _duration;
        _cycleStartPoint = Mathf.Clamp01(_cycleStartPoint);
        cycleStartPoint = _cycleStartPoint;
        StartAnimation(); // Es necesario???
    }

    public void StartAnimation()
    {
        if (!playing)
        {
            playing = true;
            if (frequency == 0 && duration > 0) frequency = duration;
            if (duration == 0 && frequency > 0) duration = frequency;
            currentDuration = 0;
            currentCycleTime = cycleStartPoint * frequency;
            animDir = 1;
            switch (type)
            {
                case UIAnimType.shake:
                    totalSpace = currentXAmplitude * 2;
                    originalLocalPos = rect.localPosition;
                    //Debug.LogWarning(" currentxAmplitude= " + currentxAmplitude + "; totalSpace = " + totalSpace);
                    break;
                case UIAnimType.color_alpha:
                    image = rect.GetComponent<Image>();
                    break;
                case UIAnimType.scale:
                    //totalScaleAmplitude = Mathf.Abs(initialScale - finalScale);
                    originalScale = new Vector2(rect.localScale.x, rect.localScale.y);
                    //Debug.LogWarning("SCALEANIM: originalScale= " + originalScale);

                    break;
                case UIAnimType.movement:
                    //totalAmplitude = new Vector2(Mathf.Abs(initialPos.x - finalPos.x), Mathf.Abs(initialPos.y - finalPos.y));
                    originalPos = rect.localPosition;
                    //Debug.LogWarning("MOVEANIM: originalPos= " + originalPos );
                    break;
            }
        }
    }

    public void RestartAnimation()
    {
        animDir = 1;
        currentCycleTime = cycleStartPoint * frequency;
        currentDuration = 0;
        switch (type)
        {
            case UIAnimType.shake:
                break;
            case UIAnimType.color_alpha:
                break;
            case UIAnimType.scale:
                break;
            case UIAnimType.movement:
                break;
        }
    }

    public bool ProcessAnimation()
    {
        //Debug.Log("ProcessAnimation: playing = " + playing);
        if (playing)
        {
            float progress = currentCycleTime / frequency;
            switch (type)
            {
                case UIAnimType.shake:
                    float xIncrement = progress * totalSpace * animDir;
                    //float xIncrement = animDir * EasingFunction.EaseInOutQuart(0, xAmplitude, progress);
                    float originX = originalLocalPos.x + (currentXAmplitude * -animDir);
                    Vector3 finalPosition = originalLocalPos;
                    finalPosition.x = originX + xIncrement;
                    rect.localPosition = finalPosition;
                    //Debug.Log("xIncrement = "+ xIncrement + "; totalSpace = "+ totalSpace + "; progress = " + progress+ "; originX = " + originX+ "; finalPos.x ="+ finalPos.x);
                    break;
                case UIAnimType.color_alpha:
                    progress = animDir == 1 ? progress : 1 - progress;
                    Color newColor = image.color;
                    float value = EasingFunction.SelectEasingFunction(easeFunction, alphaMin, alphaMax, progress);
                    //float aIncrement = value * animDir;
                    //float alphaStartPoint = animDir == 1?alphaMin:alphaMax;
                    newColor.a = value;
                    image.color = newColor;
                    //Debug.Log("Progress = " + progress + "; value = " + value);
                    break;
                case UIAnimType.scale:
                    progress = animDir == 1 ? progress : 1 - progress;
                    value = EasingFunction.SelectEasingFunction(easeFunction, initialScale, finalScale, progress);
                    rect.localScale = new Vector3(originalScale.x * value, originalScale.y * value, 1);
                    //Debug.Log("SCALENANIM: Progress = " + progress + "; value = " + value+ "; originalScale.x = " + originalScale.x + "; originalScale.y = " + originalScale.y);
                    break;
                case UIAnimType.movement:
                    progress = animDir == 1 ? progress : 1 - progress;
                    float xVal = EasingFunction.SelectEasingFunction(easeFunction, initialPos.x, finalPos.x, progress);
                    float yVal = EasingFunction.SelectEasingFunction(easeFunction, initialPos.y, finalPos.y, progress);

                    rect.localPosition = new Vector3(originalPos.x + xVal, originalPos.y + yVal, rect.localPosition.z);
                    //Debug.Log("MOVEANIM: Progress = " + progress + "; xVal = " + xVal+ "; yVal = " + yVal);
                    break;
            }

            //CHANGE ANIM DIR
            currentCycleTime += Time.deltaTime;
            if (cycleAnimDir)
            {
                if (currentCycleTime >= frequency)
                {
                    animDir = animDir == 1 ? -1 : 1;
                    currentCycleTime = 0;
                }
            }

            //END
            currentDuration += Time.deltaTime;
            if (!endless && currentDuration >= duration)
            {
                StopAnimation();
                return false;
            }
        }
        return true;
    }

    public void StopAnimation()
    {
        if (playing)
        {
            playing = false;
            if (currentDuration >= duration)
            {
                currentDuration = duration;
            }
            switch (type)
            {
                case UIAnimType.shake:
                    rect.localPosition = originalLocalPos;
                    break;
                case UIAnimType.color_alpha:
                    Color newColor = image.color;
                    newColor.a = 0;
                    image.color = newColor;
                    break;
                case UIAnimType.scale:
                    rect.localScale = new Vector3(originalScale.x * finalScale, originalScale.y * finalScale, 1);
                    break;
                case UIAnimType.movement:
                    rect.localPosition = finalPos;
                    break;
            }
        }
    }
}
