using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[ExecuteInEditMode]
public class AutomaticCenterLayoutGroup : MonoBehaviour
{
    RectTransform rectTransform;
    List<RectTransform> groupUI;
    public bool executeAutoCenter = false;
    List<TransformInfo> transformInfos;

    public  double updateFreq = 1;
    private double  maxUpdateTime = 0;
            
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        groupUI = new List<RectTransform>();
        transformInfos = new List<TransformInfo>();
        FillTransformInfo();
    }

    private void OnEnable()
    {
        if (transformInfos == null)
        {
            FillTransformInfo();
        }

        if (TransformHasChanged())
        {
            Awake();
            AutoCenterGroup();
        }
        StartTimer();
    }

    private void Update()
    {
        ProcessTimer();

        if (executeAutoCenter)
        {
            executeAutoCenter = false;
            AutoCenterGroup();
        }
    }

    void StartTimer()
    {
        maxUpdateTime = EditorApplication.timeSinceStartup + updateFreq;
    }

    void ProcessTimer()
    {
        if (EditorApplication.timeSinceStartup > maxUpdateTime)
        {
            if (TransformHasChanged())
            {
                AutoCenterGroup();
            }
            StartTimer();
        }
    }

    void AutoCenterGroup()
    {
        FillGroupUI();
        float offset = CalculateOffsetNeeded();
        MoveGroupOfUIObjects(new Vector3(offset, 0, 0));
    }

    bool TransformHasChanged()
    {
        bool result = false;

        if (transform.position != transformInfos[0].position || transform.rotation != transformInfos[0].rotation || transform.localScale != transformInfos[0].scale)
        {
            //Debug.LogWarning("Warning: transform has changed : (position changed ->" + (transform.position != transformInfos[0].position) +
            //" || rotation changed ->" + (transform.rotation != transformInfos[0].rotation) +
            //" || scale changed) ->" + (transform.localScale != transformInfos[0].scale));
            result = true;
        }
        else
        {
            for (int i = 1; i < transformInfos.Count && !result; i++)
            {
                //Debug.LogWarning(i + " child " + transformInfos[i].transform.name + "; Last pos = " + transformInfos[i].transform.position.ToString("F8") +
                //    "; current pos = " + transformInfos[i].position.ToString("F8"));
                if (transformInfos[i].transform == null || transformInfos[i].transform.position != transformInfos[i].position
                    || transformInfos[i].transform.rotation != transformInfos[i].rotation || transformInfos[i].transform.localScale != transformInfos[i].scale)
                {
                    if (transformInfos[i].transform == null)
                    {
                        //Debug.LogWarning("Warning: child " + transformInfos[i].transform.name + " transform has changed : transform doesn't exist anymore");
                    }
                    else
                    {
                        //Debug.LogWarning("Warning: child " + transformInfos[i].transform.name + " transform has changed : transform still exists..." +
                        //"(position changed ->" + (transformInfos[i].transform.position != transformInfos[i].position) +
                        //" || rotation changed ->" + (transformInfos[i].transform.rotation != transformInfos[i].rotation) +
                        //" || scale changed) ->" + (transformInfos[i].transform.localScale != transformInfos[i].scale));
                    }
                    result = true;
                }
            }
        }
        if (result)
        {
            FillTransformInfo();
        }

        return result;
    }

    void FillTransformInfo()
    {
        //Debug.Log("Adding new tranformInfos");

        if (transformInfos == null) transformInfos = new List<TransformInfo>();

        transformInfos.Clear();
        transformInfos.Add(new TransformInfo(transform, transform.position, transform.rotation, transform.localScale));
        for (int i = 0; i < transform.childCount; i++)
        {
            //Debug.Log(transform.GetChild(i).name + " added in pos " + i);
            transformInfos.Add(new TransformInfo(transform.GetChild(i), transform.GetChild(i).position, transform.GetChild(i).rotation, transform.GetChild(i).localScale));
        }
    }

    void FillGroupUI()
    {
        groupUI = new List<RectTransform>();
        string myPrint = "groupUI ={";
        for (int i = 0; i < transform.childCount; i++)
        {
            groupUI.Add(transform.GetChild(i).GetComponent<RectTransform>());
            myPrint += transform.GetChild(i).name;
            if (i != transform.childCount - 1)
            {
                myPrint += ", ";
            }
        }
        myPrint += "}";
        //print(myPrint);
    }

    Bounds CalculateBounds(RectTransform transform, float uiScaleFactor = 1)
    {
        Bounds bounds = new Bounds(transform.position, new Vector3(transform.rect.width, transform.rect.height, 0.0f) * uiScaleFactor);

        /*if (transform.childCount > 0)
        {
            foreach (RectTransform child in transform)
            {
                Bounds childBounds = new Bounds(child.position, new Vector3(child.rect.width, child.rect.height, 0.0f) * uiScaleFactor);
                bounds.Encapsulate(childBounds);
            }
        }*/

        return bounds;
    }

    float CalculateOffsetNeeded()
    {
        Vector3[] v = new Vector3[4];
        float[] vx = new float[4];
        rectTransform.GetWorldCorners(v);
        for (int i = 0; i < v.Length; i++)
        {
            vx[i] = v[i].x;
        }
        //Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.parent, rectTransform.transform);
        //Bounds bounds = CalculateBounds(rectTransform);
        float minX = Mathf.Min(vx);
        float maxX = Mathf.Max(vx);

        float groupMinX = float.MaxValue;
        float groupMaxX = float.MinValue;
        for (int i = 0; i < groupUI.Count; i++)
        {
            groupUI[i].GetWorldCorners(v);
            for (int j = 0; j < v.Length; j++)
            {
                vx[j] = v[j].x;
            }
            float min = Mathf.Min(vx);
            if (min < groupMinX)
            {
                groupMinX = min;
            }
            float max = Mathf.Max(vx);
            if (max > groupMaxX)
            {
                groupMaxX = max;
            }
        }

        //groupUI[0].GetWorldCorners(v);
        //groupMinX = v[0].x;
        //groupUI[groupUI.Count - 1].GetWorldCorners(v);
        //groupMaxX = v[3].x;
        //Bounds firstObjectBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.parent, groupUI[0].transform);
        //Bounds lastObjectBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.parent, groupUI[groupUI.Count - 1].transform);
        //Bounds firstObjectBounds = CalculateBounds(groupUI[0],groupUI[0].localScale.x);
        //Bounds lastObjectBounds = CalculateBounds(groupUI[groupUI.Count - 1], groupUI[groupUI.Count - 1].localScale.x);

        //print("AutomaticCenterLayoutGroup: minX = " + minX + "; maxX = " + maxX + "; groupMinX = " + groupMinX + "; groupMaxX = " + groupMaxX);
        float leftDistance = groupMinX - minX;
        float rightDistance = maxX - groupMaxX;
        //print("leftDistance = " + leftDistance + "; rightDistance = " + rightDistance);
        float finalOffset = 0;
        if (leftDistance > rightDistance)//More space at the left side
        {
            finalOffset = -(leftDistance - rightDistance);
        }
        else if (rightDistance > leftDistance)//More space at the right side
        {
            finalOffset = rightDistance - leftDistance;
        }
        finalOffset /= 2;
        //print("finalOffset = " + finalOffset);
        return finalOffset;
    }

    void MoveGroupOfUIObjects(Vector3 movement)
    {
        for (int i = 0; i < groupUI.Count; i++)
        {
            groupUI[i].position = groupUI[i].position + movement;
        }
    }
}

public struct TransformInfo
{
    public Transform transform;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformInfo(Transform _transform, Vector3 _position, Quaternion _rotation, Vector3 _scale)
    {
        transform = _transform;
        position = _position;
        rotation = _rotation;
        scale = _scale;
    }
}
