using System;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using Cysharp.Threading.Tasks;
using Dreamteck.Splines;

namespace _Tower_Defense_Prototype.Game.Units.Enemy.Scripts
{
    public class EnemyMovement 
    {
        public double GetPathPercentage => m_SplineFollower.result.percent;
        public float  MoveSpeed         => m_SplineFollower.followSpeed;

        private readonly EnemyBase      m_EnemyBase;
        private readonly SplineFollower m_SplineFollower;

        public EnemyMovement(EnemyBase enemyBase,SplineComputer splineComputer)
        {
            m_EnemyBase             = enemyBase;
            m_SplineFollower        = enemyBase.GetComponent<SplineFollower>();
            m_SplineFollower.spline = splineComputer;
            m_SplineFollower.RebuildImmediate();
                
            m_SplineFollower.onEndReached += OnEndReached;
        }
        
        public void Dispose()
        {
            m_SplineFollower.onEndReached -= OnEndReached;
            GC.SuppressFinalize(this);
        }
        
        public async UniTask Init()
        {
            m_SplineFollower.followSpeed   = m_EnemyBase.EnemyData.MoveSpeed * 
                                             m_EnemyBase.EnemyData.ProgressCurve.Evaluate(StorageManager.CurrentWave);
            m_SplineFollower.startPosition = 0;
            m_SplineFollower.Restart();
            m_SplineFollower.follow = false;
            
            await UniTask.WaitForFixedUpdate();
            await UniTask.Yield();
        }

        private void OnEndReached()
        {
            m_EnemyBase.OnEndReached();
        }

        public void SetMove(bool follow)
        {
            m_SplineFollower.follow = follow;
        }

        public void Dead()
        {
            m_SplineFollower.follow = false;
        }
        
        public async UniTask SetSavedData(double percent)
        {
            m_SplineFollower.startPosition = percent;
            m_SplineFollower.follow = false;
            await UniTask.WaitForFixedUpdate();
            await UniTask.Yield();
        }
    }
}