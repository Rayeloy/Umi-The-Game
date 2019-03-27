using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cañon : MonoBehaviour
{
    //public Rigidbody ball;
    public Transform target;

    public float h = 25;
    float gravity = -18;

    public bool debugPath;

    void Start(){
        //ball.useGravity = false;
    }

    void Update(){
        if (debugPath){
            DrawPath(gravity);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;

        PlayerMovement pm = other.transform.GetComponent<PlayerBody>().myPlayerMov;
        //Debug.Log(pm.gravity);
        //print("PLAYER GRAVITY = "+pm.gravity);
        LaunchData launchData = CalculateLaunchData(pm.gravity);
        //if (pm != null && pm.currentVel.y <= playerSpeed)
        //print("CANNON VELOCITY = "+launchData.initialVolicity);
	    pm.StartFixedJump(launchData.initialVolicity, launchData.timeToTarget);
    }

    /*void Launch(){
        Physics.gravity = Vector3.up * gravity;
        ball.useGravity = true;
        ball.velocity = CalculateLaunchData().initialVolicity;
    }*/

    void DrawPath(float g){
        LaunchData launchData = CalculateLaunchData(g);
        Vector3 previousDrawPoint = transform.position;//ball.position;

        const int resolution = 30;
        for (int i = 1; i<=resolution; i++){
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVolicity * simulationTime + Vector3.up * g * simulationTime * simulationTime / 2;
            Vector3 drawPoint = transform.position + displacement;
            Debug.DrawLine (previousDrawPoint, drawPoint, Color.green);
            previousDrawPoint = drawPoint;
        }

    }

    LaunchData CalculateLaunchData(float g){
        float displacementY = target.position.y - transform.position.y;
        Vector3 DisplacementXZ = new Vector3 (target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
        //print("pene = " + Mathf.Sqrt(-2 * h / g)+"h = "+h+"; g = "+g);
        float time = Mathf.Sqrt(-2*h/g) + Mathf.Sqrt(-2*(displacementY - h)/g);
        //print("pene = " + Mathf.Sqrt(-2 * h / g) + ";pene2 = " + Mathf.Sqrt(2 * Mathf.Abs((displacementY - h)) / g) + "; pene = "+ 2 * (2 * Mathf.Abs((displacementY - h)) / g));
        Vector3 velocityY = Vector3.up * Mathf.Sqrt (-2 *g * h);
        Vector3 velocityXZ = DisplacementXZ / time;
        //print("DisplacementXZ = " + DisplacementXZ + "; time = " + time);
        return new LaunchData(velocityXZ + velocityY, time);
    }

    struct LaunchData{
        public readonly Vector3 initialVolicity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVolicity, float timeToTarget){
            print("initialVolicity = " + initialVolicity);
            this.initialVolicity = initialVolicity;
            this.timeToTarget = timeToTarget;
        }
    }
}
