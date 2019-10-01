using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class AutomaticCenterLayoutGroup : MonoBehaviour
{
    RectTransform rectTransform;

    [SerializeField]
    List<RectTransform> groupUI;
    [SerializeField]
    List<LayoutElement> layoutElements;
    [SerializeField]
    List<TransformInfo> transformInfos;

    public bool executeAutoCenter = false;
    public double updateFreq = 1;
    //private double maxUpdateTime = 0;
      
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        //transformInfos = new List<TransformInfo>();
        //groupUI = new List<RectTransform>();
        //layoutElements = new List<LayoutElement>();
    }

    private void Start()
    {
        //FillTransformInfo();
        //if (groupUI.Count == 0)
        //{
        //    FillGroupUI();
        //}
        //if (layoutElements.Count == 0)
        //{
        //    FillLayoutElements();
        //}
        //if (layoutElements.Count != groupUI.Count)
        //{
        //    Debug.LogError("Error: The amount of elements in layoutElements list and groupUI list are not the same.");
        //}
    }

    private void OnEnable()
    {
        if (transformInfos == null)
        {
            FillTransformInfo();
        }

        if (!Application.isPlaying)//En Editor
        {
            print("ON UNITY EDITOR");
            if (TransformHasChangedList(transformInfos))
            {
                FillGroupUI();
                FillLayoutElements();
                AutoCenterGroup();
            }
            StartTimer();
        }
        else //En play
        {
            if (groupUI.Count > 0)
            {
                for (int i = 0;i < groupUI.Count; i++)
                {
                    ContentSizeFitter fitter = groupUI[i].GetComponent<ContentSizeFitter>();
                    if (fitter != null)//Si Tiene Content Size Fitter
                    {
                        fitter.enabled = true;
                    }
                }
            }

            if (TransformHasChangedList(transformInfos))
            {
                AutoEquispace();
                AutoCenterGroup();
            }
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            ProcessTimer();
        }
        else
        {
            if (TransformHasChangedList(transformInfos))
            {
                AutoEquispace();
                AutoCenterGroup();
            }
        }

        if (executeAutoCenter)
        {
            executeAutoCenter = false;
            FillTransformInfo();
            FillGroupUI();
            FillLayoutElements();
            AutoCenterGroup();
        }
    }

    void StartTimer()
    {
        //maxUpdateTime = EditorApplication.timeSinceStartup + updateFreq;
    }

    void ProcessTimer()
    {
        if (true)//EditorApplication.timeSinceStartup > maxUpdateTime)
        {
            if (TransformHasChangedList(transformInfos))
            {
                FillGroupUI();
                FillLayoutElements();
                AutoCenterGroup();
            }
            StartTimer();
        }
    }

    void AutoEquispace()
    {
        //print("LayoutElement "+ layoutElements[0] .rect.name+ " at localpos = " + layoutElements[0].rect.localPosition);
        for(int i = 0; i + 1 < layoutElements.Count; i++)
        {
            //print("LayoutElement " + layoutElements[i+1].rect.name + " old localpos = " + layoutElements[i + 1].rect.localPosition);
            float totalSpacing= ((layoutElements[i].rect.rect.width/2)* layoutElements[i].rect.localScale.x)+ layoutElements[i].horDistToNextElement
                + ((layoutElements[i+1].rect.rect.width / 2) * layoutElements[i+1].rect.localScale.x);
            layoutElements[i + 1].rect.localPosition = layoutElements[i].rect.localPosition + Vector3.right* totalSpacing;
            //print("LayoutElement " + layoutElements[i+1].rect.name + " new localpos = " + layoutElements[i + 1].rect.localPosition);
        }
    }

    /// <summary>
    /// Automatically positions the UI elements children of this object in the hierarchy so that they are centered inside this object's rectangle
    /// in the UI.
    /// </summary>
    void AutoCenterGroup()
    {
        float offset = CalculateOffsetNeeded();
        MoveGroupOfUIObjects(new Vector3(offset, 0, 0));
    }

    bool TransformHasChangedList(List<TransformInfo> transfInfos)
    {
        bool result = false;
        for(int i=0; i < transfInfos.Count && !result; i++)
        {
            if (TransformHasChanged(transfInfos[i]))
            {
                result = true;
            }
        }
        if (result)
        {
            FillTransformInfo();
        }
        return result;
    }

    bool TransformHasChanged(TransformInfo transfInfo)
    {
        bool result = false;

        if (transfInfo.transform == null || transfInfo.transform.position != transfInfo.position
    || transfInfo.transform.rotation != transfInfo.rotation || transfInfo.transform.localScale != transfInfo.scale
    || (transfInfo.transform as RectTransform).rect.width != transfInfo.width || (transfInfo.transform as RectTransform).rect.height != transfInfo.height)
        {
            result = true;
        }
        return result;
    }

    void FillTransformInfo()
    {
        transformInfos = new List<TransformInfo>();

        transformInfos.Clear();
        transformInfos.Add(new TransformInfo(transform, transform.position, transform.rotation, transform.localScale));
        for (int i = 0; i < transform.childCount; i++)
        {
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
            groupUI[groupUI.Count - 1].pivot = new Vector2(0.5f, 0.5f);
            myPrint += transform.GetChild(i).name;
            if (i != transform.childCount - 1)
            {
                myPrint += ", ";
            }
        }
        myPrint += "}";
        print(myPrint);
    }
    
    void FillLayoutElements()
    {
        layoutElements = new List<LayoutElement>();
        List<RectTransform> groupUICopy = new List<RectTransform>();
        for (int i=0; i<groupUI.Count; i++)
        {
            groupUICopy.Add(groupUI[i]);
        }
        List<RectTransform> groupUIInOrder = new List<RectTransform>();
        string myPrint = "groupUIInOrder = ";
        while (groupUIInOrder.Count < groupUI.Count)
        {
            int index = 0;
            float lowestX = float.MaxValue;
            for (int i = 0; i < groupUICopy.Count; i++)
            {
                float center = groupUICopy[i].position.x;
                if (center < lowestX)
                {
                    lowestX = center;
                    index = i;
                }
            }
            myPrint += groupUICopy[index].name+", ";
            groupUIInOrder.Add(groupUICopy[index]);
            groupUICopy.RemoveAt(index);
        }
        print(myPrint);
        print("groupUICopy.Count = " + groupUICopy.Count+"; groupUIInOrder.Count = "+groupUIInOrder.Count+"; groupUI.Count = "+groupUI.Count);
        myPrint= "layoutElements = ";
        for (int i = 0; i < groupUIInOrder.Count; i++)
        {
            float distToNext = 0;
            float halfWidth1=0, halfWidth2 = 0;
            if (i + 1 < groupUIInOrder.Count)
            {
                distToNext = Mathf.Abs(groupUIInOrder[i + 1].localPosition.x - groupUIInOrder[i].localPosition.x);
                halfWidth1 = (groupUIInOrder[i].rect.width / 2 * groupUIInOrder[i].localScale.x);
                halfWidth2 = (groupUIInOrder[i + 1].rect.width / 2 * groupUIInOrder[i + 1].localScale.x);
                distToNext = distToNext -(halfWidth1+halfWidth2);
            }
            myPrint+=groupUIInOrder[i]+", distToNext = "+distToNext+", halfWidth1 = "+halfWidth1+", halfWidth2 = "+halfWidth2+"; ";
            LayoutElement layEle = new LayoutElement(groupUIInOrder[i], distToNext);
            layoutElements.Add(layEle);
        }
        print(myPrint);
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

[System.Serializable]
public struct TransformInfo
{
    public Transform transform;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public float width;
    public float height;

    public TransformInfo(Transform _transform, Vector3 _position, Quaternion _rotation, Vector3 _scale)
    {
        transform = _transform;
        position = _position;
        rotation = _rotation;
        scale = _scale;
        width = (transform as RectTransform).rect.width;
        height = (transform as RectTransform).rect.height;
    }
}

[System.Serializable]
public class LayoutElement
{
    [SerializeField]
    public RectTransform rect;
    [SerializeField]
    public float horDistToNextElement;

    public LayoutElement(RectTransform _rect, float _horDistToNextElement)
    {
        rect = _rect;
        horDistToNextElement = _horDistToNextElement;
    }
}
