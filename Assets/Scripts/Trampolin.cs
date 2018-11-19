using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampolin : MonoBehaviour
{
	public float Fuerza;
	public float tiempoStuneado;
    [Tooltip("Velocidad vertical minima que tiene que llevar el jugador para saltar en el trampolin")]
    [Range(0,-100)]
    public float playerSpeed;

	private void OnTriggerEnter(Collider other)
    {
		if (other.tag != "Player") return;

        PlayerMovement pm = other.transform.GetComponent<PlayerBody>().myPlayerMov;
		if (pm != null && pm.currentVel.y <= playerSpeed)
			pm.StartFixedJump(transform.up * Fuerza, tiempoStuneado);
    }

	void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = new Color(1, 0, 0, 0.75F);
        DrawArrow.ForGizmo(transform.position, transform.up, 0.25f, 20, 1);
    }
}
