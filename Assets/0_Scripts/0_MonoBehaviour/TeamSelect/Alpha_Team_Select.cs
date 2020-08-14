using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using InControl;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public enum TeamSelectMenuState
{
    ChoosingMapVariation,
    ChoosingNumberOfPlayers,
    ChoosingTeam
}
public class Alpha_Team_Select : MonoBehaviour
{
    public static Alpha_Team_Select instance;
    public RenController myRenCont;
    //Variables
    //public GameObject stockGameInfo;
    //public GameObject stockInControlManager;
    public GameObject menusCanvas;


    PlayerActions keyboardListener;
    PlayerActions joystickListener;

    TeamSelectMenuState teamSelectMenuSt = TeamSelectMenuState.ChoosingMapVariation;

    [Header("--- MENU SETTINGS && VARIABLES---")]
    [Range(0, 1)]
    public float deadzone;
    public Camera selectNumberOfPlayersCam;
    public RenButton[] chooseMapVariationButtons;
    public RenButton[] chooseNumOfPlayersButtons;
    public float scaleSpriteBig;
    [Header("--- NUMBER OF PLAYERS MENU ---")]
    public Transform[] numPlayerSprites;
    public Transform numPlayerSpritesParent;
    public SelectPlayer[] selectPlayers;
    //public GameObject selectNPlayersCamera;
    //public bool cameraSet = false;
    int playersJoined = 0;
    int offlineMaxPlayers = 4;
    int nPlayers = 1;

    //MAP VARIATIONS MENU
    [HideInInspector] public static bool startFromMap = false;
    [HideInInspector] public static string startFromMapScene;
    [Header("--- MAP VARIATIONS MENU ---")]
    public Transform[] mapVariationsSprites;
    public string[] tutorialMapVariationNames;
    public string[] CTWMapVariationNames;
    public string[] HubVariationNames;
    string[] mapVariationNames;
    public Transform mapVariationsSpritesParent;
    public string CTWHigh;
    public string CTWLOD;
    int currentMapIndex = 0;


    private void Awake()
    {
        instance = this;
        //if (GameInfo.instance == null)
        //{
        //    stockInControlManager.SetActive(true);
        //    //stockInControlManager.GetComponent<InControlManager>().OnEnable();
        //    stockGameInfo.SetActive(true);
        //    // stockGameInfo.GetComponent<GameInfo>().Awake();

        //}

        menusCanvas.SetActive(true);
        selectNumberOfPlayersCam.gameObject.SetActive(true);
        for (int i = 0; i < selectPlayers.Length; i++)
        {
            selectPlayers[i].myCamera.gameObject.SetActive(false);
            selectPlayers[i].myCanvas.gameObject.SetActive(false);
            selectPlayers[i].deadzone = deadzone;
        }
    }

    void Start()
    {
        if (GameInfo.instance != null)
        {
            if(GameInfo.instance.inControlManager == null)
            GameInfo.instance.inControlManager = GameObject.Find("InControl manager");
            GameInfo.instance.playerTeamList.Clear();
        }
        else
        {
            Debug.LogError("Error: GameInfo is null or has not done it's awake yet. It should be awaken and ready.");
        }

        numPlayerSpritesParent.gameObject.SetActive(false);
        mapVariationsSpritesParent.gameObject.SetActive(true);

        //Debug.Log("Gamemode = " + GameInfo.instance.currentGameMode);
        switch (GameInfo.instance.currentGameMode)
        {
            case GameMode.Tutorial:
                chooseMapVariationButtons[1].DisableButtonsAndText();
                chooseMapVariationButtons[0].targetTexts[0].text = "Tutorial";
                Debug.Log("tutorialMapVariationNames = (" + tutorialMapVariationNames[0] + ")");
                mapVariationNames = tutorialMapVariationNames;
                break;
            case GameMode.CaptureTheFlag:
                mapVariationNames = CTWMapVariationNames;
                break;
            case GameMode.Hub:
                chooseMapVariationButtons[1].DisableButtonsAndText();
                chooseMapVariationButtons[0].targetTexts[0].text = "Hub";
                mapVariationNames = HubVariationNames;
                break;
            default:
                mapVariationNames = CTWMapVariationNames;
                break;
        }
        //Debug.Log("mapVariationNames = ("+ mapVariationNames [0] + ")");
        //MakeSpriteBig(mapVariationsSprites[currentMapIndex]);

    }

