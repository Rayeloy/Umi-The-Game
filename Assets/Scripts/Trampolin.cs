using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampolin : MonoBehaviour
{
	public float Fuerza;
	public float tiempoStuneado;

	private void OnTriggerEnter(Collider other)
    {
		if (other.tag != "Player") return;

        PlayerMovement pm = other.gameObject.GetComponent<PlayerMovement>();
		if (pm != null)
			pm.StartFixedJump(transform.up * Fuerza, tiempoStuneado);
    }
}
