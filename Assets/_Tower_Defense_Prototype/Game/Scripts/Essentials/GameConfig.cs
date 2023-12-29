using System;
using _Tower_Defense_Prototype.Game.Levels.Scripts;
using _Tower_Defense_Prototype.Game.Units.Enemy.Scripts;
using _Tower_Defense_Prototype.Game.Units.Tower.Projectile.Scripts;
using _Tower_Defense_Prototype.Game.Units.Tower.Scripts;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Scripts.Essentials
{
    //This could be remote config
    [CreateAssetMenu(fileName = "GameConfig")]
    public class GameConfig : SingletonScriptableObject<GameConfig>
    {
        public int            TargetFrameRate;
        public LevelVariables Level;
        public UnitVariables  Units;

        [Serializable]
        public struct LevelVariables
        {
            public LevelData[] LevelData;
        }

        [Serializable]
        public struct UnitVariables
        {
            public EnemyVariables      Enemy;
            public ProjectileVariables Projectile;
            public TowerVariables      Tower;
            
            [Serializable]
            public struct EnemyVariables
            {
                public EnemyData[] EnemyData;
            }
            
            [Serializable]
            public struct TowerVariables
            {
                public TowerData[] TowerData;
            }
            
            [Serializable]
            public struct ProjectileVariables
            {
                public ProjectileData[] ProjectileData;
            }
        }
    }
}