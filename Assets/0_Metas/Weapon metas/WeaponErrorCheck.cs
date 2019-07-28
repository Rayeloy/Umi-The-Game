using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WeaponErrorCheck : MonoBehaviour
{
    public WeaponData[] allWeapons;
    public bool doErrorCheck = false;
    private void Update()
    {
        if (doErrorCheck)
        {
            doErrorCheck = false;
            for(int i = 0; i < allWeapons.Length; i++)
            {
                allWeapons[i].ErrorCheck();
            }
        }
    }
}