    void Update()
    {
        //if (GameInfo.instance.myControls.A.WasPressed) Debug.Log("A WAS PRESSED");
        switch (teamSelectMenuSt)
        {
            case TeamSelectMenuState.ChoosingMapVariation:
                if (myRenCont.currentControls.B.WasReleased)
                {
                    SceneManager.LoadScene("Demo_MainMenu 1");
                }
                break;

            case TeamSelectMenuState.ChoosingNumberOfPlayers:
                if (myRenCont.currentControls.B.WasReleased)
                {
                    BackToChooseMapVariation();
                }
                break;
            case TeamSelectMenuState.ChoosingTeam:

                if(playersJoined < nPlayers)
                {
                    if (JoinButtonWasPressedOnListener(joystickListener))
                    {
                        InputDevice inputDevice = InputManager.ActiveDevice;

                        if (ThereIsNoPlayerUsingJoystick(inputDevice))
                        {
                            CreatePlayer(inputDevice);
                        }
                    }

                    if (JoinButtonWasPressedOnListener(keyboardListener))
                    {
                        if (ThereIsNoPlayerUsingKeyboard())
                        {
                            CreatePlayer(null);
                        }
                    }
                }

                //CHECK IF ALL READY
                bool allReady = true;
                for (int i = 0; i < selectPlayers.Length; i++)
                {
                    if (i < nPlayers && selectPlayers[i].teamSelectPlayerSt != TeamSelectPlayerState.Locked)
                    {
                        allReady = false;
                    }
                }
                if (allReady)
                {
                    for (int i = 0; i < playersJoined; i++)
                    {
                        //GameInfo.playerActionsList[i] = players[i].Actions;
                        GameInfo.instance.playerActionsList.Add(selectPlayers[i].myControls);
                        GameInfo.instance.playerTeamList.Add(selectPlayers[i].myTeam);
                        //Debug.Log("ALL READY: adding player of team " + selectPlayers[i].myTeam);
                        //GameInfo.instance.PrintTeamList();
                        //Debug.Log(ps.Actions);
                    }
                    GameInfo.instance.nPlayers = playersJoined;
                    Debug.Log("LOAD GAME ");
                    if (startFromMap)
                    {
                        Debug.Log("Start from map: " + startFromMapScene);
                        SceneManager.LoadScene(startFromMapScene);
                    }
                    else
                    {
                        Debug.Log("Start from menu");
                        switch (GameInfo.instance.currentGameMode)
                        {
                            case GameMode.CaptureTheFlag:
                                switch (currentMapIndex)
                                {
                                    case 1:
                                        if (nPlayers > 1)
                                            SceneManager.LoadScene(CTWLOD);
                                        else
                                        {
                                            SceneManager.LoadScene(CTWHigh);
                                        }
                                        break;
                                    default:
                                        SceneManager.LoadScene(mapVariationNames[currentMapIndex]);
                                        break;
                                }
                                break;
                            case GameMode.Tutorial:
                                Debug.Log("currentMapIndex = " + currentMapIndex);
                                Debug.Log("mapVariationNames.Length = " + mapVariationNames.Length);
                                SceneManager.LoadScene(mapVariationNames[currentMapIndex]);
                                break;
                            default:
                                SceneManager.LoadScene(mapVariationNames[currentMapIndex]);
                                break;
                        }
                    }
                }

                if (myRenCont.currentControls.B.WasReleased)
                {
                    bool allNotJoined = true;
                    for (int i = 0; i < selectPlayers.Length; i++)
                    {
                        if (selectPlayers[i].teamSelectPlayerSt != TeamSelectPlayerState.NotJoined)
                        {
                            allNotJoined = false;
                        }
                    }
                    if (allNotJoined)
                    {
                        BackToChooseNumberOfPlayers();
                    }
                }

                for (int i = 0; i < selectPlayers.Length; i++)
                {
                    selectPlayers[i].KonoUpdate();
                }
                break;
        }
    }

    void OnEnable()
    {
        InputManager.OnDeviceDetached += OnDeviceDetached;
        keyboardListener = PlayerActions.CreateWithKeyboardBindings();
        joystickListener = PlayerActions.CreateWithJoystickBindings();
    }

    void OnDisable()
    {
        InputManager.OnDeviceDetached -= OnDeviceDetached;
        joystickListener.Destroy();
        keyboardListener.Destroy();
    }

