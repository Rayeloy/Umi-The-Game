using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BadDog
{
    [ExecuteInEditMode]
    public class BGMainLight : MonoBehaviour
    {
        [Header("Lighting Params")]

        [Range(0.001f, 0.9999f)]
        public float depthThreshold = 0.995f;
        [Range(0.001f, 1.5f)]
        public float lightingRadius = 1.0f;
        [Range(0.1f, 1)]
        public float lightingSampleWeight = 1.0f;
        [Range(0.1f, 1)]
        public float lightingDecay = 1.0f;
        [Range(0.5f, 20)]
        public float lightingIntensity = 1.0f;
        public Color lightingColor = Color.white;


        private Vector3 m_ViewPosition;
        public static List<BGMainLight> m_MainLightList = new List<BGMainLight>();


        private void OnEnable()
        {
            if (!m_MainLightList.Contains(this))
            {
                m_MainLightList.Add(this);
            }
        }

        private void OnDisable()
        {
            if(m_MainLightList.Contains(this))
            {
                m_MainLightList.Remove(this);
            }
        }

        public bool UpdateViewPosition(Camera camera)
        {
            m_ViewPosition = camera.WorldToViewportPoint(camera.transform.position - transform.forward);

            if (m_ViewPosition.x < -1 || m_ViewPosition.x > 2 || m_ViewPosition.y < -1 || m_ViewPosition.y > 2)
            {
                return false;
            }

            if (m_ViewPosition.z <= 0)
            {
                return false;
            }

            return true;
        }

        public Vector3 GetViewPosition()
        {
            return m_ViewPosition;
        }

        public static List<BGMainLight> GetAllLights()
        {
            return m_MainLightList;
        }
    }
}
