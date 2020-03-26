using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class EventSystemManager : MonoBehaviour
{
	public GameObject DefaultButton;
	void Update () {
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)){
			EventSystem.current.SetSelectedGameObject(DefaultButton);
		}
	}
	public void Load (string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
}
