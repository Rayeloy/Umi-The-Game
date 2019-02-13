using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    #region Funciones del menu
    public void Load (string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
    public void ExitGame(){
		Application.Quit();
	}
#endregion

}
