using UnityEngine;
using UnityEngine.UI;

namespace BadDog
{
    public class FPSCounter : MonoBehaviour
    {
        public float updateInterval = 0.5F;

        private float accum = 0;
        private int totalFrames = 0;
        private float timeLeft;

        Text fpsText;

        // Use this for initialization
        void Start()
        {
            fpsText = GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {

            timeLeft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++totalFrames;

            if (timeLeft <= 0.0)
            {
                float fps = accum / totalFrames;
                string format = System.String.Format("FPS: {0:F1}", fps);
                fpsText.text = format;

                timeLeft = updateInterval;
                accum = 0.0F;
                totalFrames = 0;
            }
        }
    }
}
