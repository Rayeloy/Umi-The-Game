using Bolt.Matchmaking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineGameController: Bolt.GlobalEventListener
{
    [Header(" --- Player components prefabs ---")]
    public GameObject playerCanvasPrefab;
    public GameObject playerCameraPrefab;
    public GameObject playerUICameraPrefab;

    [Header(" --- Game Controller ---")]
    public GameControllerCMF gameController;

    public override void SceneLoadLocalDone(string Scenename)
    {
        PlayerMovementCMF newPlayer;
        GameObject newPlayerCanvas;
        CameraControllerCMF newPlayerCamera;
        Camera newPlayerUICamera;
        newPlayer = BoltNetwork.Instantiate(BoltPrefabs.PlayerPrefCMF_actual_online).GetComponent<PlayerMovementCMF>() ;
        newPlayer.mySpawnInfo = new PlayerSpawnInfo();
        newPlayerCanvas = Instantiate(playerCanvasPrefab, gameController.playersCanvasParent);
        newPlayerCamera = Instantiate(playerCameraPrefab, gameController.playersCamerasParent).GetComponent<CameraControllerCMF>();
        newPlayerUICamera = Instantiate(playerUICameraPrefab, newPlayerCamera.myCamera).GetComponent<Camera>();

        InitializePlayerReferences(newPlayer, newPlayerCanvas, newPlayerCamera, newPlayerUICamera);
        StartGame(newPlayer);
    }

    void InitializePlayerReferences(PlayerMovementCMF player, GameObject canvas, CameraControllerCMF cameraBase, Camera UICamera)
    {
        //Inicializar referencias
        PlayerHUDCMF playerHUD = canvas.GetComponent<PlayerHUDCMF>();
        //Player
        player.myCamera = cameraBase;
        player.myPlayerHUD = playerHUD;
        player.myUICamera = UICamera;
        //player.myPlayerCombat.attackNameText = playerHUD.attackNameText;
        //Canvas
        playerHUD.myCamera = cameraBase.myCamera.GetComponent<Camera>();//newPlayerUICamera;
        playerHUD.myUICamera = UICamera;//newPlayerUICamera;
        playerHUD.myPlayerMov = player;
        playerHUD.myPlayerCombat = player.transform.GetComponent<PlayerCombatCMF>();
        canvas.GetComponent<Canvas>().worldCamera = UICamera;
        //CameraBase
        cameraBase.myPlayerMov = player;
        cameraBase.myPlayer = player.transform;
        cameraBase.cameraFollowObj = player.cameraFollow;
    }

    void StartGame(PlayerMovementCMF newPlayer)
    {
        newPlayer.KonoStart();
        newPlayer.SetVelocity(Vector3.zero);
        newPlayer.myCamera.InstantPositioning();
        newPlayer.myCamera.InstantRotation();
        newPlayer.ResetPlayer();
        newPlayer.myPlayerAnimation.RestartAnimation();
        gameController.playing = true;
        gameController.gamePaused = false;
    }

}
