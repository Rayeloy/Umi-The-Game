using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickups : MonoBehaviour
{
	public List<PickupData> pickupList = new List<PickupData>();//PickupData[] pickupList;
	public int maxPickups;
	public int activePickup = 0;

	[Header ("Referencias")]
	public PlayerMovement myPlayerMovement;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	float cooldownTime = 0;
	const float maxCooldownTime = 100;
	void Update ()
	{
		//Control cooldown
		cooldownTime += Time.deltaTime;
		cooldownTime = Mathf.Clamp(cooldownTime, 0, maxCooldownTime);

		//Si se ha presionado el boton
		if (myPlayerMovement.Actions.UsePickup.WasPressed) {
			//Condiciones para poder usarlo
			if (cooldownTime >= pickupList[activePickup].Cooldown && pickupList[activePickup].Cantidad > 0){
				Usar();
				cooldownTime = 0;
				pickupList[activePickup].Cantidad --;

				if (pickupList[activePickup].Cantidad == 0) ActivePickupEmpty();//pickupList.RemoveAt(activePickup);
			}
		}
	}

	public void Usar ()
	{
		if (pickupList.Count < 0) return;

		switch (pickupList[activePickup].Type)
        {
            case PickupType.Mele:
                //uso de pickup mele
				Debug.Log("Uso:" + pickupList[activePickup].name + " Tipo: Mele");
                break;
			case PickupType.Range:
                //uso de pickup mele
				Debug.Log("Uso:" + pickupList[activePickup].name + " Tipo: Range");
                break;
		}
	}

	public void equipar (int n)
	{
		if (pickupList.Count < 0) return;

		if(n < 0){
			activePickup = ( activePickup + 1 ) % pickupList.Count;
		}
		else if(n > 0){
			activePickup = ( activePickup - 1 ) % pickupList.Count;
		}
	}

	public void ActivePickupEmpty(){
		Debug.Log ("Pick-Up Vacio");
		// que ocurre cuendo se acab un pickup;
		// cambiar a otro pickup.
	}

#region Coger ---------------------------------------------------------------------
	/// <summary>
	/// Informar a PlayerPickups que se ha colisionado con un pickups.
	/// </summary>
	/// <param name="gameObject">El Game Object que se quiere recoge.</param>
	public void CogerPickup(GameObject gObject)
	{
		PickupData pickupData = gObject.GetComponent<Pickup>().pickupData;
        if (pickupData != null && puedeCoger(pickupData)){
			Debug.Log("Cogido: " + pickupData.name);
			Destroy(gObject);
		}
	}
	
	/// <summary>
	/// Comprueva si se puede coger el pick up que se le pasa.
	/// </summary>
	/// <param name="pickupData">pickUp que se quiere recoger.</param>
	/// <returns>revuelve true si se puede recoger el pickup y false si no.</returns>
	public bool puedeCoger(PickupData pickupData)
	{
		if (pickupList.Count > 0){
			foreach (PickupData p in pickupList){
				if (p == pickupData && p.Cantidad < p.MaxCantidad){
					p.Cantidad += pickupData.Cantidad;
					p.Cantidad = Mathf.Clamp(p.Cantidad, 0, p.MaxCantidad);
					return true;
				}
			}
		}

		if (pickupList.Count < maxPickups){
			pickupList.Add(pickupData);
			return true;
		}

		return false;
	}
#endregion
}
