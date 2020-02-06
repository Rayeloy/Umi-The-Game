using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideMeshesOnPlay : MonoBehaviour
{
    public bool hideMeshes = true;
    public HideMeshesMode mode = HideMeshesMode.GameObjectAndChildren;

    public enum HideMeshesMode
    {
        GameObjectAndChildren,
        OnlyGameObject,
        OnlyChildren
    }
    private void Awake()
    {
        if (hideMeshes)
        {
            switch (mode)
            {
                case HideMeshesMode.OnlyGameObject:
                    GetComponent<MeshRenderer>().enabled = false;
                    break;
                case HideMeshesMode.OnlyChildren:
                    MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
                    for (int i = 0; i < meshes.Length; i++)
                    {
                        meshes[i].enabled = false;
                    }
                    GetComponent<MeshRenderer>().enabled = true;
                    break;
                case HideMeshesMode.GameObjectAndChildren:
                    meshes = GetComponentsInChildren<MeshRenderer>();
                    for (int i = 0; i < meshes.Length; i++)
                    {
                        meshes[i].enabled = false;
                    }
                    break;
            }
        }
    }
}