    public void LockMapVariation(int mapIndex)
    {
        currentMapIndex = mapIndex;
        numPlayerSpritesParent.gameObject.SetActive(true);
        myRenCont.SetSelectedButton(chooseNumOfPlayersButtons[0]);
        mapVariationsSpritesParent.gameObject.SetActive(false);

        //MakeSpriteBig(numPlayerSprites[nPlayers - 1]);
        switch (GameInfo.instance.currentGameMode)
        {
            case GameMode.CaptureTheFlag:
                break;
            case GameMode.Tutorial:
                chooseNumOfPlayersButtons[1].DisableButtonsAndText();
                chooseNumOfPlayersButtons[2].DisableButtonsAndText();
                chooseNumOfPlayersButtons[3].DisableButtonsAndText();
                break;
        }

        teamSelectMenuSt = TeamSelectMenuState.ChoosingNumberOfPlayers;
        Debug.Log("LOCK MAP VARIATION");
    }

    void BackToChooseMapVariation()
    {
        mapVariationsSpritesParent.gameObject.SetActive(true);
        myRenCont.SetSelectedButton(chooseMapVariationButtons[0]);
        numPlayerSpritesParent.gameObject.SetActive(false);
        //MakeSpriteBig(mapVariationsSprites[currentMapIndex]);
        //switch (GameInfo.instance.currentGameMode)
        //{
        //    case GameMode.Tutorial:
        //        chooseMapVariationButtons[1].DisableButtonsAndText();
        //        chooseMapVariationButtons[0].targetTexts[0].text = "Dummies Tutorial";
        //        Debug.Log("tutorialMapVariationNames = (" + tutorialMapVariationNames[0] + ")");
        //        mapVariationNames = tutorialMapVariationNames;
        //        break;
        //    case GameMode.CaptureTheFlag:
        //        mapVariationNames = CTWMapVariationNames;
        //        break;
        //    default:
        //        mapVariationNames = CTWMapVariationNames;
        //        break;
        //}

        teamSelectMenuSt = TeamSelectMenuState.ChoosingMapVariation;
        Debug.Log("BACK TO CHOOSE MAP VARIATION");
    }

    public void LockSelectNumberOfPlayers(int playerNum)
    {
        nPlayers = playerNum;
        teamSelectMenuSt = TeamSelectMenuState.ChoosingTeam;
        selectNumberOfPlayersCam.gameObject.SetActive(false);
        menusCanvas.gameObject.SetActive(false);
        for (int i = 0; i < nPlayers; i++)
        {
            selectPlayers[i].KonoAwake(nPlayers, i);
        }
        myRenCont.disabled = true;
        Debug.Log("LOCK PLAYER NUM");
    }

    void BackToChooseNumberOfPlayers()
    {
        selectNumberOfPlayersCam.gameObject.SetActive(true);
        menusCanvas.SetActive(true);
        myRenCont.disabled = false;
        myRenCont.SetSelectedButton(chooseNumOfPlayersButtons[0]);
        for (int i = 0; i < selectPlayers.Length; i++)
        {
            selectPlayers[i].ResetSelectPlayer();
            selectPlayers[i].myCamera.gameObject.SetActive(false);
            //selectPlayers[i].myUICamera.gameObject.SetActive(false);
            teamSelectMenuSt = TeamSelectMenuState.ChoosingNumberOfPlayers;
        }
        //MakeSpriteBig(numPlayerSprites[nPlayers - 1]);

        Debug.Log("BACK TO CHOOSE PLAYER NUM");
    }

