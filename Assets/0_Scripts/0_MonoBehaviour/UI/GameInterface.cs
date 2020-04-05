using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

//SE DEDICA EXCLUSIVAMENTE A CONTROLAR LA INTERFAZ DENTRO DE LA PARTIDA, es decir, todo lo que hay dentro del Canvas general principalmente.
public class GameInterface : MonoBehaviour
{

    [Header("References")]
    RenController myRenCont;
    private GameControllerBase gC;

    //GAME OVER MENU
    [Header(" --- Game Over Menu --- ")]
    public GameObject gameOverMenu;
    [HideInInspector]
    public bool gameOverMenuOn = false;
    public GameObject veil;
    public Image victoryA;
    public Image victoryB;
    public UIAnimation victoryImageReduceAnimation;
    public UIAnimation victoryImageMoveUpAnimation;
    public Text gameOverPressStart;
    public RenButton gameOverFirstButton;//intento fallido de controlar qué boton se selecciona automáticamente al iniciar el menu de Game Over
    private string sceneLoadedOnReset;
    bool pressStartToContinueStarted = false;


    [Header(" --- Pause --- ")]
    public string menuScene;
    public string teamSelectScene;
    public RenButton pauseRestartButton;
    //public GameObject pauseMenuButton;
    public GameObject pauseMenu;


    public void KonoAwake(GameControllerBase _gC)
    {
        gC = _gC;
        myRenCont = GetComponentInChildren<RenController>();
        if (myRenCont == null)
        {
            Debug.LogError("GameInterfaceCMF -> no RenController could be found!");
        }
        sceneLoadedOnReset = SceneManager.GetActiveScene().name;

        gameOverMenu.SetActive(false);
        gameOverPressStart.enabled = false;
        veil.SetActive(false);
        pauseMenu.SetActive(false);

        victoryB.gameObject.SetActive(false);
        victoryA.gameObject.SetActive(false);

        gameOverMenuOn = false;

    }

    private void Update()
    {
        ProcessPressStartToContinue();
    }

    public void StartPressStartToContinue()
    {
        if (!pressStartToContinueStarted)
        {
            pressStartToContinueStarted = true;
            GameInfo.instance.StartAnimation(victoryImageReduceAnimation, null);
            victoryImageReduceAnimation.StartAnimation();
        }
    }

    bool moveUpAnimStarted = false;
    public void ProcessPressStartToContinue()
    {
        if (pressStartToContinueStarted)
        {
            if(!victoryImageReduceAnimation.playing & !moveUpAnimStarted)
            {
                //Debug.Log("VICTORY ANIMATION MOVEUP ANIMATION STARTED");
                moveUpAnimStarted = true;
                GameInfo.instance.StartAnimation(victoryImageMoveUpAnimation, null);
            }
            else if (moveUpAnimStarted && !victoryImageMoveUpAnimation.playing)
            {
                StopPressStartToContinue();
            }
        }
    }

    public void StopPressStartToContinue()
    {
        if (pressStartToContinueStarted)
        {
            pressStartToContinueStarted = false;
            SwitchGameOverMenu();
        }
    }

    public void SwitchGameOverMenu()
    {
        //print("GAME OVER MENU");
        //print("gameOverMenuOn= " + gameOverMenuOn);

        if (gameOverMenuOn)
        {
            //GameObject incontrol = GameObject.Find("InControl manager");
            //Destroy(incontrol);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            gameOverMenuOn = false;
            gameOverMenu.SetActive(false);
            veil.SetActive(false);
            //Debug.Log("VICTORY IMAGE SET TO FALSE");
            victoryB.gameObject.SetActive(false);
            victoryA.gameObject.SetActive(false);
            gameOverPressStart.enabled = false;
            gC.gameOverStarted = false;
        }
        else
        {
            //print("ACTIVATE GAME OVER MENU");
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            //victoryRed.gameObject.SetActive(false);
            //victoryBlue.gameObject.SetActive(false);
            gameOverMenuOn = true;
            gameOverMenu.SetActive(true);
            gameOverPressStart.enabled = false;
            myRenCont.disabled = false;
            myRenCont.SetSelectedButton(gameOverFirstButton);
        }
    }

    public void StartGameOver(Team _winnerTeam)
    {
        veil.SetActive(true);
        if (_winnerTeam == Team.A)
        {
            victoryA.gameObject.SetActive(true);
            victoryImageMoveUpAnimation.rect = victoryA.GetComponent<RectTransform>();
            victoryImageReduceAnimation.rect = victoryA.GetComponent<RectTransform>();
        }
        else if (_winnerTeam == Team.B)
        {
            victoryB.gameObject.SetActive(true);
            victoryImageMoveUpAnimation.rect = victoryB.GetComponent<RectTransform>();
            victoryImageReduceAnimation.rect = victoryB.GetComponent<RectTransform>();
        }
        gameOverPressStart.enabled = true;
    }

    #region Buttons

    public virtual void ResetGame()
    {
        gC.ResetGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void BackToTeamSelect()
    {
        gC.UnPauseGame();
        if (GameInfo.instance.inControlManager != null)
        {
            //Eloy: hay que encontrar una mejor manera de resetear/borrar los controles...
            GameInfo.instance.ErasePlayerControls();
        }
        print("LOAD MAIN TEAM SELECT");
        SceneManager.LoadScene(teamSelectScene);
    }

    public void GoBackToMenu()
    {
        gC.UnPauseGame();
        if (GameInfo.instance.inControlManager != null)
        {
            //Eloy: hay que encontrar una mejor manera de resetear/borrar los controles...
            GameInfo.instance.ErasePlayerControls();
        }
        print("LOAD MAIN MENU");
        SceneManager.LoadScene(menuScene);
    }

    #endregion

    #region ---------------------------------------------- Pause ----------------------------------------------
    public void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        veil.SetActive(true);
        pauseMenu.SetActive(true);


        //EventSystem.current.SetSelectedGameObject(pauseRestartButton);
        myRenCont.SetSelectedButton(pauseRestartButton);
    }

    public void UnPauseGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        veil.SetActive(false);
        pauseMenu.SetActive(false);

    }
    #endregion
}
