using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPause : MonoBehaviour
{
	public string menuScene;

	private IEnumerator coroutine;

	[Header("Referencias")]
	public GameObject Canvas;
	public PlayerMovement myPlayerMovement;

	public void PauseGame(){
        print("PARO EL TIEMPO AQUÍ");
        Time.timeScale = 0;

		coroutine = Pause();
		StartCoroutine(coroutine);
	}

	private IEnumerator Pause()
    {
        while (true)
        {
			//Debug.Log(myPlayerMovement.Actions.Jump.WasPressed);
			if (myPlayerMovement.Actions.A.WasPressed){
				Time.timeScale = 1;
				SceneManager.LoadScene(menuScene);
				StopCoroutine(coroutine);
			}
			else if (myPlayerMovement.Actions.B.WasPressed){
				Time.timeScale = 1;
				StopCoroutine(coroutine);
			}
            yield return null;
        }
	}
}
