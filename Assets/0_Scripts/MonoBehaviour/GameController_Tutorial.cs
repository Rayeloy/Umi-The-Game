using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController_Tutorial : GameControllerBase
{
    [Header(" --- TUTORIAL --- ")]
    public GameObject[] flags;

    protected override void Start()
    {
        for (int i = 0; i < playerNum; i++)
        {
            flags[i].SetActive(true);
            flags[i].GetComponent<Flag>().gC = this;
        }
        base.Start();

    }
}
