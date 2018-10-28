using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class GameInfo : MonoBehaviour
{
    public static GameInfo instance;
    public List<PlayerActions> playerActionsList;
    public List<Team> playerTeamList;
    public int nPlayers;


    private void Awake()
    {
        instance = this;
        playerActionsList = new List<PlayerActions>();
        playerTeamList = new List<Team>();

    }
    //public static PlayerActions[] playerActionsList;
}
