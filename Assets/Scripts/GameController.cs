using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    [Tooltip("Number of players in the game")]
    [HideInInspector]
    [Range(1,4)]
    public int playerNum = 1;
    public Flag[] flags;
    //public AttackData[] allAttacks;
    public AttackData attackX;
    public AttackData attackY;
    public AttackData attackB;
    public AttackData attackHook;
    int slowmo = 0;
    public PlayerMovement[] allPlayers;
    public GameObject[] allCanvas;
    public CameraController[] allCameraBases;
    public WeaponData[] allWeapons;

    public GameObject flagPrefab;
    private void Awake()
    {
        if (GameInfo.instance == null)
        {
            string escena = TeamSetupManager.SiguenteEscena;
            print(escena);
            TeamSetupManager.SiguenteEscena = SceneManager.GetActiveScene().name;
            TeamSetupManager.startFromMap = true;
            SceneManager.LoadScene("TeamSetup");
            return;
        }
        instance = this;
        gameOverMenu.SetActive(false);
        veil.SetActive(false);
        victoryText.SetActive(false);
        gameOverMenuOn = false;
        playerNum = GameInfo.instance.nPlayers;
        playerNum = Mathf.Clamp(playerNum, 1, 4);

        //AUTOMATIC PLAYERS/CAMERAS/CANVAS SETUP
        PlayersSetup();
    }

    void PlayersSetup()
    {
        for (int i = 0; i < allCanvas.Length; i++)
        {
            if (i < playerNum)
            {
                allCanvas[i].SetActive(true);
                allCameraBases[i].gameObject.SetActive(true);
                allCanvas[i].GetComponent<Canvas>().worldCamera = allCameraBases[i].myCamera.GetComponent<Camera>();
                allPlayers[i].gameObject.SetActive(true);
                allPlayers[i].myCamera = allCameraBases[i];
                allPlayers[i].GetComponent<PlayerCombat>().attackName = allCanvas[i].transform.GetChild(0).GetComponent<Text>();
                Debug.Log(GameInfo.instance.playerActionsList[i]);
                allPlayers[i].Actions = GameInfo.instance.playerActionsList[i];
                if (GameInfo.instance.playerTeamList[i] == Team.none)
                    GameInfo.instance.playerTeamList[i] = GameInfo.instance.noneTeamSelect();
                
                allPlayers[i].team = GameInfo.instance.playerTeamList[i];
            }
            else
            {
                allPlayers[i].gameObject.SetActive(false);
                allCanvas[i].SetActive(false);
                allCameraBases[i].gameObject.SetActive(false);
            }
        }
        switch (playerNum)
        {
            case 1:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
                break;
            case 2:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0.5f, 1, 0.5f);
                allCameraBases[1].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 0.5f);
                break;
            case 3:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                allCameraBases[1].myCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                allCameraBases[2].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 0.5f);
                break;
            case 4:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                allCameraBases[1].myCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                allCameraBases[2].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 0.5f);
                allCameraBases[3].myCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                break;
        }
        for(int i=0; i < allCameraBases.Length; i++)
        {
            allCameraBases[i].KonoAwake();
        }
    }
    private void Start()
    {
        for(int i = 0; i < playerNum; i++)
        {
            allPlayers[i].KonoStart();
        }
        StartGame();

    }

    public ScoreManager scoreManager;
    // Update is called once per frame
    void Update () {
        print("GAME CONTROLLER UPDATE");
        if (!gamePaused)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                if (slowmo == 0)
                {
                    Time.timeScale = 0.25f;
                    slowmo = 1;
                }
                else if (slowmo == 1)
                {
                    Time.timeScale = 0.075f;
                    slowmo = 2;
                }
                else if (slowmo == 2)
                {
                    Time.timeScale = 1;
                    slowmo = 0;
                }
            }
            if (playing)
            {
                for (int i = 0; i < playerNum; i++)
                {
                    allPlayers[i].KonoUpdate();
                }
            }
            scoreManager.TiempoDeJuego();
        }
        else
        {
            if (playerActions.Jump.WasPressed){
                SceneManager.LoadScene(menuScene);
                UnPauseGame ();
            }
            else if (playerActions.Attack3.WasPressed){
                UnPauseGame ();
            }
        }

	}

    [HideInInspector]
    public bool playing = false;
    [HideInInspector]
    public bool gamePaused = false;
    public void StartGame()
    {
        playing = true;
        for (int i = 0; i < playerNum; i++)
        {
            RespawnPlayer(allPlayers[i]);
        }
    }

    [HideInInspector]
    public Team winnerTeam = Team.blue;
    public void ScorePoint(Team _winnerTeam)
    {
        scoreManager.ScorePoint(_winnerTeam);
    }
    public void GameOver(Team _winnerTeam)
    {
        //print("GAME OVER");
        playing = false;
        for(int i=0; i < flags.Length; i++)
        {
            flags[i].SetAway(true);
        }
        winnerTeam = _winnerTeam;
        SwitchGameOverMenu();
    }

    public Transform blueTeamSpawn;
    public Transform redTeamSpawn;
    public void RespawnPlayer(PlayerMovement player)
    {
        //print("RESPAWN PLAYER");
        player.SetVelocity(Vector3.zero);
        switch (player.team)
        {
            case Team.blue:
                player.transform.position = blueTeamSpawn.position;
                player.rotateObj.transform.localRotation = Quaternion.Euler(0, blueTeamSpawn.rotation.eulerAngles.y, 0);
                break;
            case Team.red:
                player.transform.position = redTeamSpawn.position;
                player.rotateObj.transform.localRotation = Quaternion.Euler(0, redTeamSpawn.rotation.eulerAngles.y, 0);
                break;
        }
        //player.myCamera.KonoAwake();
        //player.myCamera.SwitchCamera(player.myCamera.camMode);
        //player.myCamera.LateUpdate();
        player.myCamera.InstantPositioning();
        player.myCamera.InstantRotation();
        //player.myCamera.transform.localRotation = player.rotateObj.transform.localRotation;
        //player.myCamera.SwitchCamera(player.myCamera.camMode);
        player.ResetPlayer();
        player.myPlayerAnimation.RestartAnimation();
    }

    public Transform blueTeamFlagHome;
    public Transform redTeamFlagHome;
    public void RespawnFlag(Vector3 respawnPos)
    {
        print("RESPAWN FLAG");
        if (StoringManager.instance.IsObjectStored(flagPrefab.name))
        {
            StoringManager.instance.TakeObjectStored(flagPrefab.name, null, respawnPos, Quaternion.identity);
        }
        else
        {
            Instantiate(flagPrefab, respawnPos, Quaternion.identity, null);
        }
    }

    bool gameOverMenuOn = false;
    public GameObject gameOverMenu;
    public GameObject veil;
    public GameObject victoryText;
    public void SwitchGameOverMenu()
    {
        //print("GAME OVER MENU");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //print("gameOverMenuOn= " + gameOverMenuOn);
        if (gameOverMenuOn)
        {
            gameOverMenuOn = false;
            gameOverMenu.SetActive(false);
            veil.SetActive(false);
            victoryText.SetActive(false);
        }
        else
        {
            //print("ACTIVATE GAME OVER MENU");
            gameOverMenuOn = true;
            gameOverMenu.SetActive(true);
            veil.SetActive(true);
            victoryText.SetActive(true);
            if (winnerTeam == Team.blue)
            {
                victoryText.GetComponent<Text>().text = "BLUE TEAM WON";
                victoryText.GetComponent<Text>().color = Color.blue;
            }
            else if (winnerTeam == Team.red)
            {
                victoryText.GetComponent<Text>().text = "RED TEAM WON";
                victoryText.GetComponent<Text>().color = Color.red;
            }
        }
    }
    public string sceneLoadedOnReset;
    public void ResetGame()
    {
        ScoreManager.instance.Reset();
        playing = true;
        SwitchGameOverMenu();
        foreach(PlayerMovement pM in allPlayers)
        {
            pM.Die();
        }
        //SceneManager.LoadScene(sceneLoadedOnReset);
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    public enum controllerName
    {
        C1,
        C2,
        C3,
        C4
    }

#region ---------------------------------------------- Pause ----------------------------------------------
    [Header("Pause")]
    public string menuScene;

    public GameObject Button;
    
    private PlayerActions playerActions;

	public void PauseGame( PlayerActions p){
		Time.timeScale = 0;
        gamePaused = true;

        veil.SetActive ( true );
        Button.SetActive ( true );

		playerActions = p;

        gamePaused = true;
	}

    private void UnPauseGame (){
        Time.timeScale = 1;
        gamePaused = false;
        veil.SetActive ( false );
        Button.SetActive ( false );

		gamePaused = false;
    }

#endregion
}
