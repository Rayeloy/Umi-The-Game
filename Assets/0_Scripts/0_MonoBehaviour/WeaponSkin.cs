using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSkin : MonoBehaviour
{
    public TrailRenderer[] trailRenderers;
    public WeaponPart[] secondaryParts;
}

[System.Serializable]
public class WeaponPart
{
    public GameObject gO;
    public WeaponOffsets weaponOffsets;
}
