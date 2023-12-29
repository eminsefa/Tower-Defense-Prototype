using _Tower_Defense_Prototype.Game.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Projectile.Scripts
{
    [InlineEditor]
    [CreateAssetMenu]
    public class ProjectileData : ScriptableObject
    {
        public float                 FireSpeed;
        public float                 LifeTime;
        public int                   Damage;
        public PoolManager.ePoolType PoolType;
    }
}