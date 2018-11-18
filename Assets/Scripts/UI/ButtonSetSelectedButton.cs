using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSetSelectedButton : MonoBehaviour
{
	public void SetSelected(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(go);
	}
}
