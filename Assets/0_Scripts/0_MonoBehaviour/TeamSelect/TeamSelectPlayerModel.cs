﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelectPlayerModel : MonoBehaviour
{
    public PlayerSkinData mySkin;
    public WeaponSkinData myWeaponSkin;
    public PlayerSkinData[] allPlayerDefaultSkins;
    PlayerModel myPlayerModel;
    public RuntimeAnimatorController[] animatorControllers;//0 -> Umiboy, 1-> UmiBigBoy, 2-> UmiGirl, 3-> UmiBigGirl
    Animator myAnimator;
    int frameCount = 0;
    bool reseAnimatorStarted = false;

    public void SwitchTeam(Team team)
    {
        //Destroy old skin/model
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        GameObject model = null;
        model = Instantiate(mySkin.skinRecolorPrefabs[(int)team], transform);

        myPlayerModel = model.GetComponent<PlayerModel>();
        myAnimator = model.AddComponent<Animator>();
        myAnimator.avatar = mySkin.avatar;
        myAnimator.runtimeAnimatorController = animatorControllers[(int)mySkin.bodyType];
        //myAnimator.Play("Idle", -1);
    }

    public void LoadWeaponSelect(Team team, PlayerBodyType bodyType)
    {
        mySkin = allPlayerDefaultSkins[(int)bodyType];
        SwitchTeam(team);
        myAnimator.SetInteger("Weapon", (int)myWeaponSkin.weaponType);

        //LOAD WEAPON

        switch (myWeaponSkin.weaponType)
        {
            case WeaponType.QTip:
            case WeaponType.Hammer:
                GameObject weapon = Instantiate(myWeaponSkin.skinRecolors[(int)team].skinRecolorPrefab, myPlayerModel.rightHand);
                WeaponOffsets myWeaponOffsets = myWeaponSkin.GetWeaponOffsets(bodyType);
                weapon.transform.localPosition = myWeaponOffsets.rHandPositionOffset;
                weapon.transform.localRotation = Quaternion.Euler(myWeaponOffsets.rHandRotationOffset);
                break;
            case WeaponType.Boxing_gloves:
                break;
        }
    }

    public void Lock()
    {
        myAnimator.SetBool("Pose", true);
    }

    public void Unlock()
    {
        myAnimator.SetBool("Pose", false);
    }
}