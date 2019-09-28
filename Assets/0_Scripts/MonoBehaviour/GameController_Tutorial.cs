using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region ----[ PUBLIC ENUMS ]----
public enum TutorialPhase
{
    StartPhase = 0,
    DummyPhase = 1,
    WallJumpPhase = 2,
    GrapplePhase = 3,
    CannonPhase = 4,
    RingBattlePhase = 5
}
#endregion
public class GameController_Tutorial : GameControllerBase
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    [Header(" --- TUTORIAL MODE --- ")]
    //Referencias
    public TutorialLane[] tutorialLanes;
    public Transform ringSpawnPoint;

    [Header(" --- Ring Battle ---")]
    public Text battleText;
    public float battleMaxTimeToStart;
    float battleTimeToStart;
    public float fightTextshowingMaxTime;
    [HideInInspector]
    public bool startRingTeamBattle = false;
    #endregion

    #region ----[ PROPERTIES ]----

    List<PlayerMovement> playersInRing;
    bool cdToStartRingTeamBattle = false;
    float fightTextShowingTime = 0;

    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    protected override void SpecificAwake()
    {
        playersInRing = new List<PlayerMovement>();
        battleTimeToStart = battleMaxTimeToStart;
        battleText.gameObject.SetActive(false);
        for (int i = 0; i < tutorialLanes.Length; i++)
        {
            tutorialLanes[i].Awake();
        }
    }
    #endregion

    #region Start
    #endregion

    #region Update
    protected override void SpecificUpdate()
    {
        if (!gamePaused)
        {
            if (playing)
            {
                for(int i=0; i < tutorialLanes.Length; i++)
                {
                    tutorialLanes[i].Update();
                }
                //NO CAMBIAR EL ORDEN DE ESTAS 3
                CheckStartRingTeamBattle();
                ProcessRingTeamBattle();
                CountdownRingTeamBattle();
                //---------------------------
            }
        }
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----

    void CheckStartRingTeamBattle()
    {
        if (!cdToStartRingTeamBattle && !startRingTeamBattle)
        {
            int teamAPlayersInRing = 0;
            int teamBPlayersInRing = 0;
            bool start = false;

            for (int i = 0; i < playersInRing.Count; i++)
            {
                if (playersInRing[i].team == Team.A)
                    teamAPlayersInRing++;
                if (playersInRing[i].team == Team.B)
                    teamBPlayersInRing++;
            }
            //Debug.Log("CheckStartRingTeamBattle: Team A players in ring= " + teamAPlayersInRing + ";  Team B players in ring= " + teamBPlayersInRing);
            if ((teamAPlayersInRing == playerNumTeamA && playerNumTeamA > 0) || (teamBPlayersInRing == playerNumTeamB && playerNumTeamB > 0)) start = true;
            if (start)
            {
                StartCountdownRingTeamBattle();
            }
        }
    }

    void StartCountdownRingTeamBattle()
    {
        if (!cdToStartRingTeamBattle)
        {
            print("START COUNTDOWN RING TEAM BATTLE");
            battleTimeToStart = battleMaxTimeToStart;
            cdToStartRingTeamBattle = true;
            battleText.gameObject.SetActive(true);
        }
    }

    void CountdownRingTeamBattle()
    {
        if (cdToStartRingTeamBattle)
        {
            battleTimeToStart -= Time.deltaTime;
            battleText.text = (int)battleTimeToStart + " seconds to start the team battle!";
            if (battleTimeToStart <= 0)
            {
                StopCountdownRingTeamBattle();
            }
        }
    }

    void StopCountdownRingTeamBattle()
    {
        if (cdToStartRingTeamBattle)
        {
            cdToStartRingTeamBattle = false;
            battleText.fontSize = 100;
            battleText.text = "FIGHT!";

            //Teleport playeres out of the ring
            for (int i = 0; i < allPlayers.Count; i++)
            {
                Debug.Log("allPlayers.count = " + allPlayers.Count + "; playersInRing.Count = " + playersInRing.Count);
                if (!playersInRing.Contains(allPlayers[i]))
                {
                    allPlayers[i].TeleportPlayer(ringSpawnPoint.position); 
                }
            }
            StartRingTeamBattle();
        }
    }

    void StartRingTeamBattle()
    {
        if (!startRingTeamBattle)
        {
            startRingTeamBattle = true;
            fightTextShowingTime = 0;
        }
    }

    void ProcessRingTeamBattle()
    {
        if (startRingTeamBattle)
        {
            if (battleText.gameObject.activeInHierarchy)
            {
                fightTextShowingTime += Time.deltaTime;
                if (fightTextShowingTime >= fightTextshowingMaxTime)
                {
                    battleText.gameObject.SetActive(false);
                }
            }

            int teamAPlayersInRing = 0;
            int teamBPlayersInRing = 0;
            for (int i = 0; i < playersInRing.Count; i++)
            {
                    if (playersInRing[i].team == Team.A)
                        teamAPlayersInRing++;
                    if (playersInRing[i].team == Team.B)
                        teamBPlayersInRing++;
            }
            if (teamAPlayersInRing == 0) FinishRingTeamBattle(Team.A);
            if (teamBPlayersInRing == 0) FinishRingTeamBattle(Team.B);
        }
    }

    void FinishRingTeamBattle(Team winner)
    {
        if (startRingTeamBattle)
        {
            battleText.gameObject.SetActive(false);
            startRingTeamBattle = false;
            StartGameOver(winner);
        }
    }

    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----
    public void ProgressLane(int laneNumber)
    {
        Debug.Log("Lane " + laneNumber + ": ProgressLane -> " +tutorialLanes[laneNumber].phase);
        switch (tutorialLanes[laneNumber].phase)
        {
            case TutorialPhase.StartPhase:
                tutorialLanes[laneNumber].InflateDummyAndWalls();
                break;
            case TutorialPhase.DummyPhase:
                tutorialLanes[laneNumber].DeflateDummyAndWalls();
                break;
            case TutorialPhase.WallJumpPhase:
                break;
            case TutorialPhase.GrapplePhase:
                break;
            case TutorialPhase.CannonPhase:
                //StartRingTeamBattle();
                break;
            case TutorialPhase.RingBattlePhase:

                break;
        }
        tutorialLanes[laneNumber].ProgressPhase();

    }

    public void PlayerEnterRing(PlayerMovement player)
    {
        print("PLAYER ENTER RING");
        playersInRing.Add(player);
    }

    public void playerExitRing(PlayerMovement player)
    {
        print("PLAYER EXIT RING");
        playersInRing.Remove(player);
    }

    #endregion

    #region ----[ PUN CALLBACKS ]----
    #endregion

    #region ----[ RPC ]----
    #endregion

    #region ----[ NETWORK FUNCTIONS ]----
    #endregion

    #region ----[ IPUNOBSERVABLE ]----
    #endregion
}

#region ----[ STRUCTS & CLASSES ]----

[System.Serializable]
public class PhaseInvisibleWalls
{
    public GameObject[] invisibleWalls;
}
#endregion