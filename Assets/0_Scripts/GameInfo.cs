using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

//Esta clase es para guardar datos del juego entre escenas
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
    
    public Team NoneTeamSelect()
    {
        int nAzul = 0;
        int nRojo = 0;
        foreach (Team t in playerTeamList)
        {
            if (t == Team.blue)
                nAzul++;
            else if (t == Team.red)
                nRojo++;
        }
        if(nAzul == nRojo)
        {
            if (Random.value < 0.5f)
                return Team.blue;
            else
                return Team.red;
        }
       else if (nAzul > nRojo)
           return Team.red;
       else 
           return Team.blue;
//       else
//       {
//           if (Random.value < 0.5f){
//               Debug.Log("Random Azul");
//               return Team.blue;
//           }
//           else{
//               Debug.Log("Random Rojo");
//               return Team.red;
//           }
//       }
    }
}
