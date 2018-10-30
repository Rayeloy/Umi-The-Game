using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {

	public PickupData pickup;

	private void Start(){
		Debug.Log(pickup.name);
	}
}