    bool JoinButtonWasPressedOnListener(PlayerActions actions)
    {
        //Debug.Log("CHECK IF BUTTON WAS PRESSED ON LISTENER");
        return (actions != keyboardListener && (actions.Start.WasPressed)) ||
            (actions == keyboardListener && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Return)));
    }

    SelectPlayer FindPlayerUsingKeyboard()
    {
        for (int i = 0; i < selectPlayers.Length; i++)
        {
            if (selectPlayers[i].myControls == keyboardListener)
            {
                return selectPlayers[i];
            }
        }
        return null;
    }

    SelectPlayer FindPlayerUsingJoystick(InputDevice inputDevice)
    {
        for (int i = 0; i < selectPlayers.Length; i++)
        {
            if (selectPlayers[i].myControls != null && selectPlayers[i].myControls.Device == inputDevice)
            {
                return selectPlayers[i];
            }
        }
        return null;
    }

    bool ThereIsNoPlayerUsingJoystick(InputDevice inputDevice)
    {
        return FindPlayerUsingJoystick(inputDevice) == null;
    }

    bool ThereIsNoPlayerUsingKeyboard()
    {
        return FindPlayerUsingKeyboard() == null;
    }

    void OnDeviceDetached(InputDevice inputDevice)
    {
        SelectPlayer player = FindPlayerUsingJoystick(inputDevice);
        if (player != null)
        {
            RemovePlayer(player);
        }
    }

    SelectPlayer CreatePlayer(InputDevice inputDevice)
    {
        Debug.Log("START CREATE PLAYER?");
        if (playersJoined < offlineMaxPlayers)
        {
            //// Pop a position off the list. We'll add it back if the player is removed.
            //Vector3 playerPosition = playerPositions[0].position;
            //playerPositions.RemoveAt(0);

            //GameObject gameObject = Instantiate(playerPrefab, playerPosition, Quaternion.identity);

            SelectPlayer player = selectPlayers[FindNotJoinedPlayer()];//gameObject.GetComponent<PlayerSelected>();

            if (inputDevice == null)
            {
                // We could create a new instance, but might as well reuse the one we have
                // and it lets us easily find the keyboard player.

                //GameInfo.instance.playerActionsList.Add(keyboardListener);
                //GameInfo.instance.playerActionUno = keyboardListener;
                player.myControls = PlayerActions.CreateWithKeyboardBindings();
            }
            else
            {
                // Create a new instance and specifically set it to listen to the
                // given input device (joystick).
                PlayerActions actions = PlayerActions.CreateWithJoystickBindings();
                actions.Device = inputDevice;

                //GameInfo.instance.playerActionsList.Add(actions);
                player.myControls = actions;
                //player.isAReleased = false;
            }

            //players.Add(player);
            //player.playerSelecionUI = pUI[players.Count - 1];
            //pUI[players.Count - 1].panel.SetActive(true);
            player.JoinTeamSelect();
            playersJoined++;

            Debug.Log("PLAYER CREATED");
            return player;

        }

        return null;
    }

    int FindNotJoinedPlayer()
    {
        for (int i = 0; i < selectPlayers.Length; i++)
        {
            if (selectPlayers[i].teamSelectPlayerSt == TeamSelectPlayerState.NotJoined)
            {
                return i;
            }
        }
        return -1;
    }

    public void RemovePlayer(SelectPlayer player)
    {
        for (int i = 0; i < selectPlayers.Length; i++)
        {
            if (selectPlayers[i] == player)
            {
                RemovePlayer(i);
            }
        }
    }

    public void RemovePlayer(int index)
    {
        selectPlayers[index].myControls = null;
        selectPlayers[index].ExitTeamSelect();
        playersJoined--;
        //playerPositions.Insert(0, player.transform);
        //players.Remove(player);
        //player.Actions = null;
        //Destroy(player.gameObject);
    }

    //void MoveUp()
    //{
    //    switch (teamSelectMenuSt)
    //    {
    //        case TeamSelectMenuState.ChoosingMapVariation:
    //            MakeSpriteSmall(mapVariationsSprites[currentMapIndex]);

    //            switch (currentMapIndex)
    //            {
    //                case 0:
    //                    currentMapIndex = 1;
    //                    break;
    //                case 1:
    //                    currentMapIndex = 0;
    //                    break;
    //            }

    //            MakeSpriteBig(mapVariationsSprites[currentMapIndex]);

    //            break;
    //        case TeamSelectMenuState.ChoosingNumberOfPlayers:
    //            MakeSpriteSmall(numPlayerSprites[nPlayers - 1]);

    //            switch (nPlayers)
    //            {
    //                case 1:
    //                    nPlayers = 3;
    //                    break;
    //                case 2:
    //                    nPlayers = 4;
    //                    break;
    //                case 3:
    //                    nPlayers = 1;
    //                    break;
    //                case 4:
    //                    nPlayers = 2;
    //                    break;
    //            }

    //            MakeSpriteBig(numPlayerSprites[nPlayers - 1]);

    //            break;
    //    }
    //}
    //void MoveDown()
    //{
    //    switch (teamSelectMenuSt)
    //    {
    //        case TeamSelectMenuState.ChoosingMapVariation:
    //            MakeSpriteSmall(mapVariationsSprites[currentMapIndex]);

    //            switch (currentMapIndex)
    //            {
    //                case 0:
    //                    currentMapIndex = 1;
    //                    break;
    //                case 1:
    //                    currentMapIndex = 0;
    //                    break;
    //            }

    //            MakeSpriteBig(mapVariationsSprites[currentMapIndex]);

    //            break;
    //        case TeamSelectMenuState.ChoosingNumberOfPlayers:
    //            MakeSpriteSmall(numPlayerSprites[nPlayers - 1]);

    //            switch (nPlayers)
    //            {
    //                case 1:
    //                    nPlayers = 3;
    //                    break;
    //                case 2:
    //                    nPlayers = 4;
    //                    break;
    //                case 3:
    //                    nPlayers = 1;
    //                    break;
    //                case 4:
    //                    nPlayers = 2;
    //                    break;
    //            }
    //            MakeSpriteBig(numPlayerSprites[nPlayers - 1]);
    //            break;
    //    }
    //}
    //void MoveRight()
    //{
    //    MakeSpriteSmall(numPlayerSprites[nPlayers - 1]);

    //    switch (nPlayers)
    //    {
    //        case 1:
    //            nPlayers = 2;
    //            break;
    //        case 2:
    //            nPlayers = 1;
    //            break;
    //        case 3:
    //            nPlayers = 4;
    //            break;
    //        case 4:
    //            nPlayers = 3;
    //            break;
    //    }
    //    MakeSpriteBig(numPlayerSprites[nPlayers - 1]);

    //}
    //void MoveLeft()
    //{
    //    MakeSpriteSmall(numPlayerSprites[nPlayers - 1]);

    //    switch (nPlayers)
    //    {
    //        case 1:
    //            nPlayers = 2;
    //            break;
    //        case 2:
    //            nPlayers = 1;
    //            break;
    //        case 3:
    //            nPlayers = 4;
    //            break;
    //        case 4:
    //            nPlayers = 3;
    //            break;
    //    }
    //    MakeSpriteBig(numPlayerSprites[nPlayers - 1]);
    //}

    // public void SelectMapVariation()
    // {

    // }

    //public void SelectPlayerNum(int playerNum)
    // {
    //     nPlayers = playerNum;
    // }

    public void MakeSpriteBig(Transform spriteTransform)
    {
        //Debug.Log("MAKE SPRITE BIG -> " + spriteTransform.name);
        spriteTransform.localScale *= scaleSpriteBig;
    }
    public void MakeSpriteSmall(Transform spriteTransform)
    {
        //Debug.Log("MAKE SPRITE SMALL -> " + spriteTransform.name);
        spriteTransform.localScale /= scaleSpriteBig;
    }

    void SureExit()
    {
        // Display: "Are u sure to exit?" at any point.
    }
}

