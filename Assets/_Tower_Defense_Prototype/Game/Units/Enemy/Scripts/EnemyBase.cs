using System;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using _Tower_Defense_Prototype.Game.Units.Tower.Projectile.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Enemy.Scripts
{
    public class EnemyBase : PoolManager.PoolObject
    {
        public static event Action<EnemyBase> OnEnemyReachedBase;
        public static event Action<EnemyBase> OnEnemyDead;
        
        public  bool      IsAlive           { get; protected set; }
        public  float     CenterYOffset     => Mathf.Abs(m_Collider.bounds.extents.y / 2f);
        public  double    GetPathPercentage => m_Movement.GetPathPercentage;
        public  int       Health            => m_Health;
        public  float     MoveSpeed         => m_Movement.MoveSpeed;
        public  EnemyData EnemyData         => m_EnemyData;
        
        private int           m_Health;
        private Vector3       m_LastPosition;
        private float         m_VisualUpdateTimer;
        private EnemyVisual   m_Visual;
        private EnemyMovement m_Movement;

        [SerializeField] private   Collider2D     m_Collider;
        [SerializeField] protected SpriteRenderer m_Body;
        [SerializeField] protected Transform      m_HealthBar;
        [SerializeField] protected EnemyData      m_EnemyData;

        private void Awake()
        {
            InitializeObject();
        }

        protected override void OnLevelFailed()
        {
            if (!IsAlive) return;
            m_Movement.SetMove(false);
            m_Visual.LevelFailed();
        }

        private void InitializeObject()
        {
            m_Visual   = new EnemyVisual(this, m_HealthBar, m_Body);
            m_Movement = new EnemyMovement(this, s_Spline);
        }

        public override async void Init()
        {
            IsAlive = false;
            base.Init();

            await m_Movement.Init();
            m_Visual.Init();

            m_VisualUpdateTimer = float.MaxValue;
            m_LastPosition      = transform.position;
            m_Health            = (int) (m_EnemyData.MaxHealth * m_EnemyData.ProgressCurve.Evaluate(StorageManager.CurrentWave));
            m_Collider.enabled  = true;
            IsAlive             = true;
        }

        public async void SetSavedData(StorageManager.EnemySaveData saveData)
        {
            m_Health = saveData.Health;
            await m_Movement.SetSavedData(saveData.MovePercent);
            m_Visual.SetHealthBar((float) m_Health / m_EnemyData.MaxHealth);
            m_Visual.EnableVisual();
        }

        public async void StartLevel()
        {
            while (!IsAlive) await UniTask.Yield();
            m_Movement.SetMove(true);
            m_Visual.StartMove();
        }

        private void LateUpdate()
        {
            if (GameManager.GameState != GameManager.eGameState.Playing) return;
            if (!IsAlive) return;
            CheckDirection();
        }

        private void CheckDirection()
        {
            m_VisualUpdateTimer += Time.deltaTime;
            if (m_VisualUpdateTimer < m_EnemyData.VisualUpdateRate) return;
            m_VisualUpdateTimer = 0;

            var pos  = transform.position;
            var flip = pos.x - m_LastPosition.x < 0;
            m_LastPosition = pos;

            m_Visual.SetSpriteDirection(flip);
        }

        public void IsHit(ProjectileData i_ProjectileData)
        {
            m_Health = Mathf.Max(0, m_Health - i_ProjectileData.Damage);

            m_Visual.SetHealthBar((float) m_Health / m_EnemyData.MaxHealth);

            if (m_Health <= 0) Dead();
        }

        private void Dead()
        {
            m_Collider.enabled = false;
            m_Movement.Dead();
            m_Visual.Dead();
            IsAlive = false;

            OnEnemyDead?.Invoke(this);

            Invoke(nameof(Deactivated), 1f);
        }

        protected override void Deactivated()
        {
            if (!IsActive) return;

            m_Visual.Init();
            var t = m_Movement.Init();
            base.Deactivated();
        }

        public void OnEndReached()
        {
            OnEnemyReachedBase?.Invoke(this);
        }

        private void OnDestroy()
        {
            m_Movement.Dispose();
        }
    }
}