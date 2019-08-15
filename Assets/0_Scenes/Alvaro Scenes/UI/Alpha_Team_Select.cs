using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Alpha_Team_Select : MonoBehaviour
{
    //Variables

    [Range(0, 1)]
    public float deadzone;

    int team = 0;
    int f_Team = 0; //Team Selected while being ready

    bool ready = false; //Can´t change team
    
    public int nPlayers = 1;

    public float scaleSpriteBig;

    public Transform[] numPlayerSprites;

    public List<SelectPlayer> readyPlayers; 

    //All Camera Set

    public GameObject selectNPlayersCamera;

    public bool cameraSet = false;

    public GameObject camera1player;
    public GameObject camera2player;
    public GameObject camera3player;
    public GameObject camera4player;
    

    //Teams

    public GameObject randomTeam1;
    public GameObject greenTeam1;
    public GameObject pinkTeam1;

    public GameObject randomTeam2;
    public GameObject greenTeam2;
    public GameObject pinkTeam2;

    public GameObject randomTeam3;
    public GameObject greenTeam3;
    public GameObject pinkTeam3;

    public GameObject randomTeam4;
    public GameObject greenTeam4;
    public GameObject pinkTeam4;




    //Cosas de Eloy

    /*
     * 
     * 
     * 
     * 
     * 
     * 
     * */

    private void Awake()
    {
        readyPlayers = new List<SelectPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!cameraSet)
        {

            if (Input.GetKeyDown(KeyCode.LeftArrow) || GameInfo.instance.myControls.LeftJoystick.X < -deadzone )
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
                UpdateNPlayers();
            }
        }
    }

    public void MoveUp()
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
    public void MoveDown()
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
    public void MoveRight()
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
    public void MoveLeft()
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

    public void UpdateNPlayers()
    {
        selectNPlayersCamera.SetActive(false);

        //Difuminado a Negro

        switch (nPlayers)
        {
            case 1:

                // 1 Player

                camera1player.SetActive(true);
                camera2player.SetActive(false);
                camera3player.SetActive(false);
                camera4player.SetActive(false);

                break;

            case 2:

                // 2 player

                camera1player.SetActive(false);
                camera2player.SetActive(true);
                camera3player.SetActive(false);
                camera4player.SetActive(false);

                break;

            case 3:

                // 3 player

                camera1player.SetActive(false);
                camera2player.SetActive(false);
                camera3player.SetActive(true);
                camera4player.SetActive(false);

                break;

            case 4:

                // 4 player

                camera1player.SetActive(false);
                camera2player.SetActive(false);
                camera3player.SetActive(false);
                camera4player.SetActive(true);

                break;
        }

        cameraSet = true;
    }

    public void MakeSpriteBig(Transform spriteTransform)
    {
        spriteTransform.localScale *= scaleSpriteBig;
    }
    public void MakeSpriteSmall(Transform spriteTransform)
    {
        spriteTransform.localScale /= scaleSpriteBig;
    }
    
    
    void LockTeam() // Para cada player (Usar Arrays)
    {
        f_Team = team;
        ready = true;
        //Mostrar visualmente que se ha lockeado ese equipo.

    }

    void UnlockTeam()
    {
        ready = false;
        //Mostrar visualmente que se ha unlockeado ese equipo.
    }

    void SureExit()
    {
        // Display: "Are u sure to exit?" at any point.
    }

}

public class SelectPlayer
{

    public float deadzone;

    public PlayerActions myControls;

    public bool ready;

    public Team myTeam;

    public SelectPlayer(float _deadzone = 0.2f, bool _ready = false, Team _myTeam = Team.none)
    {

    }

    public void KonoUpdate()
    {
        if (ready = true && myControls.B.WasPressed)
        {
            UnlockTeam();
        }
        else if (myControls.LeftJoystick.X < -deadzone && !ready)
        {
            ChangeTeam(0);

            //Animation Left
        }
        else if (myControls.LeftJoystick.X > deadzone && !ready)
        {
            ChangeTeam(1);

            //Animation Right
        }
        else if (!ready && myControls.A.WasPressed)
        {
            LockTeam();
        }
    }


    public void ChangeTeam(int direction) // 0 es izda y 1 es derecha
    {
        switch (direction)
        {
            case 0:
                 
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
                //
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

    public void LockTeam()
    {
        ready = true;

        //Visual Lock
    }

    public void UnlockTeam()
    {
        ready = false;

        //Visual Unlock
    }
}