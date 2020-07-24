using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelData : ScriptableObject
{
    public SkinnedMeshRenderer hair;
    public SkinnedMeshRenderer skin;
    public SkinnedMeshRenderer wetsuit;
    public SkinnedMeshRenderer accesories;
    public SkinnedMeshRenderer boots;


    //PLAYER MODEL MATERIALS
    [Header("--- PLAYER MODEL MATERIALS ---")]
    public Material[] hairMats;//0 -> Team A (Green/Blue); 1 -> Team B (Pink/Red); 2 -> None
    public Material[] skinMats;
    public Material[] wetsuitMats;
    public Material[] accesoriesMats;
    public Material[] bootsMats;
}
