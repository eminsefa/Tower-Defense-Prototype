using _Tower_Defense_Prototype.Game.Scripts.Managers;
using _Tower_Defense_Prototype.Game.Units.Tower.Projectile.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Scripts
{
    [InlineEditor]
    [CreateAssetMenu]
    public class TowerData : ScriptableObject
    {
        public bool                  DebugAttack;
        public float                 AttackRate;
        public float                 SearchRadius;
        public float                 SearchRate;
        public float                 VisualUpdateRate;
        public int                   AimCheckCount;
        public PoolManager.ePoolType PoolType;
        public ProjectileData        ProjectileData;
    }
}