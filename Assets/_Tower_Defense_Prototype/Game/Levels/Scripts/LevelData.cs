using System;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using Dreamteck.Splines;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Levels.Scripts
{
    [InlineEditor]
    [CreateAssetMenu]
    public class LevelData : ScriptableObject
    {
        [Serializable]
        public struct WaveData
        {
            public AnimationCurve        StartDelay;
            public AnimationCurve        SpawnAmount;
            public AnimationCurve        SpawnRate;
            public PoolManager.ePoolType EnemyType;
        }

        public int                   BaseHealth;
        public PoolManager.ePoolType[] TowerTypes;
        public LevelPathData         PathData;
        public WaveData[]            Waves;
        
        [Serializable]
        public struct LevelPathData
        {
            public float         GridSize;
            public Vector2[]     Positions;
            public SplinePoint[] SplinePoints;
        }
    }
}