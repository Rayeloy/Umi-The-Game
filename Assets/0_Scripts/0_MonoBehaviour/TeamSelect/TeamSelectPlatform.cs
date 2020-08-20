using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelectPlatform : MonoBehaviour
{
    public Transform rotationParent;//ROTATE THIS
    public GameObject TeamSelectionModels;
    public GameObject CharacterSelectionModels;
    public GameObject WeaponSelectionModels;

    PlayerModel[] charSelectPlayerModels;

    //Platform Rotation
    bool platformRotStarted = false;
    float platformRotInitialRot;
    float platformRotTime = 0;
    float platformRotTargetRot = 0;
    float platformRotRealTargetRot = 0;
    float platformRotMaxTime = 0;


    private void Awake()
    {
        charSelectPlayerModels = CharacterSelectionModels.GetComponentsInChildren<PlayerModel>();
        platformRotInitialRot = platformRotTargetRot = platformRotRealTargetRot = rotationParent.localRotation.eulerAngles.y;
    }

    public void Update()
    {
        ProcessPlatformRotation();
    }

    public void StartTeamSelect()
    {
        TeamSelectionModels.SetActive(true);
        CharacterSelectionModels.SetActive(false);
        WeaponSelectionModels.SetActive(false);
        rotationParent.localRotation = Quaternion.Euler(0, 30, 0);
    }
    public void StartCharacterSelection()
    {
        TeamSelectionModels.SetActive(false);
        CharacterSelectionModels.SetActive(true);
        WeaponSelectionModels.SetActive(false);
        rotationParent.localRotation = Quaternion.Euler(0, 0, 0);
    }
    public void StartWeaponSelectionModels()
    {
        TeamSelectionModels.SetActive(false);
        CharacterSelectionModels.SetActive(false);
        WeaponSelectionModels.SetActive(true);
        rotationParent.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void ChangeTeamColors(Team team)
    {
        for (int i = 0; i < charSelectPlayerModels.Length; i++)
        {
            charSelectPlayerModels[i].SwitchTeam(team);
        }
    }

    public void RotatePlatform(bool dirRight, float increment)
    {
        //StopChangeTeamAnimation();
        if (!platformRotStarted)
        {
            platformRotInitialRot = rotationParent.localRotation.eulerAngles.y;
            platformRotStarted = true;
            platformRotTime = 0;
            switch (dirRight)
            {
                case true:
                    platformRotTargetRot = platformRotRealTargetRot + increment;
                    platformRotRealTargetRot = platformRotTargetRot >= 360 ? platformRotTargetRot - 360 : platformRotTargetRot;
                    break;
                case false:
                    platformRotTargetRot = platformRotRealTargetRot - increment;
                    platformRotRealTargetRot = platformRotTargetRot < 0 ? platformRotTargetRot + 360 : platformRotTargetRot;
                    break;
            }

            //Debug.Log("START CHANGE TEAM ANIMATION: changeTeamAnimationInitialRot = " + changeTeamAnimationInitialRot + "; changeTeamAnimationTargetRot = " + changeTeamAnimationTargetRot);
        }
    }

    public void ProcessPlatformRotation()
    {
        if (platformRotStarted)
        {
            platformRotTime += Time.deltaTime;
            float progress = Mathf.Clamp01(platformRotTime / platformRotMaxTime);
            float yRot = EasingFunction.EaseInOutQuart(platformRotInitialRot, platformRotTargetRot, progress);
            rotationParent.localRotation = Quaternion.Euler(0, yRot, 0);

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
            platformRotStarted = false;
        }
    }
}
