using UnityEngine;

namespace FIMSpace.Jiggling
{
    public class FJiggling_Demo_TriggerJiggleOnClick : MonoBehaviour
    {
        public FJiggling_Base JiggleScript;

        private void OnMouseDown()
        {
            if (JiggleScript)
                JiggleScript.StartJiggle();
        }
    }
}