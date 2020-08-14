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

    public SkinnedMeshRenderer body;
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
    [Header(" - BODY -")]
    public Material[] bodyMatsA;
    public Material[] bodyMatsB;
    public Material[] bodyMatsNone;
    public Material[] hairMats;//0 -> Team A (Green/Blue); 1 -> Team B (Pink/Red); 2 -> None
    public Material[] eyesMats;
    public Material[] eyebrowsMats;
    public Material[] noseMats;
    [Header(" - CLOTHES -")]
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
                for (int i = 0; i < bodyMatsA.Length; i++)
                {
                    if (bodyMatsA[i] != null) body.materials[i] = bodyMatsA[i];
                }

                if (hairMats.Length>=1 && hairMats[0] != null) hair.material = hairMats[0];
                if (eyesMats.Length >= 1 && eyesMats[0] != null) eyes.material = eyesMats[0];
                if (noseMats.Length >= 1 && noseMats[0] != null) nose.material = noseMats[0];
                if (eyebrowsMats.Length >= 1 && eyebrowsMats[0] != null) eyebrows.material = eyebrowsMats[0];

                for (int i = 0; i < clothing.Length; i++)
                {
                    if (clothingMatsA[i] != null) clothing[i].material = clothingMatsA[i];
                }
                break;

            case Team.B:
                for (int i = 0; i < bodyMatsB.Length; i++)
                {
                    if (bodyMatsB[i] != null) body.materials[i] = bodyMatsB[i];
                }

                if (hairMats.Length >= 2 && hairMats[1] != null) hair.material = hairMats[1];
                if (eyesMats.Length >= 2 && eyesMats[1] != null) eyes.material = eyesMats[1];
                if (noseMats.Length >= 2 && noseMats[1] != null) nose.material = noseMats[1];
                if (eyebrowsMats.Length >= 2 && eyebrowsMats[1] != null) eyebrows.material = eyebrowsMats[1];

                for (int i = 0; i < clothing.Length; i++)
                {
                    if (clothingMatsB[i] != null) clothing[i].material = clothingMatsB[i];
                }
                break;

            case Team.none:
                for (int i = 0; i < bodyMatsNone.Length; i++)
                {
                    if (bodyMatsNone[i] != null) body.materials[i] = bodyMatsNone[i];
                }

                if (hairMats.Length >= 1 && hairMats[2] != null) hair.material = hairMats[2];
                if (eyesMats.Length >= 1 && eyesMats[2] != null) eyes.material = eyesMats[2];
                if (noseMats.Length >= 1 && noseMats[2] != null) nose.material = noseMats[2];
                if (eyebrowsMats.Length >= 1 && eyebrowsMats[2] != null) eyebrows.material = eyebrowsMats[2];

                for (int i = 0; i < clothing.Length; i++)
                {
                    if (clothingMatsNone[i] != null) clothing[i].material = clothingMatsNone[i];
                }
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

