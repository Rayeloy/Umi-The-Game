using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAlteration : MonoBehaviour {

    private Renderer rend;

    //MakeUpColor1GlitterAlpha(RGBA) RGB - Color , A - Glitter Amount in this zone , overall controlled by the Red values of the MakeUpRGB Texture map (UV based).
    [Tooltip("Control Make-up Values : RGBA/XYZW : Color(vector3) Glitter Amount(Float)")]
    public Vector4 makeUp1;
    public Vector4 makeUp2;
    public Vector4 makeUp3;
    [Tooltip("Control Make-up channel amounts")]
    public Vector3 colorPower;




    // Use this for initialization
    void Start () {
        rend = GetComponent<Renderer>();

        // get current Values
        makeUp1 = rend.material.GetVector("_MakeUpColor1GlitterAlpha");
       // rend.material.SetVector("_MakeUpColor1GlitterAlpha", makeUp1); // assign new color

        makeUp2 = rend.material.GetVector("_MakeUpColor2GlitterAlpha");
        //rend.material.SetVector("_MakeUpColor1GlitterAlpha", makeUp2); // assign new color

        makeUp3 = rend.material.GetVector("_MakeUpColor3GlitterAlpha");
        //rend.material.SetVector("_MakeUpColor1GlitterAlpha", makeUp3); // assign new color

        colorPower = rend.material.GetVector("_ColorPowerRGB");
        //rend.material.SetVector("_ColorPowerRGB", colorPower); // assign new color
    }
	
	// Update is called once per frame
	void Update () {

       rend.material.SetVector("_MakeUpColor1GlitterAlpha", makeUp1); // assign new color
       rend.material.SetVector("_MakeUpColor2GlitterAlpha", makeUp2); // assign new color
       rend.material.SetVector("_MakeUpColor3GlitterAlpha", makeUp3); // assign new color
       rend.material.SetVector("_ColorPowerRGB", colorPower); // assign new color


    }
}
