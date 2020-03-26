using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpdateMode
{
    Update,
    FixedUpdate
}

[RequireComponent(typeof(Rigidbody))]
public class MovePlatform : MonoBehaviour
{
    public UpdateMode executionMode = UpdateMode.Update;

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

    Rigidbody myRb;

    private void Awake()
    {
        vertSentido = sideSentido = fowAndBackSentido = 1;
        currentVertAmp = currentSideAmp = currentFowAndBackAmp = 0;
        originPos = transform.position;
        myRb = GetComponent<Rigidbody>();

        //if(myRb != null)
        //{
        //    if (executionMode == UpdateMode.FixedUpdate)
        //    {
        //        myRb.isKinematic = false;
        //    }
        //    else
        //    {
        //        myRb.isKinematic = true;
        //    }
        //}
    }

    private void Update()
    {
        if (executionMode == UpdateMode.Update && !GameInfo.instance.gameIsPaused)
        {
            //Debug.LogError("PLATFORM UPDATE");
            if (moveVertically)
            {
                currentVertAmp += verticalSpeed * Time.deltaTime * vertSentido;
                transform.position = originPos + transform.up * currentVertAmp;
                if ((vertSentido == 1 && (currentVertAmp >= verticalAmplitude)) || (vertSentido == -1 && (currentVertAmp <= -verticalAmplitude))) vertSentido *= -1;
            }
            if (moveSideways)
            {
                currentSideAmp += sidewaysSpeed * Time.deltaTime * sideSentido;
                Vector3 newPos = new Vector3(originPos.x + currentSideAmp, transform.position.y, transform.position.z);
                transform.position = newPos;
                if ((sideSentido == 1 && (currentSideAmp >= sidewaysAmplitude)) || (sideSentido == -1 && (currentSideAmp <= -sidewaysAmplitude))) sideSentido *= -1;
            }
            if (moveFowardAndBackwards)
            {
                currentFowAndBackAmp += FowardAndBackwardsSpeed * Time.deltaTime * fowAndBackSentido;
                Vector3 newPos = new Vector3(transform.position.x, transform.position.y, originPos.z + currentFowAndBackAmp);
                transform.position = newPos;
                if ((fowAndBackSentido == 1 && (currentFowAndBackAmp >= fowardAndBackwardsAmplitude)) || (fowAndBackSentido == -1 && (currentFowAndBackAmp <= -fowardAndBackwardsAmplitude))) fowAndBackSentido *= -1;
            }
        }
    }

    private void FixedUpdate()
    {
        if ((executionMode == UpdateMode.FixedUpdate /*|| executionMode == UpdateMode.FixedUpdateRB*/) && !GameInfo.instance.gameIsPaused)
        {
            //Debug.LogError("PLATFORM FIXED UPDATE");
            switch (executionMode)
            {
                case UpdateMode.FixedUpdate:
                    if (moveVertically)
                    {
                        currentVertAmp += verticalSpeed * Time.deltaTime * vertSentido;
                        myRb.position = originPos + transform.up * currentVertAmp;
                        if ((vertSentido == 1 && (currentVertAmp >= verticalAmplitude)) || (vertSentido == -1 && (currentVertAmp <= -verticalAmplitude))) vertSentido *= -1;
                    }
                    if (moveSideways)
                    {
                        currentSideAmp += sidewaysSpeed * Time.deltaTime * sideSentido;
                        Vector3 newPos = new Vector3(originPos.x + currentSideAmp, myRb.position.y, myRb.position.z);
                        myRb.position = newPos;
                        if ((sideSentido == 1 && (currentSideAmp >= sidewaysAmplitude)) || (sideSentido == -1 && (currentSideAmp <= -sidewaysAmplitude))) sideSentido *= -1;
                    }
                    if (moveFowardAndBackwards)
                    {
                        currentFowAndBackAmp += FowardAndBackwardsSpeed * Time.deltaTime * fowAndBackSentido;
                        Vector3 newPos = new Vector3(myRb.position.x, myRb.position.y, originPos.z + currentFowAndBackAmp);
                        myRb.position = newPos;
                        if ((fowAndBackSentido == 1 && (currentFowAndBackAmp >= fowardAndBackwardsAmplitude)) || (fowAndBackSentido == -1 && (currentFowAndBackAmp <= -fowardAndBackwardsAmplitude))) fowAndBackSentido *= -1;
                    }
                    break;
                //case UpdateMode.FixedUpdateRB:
                //    if (moveVertically)
                //    { Debug.Log("Move Platform: FIXED UPDATE RB");
                //        float currentMovement = Mathf.Abs(transform.position.y - originPos.y);
                //        if (currentMovement > verticalAmplitude) vertSentido *= -1;
                //        myRb.velocity = transform.up * vertSentido * verticalSpeed;
                //    }
                //    break;
            }
        }
    }
}
