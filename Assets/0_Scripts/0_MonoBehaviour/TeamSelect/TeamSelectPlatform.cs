using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelectPlatform : MonoBehaviour
{
    public Transform rotationParent;//ROTATE THIS
    public GameObject teamSelectionModels;
    public GameObject characterSelectionModels;
    public GameObject weaponSelectionModels;

    TeamSelectPlayerModel[] charSelectPlayerModels;
    TeamSelectPlayerModel[] weaponSelectPlayerModels;


    //Platform Rotation
    bool platformRotStarted = false;
    //float platformRotInitialRot;
    float platformRotCurrentRot;
    float platformRotTime = 0;
    float platformRotTargetRot = 0;
    //float platformRotRealTargetRot = 0;
    public float platformRotMaxTime = 0.3f;


    private void Awake()
    {
        charSelectPlayerModels = characterSelectionModels.GetComponentsInChildren<TeamSelectPlayerModel>();
        weaponSelectPlayerModels = weaponSelectionModels.GetComponentsInChildren<TeamSelectPlayerModel>();
        platformRotTargetRot = platformRotCurrentRot = rotationParent.localRotation.eulerAngles.y;
    }

    public void Update()
    {
        ProcessPlatformRotation();
    }

    public void StartTeamSelection()
    {
        StopPlatformRotation();
        teamSelectionModels.SetActive(true);
        characterSelectionModels.SetActive(false);
        weaponSelectionModels.SetActive(false);
        rotationParent.localRotation = Quaternion.Euler(0, 30, 0);
        platformRotCurrentRot = platformRotTargetRot = 30;
    }
    public void StartCharacterSelection()
    {
        StopPlatformRotation();
        teamSelectionModels.SetActive(false);
        characterSelectionModels.SetActive(true);
        weaponSelectionModels.SetActive(false);
        rotationParent.localRotation = Quaternion.Euler(0, 0, 0);
        platformRotCurrentRot = platformRotTargetRot = 0;
    }
    public void StartWeaponSelection()
    {
        StopPlatformRotation();
        teamSelectionModels.SetActive(false);
        characterSelectionModels.SetActive(false);
        weaponSelectionModels.SetActive(true);
        rotationParent.localRotation = Quaternion.Euler(0, 0, 0);
        platformRotCurrentRot = platformRotTargetRot = 0;
    }
    public void Lock()
    {
        for (int i = 0; i < charSelectPlayerModels.Length; i++)
        {
            charSelectPlayerModels[i].Lock();
        }
    }
    public void Unlock()
    {
        for (int i = 0; i < charSelectPlayerModels.Length; i++)
        {
            charSelectPlayerModels[i].Unlock();
        }
    }

    public void ChangeCharSelectModels(Team team)
    {
        for (int i = 0; i < charSelectPlayerModels.Length; i++)
        {
            charSelectPlayerModels[i].SwitchTeam(team);
        }
    }

    public PlayerSkinData GetPlayerSkin(PlayerBodyType bodyType)
    {
        for (int i = 0; i < charSelectPlayerModels.Length; i++)
        {
            if (charSelectPlayerModels[i].mySkin.bodyType == bodyType) return charSelectPlayerModels[i].mySkin;
        }
        return null;
    }

    public void ChangeWeaponSelectModels(Team team, PlayerBodyType bodyType)
    {
        for (int i = 0; i < weaponSelectPlayerModels.Length; i++)
        {
            weaponSelectPlayerModels[i].LoadWeaponSelect(team, bodyType);
        }
    }

    public WeaponSkinData GetWeaponSkin(WeaponType weaponType)
    {
        for (int i = 0; i < weaponSelectPlayerModels.Length; i++)
        {
            if (weaponSelectPlayerModels[i].myWeaponSkin.weaponType == weaponType) return weaponSelectPlayerModels[i].myWeaponSkin;
        }
        return null;
    }

    public WeaponSkinRecolor GetWeaponSkinRecolor(Team team, WeaponType weaponType)
    {
        WeaponSkinRecolor result = new WeaponSkinRecolor();
        for (int i = 0; i < weaponSelectPlayerModels.Length; i++)
        {
            if (weaponSelectPlayerModels[i].myWeaponSkin.weaponType == weaponType) result = weaponSelectPlayerModels[i].myWeaponSkin.skinRecolors[(int)team];
        }
        return result;
    }

    public void StartPlatformRotation(bool dirRight, float increment)
    {
        StopPlatformRotation();
        Debug.Log("Starting Rotation to the " + (dirRight ? "right" : "left" )+ " with an incremet of " + increment);

        if (!platformRotStarted)
        {
            //platformRotInitialRot = rotationParent.localRotation.eulerAngles.y;
            platformRotStarted = true;
            platformRotTime = 0;
            switch (dirRight)
            {
                case true:
                    platformRotTargetRot = platformRotTargetRot + increment;
                    //platformRotRealTargetRot = platformRotTargetRot >= 360 ? platformRotTargetRot - 360 : platformRotTargetRot;
                    break;
                case false:
                    platformRotTargetRot = platformRotTargetRot - increment;
                    //platformRotRealTargetRot = platformRotTargetRot < 0 ? platformRotTargetRot + 360 : platformRotTargetRot;
                    break;
            }

            Debug.Log("Start Platform Rotation: platformRotCurrentRot = " + platformRotCurrentRot + "; platformRotTargetRot = "+ platformRotTargetRot);
        }
    }

    public void ProcessPlatformRotation()
    {
        if (platformRotStarted)
        {

            platformRotTime += Time.deltaTime;
            float progress = Mathf.Clamp01(platformRotTime / platformRotMaxTime);
            platformRotCurrentRot = EasingFunction.EaseInOutQuart(platformRotCurrentRot, platformRotTargetRot, progress);
            rotationParent.localRotation = Quaternion.Euler(0, platformRotCurrentRot, 0);
            //Debug.Log("Rotating Platform: Time = "+ platformRotTime);
            if (platformRotTime >= platformRotMaxTime)
            {
                StopPlatformRotation();
            }
        }
    }

    void StopPlatformRotation()
    {
        if (platformRotStarted)
        {
            Debug.Log("STOP Rotating Platform");
            platformRotStarted = false;
        }
    }
}
