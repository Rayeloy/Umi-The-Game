using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerCMF_Housing : GameControllerCMF
{
    [Header("--- HOUSING ---")]
    public bool showSlotMeshes = false;
    public Transform housingParent;
    public Transform housingFurnituresParent;
    public GameObject wallPrefab;
    public HousingHouseData houseMeta;
    public Vector3 houseSpawnPos = Vector3.zero;
    public bool showWallMeshes = false;
    public Material[] wallMaterials; // length = 2
    public HousingFurnitureData testFurniture;
    public HousingFurnitureData testSmallFurniture;
    public HousingFurnitureData testSmallFurniture2;
    public HousingFurnitureData testWallFurniture;
    public HousingFurnitureData testWallFurniture2;

    GameObject currentGridObject;
    HousingGrid currentGrid;

    //EDIT MODE
    bool editHouseOn = false;
    bool furnitureMenuOn = false;
    EloyAdvancedAxisControls myLeftJoyStickControls;
    EloyAdvancedButtonControls selectUp;
    EloyAdvancedButtonControls selectDown;
    EloyAdvancedButtonControls rotateClockwise;
    EloyAdvancedButtonControls rotateCounterClockwise;

    [Header(" - Edit Mode - ")]
    public HousingFurnitureMenu furnitureMenu;
    [Range(0, 1)]
    public float leftJoyStickDeadzone = 0.6f;
    public HousingEditModeCameraController editModeCameraController;
    float cameraHeightOffset = 0;
    public float cameraDistValue = 0.009f;

    protected override void SpecificAwake()
    {
        myLeftJoyStickControls = new EloyAdvancedAxisControls(allPlayers[0].actions.LeftJoystick, leftJoyStickDeadzone);
        selectUp = new EloyAdvancedButtonControls(allPlayers[0].actions.HousingMoveUp);
        selectDown = new EloyAdvancedButtonControls(allPlayers[0].actions.HousingMoveDown);
        rotateClockwise = new EloyAdvancedButtonControls(allPlayers[0].actions.HousingRotateFurnitureClockwise);
        rotateCounterClockwise = new EloyAdvancedButtonControls(allPlayers[0].actions.HousingRotateFurnitureCounterClockwise);

        currentGridObject = Instantiate(MasterManager.HousingSettings.gridPrefab, houseSpawnPos, Quaternion.identity, housingParent);
        currentGrid = currentGridObject.GetComponent<HousingGrid>();
        currentGrid.KonoAwake(houseMeta, housingFurnituresParent, wallPrefab, houseSpawnPos, MasterManager.HousingSettings.highlightedSlotMats, editModeCameraController);

        //Spawn House
        SpawnHouse(houseMeta);
        editModeCameraController.KonoAwake(currentGrid, houseSpawnPos);

        //FurnitureMenu
        furnitureMenu.KonoAwake();
    }

    protected override void SpecificUpdate()
    {
        //Camera Update
        if(editHouseOn) editModeCameraController.KonoUpdate();

        if (allPlayers[0].actions.Select.WasPressed) SwitchPlayAndHousingMode();


        if (editHouseOn)
        {
            if (allPlayers[0].actions.HousingSwitchFurnitureMenu.WasPressed) SwitchHousingFurnitureMenu();
            furnitureMenu.KonoUpdate();//Needs to be always running for animations

            if (furnitureMenuOn)
            {
            }
            else
            {
                currentGrid.KonoUpdate();

                //Moving Furniture
                if (myLeftJoyStickControls.LeftWasPressed)
                {
                    currentGrid.MoveSelectSlot(Direction.Left);
                }
                if (myLeftJoyStickControls.RightWasPressed)
                {
                    currentGrid.MoveSelectSlot(Direction.Right);
                }
                if (myLeftJoyStickControls.UpWasPressed)
                {
                    currentGrid.MoveSelectSlot(Direction.Up);
                }
                if (myLeftJoyStickControls.DownWasPressed)
                {
                    currentGrid.MoveSelectSlot(Direction.Down);
                }
                //Rotate Furniture
                if (rotateClockwise.WasPressed)
                {
                    if (!currentGrid.RotateFurniture(true)) Debug.LogError("GameControllerCMF_Housing: Can't rotate furniture clockwise!");
                }
                if (rotateCounterClockwise.WasPressed)
                {
                    if (!currentGrid.RotateFurniture(false)) Debug.LogError("GameControllerCMF_Housing: Can't rotate furniture counter clockwise!");
                }
                //Pick of Place Furniture
                if (allPlayers[0].actions.HousingPickFurniture.WasPressed)
                {
                    currentGrid.PickOrPlace();
                }

                //Test Spawn Furniture
                if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    HousingFurniture aux;
                    currentGrid.SpawnFurniture(testFurniture, out aux);
                }
                if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    HousingFurniture aux;
                    currentGrid.SpawnFurniture(testSmallFurniture, out aux);
                }
                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    HousingFurniture aux;
                    currentGrid.SpawnFurniture(testSmallFurniture2, out aux);
                }
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    HousingFurniture aux;
                    currentGrid.SpawnFurniture(testWallFurniture, out aux);
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    HousingFurniture aux;
                    currentGrid.SpawnFurniture(testWallFurniture2, out aux);
                }
            }
        }

        //IMPORTANT
        myLeftJoyStickControls.ResetJoyStick();
        selectUp.ResetButton();
        selectDown.ResetButton();
        rotateClockwise.ResetButton();
        rotateCounterClockwise.ResetButton();
    }

    protected override void SpecificLateUpdate()
    {
        if (editHouseOn)
        {
            if (!furnitureMenuOn)
            {
                //move camera
                editModeCameraController.KonoLateUpdate();
            }
        }
    }

    void SpawnHouse(HousingHouseData houseMeta)
    {
        if (currentGrid != null)
        {
            currentGrid.CreateGrid(showSlotMeshes);
            currentGrid.CreateWalls(showWallMeshes, wallMaterials);
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
            else
            {
                Debug.LogError("Can't create door");
            }
            currentGrid.StopHighlightPlacedFurniture();
        }
    }

    void SwitchPlayAndHousingMode()
    {
        if (!editHouseOn) //ACTIVATE EDIT MODE
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
            cameraHeightOffset = ((currentGrid.myHouseMeta.height / 2) + 1) * MasterManager.HousingSettings.slotSize;
            Vector3 cameraBaseCenterPos = currentGrid.worldCenter + Vector3.up * cameraHeightOffset;
            //Vector3 houseFloorCenter = new Vector3(cameraBaseCenterPos.x, houseSpawnPos.y, cameraBaseCenterPos.z + (currentGrid.myHouseMeta.depth / 3 * MasterManager.HousingSettings.slotSize));
            float volume = (currentGrid.myHouseMeta.width * MasterManager.HousingSettings.slotSize )* (currentGrid.myHouseMeta.depth * MasterManager.HousingSettings.slotSize) *
                (currentGrid.myHouseMeta.height * MasterManager.HousingSettings.slotSize);
            float cameraMaxZoomDist = volume * cameraDistValue;
            Debug.Log(" cameraBaseCenterPos = " + cameraBaseCenterPos.ToString("F4") + "; currentGrid.worldCenter = "+ currentGrid.worldCenter.ToString("F4")+
                "; cameraHeightOffset = " + cameraHeightOffset.ToString("F4") + "; cameraMaxZoomDist = " + cameraMaxZoomDist.ToString("F4"));
            editModeCameraController.Activate(cameraBaseCenterPos, cameraMaxZoomDist);

            //Highlight center Slot
            currentGrid.stickToWall = false;
            HousingGridCoordinates coord = new HousingGridCoordinates(0, currentGrid.myHouseMeta.depth / 2, currentGrid.myHouseMeta.width / 2);
            currentGrid.SelectSlotAt(coord);
        }
        else // DEACTIVE EDIT MODE
        {
            //Save room disposition to Database
            //Spawn player at door
            ActivateHostPlayer();

            //Deactivate Edit Camera
            editModeCameraController.DeActivate();

            //stop hightlight current slot
            currentGrid.StopHighlightSlot(currentGrid.currentSlotCoord);
        }
        editHouseOn = !editHouseOn;
        playing = !playing;
        //Call Interface
    }

    void SwitchHousingFurnitureMenu()
    {
        if (editHouseOn)
        {
            furnitureMenuOn = !furnitureMenuOn;
            if (furnitureMenuOn)
            {
                furnitureMenu.OpenFurnitureMenu();
            }
            else
            {
                furnitureMenu.CloseFurnitureMenu();
            }
        }
    }

    void DeactivateHostPlayer()
    {
        allPlayers[0].transform.position = new Vector3(-200, -200, -200);
        allPlayers[0].GetComponent<Rigidbody>().velocity = Vector3.zero;
        allPlayers[0].myCamera.myCamera.GetComponent<Camera>().enabled = false;
        AudioListener audioListener = allPlayers[0].myCamera.myCamera.GetComponent<AudioListener>();
        if (audioListener != null) audioListener.enabled = false;
    }

    void ActivateHostPlayer()
    {
        RespawnPlayer(allPlayers[0]);
        allPlayers[0].myCamera.myCamera.GetComponent<Camera>().enabled = true;
        AudioListener audioListener = allPlayers[0].myCamera.myCamera.GetComponent<AudioListener>();
        if (audioListener != null) audioListener.enabled = true;
    }

    //INTERFACE


}
