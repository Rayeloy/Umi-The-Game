using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

#region ----[ PUBLIC ENUMS ]----
public enum GameMode
{
    CaptureTheFlag,
    AirPump,
    Tutorial
}
#endregion

public class GameControllerBase : MonoBehaviourPunCallbacks
{

    #region ----[ HEADER REFERENCES ]----

    //referencias
    [Header(" --- Referencias --- ")]
    //este parámetro es para poner slowmotion al juego (como estados: 0=normal,1=slowmo,2=slowestmo),
    // solo se debe usar para testeo, hay que QUITARLO para la build "comercial".
    [Header(" --- Variables generales ---")]
    public GameMode gameMode;
    public bool offline;
    int slowmo = 0;

    //PREFABS
    [Header(" --- Player components prefabs ---")]
    public GameObject playerPrefab;
    public GameObject playerCanvasPrefab;
    public GameObject playerCameraPrefab;
    public GameObject playerUICameraPrefab;

    [Header(" --- Player components parents ---")]
    public Transform playersParent;
    public Transform playersCanvasParent;
    public Transform playersCamerasParent;
    public Transform playersUICamerasParent;

    [Header(" --- 'All' Player lists ---")]
    public List<WeaponData> allWeapons;//Array que contendrá las armas utilizadas, solo util en Pantalla Dividida, SIN USAR
    protected List<PlayerMovement> allPlayers;//Array que contiene a los PlayerMovement
    protected List<CameraController> allCameraBases;//Array que contiene todas las cameras bases, solo util en Pantalla Dividida
    protected List<GameObject> allCanvas;//Array que contiene los objetos de los canvas de cada jugador, solo util en Pantalla Dividida
    protected List<Camera> allUICameras;//Array que contiene todas las cameras bases, solo util en Pantalla Dividida

    [Header(" --- Spawn positions ---")]
    //Posiciones de los spawns
    public Transform blueTeamSpawn;
    public Transform redTeamSpawn;

    //Variables de HUDS
    [Header(" --- UI --- ")]
    public RectTransform[] contador;//Array que contiene todos los contadores de tiempo, solo util en Pantalla Dividida
    public RectTransform[] powerUpPanel;//Array que contiene los objetos del dash y el hook en el HUD, solo util en Pantalla Dividida
    [Header(" --- Escala UI --- ")]
    public float scaleDos = 1.25f;//escala de las camaras para 2 jugadores
    public float scaleCuatro = 1.25f;//escala para 3 jugadores y 4 jugadores

    //GAME OVER MENU
    bool gameOverMenuOn = false;
    bool gameOverStarted = false;
    [Header(" --- Game Over Menu --- ")]
    public GameObject gameOverMenu;
    public GameObject veil;
    public Image victoryRed;
    public Image victoryBlue;
    public Text gameOverPressStart;
    public GameObject gameOverFirstButton;//intento fallido de controlar qué boton se selecciona automáticamente al iniciar el menu de Game Over
    [SerializeField]
    private GameObject ResetButton;
    public string sceneLoadedOnReset;

    [Header(" --- Pause --- ")]
    public string menuScene;
    public GameObject Button;
    private PlayerActions playerActions;

    #endregion

    #region ----[ PROPERTIES ]----

    //Number of players in the game. In online it will start at 0 and add +1 every time a player joins. In offline it stays constant since the game scene starts
    [HideInInspector]
    public int playerNum = 1;
    [HideInInspector]
    public Team winnerTeam = Team.blue;
    //Pause Menu
    [HideInInspector]
    public bool gamePaused = false;
    //variables globales de la partida
    [HideInInspector]
    public bool playing = false;

    #endregion

    #region ----[ VARIABLES ]----

    //public AttackData[] allAttacks; //este array seria otra manera de organizar los distintos ataques. En el caso de haber muchos en vez de 3 puede que usemos algo como esto.
    //Estos son los ataques de los jugadores, seguramente en un futuro vayan adheridos al jugador, o a su arma. Si no, será un mega array (arriba)
    public AttackData attackX;
    public AttackData attackY;
    public AttackData attackB;

    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    protected virtual void Awake()
    {
        Debug.Log("GameController Awake empezado");
#if UNITY_EDITOR //Esto es para no entrar en escenas cuando no tenemos los controles hechos en el editor. Te devuelve a seleccion de equipo
        if (GameInfo.instance == null)
        {
            string escena = TeamSetupManager.siguienteEscena;
            print(escena);
            TeamSetupManager.siguienteEscena = SceneManager.GetActiveScene().name;
            TeamSetupManager.startFromMap = true;
            SceneManager.LoadScene("TeamSetup");
            return;
        }
#endif
        offline = GameInfo.instance.offline;

        //initialize lists
        allPlayers = new List<PlayerMovement>();
        allCameraBases = new List<CameraController>();
        allCanvas = new List<GameObject>();
        allUICameras = new List<Camera>();

        gameOverMenu.SetActive(false);
        gameOverPressStart.enabled = false;
        veil.SetActive(false);
        gameOverMenuOn = false;
        if (offline && gameMode != GameMode.Tutorial)
        {
            victoryRed.gameObject.SetActive(false);
            victoryBlue.gameObject.SetActive(false);
        }

        if (offline)
        {
            playerNum = GameInfo.instance.nPlayers;
            playerNum = Mathf.Clamp(playerNum, 1, 4);
            for (int i = 0; i < playerNum; i++)
            {
                CreatePlayer(i + 1);
            }

            //AUTOMATIC PLAYERS & CAMERAS/CANVAS SETUP
            PlayersSetup();
            SetUpCanvas();
            AllAwakes();
        }
        else //Eloy: para Juan: aqui inicia al host! playerNum deberia estar a 0 y luego ponerse a 1 cuando se crea el jugador
        {
            //CreatePlayer
            //PlayerSetup
            //PlayerSetupOnline?
            //No hace falta SetUpCanvas creo
            //Haz los awakes, y haz el awake de cada jugador nuevo(esto ultimo hay que buscar donde ponerlo... en el CreatePlayer?
        }
        Debug.Log("Game Controller Awake terminado");
    }

    
    #endregion

