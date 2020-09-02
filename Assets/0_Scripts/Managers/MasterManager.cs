using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "Singletons/MasterManager")]
public class MasterManager : SingletonScriptableObject<MasterManager>
{
    [SerializeField]
    private GameSettings _gameSettings;

    public static GameSettings GameSettings { get { return Instance._gameSettings; } }

    [SerializeField]
    private HousingSettings _housingSettings;

    public static HousingSettings HousingSettings { get { return Instance._housingSettings; } }

    //public static GameObject FindGameObjectContains(string name, Transform parent = null)
    //{
    //    GameObject gameObjects
    //}

    //[SerializeField]
    //private List<NetworkedPrefab> _networkedPrefabs = new List<NetworkedPrefab>();

    //public static GameObject NetworkInstantiate(GameObject obj, Vector3 position, Quaternion rotation)
    //{
    //    foreach (NetworkedPrefab networkedPrefab in Instance._networkedPrefabs)
    //    {
    //        if (networkedPrefab.Prefab == obj)
    //        {
    //            if (networkedPrefab.Path != string.Empty)
    //            {
    //                GameObject result = PhotonNetwork.Instantiate(networkedPrefab.Path, position, rotation);
    //                return result;
    //            }
    //            else
    //            {
    //                Debug.LogError("Path is empty for gameobject name " + networkedPrefab.Prefab);
    //                return null;
    //            }
    //        }
    //    }

    //    return null;
    //}

    //    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //    public static void PopulateNetworkedPrefabs()
    //    {
    //#if UNITY_EDITOR

    //        GameObject[] results = Resources.LoadAll<GameObject>("");
    //        Instance._networkedPrefabs.Clear();
    //        for (int i = 0; i < results.Length; i++)
    //        {
    //            if (results[i].GetComponent<PhotonView>() != null)
    //            {
    //                string path = AssetDatabase.GetAssetPath(results[i]);
    //                Instance._networkedPrefabs.Add(new NetworkedPrefab(results[i], path));
    //                //Debug.Log("Loaded gameobject " + results[i] + ", with path " + path+"; _networkedPrefabs.Count = "+ Instance._networkedPrefabs.Count);
    //            }
    //        }
    //#endif
    //    }

}

