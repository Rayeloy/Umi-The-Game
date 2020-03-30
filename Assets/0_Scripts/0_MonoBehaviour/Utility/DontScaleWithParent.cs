using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class DontScaleWithParent : MonoBehaviour
{
    Vector3 savedScale;

    [HelpBox("Does not work well when parent has rotation changed or proportions changed.", HelpBoxMessageType.Warning)]
    public bool dontScaleWithParent = true;
    bool lastDontScaleWithParent = true;

    Vector3 parentLastScale;

    private void Update()
    {
        if (transform.hasChanged && !transform.parent.hasChanged && savedScale != transform.lossyScale)
        {
            savedScale = transform.lossyScale;
            transform.hasChanged = false;
        }

        if (!lastDontScaleWithParent && dontScaleWithParent)
            savedScale = transform.lossyScale;

        //parentLastPos = transform.parent.position;
        //StartCoroutine(CheckIfParentHasMoved(0.1f));

        lastDontScaleWithParent = dontScaleWithParent;
    }

    private void LateUpdate()
    {
        if (dontScaleWithParent)
        {
            if (savedScale == Vector3.zero)
            {
                savedScale = transform.lossyScale;
            }

            if (transform.parent.hasChanged)
            {
                VectorMath.SetGlobalScale(transform, savedScale);
                transform.parent.hasChanged = false;
            }
        }
    }

    IEnumerator CheckIfParentHasChangedScale(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (parentLastScale == transform.parent.lossyScale)
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