public enum TeamSelectPlayerState
{
    NotJoined,
    SelectingTeam,
    SelectingCharacter,
    SelectingWeapon,
    Locked
}
[System.Serializable]
public class SelectPlayer
{
    [HideInInspector] public Team myTeam = Team.none;
    [HideInInspector] public TeamSelectPlayerState teamSelectPlayerSt = TeamSelectPlayerState.NotJoined;
    public Camera myCamera;
    public Camera myUICamera;
    public GameObject myCanvas;
    public GameObject notJoinedScreen;
    public TeamSelectPlatform myTeamSelectPlatform;
    [Range(0, 1)]
    [HideInInspector] public float deadzone = 0.2f;
    [HideInInspector] public PlayerActions myControls;
    EAITwoAxisControls myJoyStickControls;

    [Header("TEAM SELECT PLAYER HUD")]
    public TeamSelectPlayerCanvas myTeamSelectPlayerCanvas;

    [Header("Referencias Animator")]
    public Animator[] animators;

    private bool changeTeamAnimationStarted = false;
    private float changeTeamAnimationTime = 0;
    private float changeTeamAnimationMaxTime = 0.45f;
    private float changeTeamAnimationRealTargetRot = 0;
    private float changeTeamAnimationInitialRot = 0;
    private float changeTeamAnimationTargetRot = 0;
    private float changeTeamAnimationOriginalRot = 0;

    private Vector3 selectTeamHUDParentOriginalScale;
    private Vector3 selectTeamHUDTeamTextOriginalScale;

    //[Header("Animator Variables")]
    //public bool anim_ready;
    //AnimatorStateInfo stateInfo;
    //int idleReadyHash = Animator.StringToHash("IdleReady");
    //bool idleReady;

    //public void KonoUpdate()
    //{
    //    //if keypressed A variable
    //    if (!anim_ready)
    //    {
    //        idleReady = true;

    //    }

    //    //if keypressed B variable
    //    if (anim_ready)
    //    {
    //        idleReady = false;
    //        animator.SetBool(idleReadyHash, idleReady);
    //    }

    //}

