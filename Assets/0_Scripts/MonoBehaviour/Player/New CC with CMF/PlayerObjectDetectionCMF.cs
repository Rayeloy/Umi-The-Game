using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region ----[ PUBLIC ENUMS ]----
#endregion
public class PlayerObjectDetectionCMF : MonoBehaviour
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    public PlayerMovement myPlayerMovement;
    #endregion

    #region ----[ PROPERTIES ]----
    [HideInInspector]
    public List<HookPoint> hookPoints;
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

    #region OnTrigger
    private void OnTriggerEnter(Collider col)
    {
        switch (col.tag)
        {
            case "HookPoint":
                if (col.name.Contains("SmallTrigger"))
                {
                    HookPoint hookPoint = col.transform.parent.GetComponent<HookPoint>();
                    if (hookPoint != null)
                    {
                        if (!hookPoints.Contains(hookPoint))
                        {
                            //print("hookpoint added");
                            hookPoints.Add(hookPoint);
                        }
                    }
                    else
                    {
                        Debug.LogError("Error: the variable hookPoint is null.");
                    }
                }
                break;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        switch (col.tag)
        {
            case "HookPoint":
                if (col.name.Contains("SmallTrigger"))
                {
                    HookPoint hookPoint = col.transform.parent.GetComponent<HookPoint>();
                    if (hookPoint != null)
                    {
                        if (hookPoints.Contains(hookPoint))
                        {
                            if (!myPlayerMovement.disableAllDebugs) print("hookpoint removed");
                            hookPoints.Remove(hookPoint);
                        }
                    }
                    else
                    {
                        Debug.LogError("Error: the variable hookPoint is null.");
                    }
                }
                break;
        }
    }

    bool onTriggerStayFirstTime = false;
    private void OnTriggerStay(Collider col)
    {
        if (!onTriggerStayFirstTime)
        {
            onTriggerStayFirstTime = true;
            switch (col.tag)
            {
                case "HookPoint":
                    if (col.name.Contains("SmallTrigger"))
                    {
                        HookPoint hookPoint = col.transform.parent.GetComponent<HookPoint>();
                        if (hookPoint != null)
                        {
                            if (!hookPoints.Contains(hookPoint))
                            {
                                print("hookpoint added");
                                hookPoints.Add(hookPoint);
                            }
                        }
                        else
                        {
                            Debug.LogError("Error: the variable hookPoint is null.");
                        }
                    }
                    break;
            }
        }
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----
    #endregion
}

#region ----[ STRUCTS & CLASSES ]----
#endregion
