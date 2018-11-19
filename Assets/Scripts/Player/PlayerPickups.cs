using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickups : MonoBehaviour
{
	public List<PickupData> pickupList = new List<PickupData>();//PickupData[] pickupList;
	public int maxPickups;
	private int _activePickup = 0;
	[HideInInspector]
	public int activePickup
	{
		get{return _activePickup;}
		set{
			if (pickupList[activePickup].name == "Hichador")
				myPlayerCombat.conHinchador = true;
			_activePickup = value;
		}
	}

	[Header("Hinchador")]
	public LayerMask m_LayerMask; 
	public Vector3 collPosition;
	public Vector3 collScale;
	public Transform rota;

	[Header ("Referencias")]
	public PlayerMovement myPlayerMovement;
	public PlayerCombat myPlayerCombat;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	float cooldownTime = 0;
	const float maxCooldownTime = 100;
	void Update ()
	{
		if (pickupList.Count < 1)
			return;

		//Control cooldown
		cooldownTime += Time.deltaTime;
		cooldownTime = Mathf.Clamp(cooldownTime, 0, maxCooldownTime);

		//Si se ha presionado el boton
		if (pickupList[activePickup].name == "Hinchador" && myPlayerMovement.Actions.UsePickup.IsPressed)
		{
			Collider[] hitColliders = Physics.OverlapBox(rota.position + collPosition, collScale / 2, rota.rotation, m_LayerMask);
			int i = 0;

			while (i < hitColliders.Length)
			{
				hinchable h = hitColliders[i].GetComponent<hinchable>();
				if (h != null){
					h.Hinchar(pickupList[activePickup].Daño * Time.deltaTime);
				}

				i++;
			}
		}

		else if (myPlayerMovement.Actions.UsePickup.WasPressed) {
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

		switch (pickupList[activePickup].name)
        {
            case "Hinchador":
//				Collider[] hitColliders = Physics.OverlapBox(rota.position + collPosition, collScale / 2, rota.rotation, m_LayerMask);
//				int i = 0;
//
//				while (i < hitColliders.Length)
//				{
//					hinchable h = hitColliders[i].GetComponent<hinchable>();
//					if (h != null)
//						h.Hinchar(pickupList[activePickup].Daño);
//					i++;
//				}
                break;
//            case PickupType.Mele:
//                //uso de pickup mele
//				Debug.Log("Uso:" + pickupList[activePickup].name + " Tipo: Mele");
//                break;
//			case PickupType.Range:
//                //uso de pickup mele
//				Debug.Log("Uso:" + pickupList[activePickup].name + " Tipo: Range");
//                break;
		}
	}

	public void equipar (int n)
	{
		if (pickupList.Count < 0 || pickupList[activePickup].name == "Hinchador") return;

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

			if (pickupData.name == "Hinchador")
				activePickup = pickupList.Count - 1;

			return true;
		}

		return false;
	}
#endregion

//    void OnDrawGizmos()
//    {
//
//		Matrix4x4 rotationMatrix = Matrix4x4.TRS(rota.position, rota.rotation, rota.lossyScale);
//		Gizmos.matrix = rotationMatrix; 
//        Gizmos.color = Color.red;
//        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
//        //if (m_Started)
//            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
//            Gizmos.DrawWireCube(collPosition, collScale/2);
//    }

}
