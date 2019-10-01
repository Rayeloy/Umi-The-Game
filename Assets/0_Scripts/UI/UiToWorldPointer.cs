using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class UiToWorldPointer : MonoBehaviour
{
	public Transform Target;
	Vector3 Pointing;

	public float offsetPantalla;

	public Transform Arrow;
	public Transform Wale;
	// Update is called once per frame
	void Update () {
		Arrows();
	}

	void Arrows()
	{
		Vector3 dir = Camera.main.WorldToScreenPoint(Target.transform.position);

		if (dir.x > offsetPantalla && dir.x < Screen.width - offsetPantalla && dir.y > offsetPantalla && dir.y < Screen.height - offsetPantalla){
			Arrow.gameObject.SetActive(false);
			Wale.gameObject.SetActive(false);
		}
		else{
			Arrow.gameObject.SetActive(true);
			Wale.gameObject.SetActive(true);

			Pointing.z = Mathf.Atan2((Arrow.transform.position.y - dir.y), (Arrow.transform.position.x - dir.x)) *Mathf.Rad2Deg - 90;

			Arrow.transform.rotation = Quaternion.Euler(Pointing);
			Arrow.transform.position = new Vector3(Mathf.Clamp(dir.x, offsetPantalla,  Screen.width - offsetPantalla), Mathf.Clamp(dir.y, offsetPantalla,  Screen.height - offsetPantalla), 0);

			Wale.transform.position = Arrow.transform.position;
		}
	}

	void objetoOculto (){
		
	}
}
