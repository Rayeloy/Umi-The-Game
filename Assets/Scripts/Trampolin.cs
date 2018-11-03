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

	void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = new Color(1, 0, 0, 0.75F);
        DrawArrow.ForGizmo(transform.position, transform.up, 0.25f, 20, 1);
    }
}
