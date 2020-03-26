using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Demo_TeamSelect : MonoBehaviour
{
    //Variables

    [Range(0, 1)]
    public float deadzone;

    int windows = 0;

    int screen = 0;

    int team = 0;
    int f_Team = 0;

    int weapon = 0;
    int f_Weapon = 0;

    bool ready = false;

    bool change = false;

    //All Cameras (We don´t know how many yet)

    //Public Objects to show



    //Cosas de Eloy

        /*
         * 
         * 
         * 
         * 
         * 
         * 
         * */

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (windows == 0) // Press any key to play 
        {
            if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)) || GameInfo.instance.myControls.A.WasPressed))
            {
                windows = 1;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || GameInfo.instance.myControls.B.WasPressed)
            {
                SureExit();
            }
        } 
        else if (windows == 1) // Principal menu
        {
            if (Input.GetKeyDown(KeyCode.Escape) || GameInfo.instance.myControls.B.WasPressed)
            {
                SureExit();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || GameInfo.instance.myControls.LeftJoystick.X < -deadzone)
            {
                if (screen >= 0 && screen < 2)  screen++;

                if (screen == 2)  screen = 0; 

            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || GameInfo.instance.myControls.LeftJoystick.X > deadzone)
            {
                if (screen > 0 && screen <= 2) screen--;

                if (screen == 0) screen = 2;
            }

            if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)) || GameInfo.instance.myControls.A.WasPressed))
            {
                if (screen == 0)
                {
                    ready = true;
                }
                else if (screen == 1)
                {
                    // Cambio de cámaras a Team Select
                }
                else if (screen == 2)
                {
                    // Cambio de cámaras a Weapon Select
                }
            }
        }
        else if (windows == 2) // Team Select
        {
            if ((Input.GetKeyDown(KeyCode.LeftArrow) || GameInfo.instance.myControls.LeftJoystick.X < -deadzone) && change)
            {
                change = !change;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || GameInfo.instance.myControls.LeftJoystick.X > deadzone && !change)
            {
                change = !change;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || GameInfo.instance.myControls.LeftJoystick.Y < -deadzone && !change)
            {
                if (team > 0 && team <= 2) team--;

                if (team == 0) team = 2;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || GameInfo.instance.myControls.LeftJoystick.Y > deadzone && !change)
            {
                if (team >= 0 && screen < 2) team++;

                if (team == 2) team = 0;
            }

            if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)) || GameInfo.instance.myControls.A.WasPressed))
            {
                if (change)
                {
                    screen = 0;
                }
                else if (!change)
                {
                    SelectTeam();
                }
            }
        } 
        else if (windows == 3)
        {
            if ((Input.GetKeyDown(KeyCode.LeftArrow) || GameInfo.instance.myControls.LeftJoystick.X < -deadzone) && !change)
            {
                change = !change;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || GameInfo.instance.myControls.LeftJoystick.X > deadzone && change)
            {
                change = !change;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || GameInfo.instance.myControls.LeftJoystick.Y < -deadzone && !change)
            {
                if (weapon > 0 && weapon <= 2) weapon--;

                if (weapon == 0) weapon = 2;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || GameInfo.instance.myControls.LeftJoystick.Y > deadzone && !change)
            {
                if (weapon >= 0 && weapon < 2) weapon++;

                if (weapon == 2) weapon = 0;
            }
            
            if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)) || GameInfo.instance.myControls.A.WasPressed))
            {
                if (change)
                {
                    screen = 0;
                }
                else if (!change)
                {
                    SelectWeapon();
                }
            }
        }
    }

    void Cameras()
    {
        switch (screen)
        {
            default:
                // Menu principal

                break;

            case 1:
                // Team Select

                //Activa y desactiva las cámaras. 
                // team = f_Team; Para evitar problemas con las skins, dado que el jugador tendrá la skin que esté seleccionada, aunque no se haya fijado.

                break;

            case 2:
                // Weapon Select

                //Activa y desactiva las cámaras. 
                // weapon = f_Weapon; Para evitar problemas con las armas, dado que el jugador tendrá el arma que esté seleccionada, aunque no se haya fijado.

                break;

        }
    }

    void SelectTeam()
    {
        switch (team)
        {
            default:


                break;

            case 1:


                break;

            case 2:


                break;



        }
    }

    void SelectWeapon()
    { 
        switch (weapon)
        {
            default:


                break;

            case 1:


                break;

            case 2:


                break;


        }
    }

    void LockTeam()
    {
        f_Team = team;

        //Mostrar visualmente que se ha lockeado ese equipo.

    }

    void LockWeapon()
    {
        f_Weapon = weapon;

        //Mostrar visualmente que se ha lockeado ese arma.

    }

    void SureExit()
    {
        // Display: "Are u sure to exit?" at any point.
    }

   
}
