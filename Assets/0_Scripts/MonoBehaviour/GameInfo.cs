using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

//Esta clase es para guardar datos del juego entre escenas
public enum UIAnimType
{
    none,
    shake
}
public class GameInfo : MonoBehaviour
{
    public static GameInfo instance;
    public GameObject inControlManager;
    public List<PlayerActions> playerActionsList;
    public List<Team> playerTeamList;
    public int nPlayers;

    List<UIAnimation> uIAnimations;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        instance = this;
        playerActionsList = new List<PlayerActions>();
        playerTeamList = new List<Team>();

        uIAnimations = new List<UIAnimation>();
    }

    private void Update()
    {
        ProcessUIAnimations();
    }
    public Team NoneTeamSelect()
    {
        int nAzul = 0;
        int nRojo = 0;
        foreach (Team t in playerTeamList)
        {
            if (t == Team.A)
                nAzul++;
            else if (t == Team.B)
                nRojo++;
        }
        if(nAzul == nRojo) //Mismo num de jugadores rojos que azules, random
        {
            if (Random.value < 0.5f)
                return Team.A;
            else
                return Team.B;
        }
       else if (nAzul > nRojo)  //mas numero de jugadores azules
           return Team.B;
       else                     //mas numero de jugadores rojos
           return Team.A;
//       else
//       {
//           if (Random.value < 0.5f){
//               Debug.Log("Random Azul");
//               return Team.blue;
//           }
//           else{
//               Debug.Log("Random Rojo");
//               return Team.red;
//           }
//       }
    }

    #region UIAnimations

    public void StartAnimation(UIAnimation uIAnimation, Camera canvasCamera)
    {
        for(int i = 0;i<uIAnimations.Count; i++)
        {
            if(uIAnimations[i].rect == uIAnimation.rect)
            {
                Debug.LogError("UIAnimation Error: you are trying to animate the same RectTransform at the same time with " +
                    "more than 1 animation. Are you sure this is correct?");
            }
        }
        if (!uIAnimations.Contains(uIAnimation))
        {
            float prop = Mathf.Min(canvasCamera.rect.width, canvasCamera.rect.height);
            uIAnimation.xAmplitude *= prop;
            uIAnimations.Add(uIAnimation);
            uIAnimation.StartAnimation();
        }
        else
        {
            uIAnimation.RestartAnimation();
        }

    }

    void ProcessUIAnimations()
    {
        for(int i = 0; i< uIAnimations.Count; i++)
        {
            if (!uIAnimations[i].ProcessAnimation())
            {
                uIAnimations.RemoveAt(i);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class UIAnimation
{
    public UIAnimType type = UIAnimType.none;
    public RectTransform rect;
    public float xAmplitude = 7f;
    public float frequency = 0.06f;
    public float duration = 0.5f;
    [Range(0,1)]
    public float cycleStartPoint = 0.5f;

    Vector3 originalLocalPos;
    int animDir;//1 going right; -1 going left
    float currentDuration, currentCycleTime, totalSpace;

    public UIAnimation(UIAnimType _type, ref RectTransform _rect, float _xAmplitude = 7f, float _frequency = 0.06f, 
        float _duration = 0.5f, float _cycleStartPoint=0.5f)
    {
        type = _type;
        rect = _rect;
        xAmplitude = _xAmplitude;
        frequency = _frequency;//cycle max time
        duration = _duration;
        _cycleStartPoint = Mathf.Clamp01(_cycleStartPoint);
        cycleStartPoint = _cycleStartPoint;
        StartAnimation();
    }

    public void StartAnimation()
    {
        animDir = 1;
        currentCycleTime = cycleStartPoint * frequency;
        currentDuration = 0;
        totalSpace = xAmplitude * 2;
        originalLocalPos = rect.localPosition;
    }

    public void RestartAnimation()
    {
        animDir = 1;
        currentCycleTime = cycleStartPoint * frequency;
        currentDuration = 0;
    }

    public bool ProcessAnimation()
    {
        float progress = currentCycleTime / frequency;
        switch (type)
        {
            case UIAnimType.shake:
                float xIncrement = progress * totalSpace * animDir;
                //float xIncrement = animDir * EasingFunction.EaseInOutQuart(0, xAmplitude, progress);
                float originX = originalLocalPos.x + (xAmplitude * -animDir);
                Vector3 finalPos = originalLocalPos;
                finalPos.x = originX + xIncrement;
                rect.localPosition = finalPos;
                break;
        }

        currentCycleTime += Time.deltaTime;
        if (currentCycleTime >= frequency)
        {
            animDir = animDir == 1 ? -1 : 1;
            currentCycleTime = 0;
        }

        currentDuration += Time.deltaTime;
        if(currentDuration>= duration)
        {
            rect.localPosition = originalLocalPos;
            return false;
        }
        return true;
    }
}
