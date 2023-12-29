using _Tower_Defense_Prototype.Game.Scripts.Essentials;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using _Tower_Defense_Prototype.Game.Units.Tower.Projectile.Scripts;
using _Tower_Defense_Prototype.Game.Units.Tower.Scripts;
using Dreamteck.Splines;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Bowman
{
    public class BowmanAttack : TowerAttack
    {
        private readonly PoolManager m_PoolManager;

        public BowmanAttack(TowerBase towerBase, PoolManager poolManager, SplineComputer splineComputer) : base(towerBase, poolManager, splineComputer)
        {
            TowerBase     = towerBase;
            m_PoolManager = poolManager;
            Spline        = splineComputer;
            EnemyLayer    = LayerMask.GetMask("Enemy");
        }

        protected override void Attack()
        {
            var force = DecideAttackForce();
            if (CurrentTarget == null) return;

            base.Attack();
            var p = m_PoolManager.Get(TowerBase.TowerData.ProjectileData.PoolType) as ProjectileBase;
            p.transform.position = TowerBase.FirePoint.position;
            p.AttackForce(force);
        }

        //Decide trajectory position to hit enemy in future 
        private Vector2 DecideAttackForce()
        {
            var projectileData              = TowerBase.TowerData.ProjectileData;
            var targetCenterYOffset         = CurrentTarget.CenterYOffset;
            var targetCurrentPathPercentage = CurrentTarget.GetPathPercentage;
            var aimCheckCount               = TowerBase.TowerData.AimCheckCount;
            var towerFirePos                = (Vector2) TowerBase.FirePoint.position;
            var gravity                     = -Physics2D.gravity.y;
            
            // Guessing the possible hit direction
            for (int i = 1; i < aimCheckCount; i++)
            {
                if (!CurrentTarget.IsAlive) break;
                float interceptGuessTime = i * projectileData.LifeTime / aimCheckCount;

                // Spline follower uses delta time regardless of the update method
                float   moveDist       = CurrentTarget.MoveSpeed * interceptGuessTime;
                double  newPercent     = Spline.Travel(targetCurrentPathPercentage, moveDist);
                Vector2 futureEnemyPos = Spline.EvaluatePosition(newPercent);
                futureEnemyPos.y += targetCenterYOffset;

                var horizontalDist  = futureEnemyPos.x - towerFirePos.x;
                var horizontalSpeed = Mathf.Sign(horizontalDist) * projectileData.FireSpeed;
                var horizontalMove  = horizontalSpeed            * interceptGuessTime * Time.fixedDeltaTime;
                var hitTime         = horizontalDist             / (horizontalSpeed * Time.fixedDeltaTime);
                if (hitTime > projectileData.LifeTime) continue;
                if (hitTime < 0.1f) continue;

                var verticalDist  = futureEnemyPos.y - towerFirePos.y;
                var verticalSpeed = (verticalDist / hitTime + gravity * hitTime / 2) / Time.fixedDeltaTime;

                var futureHorizontalPos = towerFirePos.x + horizontalMove;

                if (Mathf.Abs(futureEnemyPos.x - futureHorizontalPos) > targetCenterYOffset) continue;

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

                return new Vector2(horizontalSpeed, verticalSpeed);
            }

            if (TowerBase.TowerData.DebugAttack) Debug.LogWarning($"Couldn't aim to target");
            CurrentTarget = null;
            AttackTimer   = TowerBase.TowerData.SearchRate;
            return Vector2.zero;
        }
    }
}