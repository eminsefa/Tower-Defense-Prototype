using _Tower_Defense_Prototype.Game.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Enemy.Scripts
{
    [InlineEditor]
    [CreateAssetMenu]
    public class EnemyData : ScriptableObject
    {
        public float                 MoveSpeed;
        public int                   MaxHealth;
        public float                 VisualUpdateRate;
        public int                   Damage;
        public int                   Score;
        public AnimationCurve        ProgressCurve;
        public PoolManager.ePoolType PoolType;
    }
}
