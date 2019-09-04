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
    ChoosingNumberOfPlayers,
    ChoosingTeam
}
public class Alpha_Team_Select : MonoBehaviour
{
    public static Alpha_Team_Select instance;
    //Variables
    public GameObject stockGameInfo;
    public GameObject stockInControlManager;
    public GameObject selectNumberOfPlayersCanvas;

    [HideInInspector] public static bool startFromMap = false;
    [HideInInspector] public static string startFromMapScene;
    public string nextSceneHigh;
    public string nextSceneLOD;


    PlayerActions keyboardListener;
    PlayerActions joystickListener;
    JoyStickControls myJoyStickControls;

    TeamSelectMenuState selectPlayerMenuSt = TeamSelectMenuState.ChoosingNumberOfPlayers;


    [Range(0, 1)]
    public float deadzone;
    public Camera selectNumberOfPlayersCam;
    public float scaleSpriteBig;
    public Transform[] numPlayerSprites;
    public SelectPlayer[] selectPlayers;
    //public GameObject selectNPlayersCamera;
    //public bool cameraSet = false;
    int playersJoined = 0;
    int offlineMaxPlayers = 4;
    int nPlayers = 1;


    private void Awake()
    {
        instance = this;
        if (GameInfo.instance == null)
        {
            stockInControlManager.SetActive(true);
            //stockInControlManager.GetComponent<InControlManager>().OnEnable();
            stockGameInfo.SetActive(true);
            // stockGameInfo.GetComponent<GameInfo>().Awake();

        }

        selectNumberOfPlayersCanvas.SetActive(true);
        selectNumberOfPlayersCam.gameObject.SetActive(true);
        for (int i = 0; i < selectPlayers.Length; i++)
        {
            selectPlayers[i].myCamera.gameObject.SetActive(false);
            selectPlayers[i].myCanvas.gameObject.SetActive(false);
            selectPlayers[i].deadzone = deadzone;
        }
        myJoyStickControls = new JoyStickControls();
    }

    void Start()
    {
        if (GameInfo.instance != null)
        {
            GameInfo.instance.inControlManager = GameObject.Find("InControl manager");
        }
        else
        {
            Debug.LogError("Error: GameInfo is null or has not done it's awake yet. It should be awaken and ready.");
        }

        MakeSpriteBig(numPlayerSprites[nPlayers - 1]);
    }

