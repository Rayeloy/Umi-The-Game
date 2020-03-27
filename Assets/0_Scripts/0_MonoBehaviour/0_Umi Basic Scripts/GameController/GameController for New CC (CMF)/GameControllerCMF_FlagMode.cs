using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerCMF_FlagMode : GameControllerCMF
{
    [Header(" --- FLAG MODE --- ")]
    public ScoreManagerCMF myScoreManager;
    public GameObject flagPrefab;
    public Transform flagsParent;
    [HideInInspector]
    public List<FlagCMF> flags;
    //Posiciones de las porterias
    public Transform FlagHome_TeamA;
    public Transform FlagHome_TeamB;
    public Transform centerCameraParent;

    public float minDistToSeeBeam;

    protected override void Awake()
    {
        myScoreManager.KonoAwake(this as GameControllerCMF_FlagMode);
        base.Awake();
    }
    protected override void SpecificAwake()
    {
        CreateFlag();
        HideFlagHomeLightBeam(Team.A);
        HideFlagHomeLightBeam(Team.B);

        flagsParent.GetComponent<FlagsParent>().KonoAwake(flags[0]);
    }

    public override void StartGame()
    {
        myScoreManager.KonoStart();
        base.StartGame();
    }

    protected override void UpdateModeExclusiveClasses()
    {
        myScoreManager.KonoUpdate();
        for (int i = 0; i < flags.Count; i++)
        {
            flags[i].KonoUpdate();
        }
    }

    public override void CreatePlayer(int playerNumber)
    {
        base.CreatePlayer(playerNumber);
        //if (!online)//Eloy: para Juan: en online habrá que solamente referenciar en el score manager a su player, no los de todos.
        //{
        myScoreManager.teamAScore_Text.Add(allCanvas[allCanvas.Count - 1].GetComponent<PlayerHUDCMF>().blueTeamScoreText);
        myScoreManager.teamBScore_Text.Add(allCanvas[allCanvas.Count - 1].GetComponent<PlayerHUDCMF>().redTeamScoreText);
        myScoreManager.time_Text.Add(allCanvas[allCanvas.Count - 1].GetComponent<PlayerHUDCMF>().timeText);
        //}
        //else
        //{
        //    Debug.LogWarning("Warning: aquí falta código por escribir. En online habrá que solamente referenciar en el score manager a su player, no los de todos.");
        //}
    }

    public override void RemovePlayer(PlayerMovementCMF _pM)
    {
        int index = allPlayers.IndexOf(_pM);
        base.RemovePlayer(_pM);
        //if (!online)//Eloy: para Juan: como solo se referencia el nuestro propio, no hace falta borrar cosas del score manager cuando se borra a otro player. Solo borramos cuando nos borramos a nosotros.
        //{
        myScoreManager.teamAScore_Text.RemoveAt(index);
        myScoreManager.teamBScore_Text.RemoveAt(index);
        myScoreManager.time_Text.RemoveAt(index);
        //}
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

    public void ScorePoint(Team _scoringTeam)
    {
        myScoreManager.ScorePoint(_scoringTeam);
    }

    #region FLAG FUNCTIONS
    /// <summary>
    /// Spawn flag at the beggining. Only once
    /// </summary>
    public void CreateFlag()
    {
        FlagCMF newFlag;
        newFlag = Instantiate(flagPrefab, flagsParent).GetComponent<FlagCMF>();
        newFlag.gC = this;
        flags.Add(newFlag);
    }

    public void RemoveFlag(FlagCMF _flag)
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
        Vector3 respawnPos = flag.GetComponent<FlagCMF>().respawnPos;
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

    public override void ResetGame()//Eloy: habrá que resetear muchas más cosas
    {
        base.ResetGame();
        myScoreManager.Reset();
        RespawnFlags();
    }

    public void ShowFlagHomeLightBeam(Team ownersTeam)
    {
        for (int i = 0; i < allPlayers.Count; i++)
        {
            allPlayers[i].ShowFlagHomeLightBeam(ownersTeam);
        }
    }

    public void HideFlagHomeLightBeam(Team ownersTeam)
    {
        for (int i = 0; i < allPlayers.Count; i++)
        {
            allPlayers[i].HideFlagHomeLightBeam(ownersTeam);
        }
    }
}
