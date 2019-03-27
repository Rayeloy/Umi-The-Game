using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflatableWall : MonoBehaviour
{
    [Header("Walls")]
    public GameObject inflatableWallBefore;
    public GameObject inflatableWallInflatingAlembic;
    public float inflatableWallInflatingAnimMaxTime;
    public GameObject inflatableWallAfter;
    public GameObject inflatableWallBreakingAlembic;
    public float inflatableWallBreakingAnimMaxTime;
    public GameObject inflatableWallPlatforms;
    bool inflatableWallInflating = false;
    bool inflatableWallBreaking = false;
    float inflatableWallInflatingTime = 0;
    float inflatableWallBreakingTime = 0;

    public GameObject inflatableWallStandingColliders;
    public GameObject inflatableWallFallenColliders;

    public void KonoAwake()
    {
        inflatableWallInflatingAlembic.SetActive(false);
        inflatableWallBefore.SetActive(true);
        inflatableWallBreakingAlembic.SetActive(false);
        inflatableWallAfter.SetActive(false);
        inflatableWallPlatforms.SetActive(false);
        inflatableWallStandingColliders.SetActive(true);
        inflatableWallFallenColliders.SetActive(false);
    }

    public void KonoUpdate()
    {
        InflatingWall();
        BreakingWall();
    }

    public void StartInflatingWall()
    {
        inflatableWallInflatingAlembic.SetActive(true);
        inflatableWallBefore.SetActive(false);
        inflatableWallAfter.SetActive(false);
        inflatableWallInflating = true;
        inflatableWallInflatingTime = 0;
    }

    void InflatingWall()
    {
        if (inflatableWallInflating)
        {
            inflatableWallInflatingTime += Time.deltaTime;
            if (inflatableWallInflatingTime >= inflatableWallInflatingAnimMaxTime)
            {
                StopInflatingWall();
            }
        }
    }

    void StopInflatingWall()
    {
        inflatableWallInflatingAlembic.SetActive(false);
        inflatableWallBefore.SetActive(false);
        inflatableWallAfter.SetActive(true);
        inflatableWallInflating = false;
    }

    public void StartBreakingWall()
    {
        inflatableWallBreakingAlembic.SetActive(true);
        inflatableWallAfter.SetActive(false);
        inflatableWallStandingColliders.SetActive(false);
        inflatableWallFallenColliders.SetActive(true);
        inflatableWallBreaking = true;
        inflatableWallBreakingTime = 0;
    }

    void BreakingWall()
    {
        if (inflatableWallBreaking)
        {
            inflatableWallBreakingTime += Time.deltaTime;
            if (inflatableWallBreakingTime >= inflatableWallInflatingAnimMaxTime)
            {
                StopBreakingWall();
            }
        }
    }

    void StopBreakingWall()
    {
        inflatableWallBreakingAlembic.SetActive(false);
        inflatableWallPlatforms.SetActive(true);
        inflatableWallBreaking = false;
    }
}
