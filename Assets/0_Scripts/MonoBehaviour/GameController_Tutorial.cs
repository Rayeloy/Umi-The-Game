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

    #region ----[ VARIABLES ]----
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
                CheckStartRingTeamBattle();
                CountdownRingTeamBattle();
                ProcessRingTeamBattle();
            }
        }
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----

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
        if (bluePlayersInRing == playerNumBlue &&  playerNumBlue>0) start = true;
        if (redPlayersInRing == playerNumRed && playerNumRed>0) start = true;
        if (start)
        {
            StartCountdownRingTeamBattle();
        }
    }

    void StartCountdownRingTeamBattle()
    {
        print("START COUNTDOWN RING TEAM BATTLE");
        battleTimeToStart = battleMaxTimeToStart;
        cdToStartRingTeamBattle = true;
        battleText.gameObject.SetActive(true);
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
        battleText.text = "FIGHT!";

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
        if (startRingTeamBattle)
        {
            if (battleText.enabled)
            {
                fightTextShowingTime += Time.deltaTime;
                if (fightTextShowingTime >= fightTextshowingMaxTime)
                {
                    battleText.gameObject.SetActive(false);
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
    }

    void FinishRingTeamBattle(Team winner)
    {
        startRingTeamBattle = false;
        StartGameOver(winner);
    }

    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----
    public void ProgressLane(int laneNumber)
    {
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
    public Dummy dummy;
    public InflatableWall[] inflatableWalls = new InflatableWall[2];
    [Header(" --- Shitty Animations --- ")]
    [Header("Cannon")]
    public GameObject cannon;
    public GameObject cannonShootingAlembic;
    public float cannonAnimMaxTime;
    float cannonAnimTime = 0;
    bool startShootingCannon = false;



    public TutorialLane(int _laneNumber = 0)
    {
        phase = TutorialPhase.StartPhase;
        laneNumber = _laneNumber;
        cannon.SetActive(true);
        cannonShootingAlembic.SetActive(false);
        for (int i = 0; i < inflatableWalls.Length; i++)
        {
            inflatableWalls[i].KonoAwake();
        }
    }

    public void Awake()
    {
        cannon.SetActive(true);
        cannonShootingAlembic.SetActive(false);
        for(int i = 0; i < inflatableWalls.Length; i++)
        {
            inflatableWalls[i].KonoAwake();
        }
    }

    public void Update()
    {
        for (int i = 0; i < inflatableWalls.Length; i++)
        {
            inflatableWalls[i].KonoUpdate();
        }
    }

    public void ProgressPhase()
    {/*
        for (int i = 0; i < lanePhases[(int)phase].invisibleWalls.Length; i++)
        {
            lanePhases[(int)phase].invisibleWalls[i].SetActive(false);
        }
        */
        phase++;
    }

    public void InflateDummyAndWalls()
    {
        Debug.Log("INFLATE DUMMY AND WALLS");
        dummy.StartInflatingDummy();
        //Inflate walls animation
        for (int i = 0; i < inflatableWalls.Length; i++)
        {
            inflatableWalls[i].StartInflatingWall();
        }
    }

    public void DeflateDummyAndWalls()
    {
        dummy.StartDeflateDummy();
        //Inflate walls animation
        for (int i = 0; i < inflatableWalls.Length; i++)
        {
            inflatableWalls[i].StartBreakingWall();
        }
    }

    public void StartShootingCannon()
    {
        startShootingCannon = true;
        cannonAnimTime = 0;
        cannon.SetActive(false);
        cannonShootingAlembic.SetActive(true);
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
        cannonShootingAlembic.SetActive(false);
    }

}

[System.Serializable]
public class PhaseInvisibleWalls
{
    public GameObject[] invisibleWalls;
}
#endregion