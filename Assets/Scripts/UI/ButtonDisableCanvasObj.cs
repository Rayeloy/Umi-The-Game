using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonDisableCanvasObj : MonoBehaviour
{
	public GameObject [] toDisable;

	public void DisableObjects (){
		if (toDisable.Length < 0)
			return;

		for (int i = 0; i < toDisable.Length; i++){
			toDisable[i].SetActive(false);
		}
	}
}