    public void KonoAwake(int nPlayers, int myPlayerNum)
    {
        myCamera.gameObject.SetActive(true);
        myCanvas.SetActive(true);
        notJoinedScreen.SetActive(true);
        switch (myPlayerNum)
        {
            case 0:
                switch (nPlayers)
                {
                    case 1:
                        myCamera.rect = new Rect(0, 0, 1f, 1f);
                        myUICamera.rect = new Rect(0, 0, 1f, 1f);
                        break;
                    case 2:
                        myCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
                        myUICamera.rect = new Rect(0, 0.5f, 1, 0.5f);
                        break;
                    case 3:
                        myCamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                        myUICamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                        break;
                    case 4:
                        myCamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                        myUICamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                        //myCamera.depth = 1;
                        break;
                }
                break;
            case 1:
                switch (nPlayers)
                {
                    case 2:
                        myCamera.rect = new Rect(0, 0, 1, 0.5f);
                        myUICamera.rect = new Rect(0, 0, 1, 0.5f);
                        break;
                    case 3:
                        myCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        myUICamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        break;
                    case 4:
                        myCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        myUICamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        break;
                }
                break;
            case 2:
                switch (nPlayers)
                {
                    case 3:
                        myCamera.rect = new Rect(0, 0, 1, 0.5f);
                        myUICamera.rect = new Rect(0, 0, 1, 0.5f);
                        break;
                    case 4:
                        myCamera.rect = new Rect(0, 0, 0.5f, 0.5f);
                        myUICamera.rect = new Rect(0, 0, 0.5f, 0.5f);
                        break;
                }
                break;
            case 3:
                switch (nPlayers)
                {
                    case 4:
                        myCamera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                        myUICamera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                        break;
                }
                break;
        }
        myJoyStickControls = new EAITwoAxisControls(GameInfo.instance.myControls.LeftJoystick, deadzone);

        // AJUSTE DE ESCALA DE LA HUD PARA CAMARAS CON RECT DESPROPORCIONADO
        RectTransform teamText = myTeamSelectPlayerCanvas.teamNameText.GetComponent<RectTransform>();
        selectTeamHUDParentOriginalScale = myTeamSelectPlayerCanvas.teamSelectHUDParent.localScale;
        selectTeamHUDTeamTextOriginalScale = teamText.localScale;
        float scale = myUICamera.rect.width - myUICamera.rect.height;
        if (scale != 0)
        {
            float scaleValue = scale;
            //float invScaleValue = 1 + (1 - scale);//2- SCALE
            myTeamSelectPlayerCanvas.teamSelectHUDParent.localScale = new Vector3(myTeamSelectPlayerCanvas.teamSelectHUDParent.GetComponent<RectTransform>().localScale.x * scaleValue,
                myTeamSelectPlayerCanvas.teamSelectHUDParent.GetComponent<RectTransform>().localScale.y, 1);
            teamText.localScale = new Vector3(teamText.localScale.x * 2, teamText.localScale.y, 1);
        }

        changeTeamAnimationInitialRot = changeTeamAnimationTargetRot = changeTeamAnimationRealTargetRot =
            changeTeamAnimationOriginalRot = myTeamSelectPlatform.rotationParent.localRotation.eulerAngles.y;

    }

    public void KonoUpdate()
    {
        if (myControls != null)
        {
            if (myControls.B.WasReleased || (myControls.Device == null && myControls.Start.WasReleased))
            {
                GoBack();
            }
            //Debug.Log("My controls exist: teamSelectPlayerSt = "+ teamSelectPlayerSt);
            ProcessChangeTeamAnimation();
            switch (teamSelectPlayerSt)
            {
                case TeamSelectPlayerState.NotJoined:
                    if (myControls.A.WasPressed)
                    {
                        JoinTeamSelect();
                    }
                    break;
                case TeamSelectPlayerState.SelectingTeam:
                    //Debug.Log("Selecting Team: myControls.LeftJoystick.X = "+ myControls.LeftJoystick.X);
                    if (myJoyStickControls.LeftWasPressed)
                    {
                        ChangeTeam(1);
                        //Animation Left
                    }
                    else if (myJoyStickControls.RightWasPressed)
                    {
                        ChangeTeam(0);
                        //Animation Right
                    }
                    else if (myControls.A.WasReleased)
                    {
                        SelectTeam();
                    }

                    IluminateArrows();
                    break;
                case TeamSelectPlayerState.SelectingCharacter:
                    break;
                case TeamSelectPlayerState.SelectingWeapon:
                    break;
                case TeamSelectPlayerState.Locked:
                    break;

            }

            myJoyStickControls.ResetJoyStick();
        }
    }

    public void JoinTeamSelect()
    {
        if (teamSelectPlayerSt == TeamSelectPlayerState.NotJoined)
        {
            Debug.Log("JOIN TEAM SELECT");
            notJoinedScreen.SetActive(false);
            teamSelectPlayerSt = TeamSelectPlayerState.SelectingTeam;
            ChangeHUDTeam(myTeam);
            myTeamSelectPlatform.StartTeamSelect();
        }
    }

