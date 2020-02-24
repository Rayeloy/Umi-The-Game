using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerCMF_Housing : GameControllerCMF
{
    [Header("--- HOUSING ---")]
    public bool showSlotMeshes = false;
    public Transform housingParent;
    public GameObject housingGridPrefab;
    public GameObject housingSlotPrefab;
    public GameObject housingWall;
    public HousingHouseData houseMeta;
    public Vector3 houseSpawnPos = Vector3.zero;
    GameObject currentGridObject;
    HousingGrid currentGrid;
    HousingGridCoordinates currentGridCoord;

    //EDIT MODE
    bool editHouseOn = false;
    [Header(" - Edit Mode - ")]
    public HousingEditModeCameraController editModeCameraBase;
    public float cameraHeightOffset = 0;



    protected override void SpecificAwake()
    {
        currentGridCoord = new HousingGridCoordinates();

        currentGridObject = Instantiate(housingGridPrefab, houseSpawnPos, Quaternion.identity, housingParent);
        currentGrid = currentGridObject.GetComponent<HousingGrid>();
        currentGrid.KonoAwake(houseMeta, housingSlotPrefab, housingWall, houseSpawnPos);
        //Spawn House
        SpawnHouse(houseMeta);
    }

    protected override void SpecificUpdate()
    {
        if (allPlayers[0].actions.Select.WasPressed)
        {
            SwitchPlayAndHousingMode();
        }

        if (editHouseOn)
        {
            //move camera
            editModeCameraBase.RotateCamera();
        }
    }

    void SpawnHouse(HousingHouseData houseMeta)
    {
        if(currentGrid != null)
        {
            currentGrid.CreateGrid(showSlotMeshes);
            currentGrid.CreateWalls();
            Vector3 playerSpawnPos; Quaternion playerSpawnRot;
            if (currentGrid.CreateDoor(out playerSpawnPos, out playerSpawnRot))//Change spawn to the result pos of this, and tp players to here.)
            {
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    allPlayers[i].mySpawnInfo.position = playerSpawnPos;
                    allPlayers[i].mySpawnInfo.rotation = playerSpawnRot;
                }
                Debug.Log(" playerSpawnPos = " + playerSpawnPos + "; playerSpawnRot = " + playerSpawnRot);
            }
        }
    }

    void SwitchPlayAndHousingMode()
    {
        if (!editHouseOn)
        {
            //FOR ONLINE:
            /*if(people in room>1)//can only edit room if alone in it.
             * {
             * set room to private
             * return; 
            }*/

            //set player away (invisible)
            DeactivateHostPlayer();

            //Set new Edit Camera
            editModeCameraBase.Activate(currentGrid.center + Vector3.up * cameraHeightOffset);
        }
        else
        {
            //Save room disposition to Database
            //Spawn player at door
            ActivateHostPlayer();

            //Deactivate Edit Camera
            editModeCameraBase.DeActivate();
        }
        editHouseOn = !editHouseOn;
        playing = !playing;
        //Call Interface
    }

    void DeactivateHostPlayer()
    {
        allPlayers[0].transform.position = new Vector3(-200, -200, -200);
        allPlayers[0].myCamera.myCamera.GetComponent<Camera>().enabled = false;
    }

    void ActivateHostPlayer()
    {
        RespawnPlayer(allPlayers[0]);
        allPlayers[0].myCamera.myCamera.GetComponent<Camera>().enabled = true;
    }
}
