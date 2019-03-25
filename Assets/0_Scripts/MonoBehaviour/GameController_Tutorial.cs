using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ----[ PUBLIC ENUMS ]----
public enum TutorialPhase
{
    StartPhase=0,
    DummyPhase=1,
    WallJumpPhase=2,
    GrapplePhase=3,
    CannonPhase=4,
    RingBattlePhase=5
}
#endregion
public class GameController_Tutorial : GameControllerBase
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    public TutorialLane[] tutorialLanes;
    [Header("StartPhase")]
    public Dummy dummy;
    public GameObject inflatableWall1;
    public GameObject inflatableWall2;
    [Header("DummyPhase")]
    public GameObject inflatablePlatform1;
    public GameObject inflatablePlatform2;

    #endregion

    #region ----[ PROPERTIES ]----

    #endregion

    #region ----[ VARIABLES ]----
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    protected override void SpecificAwake()
    {
        tutorialLanes = new TutorialLane[2];
    }
    #endregion

    #region Start
    #endregion

    #region Update
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    void InflateDummyAndWalls()
    {
        dummy.InflateDummy();
        //Inflate walls animation
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
                break;
            case TutorialPhase.WallJumpPhase:
                break;
            case TutorialPhase.GrapplePhase:
                break;
            case TutorialPhase.CannonPhase:
                break;
            case TutorialPhase.RingBattlePhase:
                break;
        }
        tutorialLanes[laneNumber].ProgressPhase();

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
    public int lane;
    [HideInInspector]
    public TutorialPhase phase;
    [Tooltip("Add 1 entry for every phase, then add as many invisible walls as you want for each phase.")]
    public PhaseInvisibleWalls[] laneInvisibleWalls;

    public TutorialLane(int _laneNumber=0)
    {
        phase = TutorialPhase.StartPhase;
        lane = _laneNumber;
    }

    public void ProgressPhase()
    {
        for (int i = 0; i < laneInvisibleWalls[(int)phase].invisibleWalls.Length; i++)
        {
            laneInvisibleWalls[(int)phase].invisibleWalls[i].SetActive(false);
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