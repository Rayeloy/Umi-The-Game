using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpawnParent : MonoBehaviour
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    public bool toggleMaterial = false;
    int matIndex = 0;
    public Material[] materials;
    public bool visibleTriggerMeshes = false;
    public MeshRenderer spawnTrigger;
    public MeshRenderer[] spawnWalls;
    #endregion

    #region ----[ PROPERTIES ]----
    #endregion

    #region ----[ VARIABLES ]----
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    #endregion

    #region Start
    private void Start()
    {
        if (visibleTriggerMeshes)
        {
            spawnTrigger.enabled = true;
            for(int i=0; i < spawnWalls.Length; i++)
            {
                spawnWalls[i].enabled = true;
            }
        }
        else
        {
            spawnTrigger.enabled = false;
            for (int i = 0; i < spawnWalls.Length; i++)
            {
                spawnWalls[i].enabled = false;
            }
        }
    }
    #endregion

    #region Update
    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (toggleMaterial)
            {
                if (matIndex >= materials.Length)
                {
                    matIndex = 0;
                }

                toggleMaterial = false;
                for(int i = 0; i < spawnWalls.Length; i++)
                {
                    spawnWalls[i].material = materials[matIndex];
                }
                if (matIndex >= materials.Length)
                {
                    matIndex = 0;
                }
                else
                {
                    matIndex++;
                }
            }
        }
    }
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