    public void ExitTeamSelect()
    {
        if (teamSelectPlayerSt == TeamSelectPlayerState.SelectingTeam)
        {
            Debug.Log("EXIT TEAM SELECT");
            notJoinedScreen.SetActive(true);
            teamSelectPlayerSt = TeamSelectPlayerState.NotJoined;
            Alpha_Team_Select.instance.RemovePlayer(this);
        }
    }

    public void ResetSelectPlayer()
    {
        // Devuelvo AJUSTE DE ESCALA DE LA HUD PARA CAMARAS CON RECT DESPROPORCIONADO a su estado original
        if (selectTeamHUDParentOriginalScale != Vector3.zero)
        {
            RectTransform teamText = myTeamSelectPlayerCanvas.teamNameText.GetComponent<RectTransform>();
            myTeamSelectPlayerCanvas.teamSelectHUDParent.localScale = selectTeamHUDParentOriginalScale;
            teamText.localScale = selectTeamHUDTeamTextOriginalScale;
        }

        if (changeTeamAnimationOriginalRot != 0)
        {
            myTeamSelectPlatform.rotationParent.localRotation = Quaternion.Euler(0, changeTeamAnimationOriginalRot, 0);
        }
    }

    void IluminateArrows()
    {
        if (teamSelectPlayerSt == TeamSelectPlayerState.SelectingTeam)
        {
            if (myJoyStickControls.leftIsPressed)
            {
                Color auxColor = myTeamSelectPlayerCanvas.leftArrow.color;
                auxColor.a = 1;
                myTeamSelectPlayerCanvas.leftArrow.color = auxColor;
            }
            else
            {
                Color auxColor = myTeamSelectPlayerCanvas.leftArrow.color;
                auxColor.a = myTeamSelectPlayerCanvas.alphaDeactivated;
                myTeamSelectPlayerCanvas.leftArrow.color = auxColor;
            }
            if (myJoyStickControls.rightIsPressed)
            {
                Color auxColor = myTeamSelectPlayerCanvas.rightArrow.color;
                auxColor.a = 1;
                myTeamSelectPlayerCanvas.rightArrow.color = auxColor;
            }
            else
            {
                Color auxColor = myTeamSelectPlayerCanvas.rightArrow.color;
                auxColor.a = myTeamSelectPlayerCanvas.alphaDeactivated;
                myTeamSelectPlayerCanvas.rightArrow.color = auxColor;
            }
        }
    }

    #region --- TEAM SELECTION ---
    void ChangeTeam(int direction) // 0 es izda y 1 es derecha
    {
        switch (direction)
        {
            case 0:
                //playerModelsParent.localRotation *= Quaternion.Euler(0, 120, 0);
                switch (myTeam)
                {
                    case Team.A:
                        myTeam = Team.B;
                        break;
                    case Team.B:
                        myTeam = Team.none;
                        break;
                    case Team.none:
                        myTeam = Team.A;
                        break;
                }
                break;
            case 1:
                //playerModelsParent.localRotation *= Quaternion.Euler(0, -120, 0);
                switch (myTeam)
                {
                    case Team.A:
                        myTeam = Team.none;
                        break;
                    case Team.B:
                        myTeam = Team.A;
                        break;
                    case Team.none:
                        myTeam = Team.B;
                        break;
                }
                break;
        }
        ChangeHUDTeam(myTeam);
        StartChangeTeamAnimation(direction);
    }

    void StartChangeTeamAnimation(int direction)
    {
        StopChangeTeamAnimation();
        if (!changeTeamAnimationStarted)
        {
            changeTeamAnimationInitialRot = myTeamSelectPlatform.rotationParent.localRotation.eulerAngles.y;
            changeTeamAnimationStarted = true;
            changeTeamAnimationTime = 0;
            switch (direction)
            {
                case 0:
                    changeTeamAnimationTargetRot = changeTeamAnimationRealTargetRot + 120;
                    changeTeamAnimationRealTargetRot = changeTeamAnimationTargetRot >= 360 ? changeTeamAnimationTargetRot - 360 : changeTeamAnimationTargetRot;
                    break;
                case 1:
                    changeTeamAnimationTargetRot = changeTeamAnimationRealTargetRot - 120;
                    changeTeamAnimationRealTargetRot = changeTeamAnimationTargetRot < 0 ? changeTeamAnimationTargetRot + 360 : changeTeamAnimationTargetRot;
                    break;
            }

            //Debug.Log("START CHANGE TEAM ANIMATION: changeTeamAnimationInitialRot = " + changeTeamAnimationInitialRot + "; changeTeamAnimationTargetRot = " + changeTeamAnimationTargetRot);
        }
    }

