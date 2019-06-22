using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo_MainMenu : MonoBehaviour
{
    // Valor a Cambiar para activar las camaras en el switch
    int scene;

    //Cameras
    GameObject c_Exit;
    GameObject c_Tuto;
    GameObject c_Map;
    GameObject c_Hub;
    GameObject c_Home;

    //Interface
    GameObject leftArrow;
    GameObject rightArrow;
    GameObject board;

    GameObject t_Exit;
    GameObject t_Tuto;
    GameObject t_Map;
    GameObject t_Hub;
    GameObject t_Home;

    void SelectScene()
    {
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


                break;

            case 3:
                /*Tutorial*/
                //Camera Setting
                c_Exit.SetActive(false);
                c_Tuto.SetActive(true);
                c_Map.SetActive(false);
                c_Hub.SetActive(false);
                c_Home.SetActive(false);
                break;

            case 2:
                /*2vs2*/
                //Camera Setting
                c_Exit.SetActive(false);
                c_Tuto.SetActive(false);
                c_Map.SetActive(true);
                c_Hub.SetActive(false);
                c_Home.SetActive(false);
                break;

            case 1:
                /*Hub*/
                //Camera Setting
                c_Exit.SetActive(false);
                c_Tuto.SetActive(false);
                c_Map.SetActive(false);
                c_Hub.SetActive(true);
                c_Home.SetActive(false);
                break;

            default:
                /*Menu*/
                //Camera Setting
                c_Exit.SetActive(false);
                c_Tuto.SetActive(false);
                c_Map.SetActive(false);
                c_Hub.SetActive(false);
                c_Home.SetActive(true);
                break;

        }

    }

    // Pulsar para cambiar

    /*
     * If arrows are active
     *      Lateral Arrows Keyboard, A + D, Joystick derecho, Flechas mando.
     *  B = Menú principal ¿?
     *  Start = Fundido a negro. Exit? - Aparecerá una segunda pantalla con si y no.
     * /

    //Función Cosas que Pasan

    /*
     * 0.1 Desaparece el testo
     * 0.2 Desaparece el logo
     * 0.3 Cambio de cámara, asciende lentamente mientras se ven las olas. Termina con un plano principal del Hub, que se encuentra en la parte derecha.
     *      El tablero aparece/o se encuentra a la izquierda y las felchas después.
     * 
     * 1.0 La flecha de la dirección señalada hace una animación de hacerse más grande (Efecto click) mientras que la otra desaparece.
     * 1.1 Desaparecen las flechas y comienza el cambio de cámara.
     *      Disactivate Arrows, then change cameras.
     * 1.2 Aparece el tablero (Posibilidad de dejarlo fijo para evitar mareos y movimientos tediosos).
     * 1.3 Aparece el texto (Fade in desde dentro del tablero).
     * 1.4 Aparecen las flechas.
     * 
     * 2.0 Pantalla Exit = Filtro gris.
     * 2.1 Barco en dirección al horizonte.
     * 2.2 Tablero con el texto de salir.
     *     Si se pulsa el botón, aparecerá una segunda pantalla con si y no.
     * 2.3 Aparecen las flechas.
     * 
     * 1.0 ...
     * 
     * 
     */
}
