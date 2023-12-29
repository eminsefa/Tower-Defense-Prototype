using _Tower_Defense_Prototype.Game.Scripts.Managers;
using _Tower_Defense_Prototype.Game.Units.Enemy.Scripts;
using DG.Tweening;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Projectile.Scripts
{
    public class ProjectileBase : PoolManager.PoolObject
    {
        private float m_ProjectileLifeTimer;
        private bool  m_IsActive;

        [SerializeField] private Rigidbody2D    m_Rigidbody;
        [SerializeField] private GameObject     m_Body;
        [SerializeField] private ProjectileData m_ProjectileData;

        public override void Init()
        {
            base.Init();
            SetRigidbodySimulated(false);

            m_IsActive            = false;
            m_ProjectileLifeTimer = 0;
            m_Body.SetActive(false);
        }

        private void SetBodyDirection(Vector2 moveDirection)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            m_Body.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        public void AttackForce(Vector2 force)
        {
            EnableProjectile();
            SetRigidbodySimulated(true);
            m_Rigidbody.AddForce(force);
        }
        
        public void AttackDirection(Vector2 direction)
        {
            EnableProjectile();
            SetRigidbodySimulated(true);
            m_Rigidbody.AddForce(direction * m_ProjectileData.FireSpeed);
        }

        private void Update()
        {
            if (!m_IsActive) return;

            SetBodyDirection(m_Rigidbody.velocity);
            m_ProjectileLifeTimer += Time.deltaTime;
            if (m_ProjectileLifeTimer > m_ProjectileData.LifeTime) DeactivateProjectile();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (GameManager.GameState != GameManager.eGameState.Playing)
            {
                DeactivateProjectile();
                return;
            }

            if (!col.transform.parent.TryGetComponent(out EnemyBase enemy)) return;
            DeactivateProjectile();
            enemy.IsHit(m_ProjectileData);
        }

        private void EnableProjectile()
        {
            SetRigidbodySimulated(true);
            m_IsActive = true;
            m_Body.SetActive(true);
        }

        private void SetRigidbodySimulated(bool active)
        {
            m_Rigidbody.simulated = active;
            if (m_Rigidbody.bodyType == RigidbodyType2D.Static) return;

            m_Rigidbody.velocity        = Vector2.zero;
            m_Rigidbody.angularVelocity = 0;
        }

        private void DeactivateProjectile()
        {
            SetRigidbodySimulated(false);
            m_Body.SetActive(false);
            base.Deactivated();
        }
    }
}