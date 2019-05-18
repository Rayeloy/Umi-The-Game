using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

#region ----[ PUBLIC ENUMS ]----
public enum GameMode
{
    CaptureTheFlag,
    AirPump,
    Tutorial
}
#endregion

public class GameControllerBase : MonoBehaviour
{

    #region ----[ VARIABLES FOR DESIGNERS ]----

    //referencias
    [Header(" --- Referencias --- ")]
    public GameInterface myGameInterface;
    //este parámetro es para poner slowmotion al juego (como estados: 0=normal,1=slowmo,2=slowestmo),
    // solo se debe usar para testeo, hay que QUITARLO para la build "comercial".
    [Header(" --- Variables generales ---")]
    public GameMode gameMode;
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

    //public AttackData[] allAttacks; //este array seria otra manera de organizar los distintos ataques. En el caso de haber muchos en vez de 3 puede que usemos algo como esto.
    //Estos son los ataques de los jugadores, seguramente en un futuro vayan adheridos al jugador, o a su arma. Si no, será un mega array (arriba)
    //[Header("Attacks")]
    //public AttackData attackX;
    //public AttackData attackY;
    //public AttackData attackB;

    [Header(" --- 'All' lists ---")]
    public BufferedInputData[] allBufferedInputs;

    [Header(" --- Spawn positions ---")]
    //Posiciones de los spawns
    public Transform[] blueTeamSpawns;
    public Transform[] redTeamSpawns;

    //Variables de HUDS
    [Header(" --- Players HUD --- ")]

    private List<RectTransform> contador;//Array que contiene todos los contadores de tiempo, solo util en Pantalla Dividida
    private List<RectTransform> powerUpPanel;//Array que contiene los objetos del dash y el hook en el HUD, solo util en Pantalla Dividida
    [Header(" --- Players HUD scale --- ")]
    public float scaleDos = 1.25f;//escala de las camaras para 2 jugadores
    public float scaleCuatro = 1.25f;//escala para 3 jugadores y 4 jugadores
    [Header(" --- Other Stuff --- ")]
    public Weapon startingWeaponBlue;
    public Weapon startingWeaponRed;
    #endregion

    #region ----[ PROPERTIES ]----

    protected int playerNumTeamA = 0, playerNumTeamB = 0;
    private PlayerActions playerActions;

    //GAME OVER MENU
    [HideInInspector]
    public bool gameOverStarted = false;

    //Player components lists
    protected List<PlayerMovement> allPlayers;//Array que contiene a los PlayerMovement
    protected List<CameraController> allCameraBases;//Array que contiene todas las cameras bases, solo util en Pantalla Dividida
    protected List<GameObject> allCanvas;//Array que contiene los objetos de los canvas de cada jugador, solo util en Pantalla Dividida
    protected List<Camera> allUICameras;//Array que contiene todas las cameras bases, solo util en Pantalla Dividida

    //Number of players in the game. In online it will start at 0 and add +1 every time a player joins. In offline it stays constant since the game scene starts
    [HideInInspector]
    public int playerNum = 1;

    //Pause Menu
    [HideInInspector]
    public bool gamePaused = false;
    //variables globales de la partida
    [HideInInspector]
    public bool playing = false;
    [HideInInspector]
    public bool online; //= PhotonNetwork.IsConnected; JUAN: No se puede inicializar el valor porque tira un error chungo, THX UNITY, está inicializado en el Awake

    // variables para nuestro jugador online
    [HideInInspector]
    PlayerMovement onlinePlayer;
    [HideInInspector]
    CameraController onlineCamera;
    [HideInInspector]
    GameObject onlineCanvas;
    [HideInInspector]
    Camera onlineUICamera;
    [HideInInspector]
    public PlayerActions BaseGameActions { get; set; }

