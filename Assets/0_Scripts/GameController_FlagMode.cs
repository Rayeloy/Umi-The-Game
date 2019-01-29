using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameController_FlagMode : GameControllerBase
{
    [Header(" --- FLAG MODE --- ")]
    public ScoreManager scoreManager;
    public GameObject flagPrefab;
    public Transform flagsParent;
    [HideInInspector]
    public List<Flag> flags;
    //Posiciones de las porterias
    public Transform blueTeamFlagHome;
    public Transform redTeamFlagHome;
    public Transform centerCameraParent;

    protected override void Awake()
    {
        base.Awake();
        CreateFlag();
    }
    protected override void AllAwakes()
    {
        base.AllAwakes();
        scoreManager.KonoAwake(this as GameController_FlagMode);
    }

    public override void StartGame()
    {
        scoreManager.KonoStart();
        base.StartGame();
    }

    protected override void UpdateModeExclusiveClasses()
    {
        scoreManager.KonoUpdate();
    }

    public override void StartGameOver(Team _winnerTeam)
    {
        base.StartGameOver(_winnerTeam);
        //quitar banderas (mandarlas al store manager)
        for (int i = 0; i < flags.Count; i++)
        {
            flags[i].SetAway(true);
        }
    }

    public void ScorePoint(Team _winnerTeam)
    {
        scoreManager.ScorePoint(_winnerTeam);
    }

    #region FLAG FUNCTIONS

    public void CreateFlag()
    {
        Flag newFlag = Instantiate(flagPrefab,flagsParent).GetComponent<Flag>();
        newFlag.gC = this;
        flags.Add(newFlag);
    }

    public void RemoveFlag(Flag _flag)
    {
        flags.Remove(_flag);
    }

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
        for (int i = 0; i < flags.Count; i++)
        {
            flags[i].ResetFlag();
        }
    }

    #endregion

    public override void ResetGame()
    {
        base.ResetGame();
        scoreManager.Reset();
        RespawnFlags();
    }
}
