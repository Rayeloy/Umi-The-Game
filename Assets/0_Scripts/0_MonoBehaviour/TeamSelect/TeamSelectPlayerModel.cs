using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelectPlayerModel : MonoBehaviour
{
    public PlayerSkinData mySkin;

    public void SwitchTeam(Team team)
    {
        //Destroy old skin/model
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        switch (team)
        {
            case Team.none:
                Instantiate(mySkin.skinRecolorPrefabs[0], transform);
                break;
            case Team.A:
                Instantiate(mySkin.skinRecolorPrefabs[1], transform);              
                break;
            case Team.B:
                Instantiate(mySkin.skinRecolorPrefabs[2], transform);
                break;
        }
    }
    //private void Update()
    //{
    //    if (currentModel != null)
    //    {
    //        Debug.Log("Model's position b4 = " + currentModel.transform.position);
    //        currentModel.transform.position = Vector3.zero;
    //        currentModel.transform.localRotation = Quaternion.Euler(0, 0, 0);
    //        Debug.Log("Model's position after = " + currentModel.transform.position);
    //    }
    //}
}
