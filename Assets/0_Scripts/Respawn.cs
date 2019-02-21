using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour {

    Mesh myMesh;

    public Team team;
    [HideInInspector]
    float playerHalfWidth = 0.41f;
    [Tooltip("Maximum distance that can be between two players when setting the spawns. This avoids players from spawning at the edges of the spawn. Set to a high value if no restriction is desired.")]
    public float maxSpacing = 2;

    private void Awake()
    {
        myMesh = GetComponent<MeshFilter>().mesh;
    }

    public List<Vector3> SetSpawnPositions(int playerNum)
    {
        if (myMesh == null)
        {
            myMesh = GetComponent<MeshFilter>().mesh;
        }
        Vector3 origin;
        List<Vector3> spawnPositions = new List<Vector3>();
        playerHalfWidth = Mathf.Clamp(playerHalfWidth, 0, myMesh.bounds.extents.x);
        Vector3 dir = transform.right;
        Vector3 min = transform.TransformPoint(new Vector3(myMesh.bounds.min.x, myMesh.bounds.center.y, myMesh.bounds.center.z));
        Vector3 max = transform.TransformPoint(new Vector3(myMesh.bounds.max.x, myMesh.bounds.center.y, myMesh.bounds.center.z));
        float width = Mathf.Abs((max - min).magnitude) - (playerHalfWidth * 2);
        float spacing = width /(playerNum-1);
        if(spacing > maxSpacing)
        {
            spacing = maxSpacing;
            float newWidth = spacing * (playerNum - 1);
            newWidth = Mathf.Clamp(newWidth, 0, width);
            Vector3 center = transform.TransformPoint(myMesh.bounds.center);
            origin = center + (-dir * (newWidth / 2));
        }
        else
        {
            origin = transform.TransformPoint(new Vector3(myMesh.bounds.min.x, myMesh.bounds.center.y, myMesh.bounds.center.z));
            origin += dir * playerHalfWidth;
        }
        print("Respawn "+team+" : Origin = "+origin+ "; playerHalfWidth = " + playerHalfWidth + "; dir = " + dir+
            "; spacing = "+spacing+ "; min = "+min+"; max = "+max+"; width = "+width);

        for(int i=0; i<playerNum; i++)
        {
            Vector3 spawnPoint = origin +(dir * spacing * i);
            print (spawnPoint);
            spawnPositions.Add(spawnPoint);
        }

        return spawnPositions;
    }
}
