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
    //Variables
    public GameObject stockGameInfo;
    public GameObject stockInControlManager;
    public GameObject selectNumberOfPlayersCanvas;

    PlayerActions keyboardListener;
    PlayerActions joystickListener;

    TeamSelectMenuState selectPlayerMenuSt = TeamSelectMenuState.ChoosingNumberOfPlayers;

    public string nextScene;
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
    int team = 0;
    int f_Team = 0; //Team Selected while being ready
    bool ready = false; //Can´t change team
    int nPlayers = 1;


    private void Awake()
    {
        if (GameInfo.instance == null)
        {
            stockGameInfo.SetActive(true);
            stockGameInfo.GetComponent<GameInfo>().Awake();
            stockInControlManager.SetActive(true);
            stockInControlManager.GetComponent<InControlManager>().OnEnable();
        }

        if (GameInfo.instance != null)
        {
            GameInfo.instance.inControlManager = GameObject.Find("InControl manager");
        }
        else
        {
            Debug.LogError("Error: GameInfo is null or has not done it's awake yet. It should be awaken and ready.");
        }
        selectNumberOfPlayersCanvas.SetActive(true);
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
        MakeSpriteBig(numPlayerSprites[nPlayers - 1]);
    }

    void Update()
    {
        switch (selectPlayerMenuSt)
        {
            case TeamSelectMenuState.ChoosingNumberOfPlayers:
                if (Input.GetKeyDown(KeyCode.LeftArrow) || GameInfo.instance.myControls.LeftJoystick.X < -deadzone)
                {
                    MoveLeft();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || GameInfo.instance.myControls.LeftJoystick.X > deadzone)
                {
                    MoveRight();
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) || GameInfo.instance.myControls.LeftJoystick.Y < -deadzone)
                {
                    MoveUp();
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || GameInfo.instance.myControls.LeftJoystick.Y > deadzone)
                {
                    MoveDown();
                }
                else if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)) || GameInfo.instance.myControls.A.WasPressed))
                {
                    LockSelectNumberOfPlayers();
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

                for (int i=0; i < selectPlayers.Length; i++)
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

    bool JoinButtonWasPressedOnListener(PlayerActions actions)
    {
        return actions.Y.WasPressed || actions.B.WasPressed || (actions != keyboardListener && (actions.A.WasPressed || actions.X.WasPressed)) ||
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
            if (selectPlayers[i].myControls.Device == inputDevice)
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

            return player;
        }

        return null;
    }

    int FindNotJoinedPlayer()
    {
        for(int i = 0; i < selectPlayers.Length; i++)
        {
            if(selectPlayers[i].teamSelectPlayerSt == TeamSelectPlayerState.NotJoined)
            {
                return i;
            }
        }
        return -1;
    }

    void RemovePlayer(SelectPlayer player)
    {
        for (int i = 0; i < selectPlayers.Length; i++)
        {
            if (selectPlayers[i] == player)
            {
                RemovePlayer(i);
            }
        }
    }

    void RemovePlayer(int index)
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
            selectPlayers[i].KonoAwake(nPlayers,i);
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

    //public void UpdateNPlayers()
    //{
    //    selectNPlayersCamera.SetActive(false);

    //    //Difuminado a Negro

    //    switch (nPlayers)
    //    {
    //        case 1:

    //            // 1 Player

    //            camera1player.SetActive(true);
    //            camera2player.SetActive(false);
    //            camera3player.SetActive(false);
    //            camera4player.SetActive(false);

    //            break;

    //        case 2:

    //            // 2 player

    //            camera1player.SetActive(false);
    //            camera2player.SetActive(true);
    //            camera3player.SetActive(false);
    //            camera4player.SetActive(false);

    //            break;

    //        case 3:

    //            // 3 player

    //            camera1player.SetActive(false);
    //            camera2player.SetActive(false);
    //            camera3player.SetActive(true);
    //            camera4player.SetActive(false);

    //            break;

    //        case 4:

    //            // 4 player

    //            camera1player.SetActive(false);
    //            camera2player.SetActive(false);
    //            camera3player.SetActive(false);
    //            camera4player.SetActive(true);

    //            break;
    //    }

    //    cameraSet = true;
    //}

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
    [HideInInspector]public Team myTeam = Team.none;
    [HideInInspector]public TeamSelectPlayerState teamSelectPlayerSt = TeamSelectPlayerState.NotJoined;
    public Camera myCamera;
    public Camera myUICamera;
    public GameObject myCanvas;
    public GameObject notJoinedScreen;
    public Transform playerModelsParent;
    [Range(0,1)]
    [HideInInspector]public float deadzone = 0.2f;
    [HideInInspector]public PlayerActions myControls;

    public SelectPlayer(float _deadzone = 0.2f, Team _myTeam = Team.none)
    {

    }

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
                        myCamera.depth = 1;
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
    }

    public void KonoUpdate()
    {
        if (myControls != null)
        {
            switch (teamSelectPlayerSt)
            {
                case TeamSelectPlayerState.NotJoined:
                    if (myControls.A.WasPressed)
                    {
                        JoinTeamSelect();
                    }
                    break;
                case TeamSelectPlayerState.SelectingTeam:
                    if (myControls.LeftJoystick.X < -deadzone && teamSelectPlayerSt == TeamSelectPlayerState.SelectingTeam)
                    {
                        ChangeTeam(0);
                        //Animation Left
                    }
                    else if (myControls.LeftJoystick.X > deadzone && teamSelectPlayerSt == TeamSelectPlayerState.SelectingTeam)
                    {
                        ChangeTeam(1);
                        //Animation Right
                    }
                    else if (myControls.A.WasPressed && teamSelectPlayerSt == TeamSelectPlayerState.SelectingTeam)
                    {
                        LockTeam();
                    }else if (myControls.B.WasPressed)
                    {
                        ExitTeamSelect();
                    }
                    break;
                case TeamSelectPlayerState.TeamLocked:
                    if (myControls.B.WasPressed)
                    {
                        UnlockTeam();
                    }
                    break;
            }
        }
    }

    public void JoinTeamSelect()
    {
        if(teamSelectPlayerSt == TeamSelectPlayerState.NotJoined)
        {
            notJoinedScreen.SetActive(false);
            teamSelectPlayerSt = TeamSelectPlayerState.SelectingTeam;
        }
    }

    public void ExitTeamSelect()
    {
        if(teamSelectPlayerSt == TeamSelectPlayerState.SelectingTeam)
        {
            notJoinedScreen.SetActive(true);
            teamSelectPlayerSt = TeamSelectPlayerState.NotJoined;
        }
    }

    void ChangeTeam(int direction) // 0 es izda y 1 es derecha
    {
        switch (direction)
        {
            case 0:
                playerModelsParent.localRotation *= Quaternion.Euler(0, 120, 0);
                 switch(myTeam)
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
    }

    void LockTeam()
    {
        if (teamSelectPlayerSt == TeamSelectPlayerState.SelectingTeam)
        {
            teamSelectPlayerSt = TeamSelectPlayerState.TeamLocked;
            //Visual Lock
        }
    }

    void UnlockTeam()
    {
        if (teamSelectPlayerSt == TeamSelectPlayerState.TeamLocked)
        {
            teamSelectPlayerSt = TeamSelectPlayerState.SelectingTeam;
            //Visual Unlock
        }
    }
}