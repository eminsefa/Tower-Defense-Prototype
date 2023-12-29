using _Tower_Defense_Prototype.Game.Scripts.Essentials;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Scripts.Managers
{
    public class FPSManager : MonoBehaviour
    {
        [SerializeField, Range(1, 30)] private float m_UpdatesPerSecond = 5; 

        [ShowInInspector, ReadOnly] public static int FPSAverageLastUpdate = 0;
        [ShowInInspector, ReadOnly] public static int FPSTicksLastUpdate   = 0;

        private float m_LastTime = 0;
        private float m_DeltaTime;
        private float m_DeltaTimeSum;

        private float m_CurrTime => Time.time;

        private void OnEnable()
        {
            Application.targetFrameRate = GameConfig.Instance.TargetFrameRate;
            FPSAverageLastUpdate        = 0;
            m_LastTime                  = m_CurrTime;

            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            m_DeltaTime = m_CurrTime - m_LastTime;

            if (m_DeltaTime <= 0) return; // will happen on the first tick

            m_LastTime     =  m_CurrTime;
            m_DeltaTimeSum += m_DeltaTime;

            FPSTicksLastUpdate++;
            if (m_DeltaTimeSum > 1 / m_UpdatesPerSecond)
            {
                float fps = FPSTicksLastUpdate / m_DeltaTimeSum;
                FPSAverageLastUpdate = Mathf.RoundToInt(fps);

                m_DeltaTimeSum = FPSTicksLastUpdate = 0;
            }
        }
    }
}