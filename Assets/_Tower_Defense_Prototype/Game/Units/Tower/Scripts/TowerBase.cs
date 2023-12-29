using _Tower_Defense_Prototype.Game.Scripts.Managers;
using _Tower_Defense_Prototype.Game.Scripts.Essentials;
using DG.Tweening;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Scripts
{
    public class TowerBase : PoolManager.PoolObject
    {
        private   float       m_VisualCheckTimer;
        protected TowerAttack Attack;
        protected TowerVisual Visual;

        public float AttackTimer
        {
            get => Attack.AttackTimer;
            set => Attack.AttackTimer = value;
        }

        public TowerData TowerData => m_TowerData;
        public Transform FirePoint => m_FirePoint;

        [SerializeField] protected Transform m_Top;
        [SerializeField] protected Transform m_FirePoint;
        [SerializeField] protected TowerData m_TowerData;

        private void Awake()
        {
            InitializeObject();
        }

        protected override void OnLevelFailed()
        {
            if (!IsActive) return;

            Visual.SetDead();
            m_Top.transform.DOScale(0, 1f)
                 .SetDelay(1f)
                 .SetEase(Ease.OutSine);
        }

        protected virtual void InitializeObject()
        {
            Attack = new TowerAttack(this, s_PoolManager, s_Spline);
            Visual = new TowerVisual(this);
        }

        public override void Init()
        {
            base.Init();
            Attack.Init();
            Visual.Init();

            m_Top.transform.DOKill();
            m_Top.localScale   = Vector3.one;
            m_VisualCheckTimer = float.MaxValue;
        }

        private void Update()
        {
            if (GameManager.GameState != GameManager.eGameState.Playing) return;

            Attack.CheckToAttack();
            var hasTarget = Attack.CurrentTarget != null;
            
            SetTopDirection(hasTarget);
        }

        private void SetTopDirection(bool hasTarget)
        {
            m_VisualCheckTimer += Time.deltaTime;
            if (m_VisualCheckTimer > m_TowerData.VisualUpdateRate)
            {
                m_VisualCheckTimer = 0;
                Visual.SetIdle(!hasTarget);
            }
            if (hasTarget)
            {
                var flip = m_Top.position.x - Attack.CurrentTarget.transform.position.x > -0.15f;
                m_Top.transform.SetScaleAxis(Extensions.eAxis.x, flip ? -1 : 1);
            }
        }
    }
}