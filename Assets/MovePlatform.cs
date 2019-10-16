using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlatform : MonoBehaviour
{
    public bool moveVertically = true;
    public bool moveSideways = true;
    public bool moveFowardAndBackwards = true;

    public float verticalSpeed = 2;
    public float sidewaysSpeed = 2;
    public float FowardAndBackwardsSpeed = 2;


    public float verticalAmplitude = 5;
    public float sidewaysAmplitude = 5;
    public float fowardAndBackwardsAmplitude = 5;

    int vertSentido, sideSentido, fowAndBackSentido;
    float currentVertAmp, currentSideAmp, currentFowAndBackAmp;
    Vector3 originPos;

    private void Awake()
    {
        vertSentido = sideSentido = fowAndBackSentido = 1;
        currentVertAmp = currentSideAmp = currentFowAndBackAmp = 0;
        originPos = transform.position;
    }

    private void Update()
    {
        if (moveVertically)
        {
            currentVertAmp += verticalSpeed * Time.deltaTime * vertSentido;
            transform.position = originPos + transform.up * currentVertAmp;
                if ((vertSentido==1 && (currentVertAmp >= verticalAmplitude)) || (vertSentido == -1 && (currentVertAmp <= -verticalAmplitude))) vertSentido *= -1;
        }
        if (moveSideways)
        {
            currentSideAmp += sidewaysSpeed * Time.deltaTime * sideSentido;
            transform.position = originPos + transform.right * currentSideAmp;
            if ((sideSentido == 1 && (currentSideAmp >= sidewaysAmplitude)) || (sideSentido == -1 && (currentSideAmp <= -sidewaysAmplitude))) sideSentido *= -1;
        }
        if (moveFowardAndBackwards)
        {
            currentFowAndBackAmp += FowardAndBackwardsSpeed * Time.deltaTime * fowAndBackSentido;
            transform.position = originPos + transform.forward * currentFowAndBackAmp;
            if ((fowAndBackSentido == 1 && (currentFowAndBackAmp >= fowardAndBackwardsAmplitude)) || (fowAndBackSentido == -1 && (currentFowAndBackAmp <= -fowardAndBackwardsAmplitude))) fowAndBackSentido *= -1;
        }
    }

}
