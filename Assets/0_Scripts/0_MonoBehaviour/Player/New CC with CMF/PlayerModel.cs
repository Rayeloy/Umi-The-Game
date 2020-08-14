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
    public Transform senaka;//espalda
    public Transform rightHand;
    public Transform leftHand;

    public SkinnedMeshRenderer hair;
    public SkinnedMeshRenderer eyes;
    public SkinnedMeshRenderer eyebrows;
    public SkinnedMeshRenderer nose;
    public SkinnedMeshRenderer[] clothing;

    [Header("Old")]
    public SkinnedMeshRenderer wetsuit;
    public SkinnedMeshRenderer accesories;
    public SkinnedMeshRenderer boots;

    //PLAYER MODEL MATERIALS
    [Header("--- PLAYER MODEL MATERIALS ---")]
    public Material[] hairMats;//0 -> Team A (Green/Blue); 1 -> Team B (Pink/Red); 2 -> None
    public Material[] eyesMats;
    public Material[] eyebrowsMats;
    public Material[] noseMats;
    public Material[] clothingMatsA;
    public Material[] clothingMatsB;
    public Material[] clothingMatsNone;

    [Header("Old")]
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
                eyes.material = eyesMats[0];
                nose.material = noseMats[0];
                eyebrows.material = eyebrowsMats[0];

                for (int i = 0; i < clothing.Length; i++)
                {
                    clothing[i].material = clothingMatsA[i];
                }
                //wetsuit.material = wetsuitMats[0];
                //accesories.material = accesoriesMats[0];
                //boots.material = bootsMats[0];
                break;
            case Team.B:
                //myPlayerWeap.AttachWeapon("Churro Rojo");
                //Body.material = teamRedMat;
                //myBody = Instantiate(playerBodyPrefabs[1], bodyParent);
                hair.material = hairMats[1];
                eyes.material = eyesMats[1];
                nose.material = noseMats[1];
                eyebrows.material = eyebrowsMats[1];

                for (int i = 0; i < clothing.Length; i++)
                {
                    clothing[i].material = clothingMatsB[i];
                }
                //wetsuit.material = wetsuitMats[1];
                //accesories.material = accesoriesMats[1];
                //boots.material = bootsMats[1];
                break;
            case Team.none:
                hair.material = hairMats[2];
                eyes.material = eyesMats[2];
                nose.material = noseMats[2];
                eyebrows.material = eyebrowsMats[2];

                for (int i = 0; i < clothing.Length; i++)
                {
                    clothing[i].material = clothingMatsNone[i];
                }
                //wetsuit.material = wetsuitMats[2];
                //accesories.material = accesoriesMats[2];
                //boots.material = bootsMats[2];
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