    void Update()
    {
        if (GameInfo.instance.myControls.A.WasPressed) Debug.Log("A WAS PRESSED");
        switch (selectPlayerMenuSt)
        {
            case TeamSelectMenuState.ChoosingNumberOfPlayers:
                if (Input.GetKeyDown(KeyCode.LeftArrow) || ((GameInfo.instance.myControls.LeftJoystick.X <= -deadzone) && !myJoyStickControls.leftIsPressed))
                {
                    myJoyStickControls.leftIsPressed = true;
                    MoveLeft();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || ((GameInfo.instance.myControls.LeftJoystick.X >= deadzone) && !myJoyStickControls.rightIsPressed))
                {
                    myJoyStickControls.rightIsPressed = true;
                    MoveRight();
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) || ((GameInfo.instance.myControls.LeftJoystick.Y <= -deadzone) && !myJoyStickControls.downIsPressed))
                {
                    myJoyStickControls.downIsPressed = true;
                    MoveUp();
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || ((GameInfo.instance.myControls.LeftJoystick.Y >= deadzone) && !myJoyStickControls.upIsPressed))
                {
                    myJoyStickControls.upIsPressed = true;
                    MoveDown();
                }
                else if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)) || GameInfo.instance.myControls.A.WasPressed))
                {
                    LockSelectNumberOfPlayers();
                }
                if (GameInfo.instance.myControls.LeftJoystick.Y > -deadzone && myJoyStickControls.downIsPressed)
                {
                    myJoyStickControls.downIsPressed = false;
                }
                if (GameInfo.instance.myControls.LeftJoystick.Y < deadzone && myJoyStickControls.upIsPressed)
                {
                    myJoyStickControls.upIsPressed = false;
                }
                if (GameInfo.instance.myControls.LeftJoystick.X > -deadzone && myJoyStickControls.leftIsPressed)
                {
                    myJoyStickControls.leftIsPressed = false;
                }
                if (GameInfo.instance.myControls.LeftJoystick.X < deadzone && myJoyStickControls.rightIsPressed)
                {
                    myJoyStickControls.rightIsPressed = false;
                }
                break;
            case TeamSelectMenuState.ChoosingTeam:

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

                for (int i = 0; i < selectPlayers.Length; i++)
                {
                    selectPlayers[i].KonoUpdate();
                }

                //CHECK IF ALL READY
                bool allReady = true;
                for (int i = 0; i < selectPlayers.Length; i++)
                {
                    if (i < nPlayers && selectPlayers[i].teamSelectPlayerSt != TeamSelectPlayerState.TeamLocked)
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
                        if(nPlayers>1)
                        SceneManager.LoadScene(nextSceneLOD);
                        else
                        {
                            SceneManager.LoadScene(nextSceneHigh);

                        }
                    }
                }

                if (GameInfo.instance.myControls.B.IsPressed || Input.GetKeyDown(KeyCode.Escape))
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

    void BackToChooseNumberOfPlayers()
    {
        selectNumberOfPlayersCam.gameObject.SetActive(true);
        selectNumberOfPlayersCanvas.SetActive(true);
        for (int i = 0; i < selectPlayers.Length; i++)
        {
            selectPlayers[i].myCamera.gameObject.SetActive(false);
            //selectPlayers[i].myUICamera.gameObject.SetActive(false);
            selectPlayerMenuSt = TeamSelectMenuState.ChoosingNumberOfPlayers;
        }
    }

    bool JoinButtonWasPressedOnListener(PlayerActions actions)
    {
        //Debug.Log("CHECK IF BUTTON WAS PRESSED ON LISTENER");
        return (actions.Start.WasPressed) || (actions != keyboardListener && (actions.Start.WasPressed)) ||
            (actions == keyboardListener && (Input.GetKeyDown(KeyCode.Alpha1)));
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

    void LockSelectNumberOfPlayers()
    {
        selectPlayerMenuSt = TeamSelectMenuState.ChoosingTeam;
        selectNumberOfPlayersCam.gameObject.SetActive(false);
        selectNumberOfPlayersCanvas.gameObject.SetActive(false);
        for (int i = 0; i < nPlayers; i++)
        {
            selectPlayers[i].KonoAwake(nPlayers, i);
        }
    }

    void UnlockSelectNumberOfPlayers()
    {
        selectPlayerMenuSt = TeamSelectMenuState.ChoosingNumberOfPlayers;
        selectNumberOfPlayersCam.gameObject.SetActive(true);
    }

    void MoveUp()
    {
        MakeSpriteSmall(numPlayerSprites[nPlayers - 1]);

        switch (nPlayers)
        {
            case 1:
                nPlayers = 3;
                break;
            case 2:
                nPlayers = 4;
                break;
            case 3:
                nPlayers = 1;
                break;
            case 4:
                nPlayers = 2;
                break;
        }

        MakeSpriteBig(numPlayerSprites[nPlayers - 1]);

    }
    void MoveDown()
    {
        MakeSpriteSmall(numPlayerSprites[nPlayers - 1]);

        switch (nPlayers)
        {
            case 1:
                nPlayers = 3;
                break;
            case 2:
                nPlayers = 4;
                break;
            case 3:
                nPlayers = 1;
                break;
            case 4:
                nPlayers = 2;
                break;
        }
        MakeSpriteBig(numPlayerSprites[nPlayers - 1]);

    }
    void MoveRight()
    {
        MakeSpriteSmall(numPlayerSprites[nPlayers - 1]);

        switch (nPlayers)
        {
            case 1:
                nPlayers = 2;
                break;
            case 2:
                nPlayers = 1;
                break;
            case 3:
                nPlayers = 4;
                break;
            case 4:
                nPlayers = 3;
                break;
        }
        MakeSpriteBig(numPlayerSprites[nPlayers - 1]);

    }
    void MoveLeft()
    {
        MakeSpriteSmall(numPlayerSprites[nPlayers - 1]);

        switch (nPlayers)
        {
            case 1:
                nPlayers = 2;
                break;
            case 2:
                nPlayers = 1;
                break;
            case 3:
                nPlayers = 4;
                break;
            case 4:
                nPlayers = 3;
                break;
        }
        MakeSpriteBig(numPlayerSprites[nPlayers - 1]);
    }

    public void MakeSpriteBig(Transform spriteTransform)
    {
        spriteTransform.localScale *= scaleSpriteBig;
    }
    public void MakeSpriteSmall(Transform spriteTransform)
    {
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
    TeamLocked
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
    public Transform playerModelsParent;
    [Range(0, 1)]
    [HideInInspector] public float deadzone = 0.2f;
    [HideInInspector] public PlayerActions myControls;
    JoyStickControls myJoyStickControls;

    [Header("TEAM SELECT PLAYER HUD")]
    public TeamSelectPlayerCanvas myTeamSelectPlayerCanvas;


    [Header("Referencias Animator")]
    public Animator[] animators;

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
        myJoyStickControls = new JoyStickControls();

        float scale = myUICamera.rect.width - myUICamera.rect.height;
        if (scale != 0)
        {
            float scaleValue = scale;
            float invScaleValue = 1 + (1 - scale);//2- SCALE
            myTeamSelectPlayerCanvas.teamSelectHUDParent.localScale = new Vector3(myTeamSelectPlayerCanvas.teamSelectHUDParent.GetComponent<RectTransform>().localScale.x * scaleValue,
                myTeamSelectPlayerCanvas.teamSelectHUDParent.GetComponent<RectTransform>().localScale.y, 1);
            RectTransform teamText = myTeamSelectPlayerCanvas.teamNameText.GetComponent<RectTransform>();
            teamText.localScale = new Vector3(teamText.localScale.x * 2,
               teamText.localScale.y, 1);
        }
    }

        public void KonoUpdate()
        {
            if (myControls != null)
            {
                //Debug.Log("My controls exist: teamSelectPlayerSt = "+ teamSelectPlayerSt);
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
                        if (myControls.LeftJoystick.X < -deadzone && !myJoyStickControls.leftIsPressed)
                        {
                            myJoyStickControls.leftIsPressed = true;
                            ChangeTeam(0);
                            //Animation Left
                        }
                        else if (myControls.LeftJoystick.X > deadzone && !myJoyStickControls.rightIsPressed)
                        {
                            myJoyStickControls.rightIsPressed = true;
                            ChangeTeam(1);
                            //Animation Right
                        }
                        else if (myControls.A.WasPressed)
                        {
                            LockTeam();
                        }
                        else if (myControls.B.WasPressed)
                        {
                            ExitTeamSelect();
                        }

                        IluminateArrows();
                        break;
                    case TeamSelectPlayerState.TeamLocked:
                        if (myControls.B.WasPressed)
                        {
                            UnlockTeam();
                        }
                        break;
                }
                if (GameInfo.instance.myControls.LeftJoystick.Y > -deadzone && myJoyStickControls.downIsPressed)
                {
                    myJoyStickControls.upIsPressed = false;
                }
                if (GameInfo.instance.myControls.LeftJoystick.Y < deadzone && myJoyStickControls.upIsPressed)
                {
                    myJoyStickControls.downIsPressed = false;
                }
                if (GameInfo.instance.myControls.LeftJoystick.X > -deadzone && myJoyStickControls.leftIsPressed)
                {
                    myJoyStickControls.leftIsPressed = false;
                }
                if (GameInfo.instance.myControls.LeftJoystick.X < deadzone && myJoyStickControls.rightIsPressed)
                {
                    myJoyStickControls.rightIsPressed = false;
                }
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

        void ChangeTeam(int direction) // 0 es izda y 1 es derecha
        {
            switch (direction)
            {
                case 0:
                    playerModelsParent.localRotation *= Quaternion.Euler(0, 120, 0);
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
                    playerModelsParent.localRotation *= Quaternion.Euler(0, -120, 0);
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
        }

        void ChangeHUDTeam(Team team)
        {
            myTeamSelectPlayerCanvas.teamNameBackground.sprite = myTeamSelectPlayerCanvas.teamBackgrounds[(int)team];
            myTeamSelectPlayerCanvas.teamNameText.text = myTeamSelectPlayerCanvas.teamTexts[(int)team];
            myTeamSelectPlayerCanvas.teamIcon.sprite = myTeamSelectPlayerCanvas.teamIcons[(int)team];
        }

        void LockTeam()
        {
            if (teamSelectPlayerSt == TeamSelectPlayerState.SelectingTeam)
            {
                Debug.Log("LOCK TEAM");
                teamSelectPlayerSt = TeamSelectPlayerState.TeamLocked;
                //Visual Lock
                Debug.Log("ANIMATOR = " + animators[(int)myTeam].gameObject);
                animators[(int)myTeam].SetBool("IdleReady", true);

                //HUD VISUAL LOCK
                for (int i = 0; i < myTeamSelectPlayerCanvas.lockStateDeactivateImages.Length; i++)
                {
                    myTeamSelectPlayerCanvas.lockStateDeactivateImages[i].gameObject.SetActive(false);
                }

                for (int i = 0; i < myTeamSelectPlayerCanvas.lockStateActivateImages.Length; i++)
                {
                    myTeamSelectPlayerCanvas.lockStateActivateImages[i].gameObject.SetActive(true);
                }
            }
        }

        void UnlockTeam()
        {
            if (teamSelectPlayerSt == TeamSelectPlayerState.TeamLocked)
            {
                Debug.Log("UNLOCK TEAM");
                teamSelectPlayerSt = TeamSelectPlayerState.SelectingTeam;
                //Visual Unlock
                animators[(int)myTeam].SetBool("IdleReady", false);

                //HUD VISUAL UNLOCK
                for (int i = 0; i < myTeamSelectPlayerCanvas.lockStateDeactivateImages.Length; i++)
                {
                    myTeamSelectPlayerCanvas.lockStateDeactivateImages[i].gameObject.SetActive(true);
                }

                for (int i = 0; i < myTeamSelectPlayerCanvas.lockStateActivateImages.Length; i++)
                {
                    myTeamSelectPlayerCanvas.lockStateActivateImages[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public class JoyStickControls
    {
        public bool leftIsPressed = false;
        public bool downIsPressed = false;
        public bool rightIsPressed = false;
        public bool upIsPressed = false;
    }