    void ProcessChangeTeamAnimation()
    {
        if (changeTeamAnimationStarted)
        {
            changeTeamAnimationTime += Time.deltaTime;
            float progress = Mathf.Clamp01(changeTeamAnimationTime / changeTeamAnimationMaxTime);
            float yRot = EasingFunction.EaseInOutQuart(changeTeamAnimationInitialRot, changeTeamAnimationTargetRot, progress);
            myTeamSelectPlatform.rotationParent.localRotation = Quaternion.Euler(0, yRot, 0);

            if (changeTeamAnimationTime >= changeTeamAnimationMaxTime)
            {
                StopChangeTeamAnimation();
            }
        }
    }

    void StopChangeTeamAnimation()
    {
        if (changeTeamAnimationStarted)
        {
            changeTeamAnimationStarted = false;
        }
    }

    void ChangeHUDTeam(Team team)
    {
        myTeamSelectPlayerCanvas.teamNameBackground.sprite = myTeamSelectPlayerCanvas.teamBackgrounds[(int)team];
        myTeamSelectPlayerCanvas.teamNameText.text = myTeamSelectPlayerCanvas.teamTexts[(int)team];
        myTeamSelectPlayerCanvas.teamIcon.sprite = myTeamSelectPlayerCanvas.teamIcons[(int)team];
    }

    void SelectTeam()
    {
        if (teamSelectPlayerSt != TeamSelectPlayerState.SelectingTeam)
            return;

            Debug.Log("LOCK TEAM: " + myTeam);
            //Visual Lock: TO DO: Fade / player models animation for TRANSITION
            //Debug.Log("ANIMATOR = " + animators[(int)myTeam].gameObject);
            //animators[(int)myTeam].SetBool("IdleReady", true);



            //HUD VISUAL LOCK
            for (int i = 0; i < myTeamSelectPlayerCanvas.lockStateDeactivateImages.Length; i++)
            {
                myTeamSelectPlayerCanvas.lockStateDeactivateImages[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < myTeamSelectPlayerCanvas.lockStateActivateImages.Length; i++)
            {
                myTeamSelectPlayerCanvas.lockStateActivateImages[i].gameObject.SetActive(true);
            }
            StartCharacterSelection();
    }
    #endregion

    #region --- CHARACTER SELECTION ---
    void StartCharacterSelection()
    {
        teamSelectPlayerSt = TeamSelectPlayerState.SelectingCharacter;
        myTeamSelectPlatform.StartCharacterSelection();
        myTeamSelectPlatform.ChangeTeamColors(myTeam);
    }
    void StopCharacterSelection() //GO BACK
    {
        teamSelectPlayerSt = TeamSelectPlayerState.SelectingTeam;
        //Visual Back
        animators[(int)myTeam].SetBool("IdleReady", false);

        //HUD VISUAL Back
        for (int i = 0; i < myTeamSelectPlayerCanvas.lockStateDeactivateImages.Length; i++)
        {
            myTeamSelectPlayerCanvas.lockStateDeactivateImages[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < myTeamSelectPlayerCanvas.lockStateActivateImages.Length; i++)
        {
            myTeamSelectPlayerCanvas.lockStateActivateImages[i].gameObject.SetActive(false);
        }
    }
    #endregion

    #region --- WEAPON SELECTION ---
    void StartWeaponSelection()
    {
        teamSelectPlayerSt = TeamSelectPlayerState.SelectingWeapon;
    }
    void StopWeaponSelection()// GO BACK
    {
        teamSelectPlayerSt = TeamSelectPlayerState.SelectingCharacter;
    }
    #endregion

    #region --- LOCK ---
    void Lock()
    {
        teamSelectPlayerSt = TeamSelectPlayerState.Locked;
    }
    void Unlock()
    {
        teamSelectPlayerSt = TeamSelectPlayerState.SelectingWeapon;

    }
    #endregion

    void GoBack()
    {
        Debug.Log("Go Back");
        switch (teamSelectPlayerSt)
        {
            case TeamSelectPlayerState.SelectingTeam:
                ExitTeamSelect();
                break;
            case TeamSelectPlayerState.SelectingCharacter:
                StopCharacterSelection();
                break;
            case TeamSelectPlayerState.SelectingWeapon:
                StopWeaponSelection();
                break;
            case TeamSelectPlayerState.Locked:
                Debug.Log("UNLOCK Weapon");
                Lock();
                break;
        }
    }
}