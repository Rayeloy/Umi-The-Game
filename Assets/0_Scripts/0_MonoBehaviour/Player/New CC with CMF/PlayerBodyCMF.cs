using Crest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyCMF : MonoBehaviour
{
    public bool debugAllRaycasts = false;

    public PlayerMovementCMF myPlayerMov;
    PlayerWeaponsCMF myPlayerWeapons;

    [HideInInspector] public PlayerSkinData myPlayerSkin;
    Animator myAnimator;

    //OCEAN RENDERER FOR FLOATING
    [Header("Ocean Renderer")]
    public float bodyWidth = 4;
    [Tooltip("Offsets center of object to raise it (or lower it) in the water."), SerializeField]
    float raiseBody = -2f;
    public float buoyancyCoeff =4.5f;
    public float maxBuoyancyAcceleration = 20;
    public float dragInWaterUp = 9f;
    public float floatingSlack = 0.7f;
    [HideInInspector]
    public float waterSurfaceHeight = 0;
    [HideInInspector] public Vector3 displacementToObject = Vector3.zero;

    SampleHeightHelper _sampleHeightHelper = new SampleHeightHelper();
    SampleFlowHelper _sampleFlowHelper = new SampleFlowHelper();

    [HideInInspector]
    public Vector3 buoyancy;
    [HideInInspector]
    public Vector3 verticalDrag;


    public void KonoAwake()
    {
        myPlayerWeapons = myPlayerMov.myPlayerWeap;

        Debug.Log("Player Body: Load Player Skin");
        GameObject myBody = Instantiate(myPlayerSkin.skinRecolorPrefabs[(int)myPlayerMov.team], transform);
        myBody.transform.localPosition = new Vector3(0, -0.954f, 0);
        myBody.AddComponent<Animator>();
        myAnimator = myBody.GetComponent<Animator>();
        myAnimator.runtimeAnimatorController = myPlayerSkin.animatorController;
        myAnimator.avatar = myPlayerSkin.avatar;
        myPlayerMov.myPlayerModel = myBody.GetComponent<PlayerModel>();
    }

    public void KonoFixedUpdate()
    {
        ProcessInWater();
    }

    #region  TRIGGER COLLISIONS ---------------------------------------------
    private void OnTriggerStay(Collider col)
    {
        //Debug.Log("Player Body OnTriggerStay: " + col.name);
        switch (col.tag)
        {
            case "Water":
                float waterSurface = col.GetComponent<Collider>().bounds.max.y;
                if (transform.position.y <= waterSurface)
                {
                    myPlayerMov.EnterWater();
                }
                else
                {
                    myPlayerMov.ExitWater();
                }
                break;
            case "Flag":
                col.GetComponent<FlagCMF>().PickupFlag(myPlayerMov);
                break;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log("Player Body Colliding with " + col.transform.name);
        switch (col.tag)
        {
            case "KillTrigger":
                // Debug.LogError("PLAYER DEATH");
                myPlayerMov.Die();
                break;
            case "FlagHome":
                //print("I'm " + name + " and I touched a respawn");
                myPlayerMov.CheckScorePoint(col.GetComponent<FlagHome>());
                break;
            //case "PickUp":
            //    myPlayerMov.myPlayerPickups.CogerPickup(col.gameObject);
            //    break;
            case "WeaponPickup":
                myPlayerWeapons.AddWeaponNearby(col.GetComponent<Weapon>());
                break;
            case "Player":
                if (myPlayerMov.collCheck.collideWithTriggers)
                {
                    Debug.LogWarning("Hitting player! checking team");
                    PlayerMovement otherPlayer = col.transform.GetComponentInParent<PlayerMovement>();
                    if (otherPlayer != null && myPlayerMov.team != otherPlayer.team)
                    {
                        if (myPlayerMov.myPlayerHook.enemyHooked && myPlayerMov.myPlayerHook.enemy == otherPlayer)
                        {
                            Debug.LogError("Player hooked stopped due to colliding with the player hooking him.");
                            myPlayerMov.myPlayerHook.FinishHook();
                        }
                        else
                        {
                            Debug.LogError("Player bodies collided but they were not in the middle of a hook between them.");
                        }
                    }
                }
                break;
            case "UmiCannon":
                UmiCannon cannon = col.GetComponent<UmiCannon>();
                Vector3 dir = cannon.CalculateVelocity(transform.position, myPlayerMov.currentGravity);
                myPlayerMov.StartFixedJump(dir, cannon.timeToReach * cannon.noInputPercentage, cannon.timeToReach, cannon.bounceEnabled);
                cannon.PlayEffects();
                break;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        switch (col.tag)
        {
            case "WeaponPickup":
                myPlayerWeapons.RemoveWeaponNearby(col.GetComponent<Weapon>());
                break;
        }
    }

    #endregion

    #region FLOATING SYSTEM FOR CREST OCEAN
    void ProcessInWater()
    {
        UnityEngine.Profiling.Profiler.BeginSample("UmiPlayerBody.FixedUpdate");

        if (OceanRenderer.Instance != null)
        {
            // Trigger processing of displacement textures that have come back this frame. This will be processed
            // anyway in Update(), but FixedUpdate() is earlier so make sure it's up to date now.
            if (OceanRenderer.Instance._simSettingsAnimatedWaves.CollisionSource == SimSettingsAnimatedWaves.CollisionSources.OceanDisplacementTexturesGPU && GPUReadbackDisps.Instance)
            {
                GPUReadbackDisps.Instance.ProcessRequests();
            }

            var collProvider = OceanRenderer.Instance.CollisionProvider;
            var position = transform.position;

            var normal = Vector3.up; var waterSurfaceVel = Vector3.zero;
            _sampleHeightHelper.Init(transform.position, bodyWidth);
            _sampleHeightHelper.Sample(ref displacementToObject, ref normal, ref waterSurfaceVel);

            var undispPos = transform.position - displacementToObject;
            undispPos.y = OceanRenderer.Instance.SeaLevel;

            if (debugAllRaycasts) VisualiseCollisionArea.DebugDrawCross(undispPos, 1f, Color.red);

            if (QueryFlow.Instance)
            {
                _sampleFlowHelper.Init(transform.position, bodyWidth);

                Vector2 surfaceFlow = Vector2.zero;
                _sampleFlowHelper.Sample(ref surfaceFlow);
                waterSurfaceVel += new Vector3(surfaceFlow.x, 0, surfaceFlow.y);
            }

            var velocityRelativeToWater = myPlayerMov.currentVel - waterSurfaceVel;

            var dispPos = undispPos + displacementToObject;
            if (debugAllRaycasts) VisualiseCollisionArea.DebugDrawCross(dispPos, 4f, Color.white);

            float waterHeight = dispPos.y;
            float stayInWaterOffset = myPlayerMov.vertMovSt == VerticalMovementState.FloatingInWater ? floatingSlack:0;
            float waterLevelDif = waterHeight - transform.position.y + stayInWaterOffset;
            Vector3 playerPos = transform.position + (Vector3.down * raiseBody);
            bool inWater = waterLevelDif > 0f;
            Debug.DrawLine(playerPos, dispPos, inWater ? Color.green:Color.red);
            VerticalMovementState vertState = myPlayerMov.vertMovSt;
            if (vertState != VerticalMovementState.FloatingInWater && myPlayerMov.currentVel.y<=0 && inWater)
            {
                myPlayerMov.EnterWater();
            }
            else if (myPlayerMov.vertMovSt == VerticalMovementState.FloatingInWater && !inWater)//TO CHANGE
            {
                myPlayerMov.ExitWater();
            }
            if (inWater)
            {
                float bottomDepth = waterHeight - transform.position.y + raiseBody;
                buoyancy = Vector3.up * buoyancyCoeff * bottomDepth * bottomDepth * bottomDepth;
                buoyancy.y = Mathf.Clamp(buoyancy.y, float.MinValue, maxBuoyancyAcceleration);
                // apply drag relative to water
                verticalDrag = Vector3.up * Vector3.Dot(Vector3.up, -velocityRelativeToWater) * dragInWaterUp;
            }
            else
            {
                buoyancy = Vector3.zero;
                verticalDrag = Vector3.zero;
            }

            waterSurfaceHeight = waterHeight;
            UnityEngine.Profiling.Profiler.EndSample();
        }
        else
        {
            UnityEngine.Profiling.Profiler.EndSample();

        }
    }
    #endregion
}
