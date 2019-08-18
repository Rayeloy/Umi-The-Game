using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Demo_MainMenu : MonoBehaviour
{
    // Valor a Cambiar para activar las camaras en el switch
    public string teamSetupScene;
    int scene = 0;

    [Range(0,1)]
    public float deadzone;

    //Temporizador
    float timeToStart = 0f;
    float maxTimeLeft = 1.7f;
    float timeLeft;
    
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

    public GameObject leftarrow1;
    public GameObject leftarrow2;
    public GameObject leftarrow3;
    public GameObject leftarrow4;

    public GameObject rightarrow1;
    public GameObject rightarrow2;
    public GameObject rightarrow3;
    public GameObject rightarrow4;

    bool exit1 = false;
    public GameObject askToGo;
    bool exit2 = false;

    public GameObject umiLogo;
    bool umilogo = false;
    public GameObject play;


    private void Awake()
    {
        SelectScene();
        timeLeft = maxTimeLeft;
    }

    private void Update()
    {

        Debug.Log(arrow);


        if (timer1) //Tiempos para los logos de inicio.
        {
            
            timeToStart -= Time.deltaTime;
                if (timeToStart < 0)
                {
                timer1 = false;

                Intro();
                
            }
        }

        if ((Input.GetKeyDown(KeyCode.Escape) || GameInfo.instance.myControls.B.WasPressed) && scene != 3)
        {            
            scene = 3;
            SelectScene();
        } //Ventana de salir.

        if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)) || GameInfo.instance.myControls.A.WasPressed) && exit1 == false)
        {
            BanishArrow();

            //Añadir tiempos de transición entre la selección de escena y la pantalla de carga.

            if (scene == 0)
            {
                timer1 = true;
                //scene++;
            }
            else if (scene == 1)
            {
                SceneManager.LoadScene(teamSetupScene);
            }
            else if (scene == 2)
            {
            }
            else if (scene == 4)
            {
                SceneManager.LoadScene("UmiLand");
                //SceneManager.LoadScene("Tutorial_v2");
            }
            else if (scene == 3)
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
        } //Seleccionar Escena

        if (arrow)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || GameInfo.instance.myControls.LeftJoystick.X < -deadzone)
            {
                //Animar Flecha Derecha
                //BanishArrow();
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
            if (Input.GetKeyDown(KeyCode.RightArrow) || GameInfo.instance.myControls.LeftJoystick.X > deadzone)
            {
                //Animar Flecha Izquierda
                //Debug.LogWarning("flechas a false");
                //arrow = false;
                //BanishArrow();

                if (scene > 1 && scene <= 4)
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
            if (timeLeft <= 0)
            {
                if (arrow == false)
                {
                    Flechas();
                }
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
                /*Tutorial*/
                //Camera Setting
                c_Exit.SetActive(false);
                c_Tuto.SetActive(true);
                c_Map.SetActive(false);
                c_Hub.SetActive(false);
                c_Home.SetActive(false);

                //Interface Setup
                //timer2 = true;
                BanishArrow();
                break;

            case 3:
                /*Exit*/

                //Camera Setting
                c_Exit.SetActive(true);
                c_Tuto.SetActive(false);
                c_Map.SetActive(false);
                c_Hub.SetActive(false);
                c_Home.SetActive(false);

                //Interface Setup
                //timer2 = true;
                BanishArrow();
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
                //timer2 = true;
                BanishArrow();
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
                //timer2 = true;
                BanishArrow();
                break;

            default:
                /*Menu*/

                //Camera Setting
                c_Home.SetActive(true);
                c_Exit.SetActive(false);
                c_Tuto.SetActive(false);
                c_Map.SetActive(false);
                c_Hub.SetActive(false);
                

                //Interface Setup
                timer1 = true;
                break;

        }

    }

    void Flechas() //Falta la animacion de aparecer
    {
        if (scene == 1)
        {
            leftarrow1.SetActive(true);
            leftarrow2.SetActive(false);
            leftarrow3.SetActive(false);
            leftarrow4.SetActive(false);

            rightarrow1.SetActive(true);
            rightarrow2.SetActive(false);
            rightarrow3.SetActive(false);
            rightarrow4.SetActive(false);
        }
        else if (scene == 2)
        {
            leftarrow1.SetActive(false);
            leftarrow2.SetActive(true);
            leftarrow3.SetActive(false);
            leftarrow4.SetActive(false);

            rightarrow1.SetActive(false);
            rightarrow2.SetActive(true);
            rightarrow3.SetActive(false);
            rightarrow4.SetActive(false);
        }
        else if (scene == 3)
        {
            leftarrow1.SetActive(false);
            leftarrow2.SetActive(false);
            leftarrow3.SetActive(true);
            leftarrow4.SetActive(false);

            rightarrow1.SetActive(false);
            rightarrow2.SetActive(false);
            rightarrow3.SetActive(true);
            rightarrow4.SetActive(false);
        }
        else if (scene == 4)
        {
            leftarrow1.SetActive(false);
            leftarrow2.SetActive(false);
            leftarrow3.SetActive(false);
            leftarrow4.SetActive(true);

            rightarrow1.SetActive(false);
            rightarrow2.SetActive(false);
            rightarrow3.SetActive(false);
            rightarrow4.SetActive(true);
        }

        leftArrow.SetActive(true);
        rightArrow.SetActive(true);


        arrow = true;
    }

    void BanishArrow()
    {
        arrow = false;
        //Timer
        //animación de desaparecer
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
        Debug.LogWarning("banisheado");
        timeLeft = maxTimeLeft;
        timer2 = true;
    }

    /*
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
