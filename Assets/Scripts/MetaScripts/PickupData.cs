using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Pickup", menuName = "PickUp")]
public class PickupData : ScriptableObject
{
	public PickupType Type;
	//public string name; //todos los objetos ya tiene un name que es el nombre por el que se guarda.
	public float Daño;
	public float Rango;
	public float Velocidad;
	public float recargaDeDisparo;
	public int Cantidad;
	public int MaxCantidad;
	[Range(0.1f, 100)]
	public float Cooldown;
	public float rangoDeExplosion;
	public GameObject prefab;
	/*// no es necesario
	public bool Empty = false;

	public void Clear (){
		Type = PickupType.none;
		Daño = 0;
		Rango = 0;
		Velocidad = 0;
		recargaDeDisparo = 0;
		Cantidad = 0;
		MaxCantidad = 0;
		Cooldown = 0;
		rangoDeExplosion = 0;
		prefab = null;
		Empty = true;
	}*/
}
public enum PickupType
{
	Mele,
	Range,
	none
}
