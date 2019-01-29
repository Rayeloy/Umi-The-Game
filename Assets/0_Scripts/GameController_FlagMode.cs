﻿using System.Collections;
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
        scoreManager.KonoAwake(this as GameController_FlagMode);
        base.Awake();
        CreateFlag();
    }
    protected override void AllAwakes()
    {
        base.AllAwakes();
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

    public override void CreatePlayer(int playerNum = 0)
    {
        base.CreatePlayer(playerNum);
        if (offline)//Eloy: para Juan: en online habrá que solamente referenciar en el score manager a su player, no los de todos.
        {
            scoreManager.blueTeamScore_Text.Add(allCanvas[allCanvas.Count - 1].GetComponent<PlayerHUD>().blueTeamScoreText);
            scoreManager.redTeamScore_Text.Add(allCanvas[allCanvas.Count - 1].GetComponent<PlayerHUD>().redTeamScoreText);
            scoreManager.time_Text.Add(allCanvas[allCanvas.Count - 1].GetComponent<PlayerHUD>().timeText);
        }
    }

    public override void RemovePlayer(PlayerMovement _pM)
    {
        int index = allPlayers.IndexOf(_pM);
        base.RemovePlayer(_pM);
        if (offline)//Eloy: para Juan: como solo se referencia el nuestro propio, no hace falta borrar cosas del score manager cuando se borra a otro player. Solo borramos cuando nos borramos a nosotros.
        {
            scoreManager.blueTeamScore_Text.RemoveAt(index);
            scoreManager.redTeamScore_Text.RemoveAt(index);
            scoreManager.time_Text.RemoveAt(index);
        }
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
