using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ----[ PUBLIC ENUMS ]----

#endregion

public class PlayerVFXCMF : MonoBehaviour
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    public bool debugModeOn = false;
    //Referencias
    public PlayerMovementCMF myPlayerMovement;

    public effect[] effects;
    public TrailRenderer dashTrail;
    [HideInInspector]
    public TrailRenderer[] weaponTrailRenderers;
    #endregion

    #region ----[ PROPERTIES ]----
    #endregion

    #region ----[ VARIABLES ]----
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    public void KonoAwake()
    {
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].KonoAwake();
        }
    }
    #endregion

    #region Start
    public void KonoStart()
    {
        dashTrail.emitting = false;
        if(debugModeOn) Debug.Log("My weapon skin go = " + myPlayerMovement.myPlayerWeap.currentWeaponSkin.gameObject);
        weaponTrailRenderers = myPlayerMovement.myPlayerWeap.currentWeaponSkin.trailRenderers;
        for (int i = 0; i < weaponTrailRenderers.Length; i++)
        {
            weaponTrailRenderers[i].emitting = false;
        }
    }
    #endregion

    #region Update
    //private void LateUpdate()
    //{
    //    for (int i = 0; i < transform.childCount; i++)
    //    {
    //        transform.GetChild(i).rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.z * -1.0f);
    //    }
    //}
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----

    public void ActivateEffect(PlayerVFXType effectType)
    {
        switch (effectType)
        {
            case PlayerVFXType.DashTrail:
                dashTrail.emitting = true;
                break;
            default:
                for (int i = 0; i < effects.Length; i++)
                {
                    if (effects[i].effectType == effectType)
                    {
                        effects[i].Activate();
                    }
                }
                break;
        }
    }

    public void DeactivateEffect(PlayerVFXType effectType)
    {
        switch (effectType)
        {
            case PlayerVFXType.DashTrail:
                dashTrail.emitting = false;
                break;
            default:
                for (int i = 0; i < effects.Length; i++)
                {
                    if (effects[i].effectType == effectType)
                    {
                        effects[i].Deactivate();
                    }
                }
                break;
        }
    }

    public GameObject GetEffectGO(PlayerVFXType effectType)
    {
        switch (effectType)
        {
            case PlayerVFXType.DashTrail:
                return dashTrail.gameObject;

            default:
                for (int i = 0; i < effects.Length; i++)
                {
                    if (effects[i].effectType == effectType)
                    {
                        return effects[i].effectPrefab;
                    }
                }
                break;
        }

        return null;
    }

    public void ActivateWeaponTrails()
    {
        for (int i = 0; i < weaponTrailRenderers.Length; i++)
        {
            weaponTrailRenderers[i].emitting = true;
        }
    }

    public void DeactivateWeaponTrails()
    {
        for (int i = 0; i < weaponTrailRenderers.Length; i++)
        {
            weaponTrailRenderers[i].emitting = false;
        }
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


#endregion


