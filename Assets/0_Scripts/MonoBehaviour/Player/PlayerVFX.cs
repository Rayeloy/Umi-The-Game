using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ----[ PUBLIC ENUMS ]----
public enum PlayerVFXType
{
    None,
    SwimmingEffect,
    WaterSplash
}
#endregion

public class PlayerVFX : MonoBehaviour
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    public PlayerMovement myPlayerMovement;

    public effect[] effects;
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
    #endregion

    #region Update
    private void LateUpdate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.z * -1.0f);
        }
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----

    public void ActivateEffect(PlayerVFXType effectType)
    {
        for(int i=0; i < effects.Length; i++)
        {
            if(effects[i].effectType == effectType)
            {
                effects[i].Activate();
            }
        }
    }

    public void DeactivateEffect(PlayerVFXType effectType)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].effectType == effectType)
            {
                effects[i].Deactivate();
            }
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
[System.Serializable]
public class effect
{
    public PlayerVFXType effectType;
    public GameObject effectPrefab;
    public bool playOnAwake;
    [Tooltip("If you want the animation to wait for it to stop to be able to start the animation set this to false")]
    public bool forceStart = true;
    [HideInInspector]
    public ParticleSystem effectParticleSystem;

    public effect(PlayerVFXType _effectType = PlayerVFXType.None, bool _playOnAwake = false, bool _forceStart = true, GameObject _effectPrefab = null)
    {
        effectType = _effectType;
        effectPrefab = _effectPrefab;
        playOnAwake = _playOnAwake;
        forceStart = _forceStart;
        effectParticleSystem = effectPrefab.GetComponent<ParticleSystem>();
    }

    public void KonoAwake()
    {
        effectParticleSystem= effectPrefab.GetComponent<ParticleSystem>();
        if (playOnAwake)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    public void Activate()
    {
        if (forceStart || (!forceStart && !effectParticleSystem.isPlaying))
        {
            Debug.Log("Activated effect " +effectType+ "; effectParticleSystem = "+ effectParticleSystem.name+ "; effectParticleSystem.isPlaying = "+ effectParticleSystem.isPlaying);
            effectParticleSystem.Play();
        }
    }

    public void Deactivate()
    {
        if (effectParticleSystem.isPlaying)
        {
            effectParticleSystem.Stop();
        }
    }
}
#endregion


