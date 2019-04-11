using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectDetection : MonoBehaviour
{
    List<HookPoint> hookPoints;

    private void Start()
    {
    }

    private void OnTriggerEnter(Collider col)
    {
        switch (col.tag)
        {
            case "HookPoint":
                HookPoint hookPoint = col.GetComponent<HookPoint>();
                if (!hookPoints.Contains(hookPoint))
                {
                    hookPoints.Add(hookPoint);
                }
                break;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        switch (col.tag)
        {
            case "HookPoint":
                HookPoint hookPoint = col.GetComponent<HookPoint>();
                if (hookPoints.Contains(hookPoint))
                {
                    hookPoints.Remove(hookPoint);
                }
                break;
        }
    }
    bool started = false;
    private void OnTriggerStay(Collider col)
    {
        if (!started)
        {
            switch (col.tag)
            {
                case "HookPoint":
                    HookPoint hookPoint = col.GetComponent<HookPoint>();
                    if (!hookPoints.Contains(hookPoint))
                    {
                        hookPoints.Add(hookPoint);
                    }
                    break;
            }
        }
    }
}
