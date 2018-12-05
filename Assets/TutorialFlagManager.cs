using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFlagManager : MonoBehaviour
{
	public GameObject[] flags;
	void Start ()
	{
		for(int i = 0; i < GameController.instance.playerNum; i++){
			flags[i].SetActive( true );
		}
	}
}
