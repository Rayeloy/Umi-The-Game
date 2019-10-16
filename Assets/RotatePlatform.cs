using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
    public bool rotateX = true;
    public bool rotateY = true;
    public bool rotateZ = true;

    public float rotationSpeedX = 1;
    public float rotationSpeedY = 1;
    public float rotationSpeedZ = 1;

    public float xAmplitude = 90;
    public float zAmplitude = 90;

    float xOrigin, zOrigin;
    int xSentido, zSentido;
    float xCurrentAmplitude;
    float zCurrentAmplitude;

    private void Awake()
    {
        xOrigin = transform.localRotation.eulerAngles.x;
        zOrigin = transform.localRotation.eulerAngles.z;
        xSentido = zSentido = 1;
        xCurrentAmplitude = zCurrentAmplitude = 0;
    }

    private void Update()
    {
        float finalRotationX, finalRotationY, finalRotationZ;
        finalRotationX = finalRotationY = finalRotationZ = 0;
        if (rotateX)
        {
            xCurrentAmplitude += (rotationSpeedX * Time.deltaTime * xSentido);
            finalRotationX = xOrigin + xCurrentAmplitude;
            if (xSentido == 1)
            {
                if (xCurrentAmplitude >= xAmplitude)
                {
                    xSentido *= -1;
                    //xCurrentAmplitude = 0;
                }
            }
            else
            {
                if (xCurrentAmplitude <= -xAmplitude)
                {
                    xSentido *= -1;
                    //xCurrentAmplitude = 0;
                }
            }
            //Debug.Log("X ROTATION-> xSentido = " + xSentido + "; xCurrentAmplitude = " + xCurrentAmplitude);
        }
        if (rotateY)
        {
            finalRotationY = transform.localRotation.eulerAngles.y + (rotationSpeedY * Time.deltaTime);
        }
        if (rotateZ)
        {
            zCurrentAmplitude += (rotationSpeedZ * Time.deltaTime * zSentido);
            finalRotationZ = zOrigin + zCurrentAmplitude;
            if (zSentido == 1)
            {
                if (zCurrentAmplitude >= zAmplitude)
                {
                    zSentido *= -1;
                    //zCurrentAmplitude = 0;
                }
            }
            else
            {
                if (zCurrentAmplitude <= -zAmplitude)
                {
                    zSentido *= -1;
                    //zCurrentAmplitude = 0;
                }
            }
            //Debug.Log("Z ROTATION-> zSentido = "+ zSentido + "; zCurrentAmplitude = "+ zCurrentAmplitude);
        }
        transform.localRotation = Quaternion.Euler(finalRotationX, finalRotationY, finalRotationZ);
    }
}
