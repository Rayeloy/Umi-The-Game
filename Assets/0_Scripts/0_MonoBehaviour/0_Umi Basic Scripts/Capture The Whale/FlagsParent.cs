using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagsParent : MonoBehaviour
{
    FlagCMF myFlag;
    [Header("-- Flag Physics --")]
    [Tooltip("Vertical distance you want the orca to be 'levitating' over the floor")]
    public float heightFromFloor = 4;
    [Header("-- Idle Animation Param --")]
    [Tooltip("Distance from the middle (default) position to the max height and min height. The total distance of the animation will be the double of this value")]
    public float idleAnimVertDist = 2;
    [Tooltip("How much seconds per half animation cycle (i.e. from bottom to top height). The shorter the time, the faster the animation.")]
    public float idleAnimFrequency = 2;


    public void KonoAwake(FlagCMF flag)
    {
        myFlag = flag;
        myFlag.heightFromFloor = heightFromFloor;
        myFlag.idleAnimVertDist = idleAnimVertDist;
        myFlag.idleAnimFrequency = idleAnimFrequency;
    }
}
