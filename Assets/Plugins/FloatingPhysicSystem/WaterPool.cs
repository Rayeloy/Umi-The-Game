using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPool : MonoBehaviour
{
    private void OnEnable()
    {
        addPool();
    }

    private void OnDisable()
    {
        removePool();
    }

    private void OnDestroy()
    {
        removePool();
    }

    [HideInInspector]
    public BoxCollider _water = null;
    //METTERE ACTIVE IN EDITOR
    void addPool()
    {
        foreach (BoxCollider _box in GetComponents<BoxCollider>())
            if (_box.isTrigger)
            {
                _water = _box;
                break;
            }
        if (_water == null)
            Debug.LogError("There are no BOX COLLIDERS MARKED AS TRIGGER in this watert pool! Please, add at least one BOX COLLIDER and mark it as TRIGGER. this will be the water pool area.",gameObject);
        else
            if (!Poolsmanager.pools.Contains(this))
            Poolsmanager.pools.Add(this);
    }


    void removePool()
    {
        if (Poolsmanager.pools.Contains(this))
            Poolsmanager.pools.Remove(this);
    }
    
    
    public bool positionIsInside(Vector3 _pos)
    {
        return GetComponent<BoxCollider>().bounds.Contains(_pos);
    }
}
