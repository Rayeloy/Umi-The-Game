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
    public Material[] hairMats;//0 -> Team A (Green/Blue); 1 -> Team B (Pink/Red); 2 -> None
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
    public void SwitchTeam(Team team)
    {
        switch (team)
        {
            case Team.A:
                //myPlayerWeap.AttachWeapon("Churro Azul");
                //Body.material = teamBlueMat;
                //myBody = Instantiate(playerBodyPrefabs[0], bodyParent);
                hair.material = hairMats[0];
                skin.material = skinMats[0];
                wetsuit.material = wetsuitMats[0];
                accesories.material = accesoriesMats[0];
                boots.material = bootsMats[0];
                break;
            case Team.B:
                //myPlayerWeap.AttachWeapon("Churro Rojo");
                //Body.material = teamRedMat;
                //myBody = Instantiate(playerBodyPrefabs[1], bodyParent);
                hair.material = hairMats[1];
                skin.material = skinMats[1];
                wetsuit.material = wetsuitMats[1];
                accesories.material = accesoriesMats[1];
                boots.material = bootsMats[1];
                break;
            case Team.none:
                hair.material = hairMats[2];
                skin.material = skinMats[2];
                wetsuit.material = wetsuitMats[2];
                accesories.material = accesoriesMats[2];
                boots.material = bootsMats[2];
                break;
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

#region ----[ STRUCTS ]----
#endregion

