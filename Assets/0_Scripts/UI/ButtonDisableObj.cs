using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class ButtonDisableObj : MonoBehaviour
{
   	public GameObject [] toDisable;
	public PostProcessingBehaviour otherProfile;

	public void DisableObjects (){
		if (toDisable.Length < 0)
			return;

		for (int i = 0; i < toDisable.Length; i++){
			toDisable[i].SetActive(false);
		}
	}

	public void DisablePostprocesing()
	{
		otherProfile.enabled = false;
	}
}
