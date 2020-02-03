using Crest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyCMF : MonoBehaviour
{
    public bool debugAllRaycasts = false;

    public PlayerMovementCMF myPlayerMov;
    PlayerWeaponsCMF myPlayerWeapons;

    //OCEAN RENDERER FOR FLOATING
    [Header("Ocean Renderer")]
    public float bodyWidth = 4;
    [Tooltip("Offsets center of object to raise it (or lower it) in the water."), SerializeField]
    float raiseBody = -2f;
    public float buoyancyCoeff =4.5f;
    public float dragInWaterUp = 9f;
    [HideInInspector]
    public float waterSurfaceHeight = 0;
    Vector3 displacementToObject = Vector3.zero;

    SamplingData samplingData = new SamplingData();
    SamplingData samplingDataFlow = new SamplingData();

    [HideInInspector]
    public Vector3 buoyancy;
    [HideInInspector]
    public Vector3 verticalDrag;


    public void KonoAwake()
    {
        myPlayerWeapons = myPlayerMov.myPlayerWeap;
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

    #region Enter Water / Exit Water
    void ProcessInWater()
    {
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

            var thisRect = new Rect(transform.position.x, transform.position.z, 0f, 0f);
            if (!collProvider.GetSamplingData(ref thisRect, bodyWidth, samplingData))
            {
                // No collision coverage for the sample area, in this case use the null provider.
                collProvider = CollProviderNull.Instance;
            }

            if (debugAllRaycasts)
            {
                var result = collProvider.CheckAvailability(ref position, samplingData);
                if (result != AvailabilityResult.DataAvailable)
                {
                    Debug.LogWarning("Validation failed: " + result.ToString() + ". See comments on the AvailabilityResult enum.", this);
                }
            }

            Vector3 undispPos;
            if (!collProvider.ComputeUndisplacedPosition(ref position, samplingData, out undispPos))
            {
                // If we couldn't get wave shape, assume flat water at sea level
                undispPos = position;
                undispPos.y = OceanRenderer.Instance.SeaLevel;
            }
            if (!myPlayerMov.disableAllDebugs) DebugDrawCross(undispPos, 1f, Color.red);

            Vector3 waterSurfaceVel, displacement;
            bool dispValid, velValid;
            collProvider.SampleDisplacementVel(ref undispPos, samplingData, out displacement, out dispValid, out waterSurfaceVel, out velValid);
            if (dispValid)
            {
                displacementToObject = displacement;
            }

            if (GPUReadbackFlow.Instance)
            {
                GPUReadbackFlow.Instance.ProcessRequests();

                var flowRect = new Rect(position.x, position.z, 0f, 0f);
                GPUReadbackFlow.Instance.GetSamplingData(ref flowRect, bodyWidth, samplingDataFlow);
                
                Vector2 surfaceFlow;
                GPUReadbackFlow.Instance.SampleFlow(ref position, samplingDataFlow, out surfaceFlow);
                waterSurfaceVel += new Vector3(surfaceFlow.x, 0, surfaceFlow.y);

                GPUReadbackFlow.Instance.ReturnSamplingData(samplingDataFlow);
            }

            if (debugAllRaycasts)
            {
                Debug.DrawLine(transform.position + 5f * Vector3.up, transform.position + 5f * Vector3.up + waterSurfaceVel,
                    new Color(1, 1, 1, 0.6f));
            }

            var velocityRelativeToWater = myPlayerMov.currentVel - waterSurfaceVel;

            var dispPos = undispPos + displacementToObject;
            if (debugAllRaycasts) DebugDrawCross(dispPos, 4f, Color.white);

            float waterHeight = dispPos.y;
            float stayInWaterOffter = myPlayerMov.vertMovSt == VerticalMovementState.FloatingInWater ?0.5f:0;
            float waterLevelDif = waterHeight - transform.position.y + stayInWaterOffter;
            Vector3 playerPos = transform.position + (Vector3.down * raiseBody);
            bool inWater = waterLevelDif > 0f;
            Debug.DrawLine(playerPos, dispPos, inWater ? Color.green:Color.red);
            VerticalMovementState vertState = myPlayerMov.vertMovSt;
            if (vertState != VerticalMovementState.FloatingInWater && myPlayerMov.currentVel.y<=0 && inWater)
            {
                myPlayerMov.EnterWater();
            }
            else if (myPlayerMov.vertMovSt == VerticalMovementState.FloatingInWater && myPlayerMov.vertMovSt != VerticalMovementState.Jumping &&!inWater)//TO CHANGE
            {
                myPlayerMov.ExitWater();
            }
            if (inWater)
            {
                float bottomDepth = waterHeight - transform.position.y + raiseBody;
                buoyancy = Vector3.up * buoyancyCoeff * bottomDepth * bottomDepth * bottomDepth;
                // apply drag relative to water
                verticalDrag = Vector3.up * Vector3.Dot(Vector3.up, -velocityRelativeToWater) * dragInWaterUp;
            }
            else
            {
                buoyancy = Vector3.zero;
                verticalDrag = Vector3.zero;
            }

            waterSurfaceHeight = waterHeight;
            collProvider.ReturnSamplingData(samplingData);
        }
    }
    #endregion

    void DebugDrawCross(Vector3 pos, float r, Color col)
    {
        Debug.DrawLine(pos - Vector3.up * r, pos + Vector3.up * r, col);
        Debug.DrawLine(pos - Vector3.right * r, pos + Vector3.right * r, col);
        Debug.DrawLine(pos - Vector3.forward * r, pos + Vector3.forward * r, col);
    }
}
