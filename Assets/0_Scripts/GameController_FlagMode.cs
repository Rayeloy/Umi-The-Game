using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController_FlagMode : GameControllerBase
{
    public Flag[] flags;
    //Posiciones de las porterias
    public Transform blueTeamFlagHome;
    public Transform redTeamFlagHome;

    protected override void Awake()
    {
        base.Awake();

    }

    public override void StartGameOver(Team _winnerTeam)
    {
        base.StartGameOver(_winnerTeam);
        //quitar banderas (mandarlas al store manager)
        for (int i = 0; i < flags.Length; i++)
        {
            flags[i].SetAway(true);
        }
    }

    public void ScorePoint(Team _winnerTeam)
    {
        scoreManager.ScorePoint(_winnerTeam);
    }

    #region FLAG FUNCTIONS

    #region No se usa
    //Respawnea bandera en la posicion dada
    public void RespawnFlag(Vector3 respawnPos)
    {
        print("RESPAWN FLAG");
        RespawnFlagIssho(respawnPos);
    }

    //Respawnea bandera en la posicion guardada en el script de la bandera (Flag)
    public void RespawnFlag()
    {
        print("RESPAWN FLAG");
        Transform flag = StoringManager.instance.LookForObjectStoredTag("Flag");
        Vector3 respawnPos = flag.GetComponent<Flag>().respawnPos;
        RespawnFlagIssho(respawnPos);
    }

    //issho significa "junto" en japo, respawnea una bandera segun una posicion dada o predeterminada. SIN USAR
    void RespawnFlagIssho(Vector3 respawnPos)
    {

    }
    #endregion

    public void RespawnFlags()
    {
        for (int i = 0; i < flags.Length; i++)
        {
            flags[i].ResetFlag();
        }
    }

    #endregion

    public override void ResetGame()
    {
        base.ResetGame();
        ScoreManager.instance.Reset();
        RespawnFlags();
    }
}