    #region Start
    private void Start()
    {
        StartPlayers();
        StartGame();
        Debug.Log("GameController Start terminado");
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
    public virtual void StartGame()
    {
        playing = true;
        gamePaused = false;
        for (int i = 0; i < playerNum; i++)
        {
            RespawnPlayer(allPlayers[i]);
        }
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
                UpdateModeExclusiveClasses();
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

    protected virtual void UpdateModeExclusiveClasses()
    {
    }
    #endregion

    #endregion

    #region ----[ CLASS FUNCTIONS ]----

    #region AWAKE AND CREATE PLAYERS
    /// <summary>
    /// Funcion que Inicializa valores de todos los jugadores y sus cámaras.
    /// </summary>
    void PlayersSetup()
    {
        if (offline)
        {
            for (int i = 0; i < allPlayers.Count; i++)
            {
                if (i < playerNum)
                {
                    //LE DAMOS AL JUGADOR SUS CONTROLES (Mando/teclado) y SU EQUIPO
                    allPlayers[i].Actions = GameInfo.instance.playerActionsList[i];

                    if (GameInfo.instance.playerTeamList[i] == Team.none)
                    {
                        GameInfo.instance.playerTeamList[i] = GameInfo.instance.NoneTeamSelect();
                    }

                    allPlayers[i].team = GameInfo.instance.playerTeamList[i];
                }
            }
            //SETUP CAMERAS
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
        }
    }

    protected virtual void AllAwakes()
    {
        for (int i = 0; i < allCameraBases.Count; i++)
        {
            allPlayers[i].KonoAwake();
            allCameraBases[i].KonoAwake();
        }
    }

    public void CreatePlayer(int playerNumber = 0)
    {
        if (offline)
        {
            PlayerMovement newPlayer;
            GameObject newPlayerCanvas;
            CameraController newPlayerCamera;
            Camera newPlayerUICamera;

            //playerNum++;
            newPlayer = Instantiate(playerPrefab, playersParent).GetComponent<PlayerMovement>();
            newPlayerCanvas = Instantiate(playerCanvasPrefab, playersCanvasParent);
            newPlayerCamera = Instantiate(playerCameraPrefab, playersCamerasParent).GetComponent<CameraController>();
            newPlayerUICamera = Instantiate(playerUICameraPrefab, playersUICamerasParent).GetComponent<Camera>();

            //nombrado de objetos nuevos
            newPlayer.gameObject.name = "Player";
            newPlayer.gameObject.name = newPlayer.gameObject.name + playerNum;

            //Inicializar referencias
            //Player
            newPlayer.gC = this;
            newPlayer.myCamera = newPlayerCamera;
            newPlayer.myPlayerHUD = newPlayerCanvas.GetComponent<PlayerHUD>();
            newPlayer.myUICamera = newPlayerUICamera;
            //Canvas
            newPlayerCanvas.GetComponent<PlayerHUD>().gC = this;
            newPlayerCanvas.GetComponent<Canvas>().worldCamera = newPlayerUICamera;
            //CameraBase
            newPlayerCamera.myPlayerMov = newPlayer;
            newPlayerCamera.myPlayer = newPlayer.transform;
            newPlayerCamera.cameraFollowObj = newPlayer.cameraFollow;

            //Añadir a los arrays todos los componentes del jugador
            //guarda jugador
            allPlayers.Add(newPlayer);
            allCanvas.Add(newPlayerCanvas);
            allCameraBases.Add(newPlayerCamera);
            allUICameras.Add(newPlayerUICamera);
        }
    }

    public void RemovePlayer(PlayerMovement _pM)//solo para online
    {
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i] == _pM)
            {
                allPlayers.Remove(_pM);
                allCanvas.Remove(_pM.myPlayerHUD.gameObject);
                allCameraBases.Remove(_pM.myCamera);
                allUICameras.Remove(_pM.myUICamera);

                Destroy(allPlayers[i].gameObject);
                Destroy(allCanvas[i]);
                Destroy(allCameraBases[i].gameObject);
                Destroy(allUICameras[i].gameObject);
            }
        }
    }

    /// <summary>
    /// Inicia los canvas con las variables y tamaños necesarios para el numero de jugadores. 
    /// </summary>
    private void SetUpCanvas()//Para PantallaDividida
    {
        if (offline)
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
    }

    #endregion

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

    #region ----[ PUN CALLBACKS ]----
    #endregion

    #region ----[ RPC ]----
    #endregion

    #region ----[ NETWORK FUNCTIONS ]----
    #endregion

    #region ----[ IPUNOBSERVABLE ]----
    #endregion
}
