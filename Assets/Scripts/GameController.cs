using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public static GameController instance;
    public AttackData[] allAttacks;
    bool slowmo = false;
    public PlayerMovement[] allPlayers;
    public GameObject[] allCanvas;
    public CameraControler[] allCameraBases;
    private void Awake()
    {
        instance = this;
        gameOverMenu.SetActive(false);
        veil.SetActive(false);
        victoryText.SetActive(false);
        gameOverMenuOn = false;

        //AUTOMATIC PLAYERS/CAMERAS/CANVAS SETUP
        PlayersSetup();
    }

    void PlayersSetup()
    {
        for (int i = 0; i < allCanvas.Length; i++)
        {
            if (i < allPlayers.Length)
            {
                allCanvas[i].SetActive(true);
                allCameraBases[i].gameObject.SetActive(true);
                allCanvas[i].GetComponent<Canvas>().worldCamera = allCameraBases[i].myCamera.GetComponent<Camera>();
                allPlayers[i].myCamera = allCameraBases[i];
                allPlayers[i].GetComponent<PlayerCombat>().attackName = allCanvas[i].transform.GetChild(0).GetComponent<Text>();
            }
            else
            {
                allCanvas[i].SetActive(false);
                allCameraBases[i].gameObject.SetActive(false);
            }
        }
        switch (allPlayers.Length)
        {
            case 1:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
                break;
            case 2:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0.5f, 1, 0.5f);
                allCameraBases[1].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 0.5f);
                break;
            case 3:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0.5f, 1, 0.5f);
                allCameraBases[1].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 0.5f);
                allCameraBases[2].myCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0, 1, 0.5f);
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
        StartGame();
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            if (!slowmo)
            {
                Time.timeScale = 0.25f;
                slowmo = true;
            }
            else
            {
                Time.timeScale = 1;
                slowmo = false;
            }
        }
        if (playing)
        {
            foreach (PlayerMovement pM in allPlayers)
            {
                pM.KonoUpdate();
            }
        }
	}
    [HideInInspector]
    public bool playing = false;
    public void StartGame()
    {
        playing = true;
        foreach(PlayerMovement pM in allPlayers)
        {
            switch (pM.team)
            {
                case PlayerMovement.Team.blue:
                    pM.transform.position = blueTeamSpawn.position;
                    break;
                case PlayerMovement.Team.red:
                    pM.transform.position = redTeamSpawn.position;
                    break;
            }
        }
    }

    [HideInInspector]
    public PlayerMovement.Team winnerTeam = PlayerMovement.Team.blue;
    public void GameOver(PlayerMovement.Team _winnerTeam)
    {
        print("GAME OVER");
        playing = false;
        winnerTeam = _winnerTeam;
        SwitchGameOverMenu();
    }

    public Transform blueTeamSpawn;
    public Transform redTeamSpawn;
    public void RespawnPlayer(PlayerMovement player)
    {
        print("RESPAWN PLAYER");
        switch (player.team)
        {
            case PlayerMovement.Team.blue:
                player.transform.position = blueTeamSpawn.position;
                break;
            case PlayerMovement.Team.red:
                player.transform.position = redTeamSpawn.position;
                break;
        }
    }

    public void RespawnFlag(Flag flag)
    {
        print("RESPAWN FLAG"); 
        flag.transform.SetParent(null);
        flag.transform.position = flag.respawnPos;
    }

    bool gameOverMenuOn = false;
    public GameObject gameOverMenu;
    public GameObject veil;
    public GameObject victoryText;
    public void SwitchGameOverMenu()
    {
        print("GAME OVER MENU");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        print("gameOverMenuOn= " + gameOverMenuOn);
        if (gameOverMenuOn)
        {
            gameOverMenuOn = false;
            gameOverMenu.SetActive(false);
            veil.SetActive(false);
            victoryText.SetActive(false);
        }
        else
        {
            print("ACTIVATE GAME OVER MENU");
            gameOverMenuOn = true;
            gameOverMenu.SetActive(true);
            veil.SetActive(true);
            victoryText.SetActive(true);
            if (winnerTeam == PlayerMovement.Team.blue)
            {
                victoryText.GetComponent<Text>().text = "BLUE TEAM WON";
                victoryText.GetComponent<Text>().color = Color.blue;
            }
            else if (winnerTeam == PlayerMovement.Team.red)
            {
                victoryText.GetComponent<Text>().text = "RED TEAM WON";
                victoryText.GetComponent<Text>().color = Color.red;
            }
        }
    }

    public string sceneLoadedOnReset;
    public void ResetGame()
    {
        
        playing = true;
        SwitchGameOverMenu();
        SceneManager.LoadScene(sceneLoadedOnReset);
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
}
