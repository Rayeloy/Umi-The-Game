using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFlagManager : MonoBehaviour
{
    public GameController_Tutorial gC;
	public GameObject[] flags;

	void Start ()
	{
		for(int i = 0; i < gC.playerNum; i++){
			flags[i].SetActive( true );
		}
	}
}
