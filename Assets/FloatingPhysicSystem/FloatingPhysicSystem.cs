using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPhysicSystem : MonoBehaviour 
{

	public Vector3 boatCenterOffset=new Vector3();

	public float boatLenght=1,boatWidth=1;

	[Range(0.001f, 1.0f)]
	public float waterFriction=0.7f;

	[Range(0.0f, 2.0f)]
	public float floatCoefficent=1f;

	public float waterLevel=0f;

	[Range(0.0f, 5.0f)]
	public float waveScale=0.7f;

	[Range(0.0f, 5.0f)]
	public float waveSpeed=0.7f;

    float waterFloatLevel = 0;

	void FixedUpdate () 
	{
        Vector3 offset = transform.forward * boatCenterOffset.z + transform.up * boatCenterOffset.y + transform.right * boatCenterOffset.x;

		Vector3 poppa = 	transform.position - transform.forward * boatLenght/2+ offset;
		Vector3 prua = 		transform.position + transform.forward * boatLenght/2+ offset;
		Vector3 babordo = 	transform.position - transform.right * boatWidth/2+ offset;
		Vector3 tribordo = 	transform.position + transform.right * boatWidth/2+ offset;

		if (isOnWater(poppa)) 
		{
			float forceFactor = 1- poppa.y+ waterFloatLevel;
            if (wavePowerX < waterFloatLevel)
                forceFactor += wavePowerX;
			Vector3 uplift = -Physics.gravity*(forceFactor- GetComponent<Rigidbody>().velocity.y)*GetComponent<Rigidbody>().mass/4*floatCoefficent;
			GetComponent<Rigidbody> ().AddForceAtPosition (uplift,poppa);
		}
		if (isOnWater(prua)) 
		{
			float forceFactor = 1- prua.y+ waterFloatLevel;
            if (wavePowerX > waterFloatLevel)
                forceFactor -= wavePowerX;
            Vector3 uplift = -Physics.gravity*(forceFactor- GetComponent<Rigidbody>().velocity.y)*GetComponent<Rigidbody>().mass/4*floatCoefficent;
			GetComponent<Rigidbody> ().AddForceAtPosition (uplift,prua);
		}
		if (isOnWater(babordo)) 
		{
			float forceFactor = 1- babordo.y+ waterFloatLevel;
            if (wavePowerY < waterFloatLevel)
                forceFactor += wavePowerY;
            Vector3 uplift = -Physics.gravity*(forceFactor- GetComponent<Rigidbody>().velocity.y)*GetComponent<Rigidbody>().mass/4*floatCoefficent;
			GetComponent<Rigidbody> ().AddForceAtPosition (uplift,babordo);
		}
		if (isOnWater(tribordo)) 
		{
			float forceFactor = 1- tribordo.y+ waterFloatLevel;
            if (wavePowerY > waterFloatLevel)
                forceFactor -= wavePowerY;
            Vector3 uplift = -Physics.gravity*(forceFactor- GetComponent<Rigidbody>().velocity.y)*GetComponent<Rigidbody>().mass/4*floatCoefficent;
			GetComponent<Rigidbody> ().AddForceAtPosition (uplift,tribordo);
		}

		if (isOnWater(tribordo) && isOnWater(babordo) && isOnWater(prua) && isOnWater(poppa)) //frena in acqua e non si sposta all'infinito
		{
			GetComponent<Rigidbody> ().angularVelocity *= waterFriction;
			GetComponent<Rigidbody> ().velocity *= waterFriction;
		}

		if (waveScale > 0) 
		{
			if (invertWaveXTime)
				StartCoroutine (changeWaveX ());
			if (invertWaveYTime)
				StartCoroutine (changeWaveY ());
			wavePowerX = Mathf.Lerp (wavePowerX, waveScale * waveDirX,Time.deltaTime*waveSpeed);
			wavePowerY = Mathf.Lerp (wavePowerY, waveScale * waveDirY,Time.deltaTime*waveSpeed);
		}
		else
		{
			wavePowerX=wavePowerY=0;
		}
	}

	private float waveDirX = 1, wavePowerX;
	private bool invertWaveXTime = true;
	private IEnumerator changeWaveX()
	{
		invertWaveXTime = false;
		waveDirX *= -1;
		yield return new WaitForSeconds (Random.Range(2f,3.5f));
		invertWaveXTime = true;
	}


	private float waveDirY = 1, wavePowerY;
	private bool invertWaveYTime = true;
	private IEnumerator changeWaveY()
	{
		invertWaveYTime = false;
		waveDirY *= -1;
		yield return new WaitForSeconds (Random.Range(2f,3.5f));
		invertWaveYTime = true;
	}

    bool isOnWater (Vector3 pos)
    {
        waterFloatLevel = 0;
        if (pos.y < waterLevel)
            return true;

        foreach (WaterPool _pool in Poolsmanager.pools)
            if (_pool.positionIsInside(pos))
            {
                waterFloatLevel=_pool.transform.position.y+_pool._water.center.y+ (_pool._water.size.y/2);//cambia punto di galleggiamento
                return true;
            }

        return false;
    }


    void OnDrawGizmosSelected()
    {
        float _bubblesize = (boatLenght > boatWidth) ? boatWidth / 5 : boatLenght / 5;

        Vector3 offset = transform.forward * boatCenterOffset.z + transform.up * boatCenterOffset.y + transform.right * boatCenterOffset.x;
        Color _gizcolor = Color.red;
        _gizcolor.a = 0.4f;
        Gizmos.color = _gizcolor;

        Gizmos.DrawSphere(transform.position - transform.forward * boatLenght / 2 + offset, _bubblesize);
        Gizmos.DrawSphere(transform.position + transform.forward * boatLenght / 2 + offset, _bubblesize);
        Gizmos.DrawSphere(transform.position - transform.right * boatWidth / 2 + offset, _bubblesize);
        Gizmos.DrawSphere(transform.position + transform.right * boatWidth / 2 + offset, _bubblesize);

        _gizcolor = Color.yellow;
        _gizcolor.a = 1f;
        Gizmos.color = _gizcolor;

        Gizmos.DrawWireSphere(transform.position - transform.forward * boatLenght / 2 + offset, _bubblesize);
        Gizmos.DrawWireSphere(transform.position + transform.forward * boatLenght / 2 + offset, _bubblesize);
        Gizmos.DrawWireSphere(transform.position - transform.right * boatWidth / 2 + offset, _bubblesize);
        Gizmos.DrawWireSphere(transform.position + transform.right * boatWidth / 2 + offset, _bubblesize);
    }
}