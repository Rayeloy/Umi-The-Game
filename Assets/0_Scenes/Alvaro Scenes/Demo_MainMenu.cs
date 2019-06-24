using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Demo_MainMenu : MonoBehaviour
{
    // Valor a Cambiar para activar las camaras en el switch
    int scene = 0;

    [Range(0,1)]
    public float deadzone;

    //Temporizador
    float timeToStart = 3;
    float timeLeft = 1;
    
    bool timer1 = false;
    bool timer2 = false;

    //Cameras
    public GameObject c_Exit;
    public GameObject c_Tuto;
    public GameObject c_Map;
    public GameObject c_Hub;
    public GameObject c_Home;

    //Interface
    bool arrow = false;
    public GameObject leftArrow;
    public GameObject rightArrow;

    bool exit1 = false;
    public GameObject askToGo;
    bool exit2 = false;

    public GameObject umiLogo;
    bool umilogo = false;
    public GameObject play;


    private void Awake()
    {
        SelectScene();
    }

    private void Update()
    {
        if (timer1)
        {
                timer1 = false;
                timeToStart -= Time.deltaTime;
                if (timeToStart < 0)
                {
                    Intro();
                }
        }

        if ((Input.GetKeyDown(KeyCode.Escape) || GameInfo.instance.myControls.B.WasPressed) && scene != 4)
        {            
            scene = 4;
            SelectScene();
        }

        
        if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)) || GameInfo.instance.myControls.A.WasPressed) && exit1 == false)
        {
            BanishArrow();

            if (scene == 0)
            {
                timer1 = true;
            }
            else if (scene == 1)
            {
                SceneManager.LoadScene("UmiLand");
            }
            else if (scene == 2)
            {
                SceneManager.LoadScene("Capture The Whale");
            }
            else if (scene == 3)
            {
                SceneManager.LoadScene("Tutorial_v2");
            }
            else if (scene == 4)
            {
                exit1 = true;
                {
                    askToGo.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.Escape) || GameInfo.instance.myControls.B.WasPressed)
                    {
                        askToGo.SetActive(false);
                        exit1 = false;
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow) || GameInfo.instance.myControls.LeftJoystick.X < deadzone || Input.GetKeyDown(KeyCode.RightArrow) || GameInfo.instance.myControls.LeftJoystick.X > deadzone)
                    {
                        if (exit2)
                        {
                            exit2 = false;
                        }
                        else if (exit2 == false)
                        {
                            exit2 = true;
                        }
                    }
                }
                if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)) || GameInfo.instance.myControls.A.WasPressed) && exit2 == true)
                {
                    Application.Quit();
                }
            }
        }

        if (arrow)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || GameInfo.instance.myControls.LeftJoystick.X > deadzone)
            {
                //Animar Flecha Derecha
                BanishArrow();
                if (scene >= 0 && scene < 4)
                {
                    scene ++;
                    SelectScene();
                }
                else if (scene == 4)
                {
                    scene = 1;
                    SelectScene();
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || GameInfo.instance.myControls.LeftJoystick.X < deadzone)
            {
                //Animar Flecha Izquierda
                BanishArrow();
                if (scene >= 1 && scene <= 4)
                {
                    scene --;
                    SelectScene();
                }
                else if (scene == 1)
                {
                    scene = 4;
                    SelectScene();
                }
            }
        }

        if (timer2){
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                Flechas();
                timer2 = false;
            }
        }
     }

    void Intro()
    {
        if (umilogo == false)
        {
            umiLogo.SetActive(true);
            play.SetActive(true);
            umilogo = true;
        }
        else if (umilogo == true)
        {  
            //Fade Out
            umiLogo.SetActive(false);
            play.SetActive(false);
            scene = 1;
            SelectScene();
        }
    }

    void SelectScene()
    {
        // Otra forma de if: int pene = timer == false ? 1 : 0;

        switch (scene)
        {
            case 4:
                /*Exit*/

                //Camera Setting
                c_Exit.SetActive(true);
                c_Tuto.SetActive(false);
                c_Map.SetActive(false);
                c_Hub.SetActive(false);
                c_Home.SetActive(false);

                //Interface Setup
                timer2 = true;
                break;

            case 3:
                /*Tutorial*/
                //Camera Setting
                c_Exit.SetActive(false);
                c_Tuto.SetActive(true);
                c_Map.SetActive(false);
                c_Hub.SetActive(false);
                c_Home.SetActive(false);

                //Interface Setup
                timer2 = true;
                break;

            case 2:
                /*2vs2*/
                //Camera Setting
                c_Exit.SetActive(false);
                c_Tuto.SetActive(false);
                c_Map.SetActive(true);
                c_Hub.SetActive(false);
                c_Home.SetActive(false);

                //Interface Setup
                timer2 = true;
                break;

            case 1:
                /*Hub*/

                //Camera Setting
                c_Exit.SetActive(false);
                c_Tuto.SetActive(false);
                c_Map.SetActive(false);
                c_Hub.SetActive(true);
                c_Home.SetActive(false);

                //Interface Setup
                timer2 = true;
                break;

            default:
                /*Menu*/

                //Camera Setting
                c_Exit.SetActive(false);
                c_Tuto.SetActive(false);
                c_Map.SetActive(false);
                c_Hub.SetActive(false);
                c_Home.SetActive(true);

                //Interface Setup
                timer1 = true;
                break;

        }

    }

    void Flechas() //Falta la animacion de aparecer
    {
        leftArrow.SetActive(true);
        rightArrow.SetActive(true);

        arrow = true;
    }

    void BanishArrow()
    {
        //Timer
        //animación de desaparecer
        arrow = false;
    }




    /*

     * 0.3 Cambio de cámara, asciende lentamente mientras se ven las olas. Termina con un plano principal del Hub, que se encuentra en la parte derecha.
     * 
     * 1.0 La flecha de la dirección señalada hace una animación de hacerse más grande (Efecto click) mientras que la otra desaparece.
     * 
     * 2.0 Pantalla Exit = Filtro gris.
     * 
     * 2.1 Barco en dirección al horizonte.
     * 
     * 2.2 Tablero con el texto de salir.
     * 
     */
}
