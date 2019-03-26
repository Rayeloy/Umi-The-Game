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
    [Header(" --- FLAG MODE --- ")]
    //Referencias
    public TutorialLane[] tutorialLanes;
    public Transform ringSpawnPoint;
    public Dummy dummy;
    [Header(" --- Shitty Animations --- ")]
    [Header("Walls")]
    public GameObject[] inflatableWallsInflatingAlembic = new GameObject[2];
    public float inflatableWallsInflatingAnimMaxTime;
    public GameObject[] inflatableWallsBefore = new GameObject[2];
    public GameObject[] inflatableWallsBreakingAlembic = new GameObject[2];
    public float inflatableWallsBreakingAnimMaxTime;
    public GameObject[] inflatableWallsAfter = new GameObject[2];
    public GameObject[] inflatableWallsPlatforms = new GameObject[2];
    bool inflatableWallsInflating = false;
    bool inflatableWallsBreaking = false;
    float inflatableWallsInflatingTime = 0;
    float inflatableWallsBreakingTime = 0;
    [Header("Cannon")]
    public GameObject cannon;
    public GameObject cannonShootingAembic;
    public float cannonAnimMaxTime;
    [Header(" --- Ring Battle ---")]
    public Text battleText;
    public float battleMaxTimeToStart;
    float battleTimeToStart;
    public float fightTextshowingMaxTime;
    [HideInInspector]
    public bool startRingTeamBattle = false;
    #endregion

    #region ----[ PROPERTIES ]----
    float cannonAnimTime = 0;
    bool startShootingCannon = false;

    List<PlayerMovement> playersInRing;
    bool cdToStartRingTeamBattle = false;
    float fightTextShowingTime = 0;

    #endregion

    #region ----[ VARIABLES ]----
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    protected override void SpecificAwake()
    {
        tutorialLanes = new TutorialLane[2];
        for (int i = 0; i < inflatableWallsBefore.Length; i++)
        {
            inflatableWallsInflatingAlembic[i].SetActive(false); 
            inflatableWallsBefore[i].SetActive(true);
            inflatableWallsBreakingAlembic[i].SetActive(false);
            inflatableWallsAfter[i].SetActive(false);
            inflatableWallsPlatforms[i].SetActive(false);
        }
        playersInRing = new List<PlayerMovement>();
        battleTimeToStart = battleMaxTimeToStart;
        battleText.enabled = false;
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
                InflatingWalls();
                BreakingWalls();
                CheckStartRingTeamBattle();
                CountdownRingTeamBattle();
                ProcessRingTeamBattle();
            }
        }
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    void InflateDummyAndWalls()
    {
        dummy.StartInflateDummy();
        //Inflate walls animation
        StartInflatingWalls();
    }

    void DeflateDummyAndWalls()
    {
        dummy.StartDeflateDummy();
        //Inflate walls animation
        StartBreakingWalls();
    }

    void CheckStartRingTeamBattle()
    {
        int bluePlayersInRing = 0;
        int redPlayersInRing = 0;
        bool start = false;

        for(int i=0; i < playersInRing.Count; i++)
        {
            if (playersInRing[i].team == Team.blue)
                bluePlayersInRing++;
            if (playersInRing[i].team == Team.red)
                redPlayersInRing++;
        }
        if (bluePlayersInRing == playerNumBlue) start = true;
        if (redPlayersInRing == playerNumRed) start = true;
        if (start)
        {
            StartCountdownRingTeamBattle();
        }
    }

    void StartCountdownRingTeamBattle()
    {
        battleTimeToStart = battleMaxTimeToStart;
        cdToStartRingTeamBattle = true;
        battleText.enabled = true;
    }

    void CountdownRingTeamBattle()
    {
        if (cdToStartRingTeamBattle)
        {
            battleTimeToStart -= Time.deltaTime;
            battleText.text = (int)battleTimeToStart + "seconds to start the team battle!";
            if (battleTimeToStart <= 0)
            {
                StopCountdownRingTeamBattle();
            }
        }
    }

    void StopCountdownRingTeamBattle()
    {
        cdToStartRingTeamBattle = false;
        battleText.text = "Fight!";

        //Teleport playeres out of the ring
        for(int i = 0; 0 <= allPlayers.Count; i++)
        {
            if (!playersInRing.Contains(allPlayers[i]))
            {
                allPlayers[i].transform.position = ringSpawnPoint.position;
            }
        }
        StartRingTeamBattle();
    }

    void StartRingTeamBattle()
    {
        startRingTeamBattle = true;
        fightTextShowingTime = 0;


    }

    void ProcessRingTeamBattle()
    {
        if (battleText.enabled)
        {
            fightTextShowingTime += Time.deltaTime;
            if (fightTextShowingTime >= fightTextshowingMaxTime)
            {
                battleText.enabled = false;
            }
        }

        int bluePlayersInRing = 0;
        int redPlayersInRing = 0;
        for (int i = 0; 0 <= allPlayers.Count; i++)
        {
            if (allPlayers[i].inWater)
            {
                if (playersInRing[i].team == Team.blue)
                    bluePlayersInRing++;
                if (playersInRing[i].team == Team.red)
                    redPlayersInRing++;
            }
        }
        if (bluePlayersInRing == playerNumBlue) FinishRingTeamBattle(Team.blue);
        if (redPlayersInRing == playerNumRed) FinishRingTeamBattle(Team.red);
    }

    void FinishRingTeamBattle(Team winner)
    {
        startRingTeamBattle = false;
        StartGameOver(winner);
    }

    void StartInflatingWalls()
    {
        for (int i = 0; i < inflatableWallsBefore.Length; i++)
        {
            inflatableWallsInflatingAlembic[i].SetActive(true);
            inflatableWallsBefore[i].SetActive(false);
            inflatableWallsAfter[i].SetActive(false);
        }
        inflatableWallsInflating = true;
        inflatableWallsInflatingTime = 0;
    }

    void InflatingWalls()
    {
        if (inflatableWallsInflating)
        {
            inflatableWallsInflatingTime += Time.deltaTime;
            if (inflatableWallsInflatingTime >= inflatableWallsInflatingAnimMaxTime)
            {
                StopInflatingWalls();
            }
        }
    }

    void StopInflatingWalls()
    {
        for (int i = 0; i < inflatableWallsBefore.Length; i++)
        {
            inflatableWallsInflatingAlembic[i].SetActive(false);
            inflatableWallsBefore[i].SetActive(false);
            inflatableWallsAfter[i].SetActive(true);
        }
        inflatableWallsInflating = false;
    }

    void StartBreakingWalls()
    {
        for (int i = 0; i < inflatableWallsBefore.Length; i++)
        {
            inflatableWallsBreakingAlembic[i].SetActive(true);
            inflatableWallsAfter[i].SetActive(false);
        }
        inflatableWallsBreaking = true;
        inflatableWallsBreakingTime = 0;
    }

    void BreakingWalls()
    {
        if (inflatableWallsBreaking)
        {
            inflatableWallsBreakingTime += Time.deltaTime;
            if (inflatableWallsBreakingTime >= inflatableWallsInflatingAnimMaxTime)
            {
                StopBreakingWalls();
            }
        }
    }

    void StopBreakingWalls()
    {
        for (int i = 0; i < inflatableWallsBefore.Length; i++)
        {
            inflatableWallsBreakingAlembic[i].SetActive(false);
            inflatableWallsPlatforms[i].SetActive(true);
        }
        inflatableWallsBreaking = false;
    }

    void StartShootingCannon()
    {
        startShootingCannon = true;
        cannonAnimTime = 0;
        cannon.SetActive(false);
        cannonShootingAembic.SetActive(true);
    }

    void ShootingCannon()
    {
        if (startShootingCannon)
        {
            cannonAnimTime += Time.deltaTime;
            if (cannonAnimTime >= cannonAnimMaxTime)
            {
                StopShootingCannon();
            }
        }
    }

    void StopShootingCannon()
    {
        startShootingCannon = false;
        cannon.SetActive(true);
        cannonShootingAembic.SetActive(false);
    }
    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----
    public void ProgressLane(int laneNumber)
    {
        switch (tutorialLanes[laneNumber].phase)
        {
            case TutorialPhase.StartPhase:
                InflateDummyAndWalls();
                break;
            case TutorialPhase.DummyPhase:
                DeflateDummyAndWalls();
                break;
            case TutorialPhase.WallJumpPhase:
                break;
            case TutorialPhase.GrapplePhase:
                break;
            case TutorialPhase.CannonPhase:
                StartRingTeamBattle();
                break;
            case TutorialPhase.RingBattlePhase:

                break;
        }
        tutorialLanes[laneNumber].ProgressPhase();

    }

    public void PlayerEnterRing(PlayerMovement player)
    {
        playersInRing.Add(player);
    }

    public void playerExitRing(PlayerMovement player)
    {
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
public class TutorialLane
{
    public int laneNumber;
    [HideInInspector]
    public TutorialPhase phase;
    [Tooltip("Add 1 entry for every phase, then add as many invisible walls as you want for each phase.")]
    public PhaseInvisibleWalls[] lanePhases;

    public TutorialLane(int _laneNumber = 0)
    {
        phase = TutorialPhase.StartPhase;
        laneNumber = _laneNumber;
    }

    public void ProgressPhase()
    {
        for (int i = 0; i < lanePhases[(int)phase].invisibleWalls.Length; i++)
        {
            lanePhases[(int)phase].invisibleWalls[i].SetActive(false);
        }
        phase++;
    }
}

[System.Serializable]
public class PhaseInvisibleWalls
{
    public GameObject[] invisibleWalls;
}
#endregion