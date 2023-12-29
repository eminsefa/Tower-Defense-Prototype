using System;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using _Tower_Defense_Prototype.Game.Units.Enemy.Scripts;
using Dreamteck.Splines;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Scripts
{
    public class TowerAttack
    {
        public float AttackTimer { get; set; }

        protected SplineComputer Spline;
        protected TowerBase      TowerBase;
        protected LayerMask      EnemyLayer;
        public    EnemyBase      CurrentTarget { get; protected set; }

        private readonly Collider2D[] m_EnemiesInRadius = new Collider2D[8];

        public TowerAttack(TowerBase towerBase, PoolManager poolManager, SplineComputer splineComputer)
        {
            TowerBase  = towerBase;
            EnemyLayer = LayerMask.GetMask("Enemy");
            Spline     = splineComputer;
        }

        public void Init()
        {
            AttackTimer = TowerBase.TowerData.AttackRate;
        }

        //Attacks every rate time
        public void CheckToAttack()
        {
            AttackTimer += Time.deltaTime;
            if (AttackTimer < TowerBase.TowerData.AttackRate) return;

            if (CurrentTarget == null || !CurrentTarget.IsAlive) CheckForEnemies();
            else Attack();
        }

        //Checks area around tower, finds all colliders
        private void CheckForEnemies()
        {
            Array.Clear(m_EnemiesInRadius, 0, m_EnemiesInRadius.Length);
            var count = Physics2D.OverlapCircleNonAlloc(TowerBase.transform.position, TowerBase.TowerData.SearchRadius, m_EnemiesInRadius, EnemyLayer);
            if (count > 0) SetTargetEnemy(count);
            else             CurrentTarget = null;
        }

        //Decide the target by logic(s)
        private void SetTargetEnemy(int count)
        {
            float closest = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var e = m_EnemiesInRadius[i];
                if (!e.transform.parent.TryGetComponent(out EnemyBase target)) continue;
                var dist = Vector2.Distance(TowerBase.transform.position, target.transform.position);
                if (dist < closest)
                {
                    closest       = dist;
                    CurrentTarget = target;
                }
            }
        }

        protected virtual void Attack()
        {
            AttackTimer = 0;
        }
    }
}