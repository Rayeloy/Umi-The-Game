using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerBodyType
{
    UmiBoy=0,
    UmiBigBoy=1,
    UmiGirl=2,
    UmiBigGirl=3
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
