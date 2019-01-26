using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameControllerBase : MonoBehaviour
{
    #region Variables
    public static GameControllerBase instance;

    //referencias
    public ScoreManager scoreManager;

    //este parámetro es para poner slowmotion al juego (como estados: 0=normal,1=slowmo,2=slowestmo),
    // solo se debe usar para testeo, hay que QUITARLO para la build "comercial".
    int slowmo = 0;
    public string sceneLoadedOnReset;
    public GameMode gameMode;

    //Number of players in the game. This will be a constant in the future, the number being variable is just for testing purposes.
    [HideInInspector]
    [Range(1, 4)]
    public int playerNum = 1;

    public PlayerMovement[] allPlayers;//Array que contiene a los PlayerMovement
    public CameraController[] allCameraBases;//Array que contiene todas las cameras bases, solo util en Pantalla Dividida
    public Camera[] allUICameras;//Array que contiene todas las cameras bases, solo util en Pantalla Dividida
    public WeaponData[] allWeapons;//Array que contendrá las armas utilizadas, solo util en Pantalla Dividida, SIN USAR

    //public AttackData[] allAttacks; //este array seria otra manera de organizar los distintos ataques. En el caso de haber muchos en vez de 3 puede que usemos algo como esto.
    //Estos son los ataques de los jugadores, seguramente en un futuro vayan adheridos al jugador, o a su arma. Si no, será un mega array (arriba)
    public AttackData attackX;
    public AttackData attackY;
    public AttackData attackB;
    public AttackData attackHook;

    //Posiciones de los spawns
    public Transform blueTeamSpawn;
    public Transform redTeamSpawn;

    //Variables de HUDS
    [Header(" --- VARIABLES DE LA UI --- ")]
    public GameObject[] allCanvas;//Array que contiene los objetos de los canvas de cada jugador, solo util en Pantalla Dividida
    public RectTransform[] contador;//Array que contiene todos los contadores de tiempo, solo util en Pantalla Dividida
    public RectTransform[] powerUpPanel;//Array que contiene los objetos del dash y el hook en el HUD, solo util en Pantalla Dividida
    [Header("Escala UI")]
    public float scaleDos = 1.25f;//escala de las camaras para 2 jugadores
    public float scaleCuatro = 1.25f;//escala para 3 jugadores y 4 jugadores

    //GAME OVER MENU
    bool gameOverMenuOn = false;
    public GameObject gameOverMenu;
    public GameObject veil;
    public Image victoryRed;
    public Image victoryBlue;
    public Text gameOverPressStart;
    public GameObject gameOverFirstButton;//intento fallido de controlar qué boton se selecciona automáticamente al iniciar el menu de Game Over
    [SerializeField]
    private GameObject ResetButton;

    //Pause Menu
    [Header("Pause")]
    [HideInInspector]
    public bool gamePaused = false;
    public string menuScene;
    public GameObject Button;
    private PlayerActions playerActions;

    //variables globales de la partida
    [HideInInspector]
    public bool playing = false;
    [Header("Game Over Menu")]
    [HideInInspector]
    public Team winnerTeam = Team.blue;
    bool gameOverStarted = false;


    #endregion

    #region Funciones de Monobehaviour

    #region Awake
    protected virtual void Awake()
    {
#if UNITY_EDITOR //Esto es para no entrar en escenas cuando no tenemos los controles hechos en el editor. Te devuelve a seleccion de equipo
        if (GameInfo.instance == null)
        {
            string escena = TeamSetupManager.SiguenteEscena;
            print(escena);
            TeamSetupManager.SiguenteEscena = SceneManager.GetActiveScene().name;
            TeamSetupManager.startFromMap = true;
            SceneManager.LoadScene("TeamSetup");
            return;
        }
#endif
        instance = this;
        gameOverMenu.SetActive(false);
        gameOverPressStart.enabled = false;
        veil.SetActive(false);
        if (gameMode != GameMode.Tutorial)
        {
            victoryRed.gameObject.SetActive(false);
            victoryBlue.gameObject.SetActive(false);
        }
        gameOverMenuOn = false;
        playerNum = GameInfo.instance.nPlayers;
        playerNum = Mathf.Clamp(playerNum, 1, 4);

        //AUTOMATIC PLAYERS/CAMERAS/CANVAS SETUP
        PlayersSetup();
    }

    /// <summary>
    /// Funcion que inicializa a los jugadores, sus cámaras y canvases. Para pantalla dividida.
    /// </summary>
    void PlayersSetup()//para Pantalla Dividida
    {
        for (int i = 0; i < allCanvas.Length; i++)
        {
            if (i < playerNum)
            {
                allCanvas[i].SetActive(true);
                allCameraBases[i].gameObject.SetActive(true);
                allCanvas[i].GetComponent<Canvas>().worldCamera = allUICameras[i];
                allPlayers[i].gameObject.SetActive(true);
                allPlayers[i].myCamera = allCameraBases[i];
                allPlayers[i].GetComponent<PlayerCombat>().attackName = allCanvas[i].transform.GetChild(0).GetComponent<Text>();
                allPlayers[i].Actions = GameInfo.instance.playerActionsList[i];
                if (GameInfo.instance.playerTeamList[i] == Team.none)
                    GameInfo.instance.playerTeamList[i] = GameInfo.instance.NoneTeamSelect();

                allPlayers[i].team = GameInfo.instance.playerTeamList[i];
            }
            else
            {
                allPlayers[i].gameObject.SetActive(false);
                allCanvas[i].SetActive(false);
                allCameraBases[i].gameObject.SetActive(false);
                allUICameras[i].gameObject.SetActive(false);
            }
        }
        switch (playerNum)
        {
            case 1:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
                allUICameras[0].rect = new Rect(0, 0, 1, 1);
                break;
            case 2:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0.5f, 1, 0.5f);
                allCameraBases[1].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 0.5f);
                allUICameras[0].rect = new Rect(0, 0.5f, 1, 0.5f);
                allUICameras[1].rect = new Rect(0, 0, 1, 0.5f);
                break;
            case 3:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                allCameraBases[1].myCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                allCameraBases[2].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 0.5f);
                allUICameras[0].rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                allUICameras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                allUICameras[2].rect = new Rect(0, 0, 1, 0.5f);
                break;
            case 4:
                allCameraBases[0].myCamera.GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                allCameraBases[1].myCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                allCameraBases[2].myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 0.5f);
                allCameraBases[3].myCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                allUICameras[0].rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                allUICameras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                allUICameras[2].rect = new Rect(0, 0, 0.5f, 0.5f);
                allUICameras[3].rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                break;
        }
        for (int i = 0; i < allCameraBases.Length; i++)
        {
            allCameraBases[i].KonoAwake();
        }
    }

    /// <summary>
    /// Funcion que inicializa a un jugador, su cámara y canvas. Para online.
    /// </summary>
    void PlayerSetupOnline()
    {
        //inicializa jugador
        //inicializa camara
        //inicializa canvas
    }

    /// <summary>
    /// Inicia los canvas con las variables y tamaños necesarios para el numero de jugadores. 
    /// </summary>
    private void SetUpCanvas()//Para PantallaDividida
    {
        if (playerNum >= 2)
        {
            contador[0].anchoredPosition = new Vector3(contador[0].anchoredPosition.x, 100, contador[0].anchoredPosition.y);
            contador[1].anchoredPosition = new Vector3(contador[1].anchoredPosition.x, 100, contador[1].anchoredPosition.y);
            contador[2].anchoredPosition = new Vector3(contador[2].anchoredPosition.x, 100, contador[2].anchoredPosition.y);
            contador[3].anchoredPosition = new Vector3(contador[3].anchoredPosition.x, 100, contador[3].anchoredPosition.y);
        }

        if (playerNum == 2)
        {
            contador[0].localScale /= scaleDos;
            contador[1].localScale /= scaleDos;

            powerUpPanel[0].localScale /= scaleDos;
            powerUpPanel[1].localScale /= scaleDos;
        }
        else if (playerNum == 3)
        {
            contador[0].localScale /= scaleCuatro;
            contador[1].localScale /= scaleCuatro;
            contador[2].localScale /= scaleDos;

            powerUpPanel[0].localScale /= scaleCuatro;
            powerUpPanel[1].localScale /= scaleCuatro;
            powerUpPanel[2].localScale /= scaleDos;
        }
        else if (playerNum == 4)
        {
            contador[0].localScale /= scaleCuatro;
            contador[1].localScale /= scaleCuatro;
            contador[2].localScale /= scaleCuatro;
            contador[3].localScale /= scaleCuatro;

            powerUpPanel[0].localScale /= scaleCuatro;
            powerUpPanel[1].localScale /= scaleCuatro;
            powerUpPanel[2].localScale /= scaleCuatro;
            powerUpPanel[3].localScale /= scaleCuatro;
        }
    }
    #endregion

    #region Start
    private void Start()
    {
        StartPlayers();
        StartGame();
    }

    //Funcion que llama al Start de los jugadores. Eloy: Juan, ¿solo pantalla dividida?
    void StartPlayers()
    {
        for (int i = 0; i < playerNum; i++)
        {
            allPlayers[i].KonoStart();
        }
    }

    //Funcion que se llama al comenzar la partida, que inicicia las variables necesarias, y que posiciona a los jugadores y ¿bandera?
    public void StartGame()
    {
        switch (gameMode)
        {
            case GameMode.CaptureTheFlag:
                ScoreManager.instance.KonoStart();
                break;
            case GameMode.AirPump:
                break;
            case GameMode.Tutorial:
                break;
        }
        playing = true;
        gamePaused = false;
        for (int i = 0; i < playerNum; i++)
        {
            RespawnPlayer(allPlayers[i]);
        }

        SetUpCanvas();
    }
    #endregion

    #region Update
    void Update()
    {
        //if (scoreManager.End) return;

        if (!gamePaused)
        {

            if (playing)
            {
                UpdatePlayers();
            }

            switch (gameMode)
            {
                case GameMode.CaptureTheFlag:
                    scoreManager.KonoUpdate();
                    break;
                case GameMode.AirPump:
                    break;
                case GameMode.Tutorial:
                    break;
            }

        }
        else
        {
            if (playing)
            {
                if (playerActions.Jump.WasPressed)
                {
                    GoBackToMenu();
                }
                else if (playerActions.Attack3.WasPressed || playerActions.Options.WasPressed)
                {
                    UnPauseGame();
                }
            }
            else
            {
                if (gameOverStarted && !gameOverMenuOn)
                {
                    for (int i = 0; i < playerNum; i++)
                    {
                        if (allPlayers[i].Actions.Options.WasPressed)
                        {
                            SwitchGameOverMenu();
                            i = playerNum;//BREAK
                        }
                    }
                }
            }
        }
    }

    void UpdatePlayers()
    {
        for (int i = 0; i < playerNum; i++)
        {
            allPlayers[i].KonoUpdate();
        }
    }
    #endregion

    #endregion

    #region Funciones
    void SlowMotion()
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
    }

    public virtual void StartGameOver(Team _winnerTeam)
    {
        //print("GAME OVER");
        if (!gameOverStarted)
        {
            playing = false;
            gamePaused = true;
            winnerTeam = _winnerTeam;
            gameOverStarted = true;
            veil.SetActive(true);
            if (winnerTeam == Team.blue)
            {
                victoryBlue.gameObject.SetActive(true);
            }
            else if (winnerTeam == Team.red)
            {
                victoryRed.gameObject.SetActive(true);
            }
            gameOverPressStart.enabled = true;
        }
    }

    public void SwitchGameOverMenu()
    {
        //print("GAME OVER MENU");
        //print("gameOverMenuOn= " + gameOverMenuOn);
        if (gameOverStarted)
        {
            if (gameOverMenuOn)
            {
                //GameObject incontrol = GameObject.Find("InControl manager");
                //Destroy(incontrol);
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
                gameOverMenuOn = false;
                gameOverMenu.SetActive(false);
                veil.SetActive(false);
                victoryRed.gameObject.SetActive(false);
                victoryBlue.gameObject.SetActive(false);
                gameOverPressStart.enabled = false;
                gameOverStarted = false;
            }
            else
            {
                //print("ACTIVATE GAME OVER MENU");
                //Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;
                gameOverMenuOn = true;
                gameOverMenu.SetActive(true);
                gameOverPressStart.enabled = false;
                EventSystem.current.SetSelectedGameObject(gameOverFirstButton);
            }
        }
    }

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

    public virtual void ResetGame()
    {
        playing = true;
        gamePaused = false;
        SwitchGameOverMenu();
        foreach (PlayerMovement pM in allPlayers)
        {
            pM.Die();
        }
        if (gamePaused)
            UnPauseGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    #region ---------------------------------------------- Pause ----------------------------------------------
    public void PauseGame(PlayerActions p)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0;
        gamePaused = true;

        veil.SetActive(true);
        Button.SetActive(true);

        playerActions = p;

        gamePaused = true;

        EventSystem.current.SetSelectedGameObject(ResetButton);
    }

    private void UnPauseGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1;
        veil.SetActive(false);
        Button.SetActive(false);

        gamePaused = false;
    }

    public void GoBackToMenu()
    {
        UnPauseGame();
        GameObject inControlManager = GameObject.Find("InControl manager");
        Destroy(inControlManager);
        SceneManager.LoadScene(menuScene);
    }

    #endregion
    #endregion
}

public enum GameMode
{
    CaptureTheFlag,
    AirPump,
    Tutorial
}
