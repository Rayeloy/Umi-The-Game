using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class GameInfo : MonoBehaviour
{
    public static GameInfo instance;
    public List<PlayerActions> playerActionsList = new List<PlayerActions>();
    public int nPlayers;


    private void Awake()
    {
        instance = this;
        playerActionsList = new List<PlayerActions>();

    }
    //public static PlayerActions[] playerActionsList;
}
