using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerBodyType
{
    UmiBoy,
    UmiBigBoy,
    UmiGirl,
    UmiBigGirl
}

[CreateAssetMenu(fileName = "New player skin", menuName = "Player/Skin")]
public class PlayerSkinData : ScriptableObject
{
    public PlayerBodyType bodyType;
    public string skinName;
    public string skinID;
    public Avatar avatar;
    public RuntimeAnimatorController animatorController;
    public GameObject[] skinRecolorPrefabs; // 0-> BASE, 1-> GREEN, 2-> PINK
}
