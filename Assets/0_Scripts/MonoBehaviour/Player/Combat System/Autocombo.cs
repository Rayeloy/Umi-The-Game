using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autocombo : ScriptableObject
{
    public string autocomboName;
    public AttackData[] attacks;
    public float maxTimeBetweenAttacks = 0.2f;

    public void ErrorCheck()
    {
        if (attacks.Length < 2) Debug.LogError("Autocombo -> Error: The autocombo "+ autocomboName + " has less than 2 attacks.");
        for(int i = 0; i < attacks.Length; i++)
        {
            attacks[i].ErrorCheck();
        }
    }
}