    [HideInInspector]
    public bool HasPlayerFlatCamera(PlayerMovement pM)
    {
        if (allPlayers.Count == 2)
        {
            return true;
        }
        else if(allPlayers.Count == 3)
        {
            for(int i=0; i<allPlayers.Count; i++)
            {
                if (allPlayers[i] == pM)
                {
                    if (i == 2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region ----[ VARIABLES ]----



    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    protected virtual void Awake()
    {
        //online = PhotonNetwork.IsConnected;
        online = false;
        if (online)
        {
            Debug.Log("GameControllerBase: estamos conectados y la base del game controller está funcionando correctamente");
        }
        else
        {
            //Esto es para no entrar en escenas cuando no tenemos los controles. Te devuelve a seleccion de equipo
            //Eloy: he cambiado esto porque me he dado cuenta de que es necesario hasta en la build final, no solo en el editor.
            if (GameInfo.instance == null || GameInfo.instance.inControlManager == null)
            {
                string escena = TeamSetupManager.siguienteEscena;
                //print(escena);
                TeamSetupManager.siguienteEscena = SceneManager.GetActiveScene().name;
                TeamSetupManager.startFromMap = true;
                SceneManager.LoadScene("TeamSetup");
                return;
            }
        }

        //Deactivate test player components. DO NOT MOVE!
        DeactivatePlayerComponents();


        //initialize lists
        allPlayers = new List<PlayerMovement>();
        allCameraBases = new List<CameraController>();
        allCanvas = new List<GameObject>();
        allUICameras = new List<Camera>();

        contador = new List<RectTransform>();
        powerUpPanel = new List<RectTransform>();

        //Check data
        CheckValidInputsBuffer();

        //Set stuff active
        myGameInterface.gameObject.SetActive(true);

        if (!online)
        {
            playerNum = GameInfo.instance.nPlayers;
            playerNum = Mathf.Clamp(playerNum, 1, 4);
            for (int i = 0; i < playerNum; i++)
            {
                CreatePlayer(""+(i+1));
            }

            //AUTOMATIC PLAYERS & CAMERAS/CANVAS SETUP
            PlayersSetup();
            SetUpCanvases();
            AllAwakes();
        }
        else //Eloy: para Juan: aqui inicia al host! playerNum deberia estar a 0 y luego ponerse a 1 cuando se crea el jugador
        {
            //CreatePlayer
            //PlayerSetup
            //PlayerSetupOnline?
            //No hace falta SetUpCanvas creo
            //Haz los awakes, y haz el awake de cada jugador nuevo(esto ultimo hay que buscar donde ponerlo... en el CreatePlayer?
            //int playernumber = PhotonNetwork.CurrentRoom.PlayerCount;
            //CreatePlayer(""+playernumber);

            onlinePlayer.Actions = GameInfo.instance.myControls == null?PlayerActions.CreateWithKeyboardBindings():GameInfo.instance.myControls;
            //Juan: hay que hacer la toma de valores de TeamSetupManager aquí pero bueh...
            onlineCamera.myCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            onlineUICamera.rect = new Rect(0, 0, 1, 1);

            myGameInterface.KonoAwake(this);
            onlinePlayer.KonoAwake();
            onlineCamera.KonoAwake();

            gameOverStarted = false;
            contador[0].anchoredPosition = new Vector3(contador[0].anchoredPosition.x, 100, contador[0].anchoredPosition.y);

            //OnlinePlayerSetup();
            //OnlineCanvasSetUp();
            //OnlineAwakePlayer();

        }
        SpecificAwake();
    }

    protected virtual void SpecificAwake()
    {
        //code for children's awake
    }
 
    #endregion

    #region Start
    protected virtual void Start()
    {
        if (!online)
        {
            StartPlayers();
            StartGame();
            Debug.Log("GameController Start terminado");
        }
        else
        {
            onlinePlayer.KonoStart();
            playing = true;
            gamePaused = false;

            onlinePlayer.SetVelocity(Vector3.zero);
            onlinePlayer.myCamera.InstantPositioning();
            onlinePlayer.myCamera.InstantRotation();
            onlinePlayer.ResetPlayer();
            onlinePlayer.myPlayerAnimation.RestartAnimation();

            //if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            //{
            //    Debug.Log("GameControllerBase: Empezamos el juego pues se han unido todos los jugadores");
            //}
        }
    }

    //Funcion que llama al Start de los jugadores. Eloy: Juan, ¿solo pantalla dividida?, JUAN: Sí Eloy, sólo pantalla dividida.
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
        //if (playerNum == 1)
        //{
        //    allPlayers[0].Actions = GameInfo.instance.myControls;
        //}
        //if (scoreManager.End) return;
        SlowMotion();

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
                if (playerActions.B.WasPressed || playerActions.Start.WasPressed)
                {
                    UnPauseGame();
                }
            }
            else
            {
                if (gameOverStarted && !myGameInterface.gameOverMenuOn)
                {
                    for (int i = 0; i < playerNum; i++)
                    {
                        if (allPlayers[i].Actions.Start.WasPressed)
                        {
                            SwitchGameOverMenu();
                            i = playerNum;//BREAK
                        }
                    }
                }
            }
        }
        SpecificUpdate();
    }

    private void FixedUpdate()
    {
    }

    void UpdatePlayers()
    {
        for (int i = 0; i < playerNum; i++)
        {
            allPlayers[i].KonoUpdate();
        }
    }

    protected virtual void SpecificUpdate()
    {

    }

    protected virtual void UpdateModeExclusiveClasses()//no borrar, es para los hijos
    {
    }
    #endregion

    #endregion

    #region ----[ CLASS FUNCTIONS ]----

    #region AWAKE / CREATE PLAYERS / SPAWN POSITIONS

    /// <summary>
    /// Funcion que Inicializa valores de todos los jugadores y sus cámaras.
    /// </summary>
    void PlayersSetup()
    {
        if (!online)
        {
            for (int i = 0; i < allPlayers.Count; i++)
            {
                if (i < playerNum)
                {
                    //LE DAMOS AL JUGADOR SUS CONTROLES (Mando/teclado) y SU EQUIPO
                    if (allPlayers.Count == 1)
                    {
                        allPlayers[0].Actions = GameInfo.instance.myControls;
                    }
                    else
                    {
                        allPlayers[i].Actions = GameInfo.instance.playerActionsList[i];
                    }

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
                    allUICameras[0].rect = new Rect(0, 0, 1f, 1);
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

            SetSpawnPositions();//Eloy: Para Juan: Esto tendrás que usarlo en online también supongo... JUAN: nup, con el photonNetwork.Instantiate ya les coloco en los spawns de entrada
        }
    }

    protected virtual void AllAwakes()
    {
        for (int i = 0; i < allCameraBases.Count; i++)
        {
            myGameInterface.KonoAwake(this);
            allPlayers[i].KonoAwake();
            allCameraBases[i].KonoAwake();
        }
    }


    public virtual void CreatePlayer(string playerNumber)
    {
        PlayerMovement newPlayer;
        GameObject newPlayerCanvas;
        CameraController newPlayerCamera;
        Camera newPlayerUICamera;

        if (online)
        {
            if (playerPrefab == null)
            {
                Debug.Log("GamerControllerBase: Color=Red><a>Missing playerPrefab Reference in GameController</a></Color>");
            }
            else
            {
                Debug.Log("GameControllerBase: Instantiating player over the network");
                //JUAN: WARNING!!, el objeto que se instancie debe estar siempre en la carpeta de Resources de Photon, o ir al método de instantiate para cambiarlo
                //JUAN: Eloy, donde dice Vector3 y Quartenion debe ser para establecer la posición del spawn del jugador, para hacer las pruebas lo dejo to random pero hay que mirarlo
                if (PlayerMovement.LocalPlayerInstance == null)
                {
                        //Debug.LogFormat("GameControllerBase: We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                        Team newPlayerTeam = GameInfo.instance.NoneTeamSelect();
                        Vector3 respawn = new Vector3(-200, -200, -200);
                        if (newPlayerTeam == Team.A)
                        {
                            respawn = blueTeamSpawns[0].position;
                        }
                        else if (newPlayerTeam == Team.B)
                        {
                            respawn = redTeamSpawns[0].position;
                        }
                    //newPlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, respawn, Quaternion.identity, 0).GetComponent<PlayerMovement>();
                    newPlayer = null;
                        newPlayerCanvas = Instantiate(playerCanvasPrefab, playersCanvasParent);
                        newPlayerCamera = Instantiate(playerCameraPrefab, playersCamerasParent).GetComponent<CameraController>();
                        newPlayerUICamera = Instantiate(playerUICameraPrefab, playersUICamerasParent).GetComponent<Camera>();

                        //nombrado de objetos nuevos
                        newPlayer.gameObject.name = "Player " + playerNumber;
                        newPlayerCanvas.gameObject.name = "Canvas " + playerNumber;
                        newPlayerCamera.gameObject.name = "CameraBase " + playerNumber;
                        newPlayerUICamera.gameObject.name = "UICamera " + playerNumber;

                        InitializePlayerReferences(newPlayer, newPlayerCanvas, newPlayerCamera, newPlayerUICamera);

                        
                        if (GameInfo.instance.playerTeamList[0] == Team.none)
                        {
                            GameInfo.instance.playerTeamList[0] = GameInfo.instance.NoneTeamSelect();
                        }

                        allPlayers[0].team = GameInfo.instance.playerTeamList[0];
                        onlinePlayer = newPlayer;
                        onlineCamera = newPlayerCamera;
                        onlineCanvas = newPlayerCanvas;
                        onlineUICamera = newPlayerUICamera;

                }
                else
                {
                    Debug.Log("GameControllerBase: Ignoring CreatePlayer() call because we already exist");
                }
            }
        }
        else
        {
            newPlayer = Instantiate(playerPrefab, playersParent).GetComponent<PlayerMovement>();
            newPlayerCanvas = Instantiate(playerCanvasPrefab, playersCanvasParent);
            newPlayerCamera = Instantiate(playerCameraPrefab, playersCamerasParent).GetComponent<CameraController>();
            newPlayerUICamera = Instantiate(playerUICameraPrefab, newPlayerCamera.myCamera).GetComponent<Camera>();

            //nombrado de objetos nuevos
            newPlayer.gameObject.name = "Player " + playerNumber;
            newPlayerCanvas.gameObject.name = "Canvas " + playerNumber;
            newPlayerCamera.gameObject.name = "CameraBase " + playerNumber;
            newPlayerUICamera.gameObject.name = "UICamera " + playerNumber;

            InitializePlayerReferences(newPlayer,newPlayerCanvas, newPlayerCamera, newPlayerUICamera);
        }
    }

    //Eloy: Juan, he creado este método porque copiar y pegar lo mismo en ambos lados del if(online/offline) era un horror para mi cerebro. cada nueva referencia sería un lío, así mejor.
    void InitializePlayerReferences(PlayerMovement player, GameObject canvas, CameraController cameraBase, Camera UICamera)
    {
        //Inicializar referencias
        PlayerHUD playerHUD = canvas.GetComponent<PlayerHUD>();
        //Player
        player.gC = this;
        player.myCamera = cameraBase;
        player.myPlayerHUD = playerHUD;
        player.myUICamera = UICamera;
        //player.myPlayerCombat.attackNameText = playerHUD.attackNameText;
        //Canvas
        playerHUD.gC = this;
        playerHUD.myCamera = cameraBase.myCamera.GetComponent<Camera>();//newPlayerUICamera;
        playerHUD.myUICamera = UICamera;//newPlayerUICamera;
        playerHUD.myPlayerMov = player;
        canvas.GetComponent<Canvas>().worldCamera = UICamera;
        //CameraBase
        cameraBase.myPlayerMov = player;
        cameraBase.myPlayer = player.transform;
        cameraBase.cameraFollowObj = player.cameraFollow;

        //Añadir a los arrays todos los componentes del jugador
        //guarda jugador
        allPlayers.Add(player);
        allCanvas.Add(canvas);
        allCameraBases.Add(cameraBase);
        allUICameras.Add(UICamera);
        contador.Add(playerHUD.contador);
        powerUpPanel.Add(playerHUD.powerUpPanel);
    }

    //actualmente en desuso
    public virtual void RemovePlayer(PlayerMovement _pM)//solo para online
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
    /// Calcula las posiciones de spawn dentro de cada spawn (rojo y azul), de manera equidistante y centrada y se las da a los Players.
    /// </summary>
    void SetSpawnPositions()
    {
        List<PlayerMovement> redPlayers = new List<PlayerMovement>();
        List<PlayerMovement> bluePlayers = new List<PlayerMovement>();
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].team == Team.A)
            {
                bluePlayers.Add(allPlayers[i]);
            }
            else
            {
                redPlayers.Add(allPlayers[i]);
            }
        }
        
        
        int playerNumBlueCopy = playerNumTeamA = bluePlayers.Count;
        int playerNumRedCopy = playerNumTeamB = redPlayers.Count;
        print("Blue players: " + playerNumBlueCopy + "; Red Players: " + playerNumRedCopy);

        //Divide number of players in a team by number of spawns in that team && set spawnRotation of players (because is more efficient to do it here)
        int[] blueSpawnsNumPlayers = new int[blueTeamSpawns.Length];
        int[] redSpawnsNumPlayers = new int[redTeamSpawns.Length];


        int playersPerSpawn = 0;
        //BLUE TEAM PLAYERS PER SPAWN
        if (blueTeamSpawns.Length > 0 && playerNumTeamA > 0)
        {
            print("Blue Team Spawns Players:");
            float pps = (float)playerNumTeamA / (float)blueTeamSpawns.Length;
            playersPerSpawn = Mathf.CeilToInt(Mathf.Clamp(pps, 1,float.MaxValue));
            //print("playerNumBlue = "+ playerNumBlue + "; blueTeamSpawns.Length = "+ blueTeamSpawns.Length + "; playersPerSpawn = " + pps + "; rounded number = "+playersPerSpawn);
            for (int i = 0; i < blueTeamSpawns.Length && playerNumBlueCopy > 0; i++)
            {
                blueSpawnsNumPlayers[i] = Mathf.Clamp(playerNumBlueCopy, 0, playersPerSpawn);
                playerNumBlueCopy -= blueSpawnsNumPlayers[i];
                //print("Respawn "+ i + ": " + blueSpawnsNumPlayers[i] + " players");
                for (int j = 0; j < blueSpawnsNumPlayers[i]; j++)
                {
                    bluePlayers[0].spawnRotation = Quaternion.Euler(0, blueTeamSpawns[i].rotation.eulerAngles.y, 0);
                    //print("SpawnRotation " + bluePlayers[0].gameObject.name + " = " + bluePlayers[0].spawnRotation.eulerAngles);
                    bluePlayers.RemoveAt(0);
                }
            }
        }


        //RED TEAM PLAYERS PER SPAWN
        if (redTeamSpawns.Length > 0 && playerNumTeamB > 0)
        {
            print("Red Team Spawns Players:");
            float pps = (float)playerNumTeamB / (float)redTeamSpawns.Length;
            playersPerSpawn = Mathf.CeilToInt(Mathf.Clamp(pps, 1, float.MaxValue));
            for (int i = 0; i < redTeamSpawns.Length && playerNumRedCopy > 0; i++)
            {
                redSpawnsNumPlayers[i] = Mathf.Clamp(playerNumRedCopy, 0, playersPerSpawn);
                playerNumRedCopy -= redSpawnsNumPlayers[i];
                print(i + ": " + redSpawnsNumPlayers[i] + " players");
                for (int j = 0; j < redSpawnsNumPlayers[i]; j++)
                {
                    redPlayers[0].spawnRotation = Quaternion.Euler(0, redTeamSpawns[i].rotation.eulerAngles.y, 0);
                    redPlayers.RemoveAt(0);
                }
            }
        }

        //ALL SPAWN POSITIONS CONCATENATED
        List<Vector3> spawnPosBlue = new List<Vector3>();
        List<Vector3> spawnPosRed = new List<Vector3>();
        for(int i=0; i < blueTeamSpawns.Length && playerNumTeamA > 0; i++)
        {
            List<Vector3> auxPositions = blueTeamSpawns[i].GetComponent<Respawn>().SetSpawnPositions(blueSpawnsNumPlayers[i]);
            spawnPosBlue.AddRange(auxPositions);
        }

        for (int i = 0; i < redTeamSpawns.Length && playerNumTeamB > 0; i++)
        {
            List<Vector3> auxPositions = redTeamSpawns[i].GetComponent<Respawn>().SetSpawnPositions(redSpawnsNumPlayers[i]);
            spawnPosRed.AddRange(auxPositions);
        }
        if (spawnPosBlue.Count != playerNumTeamA) Debug.LogError("Error: Spawn positions("+spawnPosBlue.Count+") for blue team are not equal to number of blue team players("+ playerNumTeamA + ").");
        if (spawnPosRed.Count != playerNumTeamB) Debug.LogError("Error: Spawn positions (" + spawnPosRed.Count + ") for red team are not equal to number of red team players(" + playerNumTeamB + ").");
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].team == Team.A)
            {
                allPlayers[i].spawnPosition = spawnPosBlue[0];
                spawnPosBlue.RemoveAt(0);
            }
            else
            {
                allPlayers[i].spawnPosition = spawnPosRed[0];
                spawnPosRed.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Inicia los canvas con las variables y tamaños necesarios para el numero de jugadores. 
    /// </summary>
    private void SetUpCanvases()//Para PantallaDividida
    {
        if (!online)
        {
            for(int i = 0; i < allCanvas.Count; i++)
            {
                allCanvas[i].GetComponent<PlayerHUD>().AdaptCanvasHeightScale();
            }

            //for(int i = 0; playerNum >= 2 && i < contador.Count;i++)
            //{
            //    contador[i].anchoredPosition = new Vector3(contador[i].anchoredPosition.x, 100, contador[i].anchoredPosition.y);
            //}

            //if (playerNum == 2)
            //{
            //    contador[0].localScale /= scaleDos;
            //    contador[1].localScale /= scaleDos;

            //    powerUpPanel[0].localScale /= scaleDos;
            //    powerUpPanel[1].localScale /= scaleDos;
            //}
            //else if (playerNum == 3)
            //{
            //    contador[0].localScale /= scaleCuatro;
            //    contador[1].localScale /= scaleCuatro;
            //    contador[2].localScale /= scaleDos;

            //    powerUpPanel[0].localScale /= scaleCuatro;
            //    powerUpPanel[1].localScale /= scaleCuatro;
            //    powerUpPanel[2].localScale /= scaleDos;
            //}
            //else if (playerNum == 4)
            //{
            //    contador[0].localScale /= scaleCuatro;
            //    contador[1].localScale /= scaleCuatro;
            //    contador[2].localScale /= scaleCuatro;
            //    contador[3].localScale /= scaleCuatro;

            //    powerUpPanel[0].localScale /= scaleCuatro;
            //    powerUpPanel[1].localScale /= scaleCuatro;
            //    powerUpPanel[2].localScale /= scaleCuatro;
            //    powerUpPanel[3].localScale /= scaleCuatro;
            //}
        }
    }

    //Eloy: this is for checking if the inputsBuffer array is well set up
    void CheckValidInputsBuffer()
    {
        List<PlayerInput> knownInputs = new List<PlayerInput>();
        for (int i = 0; i < allBufferedInputs.Length; i++)
        {
            PlayerInput auxInput = allBufferedInputs[i].inputType;
            bool found = false;
            for (int j = 0; j < knownInputs.Count && !found; j++)
            {
                if (auxInput == knownInputs[i])
                {
                    found = true;
                }
            }
            if (found)
            {
                Debug.LogError("Error: There is more than one BufferedInput of the type " + auxInput.ToString() + "in the inputsBuffer.");
            }
            else
            {
                knownInputs.Add(auxInput);
            }
        }
    }

    #endregion

    void SlowMotion()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            switch (slowmo)
            {
                case 0:
                    Time.timeScale = 0.25f;
                    slowmo = 1;
                    break;
                case 1:
                    Time.timeScale = 0.075f;
                    slowmo = 2;
                    break;
                case 2:
                    Time.timeScale = 0.025f;
                    slowmo = 3;
                    break;
                case 3:
                    Time.timeScale = 1;
                    slowmo = 0;
                    break;
            }
        }
    }

    public virtual void StartGameOver(Team _winnerTeam)
    {
        //print("GAME OVER");
        if (!gameOverStarted && !online)
        {
            playing = false;
            gamePaused = true;
            gameOverStarted = true;
            myGameInterface.StartGameOver(_winnerTeam);
        }
    }

    public void RespawnPlayer(PlayerMovement player)
    {
        //print("RESPAWN PLAYER");
        player.SetVelocity(Vector3.zero);
        player.transform.position = player.spawnPosition;
        player.rotateObj.transform.rotation = player.spawnRotation;
        //print("Player " + player.gameObject.name + " respawn rotation = " + player.spawnRotation.eulerAngles);

        //player.myCamera.KonoAwake();
        //player.myCamera.SwitchCamera(player.myCamera.camMode);
        //player.myCamera.LateUpdate();
        player.myCamera.InstantPositioning();
        player.myCamera.InstantRotation();
        //player.myCamera.transform.localRotation = player.rotateObj.transform.localRotation;
        //player.myCamera.SwitchCamera(player.myCamera.camMode);
        player.ResetPlayer();
        player.myPlayerAnimation_01.RestartAnimation();
    }

    private void SwitchGameOverMenu()
    {
        if (gameOverStarted)
        {
            myGameInterface.SwitchGameOverMenu();
        }
    }

    public void PauseGame(PlayerActions p)
    {
        if (!online)
        {
            Time.timeScale = 0;
            myGameInterface.PauseGame();
            playerActions = p;
            gamePaused = true;
        }
    }

    public void UnPauseGame()
    {
        switch (slowmo)
        {
            case 0:
                Time.timeScale = 1;
                break;
            case 1:
                Time.timeScale = 0.25f;
                break;
            case 2:
                Time.timeScale = 0.075f;
                break;
            case 3:
                Time.timeScale = 0.025f;
                break;
        }
        myGameInterface.UnPauseGame();
        gamePaused = false;
    }

    public virtual void ResetGame()//Eloy: habrá que resetear muchas más cosas
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        /*playing = true;
        SwitchGameOverMenu();
        foreach (PlayerMovement pM in allPlayers)
        {
            RespawnPlayer(pM);
            pM.ResetPlayer();
        }
        if (gamePaused)
        {
            UnPauseGame();
        }*/
    }

    void DeactivatePlayerComponents()
    {
        int count = playersParent.childCount;
        for(int i=0; i < count; i++)
        {
            playersParent.GetChild(i).gameObject.SetActive(false);
        }
        count = playersCamerasParent.childCount;
        for (int i = 0; i < count; i++)
        {
            playersCamerasParent.GetChild(i).gameObject.SetActive(false);
        }
        count = playersUICamerasParent.childCount;
        for (int i = 0; i < count; i++)
        {
            playersUICamerasParent.GetChild(i).gameObject.SetActive(false);
        }
        count = playersCanvasParent.childCount;
        for (int i = 0; i < count; i++)
        {
            playersCanvasParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    #region Auxiliar
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

#region Struct
#endregion