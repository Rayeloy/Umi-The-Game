using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePlus : MonoBehaviour
{
    public static float betterTime()
    {
        if(BoltNetwork.IsConnected && BoltNetwork.IsClient)
        {
            return BoltNetwork.ServerTime;
        }
        else
        {
            return Time.deltaTime;
        }
    }
}
