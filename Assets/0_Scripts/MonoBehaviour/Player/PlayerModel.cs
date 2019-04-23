using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region ----[ PUBLIC ENUMS ]----
#endregion
public class PlayerModel : MonoBehaviour
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    [Header(" --- References ---")]
    public SkinnedMeshRenderer hair;
    public SkinnedMeshRenderer skin;
    public SkinnedMeshRenderer wetsuit;
    public SkinnedMeshRenderer accesories;
    public SkinnedMeshRenderer boots;

    public Transform senaka;//espalda
    public Transform rightHand;
    public Transform leftHand;

    //PLAYER MODEL MATERIALS
    [Header("--- PLAYER MODEL MATERIALS ---")]
    public Material[] hairMats;//0 -> Team A (Green/Blue); 1 -> Team B (Pink/Red)
    public Material[] skinMats;
    public Material[] wetsuitMats;
    public Material[] accesoriesMats;
    public Material[] bootsMats;
    #endregion

    #region ----[ PROPERTIES ]----
    #endregion

    #region ----[ VARIABLES ]----
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    #endregion

    #region Start
    #endregion

    #region Update
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----
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

#region ----[ STRUCTS ]----
#endregion

