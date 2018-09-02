using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController instance;
    public AttackData[] allAttacks;
    bool slowmo = false;
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            if (slowmo)
            {
                Time.timeScale = 0.25f;
                slowmo = false;
            }
            else
            {
                Time.timeScale = 1;
                slowmo = true;
            }
        }
	}

    public enum controllerName
    {
        C1,
        C2,
        C3,
        C4
    }
}
