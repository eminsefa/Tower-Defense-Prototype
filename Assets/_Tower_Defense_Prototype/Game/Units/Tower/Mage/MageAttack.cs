using _Tower_Defense_Prototype.Game.Scripts.Essentials;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using _Tower_Defense_Prototype.Game.Units.Tower.Projectile.Scripts;
using _Tower_Defense_Prototype.Game.Units.Tower.Scripts;
using Dreamteck.Splines;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Mage
{
    public class MageAttack : TowerAttack
    {
        private readonly PoolManager m_PoolManager;

        public MageAttack(TowerBase towerBase, PoolManager poolManager, SplineComputer splineComputer) : base(towerBase, poolManager, splineComputer)
        {
            TowerBase     = towerBase;
            m_PoolManager = poolManager;
            Spline        = splineComputer;
            EnemyLayer    = LayerMask.GetMask("Enemy");
        }

        protected override void Attack()
        {
            var dir = DecideAttackDirection();
            if (CurrentTarget == null) return;

            base.Attack();
            var p = m_PoolManager.Get(TowerBase.TowerData.ProjectileData.PoolType) as ProjectileBase;
            p.transform.position = TowerBase.FirePoint.position;
            p.AttackDirection(dir);
        }

        //Decide direction to hit enemy in future position
        private Vector2 DecideAttackDirection()
        {
            var projectileData              = TowerBase.TowerData.ProjectileData;
            var targetCenterYOffset         = CurrentTarget.CenterYOffset;
            var targetCurrentPathPercentage = CurrentTarget.GetPathPercentage;
            var towerFirePos                = (Vector2) TowerBase.FirePoint.position;
            var aimCheckCount               = TowerBase.TowerData.AimCheckCount;

            //Guessing the possible hit direction
            for (int i = 1; i < aimCheckCount; i++)
            {
                if (!CurrentTarget.IsAlive) break;
                float interceptGuessTime = i * projectileData.LifeTime / aimCheckCount;

                //Spline follower use delta time regardless of update method
                float  moveDist = CurrentTarget.MoveSpeed * interceptGuessTime;
                double movePerc = Spline.Travel(targetCurrentPathPercentage, moveDist);
                // Vector2 futureEnemyPos = Spline.Evaluate(movePerc, SplineComputer.EvaluateMode.Calculate).position; //More accurate method
                Vector2 futureEnemyPos = Spline.EvaluatePosition(movePerc);
                futureEnemyPos.y += targetCenterYOffset;

                Vector2 dir = futureEnemyPos - towerFirePos;
                dir = dir.normalized;

                Vector2 futureProjectilePos = towerFirePos + dir * (projectileData.FireSpeed * interceptGuessTime * Time.fixedDeltaTime);
                float   distance            = (futureEnemyPos - futureProjectilePos).magnitude;

                if (distance > targetCenterYOffset) continue;

                var targetDist          = (Vector2) TowerBase.transform.InverseTransformPoint(futureEnemyPos);
                var targetLeftTowerArea = targetDist.magnitude > TowerBase.TowerData.SearchRadius;
                if (targetLeftTowerArea)
                {
                    if (TowerBase.TowerData.DebugAttack) Debug.Log("Target left radius");
                    CurrentTarget = null;
                    AttackTimer   = TowerBase.TowerData.SearchRate;
                    return Vector2.zero;
                }

                if (TowerBase.TowerData.DebugAttack) Debug.Log("Aimed to target");
                return dir;
            }

            if (TowerBase.TowerData.DebugAttack) Debug.LogWarning($"Couldn't aim to target");
            CurrentTarget = null;
            AttackTimer   = TowerBase.TowerData.SearchRate;
            return Vector2.zero;
        }
    }
}