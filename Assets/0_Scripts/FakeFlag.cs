using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeFlag : MonoBehaviour {

    bool started = false;
    float time = 0;
    float timeToDespawn;
    float fallSpeed;

    public void KonoAwake(float _timeToDespawn, float _fallSpeed)
    {
        started = true;
        time = 0;
        timeToDespawn = _timeToDespawn;
        fallSpeed = _fallSpeed;
    }

    private void Update()
    {
        if (started)
        {
            Fall();

            time += Time.deltaTime;
            if (time >= timeToDespawn)
            {
                started = false;
                StoringManager.instance.StoreObject(transform);
            }
        }
    }

    void Fall()
    {
        Vector3 vel = Vector3.down * fallSpeed * Time.deltaTime;
        transform.Translate(vel, Space.World);
    }
}
