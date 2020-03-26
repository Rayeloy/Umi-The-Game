using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLane : MonoBehaviour
{
    public int laneNumber;
    [HideInInspector]
    public TutorialPhase phase;
    [Tooltip("Add 1 set for every phase, then add as many invisible walls as you want for each phase.")]
    public PhaseInvisibleWalls[] invisibleWallsSets;
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
        for (int i = 0; i < inflatableWalls.Length; i++)
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
        //dummy.StartInflatingDummy();
        //Inflate walls animation
        for (int i = 0; i < inflatableWalls.Length; i++)
        {
            inflatableWalls[i].StartInflatingWall();
        }
    }

    public void DeflateDummyAndWalls()
    {
        //dummy.StartDeflateDummy();
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
