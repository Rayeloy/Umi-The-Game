using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class DontMoveWithParent : MonoBehaviour
{
    Vector3 savedPosition;

    [HelpBox("When DontMoveWithParent is on, Ctrl+Z doesn't work for movement changes on this GameObject.",HelpBoxMessageType.Warning)]
    public bool dontMoveWithParent = true;
    bool lastDontMoveWithParent = true;

    Vector3 parentLastPos;

    private void Update()
    {


        if (transform.hasChanged && !transform.parent.hasChanged && savedPosition != transform.position)
        {
            savedPosition = transform.position;
            transform.hasChanged = false;
        }

        if (!lastDontMoveWithParent && dontMoveWithParent)
            savedPosition = transform.position;

            //parentLastPos = transform.parent.position;
            //StartCoroutine(CheckIfParentHasMoved(0.1f));

            lastDontMoveWithParent = dontMoveWithParent;
    }

    private void LateUpdate()
    {
        if (dontMoveWithParent)
        {
            if (savedPosition == Vector3.zero)
            {
                savedPosition = transform.position;
            }

            if (transform.parent.hasChanged)
            {
                transform.position = savedPosition;
                transform.parent.hasChanged = false;
            }
        }
    }

    IEnumerator CheckIfParentHasMoved(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if(parentLastPos == transform.parent.position)
        {
            transform.parent.hasChanged = false;
            //Debug.Log("My parent has changed? " + transform.parent.hasChanged);
        }
        else
        {
            //Debug.Log("My parent has changed? " + transform.parent.hasChanged);
        }
    }
}
